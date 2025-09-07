using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                // var pizzas = _context.Pizzas.ToList();
                // ViewData["Pizzas"] = pizzas;
                    var pizzas = await _context.Pizzas.ToListAsync();
    var crusts = await _context.PizzaCrustCategory.ToListAsync();

    // Map Pizza.Id to Crust Category Name
    var pizzaCrustMap = new Dictionary<int, string>();
    foreach (var pizza in pizzas)
    {
        string crustName = "N/A";
        if (!string.IsNullOrWhiteSpace(pizza.CrustCategory))
        {
            var crust = crusts.FirstOrDefault(c => c.Id.ToString() == pizza.CrustCategory.Trim());
            if (crust == null)
                crust = crusts.FirstOrDefault(c => c.CategoryName == pizza.CrustCategory.Trim());
            crustName = crust?.CategoryName ?? "N/A";
        }
        pizzaCrustMap[pizza.Id] = crustName;
    }

    ViewData["Pizzas"] = pizzas;
    ViewBag.PizzaCrustMap = pizzaCrustMap;
            }
            else if (page == "ViewBeverages")
            {
                var beverages = _context.Beverages.ToList();
                ViewData["Beverages"] = beverages;
            }
            else if (page == "ViewPizzaCategory")
            {
                var categories = _context.PizzaCrustCategory.ToList();
                ViewData["PizzaCategories"] = categories;
            }
            else if (page == "AddPizzas")
            {
                var crusts = await _context.PizzaCrustCategory.AsNoTracking().ToListAsync();
                ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName");
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