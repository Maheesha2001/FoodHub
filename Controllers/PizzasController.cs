using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

public class PizzasController : Controller
{
    private readonly FoodHubContext _context;

    public PizzasController(FoodHubContext context)
    {
        _context = context;
    }

    // GET: /Pizzas
    public async Task<IActionResult> Index()
    {
        var pizzas = await _context.Pizzas .Include(p => p.PizzaPrices)
            .ThenInclude(pp => pp.Crust)
        .ToListAsync();
        return View(pizzas);  // Looks for Views/Pizzas/Index.cshtml
    }

    // Optional: GET: /Pizzas/Details/5
    public async Task<IActionResult> Details(string id)
    {
        var pizza = await _context.Pizzas.FirstOrDefaultAsync(p => p.Id == id);
        if (pizza == null) return NotFound();
        return View(pizza);  // Views/Pizzas/Details.cshtml
    }
}
