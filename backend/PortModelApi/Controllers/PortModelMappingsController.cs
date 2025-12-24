using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PortModelApi.Data;
using PortModelApi.Models;
using PortModelApi.Services;
using System;
using System.Reflection;

namespace PortModelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortModelMappingsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PortModelMappingsController> _logger;
    private readonly IColumnLengthProvider _columnLengthProvider;
    private readonly TimeZoneInfo _configuredTimeZone;

    public PortModelMappingsController(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PortModelMappingsController> logger,
        IColumnLengthProvider columnLengthProvider,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _columnLengthProvider = columnLengthProvider;

        // Resolve configured timezone (supports Windows and Linux ids, with fallback to UTC)
        var tzId = configuration["AppSettings:TimeZone"];
        _configuredTimeZone = ResolveTimeZone(tzId);
    }

    private static TimeZoneInfo ResolveTimeZone(string? tzId)
    {
        if (string.IsNullOrWhiteSpace(tzId)) return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }
        catch (TimeZoneNotFoundException)
        {
            // Try common alternate names for Bangkok
            if (string.Equals(tzId, "SE Asia Standard Time", StringComparison.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok"); } catch { }
            }
            if (string.Equals(tzId, "Asia/Bangkok", StringComparison.OrdinalIgnoreCase))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); } catch { }
            }

            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }

    private DateTime GetConfiguredNow()
    {
        var utcNow = DateTime.UtcNow;
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _configuredTimeZone);
        return DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PortModelMapping>>> GetRecords()
    {
        return await _context.PortModelMappings.ToListAsync();
    }

    [HttpGet("{accno}/{date}")]
    public async Task<ActionResult<PortModelMapping>> GetRecord(string accno, DateOnly date)
    {
        var record = await _context.PortModelMappings.FindAsync(accno, date);
        if (record == null || record.IsDeleted) return NotFound();
        return record;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecord(PortModelMapping record)
    {
        var user = GetCurrentUser();
        _logger.LogInformation("Creating record for {AccnoSleeve} on {EffectiveDate} by {User}", record.AccnoSleeve, record.EffectiveDate, user);
        // Validate Portfolio
        if (!await _context.Portfolios.AnyAsync(p => p.Code == record.AccnoSleeve))
        {
            return BadRequest(new { message = $"Portfolio '{record.AccnoSleeve}' does not exist in 'dro.vPortfolio'." });
        }


        
        // Check for soft-deleted record with same key; if exists, un-delete and reuse it
        var existing = await _context.PortModelMappings.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.AccnoSleeve == record.AccnoSleeve && r.EffectiveDate == record.EffectiveDate);
        
        if (existing != null && existing.IsDeleted)
        {
            // Un-delete and update the existing soft-deleted record
            _logger.LogInformation("Restoring soft-deleted record for {AccnoSleeve} on {EffectiveDate}", record.AccnoSleeve, record.EffectiveDate);
            existing.ModelName = record.ModelName;
            existing.CurrencyModel = record.CurrencyModel;
            existing.HedgeModelName = record.HedgeModelName;
            existing.IsDeleted = false;
            existing.CreatedBy = user;
            existing.CreatedAt = GetConfiguredNow();
            
            var warningsRestore = await TruncateStringPropertiesAsync(existing);
            
            await _context.SaveChangesAsync();
            await LogAudit(existing, "I", user);
            
            var responseBody = new { record = existing, warnings = warningsRestore };
            return CreatedAtAction(nameof(GetRecord), new { accno = existing.AccnoSleeve, date = existing.EffectiveDate }, responseBody);
        }
        else if (existing != null && !existing.IsDeleted)
        {
            // Active record already exists with this key
            return Conflict(new { message = "Record already exists." });
        }
        
        record.CreatedBy = user;
        record.CreatedAt = GetConfiguredNow();
        record.IsDeleted = false;

        var warnings = await TruncateStringPropertiesAsync(record);

        _context.PortModelMappings.Add(record);

        try
        {
            await _context.SaveChangesAsync();
            await LogAudit(record, "I", user);

            var responseBody = new { record, warnings };
            return CreatedAtAction(nameof(GetRecord), new { accno = record.AccnoSleeve, date = record.EffectiveDate }, responseBody);
        }
        catch (DbUpdateException ex)
        {
            // SQL Server duplicate key errors: 2627 (violation of primary key/unique), 2601 (unique index)
            if (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
            {
                _logger.LogWarning(ex, "Duplicate insert attempted for {AccnoSleeve} on {EffectiveDate}", record.AccnoSleeve, record.EffectiveDate);
                return Conflict(new { message = "Record already exists." }); // short, clean message
            }

            _logger.LogError(ex, "Database update failed while creating record for {AccnoSleeve}", record.AccnoSleeve);
            return StatusCode(500, new { message = "Database error." });
        }
    }

    [HttpPut("{accno}/{date}")]
    public async Task<IActionResult> UpdateRecord(string accno, DateOnly date, PortModelMapping update)
    {
        _logger.LogInformation("Updating record for {AccnoSleeve} on {EffectiveDate}", accno, date);
        var existing = await _context.PortModelMappings.FindAsync(accno, date);
        if (existing == null || existing.IsDeleted) return NotFound();

        var user = GetCurrentUser();
        existing.ModelName = update.ModelName;
        existing.CurrencyModel = update.CurrencyModel;
        existing.HedgeModelName = update.HedgeModelName;
        existing.UpdatedBy = user;
        existing.UpdatedAt = GetConfiguredNow();

        var warnings = await TruncateStringPropertiesAsync(existing);

        await _context.SaveChangesAsync();
        await LogAudit(existing, "U", user);

        if (warnings.Count > 0)
            return Ok(new { record = existing, warnings });

        return NoContent();
    }

    [HttpDelete("{accno}/{date}")]
    public async Task<IActionResult> DeleteRecord(string accno, DateOnly date)
    {
        _logger.LogInformation("Deleting record for {AccnoSleeve} on {EffectiveDate}", accno, date);
        var record = await _context.PortModelMappings.FindAsync(accno, date);
        if (record == null || record.IsDeleted) return NotFound();

        var user = GetCurrentUser();
        record.IsDeleted = true;
        record.DeletedBy = user;
        record.DeletedAt = GetConfiguredNow();
        record.UpdatedBy = user;
        record.UpdatedAt = GetConfiguredNow();

        await _context.SaveChangesAsync();
        await LogAudit(record, "D", user);
        return NoContent();
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx files are supported." });

        var user = GetCurrentUser();
        var now = GetConfiguredNow();
        var results = new
        {
            TotalRows = 0,
            Created = 0,
            Updated = 0,
            Errors = new List<string>()
        };

        try
        {
            using var stream = file.OpenReadStream();
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null) return BadRequest(new { message = "Workspace is empty." });

            var usedRange = worksheet.RangeUsed();
            if (usedRange is null)
            {
                return BadRequest(new { message = "Worksheet has no data." });
            }
            var rows = usedRange.RowsUsed().Skip(1); // Skip header row
            int totalRows = 0;
            int createdCount = 0;
            int updatedCount = 0;
            var errorList = new List<string>();

            foreach (var row in rows)
            {
                totalRows++;
                try
                {
                    var accnoSleeve = row.Cell(1).GetValue<string>().Trim();
                    var effectiveDateStr = row.Cell(2).GetValue<string>().Trim();
                    var modelName = row.Cell(3).GetValue<string>().Trim();
                    var currencyModel = row.Cell(4).GetValue<string>().Trim();
                    var hedgeModel = row.Cell(5).GetValue<string>().Trim();

                    if (string.IsNullOrEmpty(accnoSleeve) || string.IsNullOrEmpty(effectiveDateStr) || string.IsNullOrEmpty(modelName))
                    {
                        errorList.Add($"Row {row.RowNumber()}: Missing mandatory fields (Account Sleeve, Effective Date, or Model Name).");
                        continue;
                    }

                    if (!DateOnly.TryParse(effectiveDateStr, out var effectiveDate))
                    {
                        errorList.Add($"Row {row.RowNumber()}: Invalid date format '{effectiveDateStr}'. Use YYYY-MM-DD.");
                        continue;
                    }

                    // Check if Portfolio exists
                    if (!await _context.Portfolios.AnyAsync(p => p.Code == accnoSleeve))
                    {
                        errorList.Add($"Row {row.RowNumber()}: Portfolio '{accnoSleeve}' does not exist.");
                        continue;
                    }

                    var existing = await _context.PortModelMappings.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(m => m.AccnoSleeve == accnoSleeve && m.EffectiveDate == effectiveDate);

                    if (existing != null)
                    {
                        // Update existing record (active or soft-deleted)
                        existing.ModelName = modelName;
                        existing.CurrencyModel = string.IsNullOrEmpty(currencyModel) ? null : currencyModel;
                        existing.HedgeModelName = string.IsNullOrEmpty(hedgeModel) ? null : hedgeModel;
                        existing.IsDeleted = false;
                        existing.UpdatedBy = user;
                        existing.UpdatedAt = now;
                        
                        // If it was deleted, we set who "un-deleted" it
                        if (existing.DeletedAt.HasValue)
                        {
                            existing.CreatedBy = user; // Mark as "re-created" by importer
                            existing.CreatedAt = now;
                            existing.DeletedBy = null;
                            existing.DeletedAt = null;
                        }

                        await TruncateStringPropertiesAsync(existing);
                        _context.PortModelMappings.Update(existing);
                        await LogAudit(existing, "U", user);
                        updatedCount++;
                    }
                    else
                    {
                        // Create new record
                        var newRecord = new PortModelMapping
                        {
                            AccnoSleeve = accnoSleeve,
                            EffectiveDate = effectiveDate,
                            ModelName = modelName,
                            CurrencyModel = string.IsNullOrEmpty(currencyModel) ? null : currencyModel,
                            HedgeModelName = string.IsNullOrEmpty(hedgeModel) ? null : hedgeModel,
                            IsDeleted = false,
                            CreatedBy = user,
                            CreatedAt = now
                        };

                        await TruncateStringPropertiesAsync(newRecord);
                        _context.PortModelMappings.Add(newRecord);
                        await LogAudit(newRecord, "I", user);
                        createdCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorList.Add($"Row {row.RowNumber()}: Unexpected error - {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Import completed: {Total} processed, {Created} created, {Updated} updated, {Errors} errors", totalRows, createdCount, updatedCount, errorList.Count);

            return Ok(new
            {
                TotalRows = totalRows,
                Created = createdCount,
                Updated = updatedCount,
                Errors = errorList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excel import failed");
            return StatusCode(500, new { message = "Failed to process Excel file. Please ensure it follows the export format." });
        }
    }

    private async Task LogAudit(PortModelMapping record, string action, string user)
    {
        try
        {
            var sql = @"
                INSERT INTO crd.port_model_mapping_audit 
                (accno_sleeve, effectivedate, model_name, currency_model, hedge_model_name, action, changed_by, changed_at)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7})";

            await _context.Database.ExecuteSqlRawAsync(sql,
                record.AccnoSleeve,
                record.EffectiveDate,
                record.ModelName,
                record.CurrencyModel ?? (object)DBNull.Value,
                record.HedgeModelName ?? (object)DBNull.Value,
                action,
                user,
                GetConfiguredNow());

            _logger.LogInformation("Audit log created for action {Action}", action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit");
        }
    }

    private async Task<List<string>> TruncateStringPropertiesAsync(PortModelMapping record, CancellationToken ct = default)
    {
        var warnings = new List<string>();
        var propertiesToCheck = new[] { "AccnoSleeve", "ModelName", "CurrencyModel", "HedgeModelName", "CreatedBy", "UpdatedBy" };
        const int fallbackDefault = 255;

        foreach (var propName in propertiesToCheck)
        {
            var pi = typeof(PortModelMapping).GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (pi == null || pi.PropertyType != typeof(string)) continue;

            var current = (string?)pi.GetValue(record);
            if (string.IsNullOrEmpty(current)) continue;

            var maxLen = await _columnLengthProvider.GetMaxLengthAsync(typeof(PortModelMapping), propName, ct);
            int effectiveMax = maxLen ?? fallbackDefault;

            if (current.Length > effectiveMax)
            {
                var truncated = current.Substring(0, effectiveMax);
                pi.SetValue(record, truncated);

                if (maxLen.HasValue)
                    warnings.Add($"{propName} truncated to {maxLen.Value} characters.");
                else
                    warnings.Add($"{propName} exceeded {effectiveMax} characters and was truncated (no EF/DB max-length metadata; default used).");
            }
        }

        return warnings;
    }

    private string GetCurrentUser()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Requested-By", out var headerUser) == true && !string.IsNullOrWhiteSpace(headerUser))
        {
            return headerUser.ToString();
        }
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
    }
}