using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;
using PortModelApi.Controllers;
using PortModelApi.Data;
using PortModelApi.Models;
using Xunit;

namespace PortModelApi.Tests
{
    public class PortModelMappingsControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ILogger<PortModelMappingsController>> _mockLogger;

        public PortModelMappingsControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<PortModelMappingsController>>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
            }, "mock"));

            var context = new DefaultHttpContext { User = user };
            _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
        }

        private PortModelMappingsController CreateController(AppDbContext context)
        {
            var inMemoryConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string?>> { new KeyValuePair<string, string?>("AppSettings:TimeZone", "Asia/Bangkok") })
                .Build();

            var columnLengthProvider = new PortModelApi.Services.ColumnLengthProvider(context);

            return new PortModelMappingsController(context, _mockHttpContextAccessor.Object, _mockLogger.Object, columnLengthProvider, inMemoryConfig);
        }

        [Fact]
        public async Task GetRecords_ReturnsAllRecords()
        {
            using var context = new AppDbContext(_dbContextOptions);
            context.PortModelMappings.AddRange(
                new PortModelMapping { AccnoSleeve = "A1", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "M1" },
                new PortModelMapping { AccnoSleeve = "A2", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "M2" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.GetRecords();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<PortModelMapping>>>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<PortModelMapping>>(actionResult.Value);
            
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetRecord_ReturnsRecord_WhenFound()
        {
            using var context = new AppDbContext(_dbContextOptions);
            context.PortModelMappings.Add(
                new PortModelMapping { AccnoSleeve = "A1", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "M1" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.GetRecord("A1", new DateOnly(2024, 1, 1));

            var actionResult = Assert.IsType<ActionResult<PortModelMapping>>(result);
            var model = Assert.IsType<PortModelMapping>(actionResult.Value);
            
            Assert.Equal("M1", model.ModelName);
        }

        [Fact]
        public async Task GetRecord_ReturnsNotFound_WhenDoesNotExist()
        {
            using var context = new AppDbContext(_dbContextOptions);
            var controller = CreateController(context);
            var result = await controller.GetRecord("Invalid", new DateOnly(2024, 1, 1));

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateRecord_AddsRecord_AndAuditLogs()
        {
            using var context = new AppDbContext(_dbContextOptions);
            var controller = CreateController(context);
            
            var newRecord = new PortModelMapping 
            { 
                AccnoSleeve = "A3", 
                EffectiveDate = new DateOnly(2024, 1, 2), 
                ModelName = "M3" 
            };

            // Seed referenced portfolio because controller validates existence
            context.Portfolios.Add(new PortModelApi.Models.Portfolio { Code = "A3", Name = "Test Portfolio" });
            await context.SaveChangesAsync();

            var result = await controller.CreateRecord(newRecord);

            // Verify Record Added
            var createdRecord = await context.PortModelMappings.FindAsync("A3", new DateOnly(2024, 1, 2));
            Assert.NotNull(createdRecord);
            Assert.Equal("TestUser", createdRecord?.CreatedBy ?? string.Empty);
            
            // Verify Result
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetRecord", createdAtActionResult.ActionName);
            
            // Verify Logger called for Create Action
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating record")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

             _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to log audit")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>() ),
                Times.Once);
        }

        [Fact]
        public async Task UpdateRecord_UpdatesRecord_WhenFound()
        {
            using var context = new AppDbContext(_dbContextOptions);
            context.PortModelMappings.Add(
               new PortModelMapping { AccnoSleeve = "A1", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "Old" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var update = new PortModelMapping { ModelName = "New" };
            
            var result = await controller.UpdateRecord("A1", new DateOnly(2024, 1, 1), update);

            Assert.IsType<NoContentResult>(result);
            
            var updatedRecord = await context.PortModelMappings.FindAsync("A1", new DateOnly(2024, 1, 1));
            Assert.Equal("New", updatedRecord?.ModelName ?? string.Empty);
            Assert.Equal("TestUser", updatedRecord?.UpdatedBy ?? string.Empty);
        }

        [Fact]
        public async Task DeleteRecord_SoftDeletesRecord_WhenFound()
        {
            using var context = new AppDbContext(_dbContextOptions);
            context.PortModelMappings.Add(
               new PortModelMapping { AccnoSleeve = "A1", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "M1", IsDeleted = false }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            var result = await controller.DeleteRecord("A1", new DateOnly(2024, 1, 1));

            Assert.IsType<NoContentResult>(result);
            
            // Verify Soft Delete
            var deletedRecord = await context.PortModelMappings.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.AccnoSleeve == "A1" && r.EffectiveDate == new DateOnly(2024, 1, 1));
                
            Assert.NotNull(deletedRecord);
            Assert.True(deletedRecord.IsDeleted);
            Assert.Equal("TestUser", deletedRecord.UpdatedBy);
        }

        [Fact]
        public async Task CreateRecord_AfterSoftDelete_RestoresRecord()
        {
            using var context = new AppDbContext(_dbContextOptions);
            // Add initial record
            context.PortModelMappings.Add(
               new PortModelMapping { AccnoSleeve = "A5", EffectiveDate = new DateOnly(2024, 1, 1), ModelName = "Original" }
            );
            // Add required portfolio
            context.Portfolios.Add(new PortModelApi.Models.Portfolio { Code = "A5", Name = "Test Portfolio" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);
            
            // Delete (soft delete)
            var deleteResult = await controller.DeleteRecord("A5", new DateOnly(2024, 1, 1));
            Assert.IsType<NoContentResult>(deleteResult);
            
            // Verify record is not visible (soft deleted)
            var notFoundResult = await controller.GetRecord("A5", new DateOnly(2024, 1, 1));
            Assert.IsType<NotFoundResult>(notFoundResult.Result);
            
            // Create with same key should restore the soft-deleted record
            var newRecord = new PortModelMapping
            {
                AccnoSleeve = "A5",
                EffectiveDate = new DateOnly(2024, 1, 1),
                ModelName = "Restored"
            };
            
            var createResult = await controller.CreateRecord(newRecord);
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(createResult);
            Assert.Equal("GetRecord", createdAtResult.ActionName);
            
            // Verify record is restored and visible
            var restoredRecord = await context.PortModelMappings.FirstOrDefaultAsync(r => r.AccnoSleeve == "A5" && r.EffectiveDate == new DateOnly(2024, 1, 1));
            Assert.NotNull(restoredRecord);
            Assert.Equal("Restored", restoredRecord?.ModelName ?? string.Empty);
            Assert.False(restoredRecord?.IsDeleted ?? true);
        }
    }
}

