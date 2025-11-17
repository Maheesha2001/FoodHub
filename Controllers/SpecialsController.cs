using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;

[Route("api/[controller]")]
[ApiController]
public class SpecialsController : ControllerBase
{
    private readonly FoodHubContext _context;

    public SpecialsController(FoodHubContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSpecials()
    {
        var now = DateTime.Now;

        var specials = await _context.Specials
            .Where(s => s.IsActive) // ONLY ACTIVE
            .ToListAsync();

        return Ok(specials);
    }

    // [HttpGet]
    // public async Task<IActionResult> GetSpecials()
    // {
    //     var specials = await _context.Specials.ToListAsync();
    //     return Ok(specials);
    // }
}
