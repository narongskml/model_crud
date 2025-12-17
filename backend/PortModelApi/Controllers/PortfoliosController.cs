using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortModelApi.Data;
using PortModelApi.Models;

namespace PortModelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PortfoliosController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetPortfolios()
    {
        // Select only Code and Name as requested
        return await _context.Portfolios
            .Select(p => new { p.Code, p.Name })
            .ToListAsync();
    }
}
