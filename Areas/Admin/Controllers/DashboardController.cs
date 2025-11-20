using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;


namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly FoodHubContext _context;

        public DashboardController(FoodHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string page, string? id)
        {
            ViewData["Page"] = page;

            if (page == "ViewSpecials")
{
            var specials = await _context.Specials
                .Include(s => s.SpecialItems)
                .ToListAsync();

            var now = DateTime.Now;

            // Get item names
            var pizzas = await _context.Pizzas.ToDictionaryAsync(p => p.Id, p => p.Name);
            var beverages = await _context.Beverages.ToDictionaryAsync(b => b.Id, b => b.Name);

            // Map item names and calculate IsActive
            foreach (var special in specials)
            {
                Console.WriteLine("Sart Date "+ special.StartDate);
                Console.WriteLine("End Date "+ special.EndDate);
                // Calculate IsActive based on current time
                //special.IsActive = special.StartDate <= now && special.EndDate >= now;
                special.IsActive =
                    special.StartDate.HasValue &&
                    special.EndDate.HasValue &&
                    special.StartDate.Value.Date <= now.Date &&
                    special.EndDate.Value.Date >= now.Date;

                Console.WriteLine("Active Status "+ special.IsActive);
                foreach (var item in special.SpecialItems)
                {
                    if (item.ItemType == "Pizza" && pizzas.ContainsKey(item.ItemId))
                        item.ItemName = pizzas[item.ItemId];
                    else if (item.ItemType == "Beverage" && beverages.ContainsKey(item.ItemId))
                        item.ItemName = beverages[item.ItemId];
                    else
                        item.ItemName = "Unknown";
                }
            }

            // Pass to ViewData for normal display
            ViewData["Specials"] = specials;

            // Pass JSON for JavaScript use
            ViewData["SpecialsJson"] = JsonSerializer.Serialize(specials, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = false
            });
        }

            else if (page == "ViewPizza")
            {
                var pizzas = await _context.Pizzas
                    .Include(p => p.PizzaPrices)
                    .ThenInclude(pp => pp.Crust)
                    .ToListAsync();

                ViewData["Pizzas"] = pizzas;
            }
            else if (page == "ViewBeverages")
            {
                var beverages = await _context.Beverages.ToListAsync();
                ViewData["Beverages"] = beverages;
            }
            else if (page == "ViewPizzaCategory")
            {
                var categories = await _context.PizzaCrustCategory.ToListAsync();
                ViewData["PizzaCategories"] = categories;
            }
            else if (page == "AddSpecials")
            {
               ViewBag.Pizzas = await _context.Pizzas.ToListAsync();
                ViewBag.Beverages = await _context.Beverages.ToListAsync();
              //   return PartialView("_AddSpecials", new Special());
            }
            else if (page == "AddPizzas")
            {
                var crusts = await _context.PizzaCrustCategory.AsNoTracking().ToListAsync();
                ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName");
            }
            else if (page == "EditSpecials" && !string.IsNullOrEmpty(id))
            {
                var special = await _context.Specials
                    .Include(s => s.SpecialItems)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (special == null) return NotFound();

                ViewBag.Pizzas = await _context.Pizzas.ToListAsync();
                ViewBag.Beverages = await _context.Beverages.ToListAsync();

                ViewData.Model = special;
                return View();
            }
            else if (page == "EditPizzas" && !string.IsNullOrEmpty(id))
            {
                var pizza = await _context.Pizzas
                    .Include(p => p.PizzaPrices)
                    .ThenInclude(pp => pp.Crust)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pizza == null) return NotFound();

                ViewData.Model = pizza;
                ViewData["Page"] = "EditPizzas";

                return View("~/Areas/Admin/Views/Dashboard/Index.cshtml");
            }
            else if (page == "EditBeverages" && !string.IsNullOrEmpty(id))
            {
                var beverage = await _context.Beverages.FindAsync(id);
                if (beverage == null) return NotFound();

                ViewData.Model = beverage;
                return View();
            }
            else if (page == "EditPizzaCategory" && !string.IsNullOrEmpty(id))
            {
                var pizzaCategory = await _context.PizzaCrustCategory.FindAsync(id);
                if (pizzaCategory == null) return NotFound();

                ViewData.Model = pizzaCategory;
                return View();
            }

            return View();
        }
    }
}
