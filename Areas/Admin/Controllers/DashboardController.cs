//     using Microsoft.AspNetCore.Mvc;
//     using FoodHub.Data;
//     using FoodHub.Models;
//     using Microsoft.EntityFrameworkCore;
//     using Microsoft.AspNetCore.Mvc.Rendering;

//     namespace FoodHub.Areas.Admin.Controllers
//     {
//         [Area("Admin")]
//         public class DashboardController : Controller
//         {
//             private readonly FoodHubContext _context;

//             public DashboardController(FoodHubContext context)
//             {
//                 _context = context;
//             }

//             public async Task<IActionResult> Index(string page, string? id )
//             {
//                 ViewData["Page"] = page;

//             if (page == "ViewSpecials")
//             {
//                 var specials = await _context.Specials.ToListAsync();
//                 ViewData["Specials"] = specials;
//             }
//             else if (page == "ViewPizza")
//             {
//                 // var pizzas = _context.Pizzas.ToList();
//                 // ViewData["Pizzas"] = pizzas;
//                 var pizzas = await _context.Pizzas
//                  .Include(p => p.PizzaPrices)
//                     .ThenInclude(pp => pp.Crust)
//                     .ToListAsync();
//                // var crusts = await _context.PizzaCrustCategory.ToListAsync();

//                 // Map Pizza.Id to Crust Category Name
//                 // var pizzaCrustMap = new Dictionary<int, string>();
//                 // foreach (var pizza in pizzas)
//                 // {
//                 //     string crustName = "N/A";
//                 //     if (!string.IsNullOrWhiteSpace(pizza.CrustCategory))
//                 //     {
//                 //         var crust = crusts.FirstOrDefault(c => c.Id.ToString() == pizza.CrustCategory.Trim());
//                 //         if (crust == null)
//                 //             crust = crusts.FirstOrDefault(c => c.CategoryName == pizza.CrustCategory.Trim());
//                 //         crustName = crust?.CategoryName ?? "N/A";
//                 //     }
//                 //     pizzaCrustMap[pizza.Id] = crustName;
//                 // }

//                 ViewData["Pizzas"] = pizzas;
//                 //ViewBag.PizzaCrustMap = pizzaCrustMap;
//             }
//             else if (page == "ViewBeverages")
//             {
//                 var beverages = _context.Beverages.ToList();
//                 ViewData["Beverages"] = beverages;
//             }
//             else if (page == "ViewPizzaCategory")
//             {
//                 var categories = _context.PizzaCrustCategory.ToList();
//                 ViewData["PizzaCategories"] = categories;
//             }
//             else if (page == "AddPizzas")
//             {
//                 var crusts = await _context.PizzaCrustCategory.AsNoTracking().ToListAsync();
//                 ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName");
//             }
//             else if (page == "EditSpecials" && id.HasValue)
//             {
//                 var special = await _context.Specials.FindAsync(id.Value);
//                 if (special == null) return NotFound();
//                 ViewData.Model = special;
//                 return View();            
//             }
//          else if (page == "EditPizzas" && id.HasValue)
// {
//                 // var pizza = await _context.Pizzas.FindAsync(id.Value);
//     var pizza = await _context.Pizzas
//         .Include(p => p.PizzaPrices)
//         .ThenInclude(pp => pp.Crust)
//         .FirstOrDefaultAsync(p => p.Id == id);

//     if (pizza == null) return NotFound();

//    // var crusts = await _context.PizzaCrustCategory.ToListAsync();
//   //  ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName", pizza.CrustCategory);

//     // Set model for dashboard page
//     ViewData.Model = pizza;

//     // Tell dashboard which page to render inside it
//     ViewData["Page"] = "EditPizzas";

//     // Return the main dashboard view
//     return View("~/Areas/Admin/Views/Dashboard/Index.cshtml");
// }


//             else if (page == "EditBeverages" && id.HasValue)
//             {
//                 var beverage = await _context.Beverages.FindAsync(id.Value);
//                 if (beverage == null) return NotFound();
//                 ViewData.Model = beverage;
//                 return View();
//             }
//             else if (page == "EditPizzaCategory" && id.HasValue)
//             {
//                 var pizzaCategory = await _context.PizzaCrustCategory.FindAsync(id.Value);
//                 if (pizzaCategory == null) return NotFound();
//                 ViewData.Model = pizzaCategory;
//                 return View();
//             }   
//                 return View();
//             }
//         }
//     }

using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;


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

        public async Task<IActionResult> Index(string page, string? id)
        {
            ViewData["Page"] = page;

            if (page == "ViewSpecials")
            {
                // var specials = await _context.Specials.ToListAsync();
                // // ViewData["Pizzas"] = await _context.Pizzas.ToListAsync();
                // // ViewData["Beverages"] = await _context.Beverages.ToListAsync();
                // Console.WriteLine("DATA COMES FROM HERE");

                // ViewData["Specials"] = specials;
                // Include related SpecialItems
    var specials = await _context.Specials
        .Include(s => s.SpecialItems)
        .ToListAsync();

    // Get item names
    var pizzas = await _context.Pizzas.ToDictionaryAsync(p => p.Id, p => p.Name);
    var beverages = await _context.Beverages.ToDictionaryAsync(b => b.Id, b => b.Name);

    // Map item names for display
    foreach (var special in specials)
    {
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
    //ViewData["SpecialsJson"] = System.Text.Json.JsonSerializer.Serialize(specials);
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
