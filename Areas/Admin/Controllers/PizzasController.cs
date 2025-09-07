using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PizzasController : Controller
    {
        private readonly FoodHubContext _context;
        private readonly IWebHostEnvironment _env;

        public PizzasController(FoodHubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Pizzas
        // public async Task<IActionResult> Index()
        // {
        //     var pizzas = await _context.Pizzas.ToListAsync();
        //     return PartialView("~/Areas/Admin/Views/Dashboard/_ViewPizzas.cshtml", pizzas);
        // }
        // GET: Admin/Pizzas
public async Task<IActionResult> Index()
{
    var pizzas = await _context.Pizzas.ToListAsync();
    var crusts = await _context.PizzaCrustCategory.ToListAsync();

    // Create a dictionary to map pizza.Id => CategoryName
    var pizzaCrustMap = new Dictionary<int, string>();

    foreach (var pizza in pizzas)
    {
        string crustName = "N/A";

        if (!string.IsNullOrWhiteSpace(pizza.CrustCategory))
        {
            // Try to match by ID
            var crust = crusts.FirstOrDefault(c =>
                string.Equals(c.Id.ToString(), pizza.CrustCategory.Trim(), StringComparison.OrdinalIgnoreCase));

            // If not found by ID, try by name (in case old data stored name)
            if (crust == null)
            {
                crust = crusts.FirstOrDefault(c =>
                    string.Equals(c.CategoryName, pizza.CrustCategory.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            crustName = crust?.CategoryName ?? "N/A";
        }

        pizzaCrustMap[pizza.Id] = crustName;
    }

    // Pass dictionary to view
    ViewBag.PizzaCrustMap = pizzaCrustMap;

    return PartialView("~/Areas/Admin/Views/Dashboard/_ViewPizzas.cshtml", pizzas);
}


        // GET: Admin/Pizzas/PartialList
        public async Task<IActionResult> PartialList()
        {
            var pizzas = await _context.Pizzas.ToListAsync();
            // Get all crust categories
            var crusts = await _context.PizzaCrustCategory.ToListAsync();
            // Map crust names to pizzas
            var pizzaWithCrust = pizzas.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.ImageName,
                CrustName = crusts.FirstOrDefault(c => c.Id == int.Parse(p.CrustCategory))?.CategoryName ?? "N/A",
                p.Price,
                p.CreatedAt
            }).ToList();
            return PartialView("_ViewPizzas", pizzas); // partial for dashboard
        }

        // GET: Admin/Pizzas/Create
        // public IActionResult Create() => View();

        public async Task<IActionResult> Create()
        {
            var crusts = await _context.PizzaCrustCategory.ToListAsync();
            ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName");

            return View("~/Areas/Admin/Views/Dashboard/_AddPizzas.cshtml", new Pizza());
        }



        // POST: Admin/Pizzas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pizza pizza, IFormFile? ImageFile)
        {
            //Console.WriteLine("ONE 1111");
            if (!ModelState.IsValid)
            {
                    // Console.WriteLine("TWO FAIL!!");
                foreach (var error in ModelState)
                {
                    foreach (var subError in error.Value.Errors)
                    {
                        Console.WriteLine($"Error in {error.Key}: {subError.ErrorMessage}");
                    }
                }
                var crustsFail = await _context.PizzaCrustCategory.ToListAsync();
                ViewBag.CrustCategories = new SelectList(crustsFail, "Id", "CategoryName");
                return View("~/Areas/Admin/Views/Dashboard/_AddPizzas.cshtml", pizza);
            }

            // Ensure a valid crust is selected
            if (string.IsNullOrEmpty(pizza.CrustCategory) || !int.TryParse(pizza.CrustCategory, out _))
            {
                ModelState.AddModelError("CrustCategory", "Please select a valid crust category.");
                var crustsFail = await _context.PizzaCrustCategory.ToListAsync();
                ViewBag.CrustCategories = new SelectList(crustsFail, "Id", "CategoryName");
                return View("~/Areas/Admin/Views/Dashboard/_AddPizzas.cshtml", pizza);
            }

            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(ImageFile.FileName);
                var uploads = Path.Combine(_env.WebRootPath, "uploads/pizzas");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(stream);

                pizza.ImageName = fileName;
            }

            pizza.CreatedAt = DateTime.Now;

            // CrustCategory is already string, no conversion needed
            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();

            return Redirect("/Admin/Dashboard?page=ViewPizza");
        }



        // GET: Admin/Pizzas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza == null) return NotFound();

            return View(pizza);
        }

        // POST: Admin/Pizzas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pizza pizza, IFormFile? ImageFile)
        {
            if (id != pizza.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var uploads = Path.Combine(_env.WebRootPath, "uploads/pizzas");
                        if (!Directory.Exists(uploads))
                            Directory.CreateDirectory(uploads);

                        var filePath = Path.Combine(uploads, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await ImageFile.CopyToAsync(stream);

                        pizza.ImageName = fileName;
                    }

                    _context.Pizzas.Update(pizza);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Pizzas.Any(e => e.Id == pizza.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(pizza);
        }

        // GET: Admin/Pizzas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza == null) return NotFound();

            _context.Pizzas.Remove(pizza);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Pizzas/AddCrustCategory
        public IActionResult AddCrustCategory()
        {
            return View("_AddPizzaCategory", new PizzaCrustCategory());
        }

        // POST: Admin/Pizzas/AddCrustCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCrustCategory(PizzaCrustCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.Now;
                _context.PizzaCrustCategory.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View("_AddPizzaCategory", category);
        }

        // GET: Admin/Pizzas/ViewCategories
        public async Task<IActionResult> ViewCategories()
        {
            var categories = await _context.PizzaCrustCategory.ToListAsync();
            ViewData["Page"] = "ViewPizzaCategory";
            ViewData["PizzaCategories"] = categories;
            return View("~/Areas/Admin/Views/Dashboard/Index.cshtml");
        }

    }
}
