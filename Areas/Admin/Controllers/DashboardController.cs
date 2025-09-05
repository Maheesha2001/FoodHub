using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly FoodHubContext _context;

        public DashboardController(FoodHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string page)
        {
            ViewData["Page"] = page;

            if (page == "ViewSpecials")
            {
                var specials = await _context.Specials.ToListAsync();
                ViewData["Specials"] = specials;
            }
            else if (page == "ViewPizza")
            {
                var pizzas = _context.Pizzas.ToList();
                ViewData["Pizzas"] = pizzas;
            }
            else if (page == "ViewBeverages")
            {
                var beverages = _context.Beverages.ToList();
                ViewData["Beverages"] = beverages;
            }

            return View();
        }
    }
}

// namespace FoodHub.Areas.Admin.Controllers
// {
//     [Area("Admin")]
//     public class DashboardController : Controller
//     {
//         // GET: Admin/Dashboard
//         public IActionResult Index(string page)
//         {
//             // page = "ViewSpecials", "AddSpecial", etc.
//             ViewData["Page"] = page;
//             return View();
//         }
//     }
// }