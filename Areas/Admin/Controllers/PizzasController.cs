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
            var pizzas = await _context.Pizzas
             .Include(p => p.PizzaPrices)
        .ThenInclude(pp => pp.Crust)
        .ToListAsync();
         //   var crusts = await _context.PizzaCrustCategory.ToListAsync();

            // Create a dictionary to map pizza.Id => CategoryName
            // var pizzaCrustMap = new Dictionary<int, string>();

            // foreach (var pizza in pizzas)
            // {
            //     string crustName = "N/A";

            //     if (!string.IsNullOrWhiteSpace(pizza.CrustCategory))
            //     {
            //         // Try to match by ID
            //         var crust = crusts.FirstOrDefault(c =>
            //             string.Equals(c.Id.ToString(), pizza.CrustCategory.Trim(), StringComparison.OrdinalIgnoreCase));

            //         // If not found by ID, try by name (in case old data stored name)
            //         if (crust == null)
            //         {
            //             crust = crusts.FirstOrDefault(c =>
            //                 string.Equals(c.CategoryName, pizza.CrustCategory.Trim(), StringComparison.OrdinalIgnoreCase));
            //         }

            //         crustName = crust?.CategoryName ?? "N/A";
            //     }

               // pizzaCrustMap[pizza.Id] = crustName;
           // }

            // Pass dictionary to view
          //  ViewBag.PizzaCrustMap = pizzaCrustMap;

            return PartialView("~/Areas/Admin/Views/Dashboard/_ViewPizzas.cshtml", pizzas);
        }


        // GET: Admin/Pizzas/PartialList
        public async Task<IActionResult> PartialList()
        {
            var pizzas = await _context.Pizzas.ToListAsync();
            // Get all crust categories
          //  var crusts = await _context.PizzaCrustCategory.ToListAsync();
            // Map crust names to pizzas
            // var pizzaWithCrust = pizzas.Select(p => new
            // {
            //     p.Id,
            //     p.Name,
            //     p.Description,
            //     p.ImageName,
            //     CrustName = crusts.FirstOrDefault(c => c.Id == int.Parse(p.CrustCategory))?.CategoryName ?? "N/A",
            //     p.BasePrice,
            //     p.CreatedAt
            // }).ToList();
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
            // if (string.IsNullOrEmpty(pizza.CrustCategory) || !int.TryParse(pizza.CrustCategory, out _))
            // {
            //     ModelState.AddModelError("CrustCategory", "Please select a valid crust category.");
            //     var crustsFail = await _context.PizzaCrustCategory.ToListAsync();
            //     ViewBag.CrustCategories = new SelectList(crustsFail, "Id", "CategoryName");
            //     return View("~/Areas/Admin/Views/Dashboard/_AddPizzas.cshtml", pizza);
            // }

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

               // 🔑 Assign existing crusts to this pizza
    var crusts = await _context.PizzaCrustCategory.ToListAsync();
    foreach (var crust in crusts)
    {
        decimal price = pizza.BasePrice;

        if (crust.ExtraCharge.HasValue)
            price += crust.ExtraCharge.Value;

        if (crust.PercentageIncrease.HasValue)
            price += pizza.BasePrice * crust.PercentageIncrease.Value;

        var pizzaPrice = new PizzaPrice
        {
            PizzaId = pizza.Id,
            CrustId = crust.Id,
            Price = price,
            CreatedAt = DateTime.Now
        };

        _context.PizzaPrices.Add(pizzaPrice);
    }

    await _context.SaveChangesAsync();

            return Redirect("/Admin/Dashboard?page=ViewPizza");
        }


        // GET: Admin/Pizzas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza == null) return NotFound();
            
            // Load crust categories
         //   var crusts = await _context.PizzaCrustCategory.ToListAsync();
          //  ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName", pizza.CrustCategory);

            // ✅ Load the partial instead of default Edit.cshtml
            return PartialView("~/Areas/Admin/Views/Dashboard/_EditPizzas.cshtml", pizza);
        }


    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Pizza pizza, IFormFile? ImageFile)
{
   Console.WriteLine($"[DEBUG] Edit POST called for Pizza ID: {id}");

    if (id != pizza.Id)
    {
        Console.WriteLine("[DEBUG] Pizza ID mismatch.");
        return NotFound();
    }

    if (ModelState.IsValid)
    {
                 Console.WriteLine("[DEBUG] ModelState is valid.");

                // var existingPizza = await _context.Pizzas.FirstOrDefaultAsync(p => p.Id == id);
       var existingPizza = await _context.Pizzas
        .Include(p => p.PizzaPrices)
        .ThenInclude(pp => pp.Crust)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPizza == null)
                {
                   Console.WriteLine("[DEBUG] Pizza not found in DB.");
                    return NotFound();
                }

       Console.WriteLine($"[DEBUG] Found existing pizza: {existingPizza.Name}");

        // Handle image upload
        if (ImageFile != null && ImageFile.Length > 0)
        {
          //  Console.WriteLine($"[DEBUG] New image uploaded: {ImageFile.FileName}");

            var fileName = Path.GetFileName(ImageFile.FileName);
            var uploads = Path.Combine(_env.WebRootPath, "uploads/pizzas");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var filePath = Path.Combine(uploads, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await ImageFile.CopyToAsync(stream);

            existingPizza.ImageName = fileName;
        }
        else
        {
            Console.WriteLine("[DEBUG] No new image uploaded, keeping existing image.");
        }

        // Update other properties
        existingPizza.Name = pizza.Name;
        existingPizza.Description = pizza.Description;
        existingPizza.BasePrice = pizza.BasePrice;

        // Only update CrustCategory if a new one was selected
        // if (!string.IsNullOrWhiteSpace(pizza.CrustCategory))
        // {
        //     existingPizza.CrustCategory = pizza.CrustCategory;
        // }
        // else
        // {
        //     Console.WriteLine("[DEBUG] CrustCategory not changed, keeping existing value.");
        // }

      //  Console.WriteLine($"[DEBUG] Updating pizza: {existingPizza.Name}, Price: {existingPizza.BasePrice}, Crust: {existingPizza.CrustCategory}");
// Update pizza prices for each crust
    if (pizza.PizzaPrices != null)
    {
        foreach (var updatedPrice in pizza.PizzaPrices)
        {
            var existingPrice = existingPizza.PizzaPrices.FirstOrDefault(pp => pp.Id == updatedPrice.Id);
            if (existingPrice != null)
            {
                existingPrice.Price = updatedPrice.Price;
            }
        }
    }
        await _context.SaveChangesAsync();
        Console.WriteLine("[DEBUG] Pizza updated successfully.");

        return Redirect("/Admin/Dashboard?page=ViewPizza");
    }

    // If ModelState is invalid
    Console.WriteLine("[DEBUG] ModelState is invalid.");
    foreach (var kvp in ModelState)
    {
        foreach (var error in kvp.Value.Errors)
        {
            Console.WriteLine($"[ModelState Error] Field: {kvp.Key}, Error: {error.ErrorMessage}");
        }
    }

   // var crusts = await _context.PizzaCrustCategory.ToListAsync();
   // ViewBag.CrustCategories = new SelectList(crusts, "Id", "CategoryName", pizza.CrustCategory);

   return Redirect("/Admin/Dashboard?page=ViewPizza");
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

//=================CRUST============================================================
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
                
                 // After saving the new crust, update all pizzas with this crust
            var pizzas = _context.Pizzas.ToList();
            foreach (var pizza in pizzas)
            {
                var price = pizza.BasePrice;

                if (category.ExtraCharge.HasValue)
                    price += category.ExtraCharge.Value;

                if (category.PercentageIncrease.HasValue)
                    price += pizza.BasePrice * category.PercentageIncrease.Value;

                        _context.PizzaPrices.Add(new PizzaPrice
                        {
                            PizzaId = pizza.Id,
                            CrustId = category.Id,
                            Price = price,
                            CreatedAt = DateTime.Now
                        });
                    }

            await _context.SaveChangesAsync();
                    // return RedirectToAction(nameof(Index));
                    return Redirect("/Admin/Dashboard?page=ViewPizzaCategory");
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

        
        // GET: Admin/Pizzas/EditCrustCategory/5
        public async Task<IActionResult> EditCrustCategory(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.PizzaCrustCategory.FindAsync(id);
            if (category == null) return NotFound();
        return PartialView("~/Areas/Admin/Views/Dashboard/_EditPizzaCategory.cshtml", category);

        }

        // POST: Admin/Pizzas/EditCrustCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCrustCategory(int id, PizzaCrustCategory category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();

                     // Update all related PizzaPrices for this crust
                    var pizzaPrices = await _context.PizzaPrices
                        .Include(pp => pp.Pizza)
                        .Where(pp => pp.CrustId == category.Id)
                        .ToListAsync();

                    foreach (var pp in pizzaPrices)
                    {
                        var basePrice = pp.Pizza.BasePrice;

                        decimal newPrice = basePrice;

                        if (category.ExtraCharge.HasValue)
                            newPrice += category.ExtraCharge.Value;

                        if (category.PercentageIncrease.HasValue)
                            newPrice += basePrice * category.PercentageIncrease.Value;

                        pp.Price = newPrice;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PizzaCrustCategory.Any(c => c.Id == id)) return NotFound();
                    else throw;
                }

            // ✅ Redirect to Dashboard page showing all pizza categories
            return Redirect("/Admin/Dashboard?page=ViewPizzaCategory");
            }
        return Redirect("/Admin/Dashboard?page=ViewPizzaCategory");
        }

    }
}
