using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;

namespace FoodHub.Controllers
{
    public class FoodController : Controller
    {
        private readonly FoodHubContext _context;

        public FoodController(FoodHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var foods = await _context.FoodItems.ToListAsync();
            return View(foods);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(foodItem);
        }
    }
}
