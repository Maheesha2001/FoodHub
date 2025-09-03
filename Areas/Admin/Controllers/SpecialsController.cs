using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SpecialsController : Controller
    {
        private readonly FoodHubContext _context;

        public SpecialsController(FoodHubContext context)
        {
            _context = context;
        }

        // GET: Admin/Specials
        public async Task<IActionResult> Index()
        {
            var specials = await _context.Specials.ToListAsync();
            return View(specials);
        }

        // GET: Admin/Specials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var special = await _context.Specials
                .FirstOrDefaultAsync(s => s.Id == id);

            if (special == null) return NotFound();

            return View(special);
        }
    }
}
