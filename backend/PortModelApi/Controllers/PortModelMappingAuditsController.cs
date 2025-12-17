using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortModelApi.Data;
using PortModelApi.Models;

namespace PortModelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortModelMappingAuditsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PortModelMappingAuditsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{accno}/{date}")]
    public async Task<ActionResult<IEnumerable<PortModelMappingAudit>>> GetAuditHistory(string accno, DateOnly date)
    {
        return await _context.PortModelMappingAudits
            .Where(a => a.AccnoSleeve == accno && a.EffectiveDate == date)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }
}
