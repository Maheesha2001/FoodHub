using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;

public class BeveragesController : Controller
{
    private readonly FoodHubContext _context;

    public BeveragesController(FoodHubContext context)
    {
        _context = context;
    }

    // GET: /Beverages
    public async Task<IActionResult> Index()
    {
        var beverages = await _context.Beverages.ToListAsync();
        return View(beverages);  // looks for Views/Beverages/Index.cshtml
    }
}
