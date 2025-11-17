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
        private readonly IWebHostEnvironment _environment;

        public SpecialsController(FoodHubContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
       
        // GET: Admin/Specials
        public async Task<IActionResult> Index()
        {
            var specials = await _context.Specials
                .Include(s => s.SpecialItems)
                .ToListAsync();

            var pizzas = await _context.Pizzas.ToDictionaryAsync(p => p.Id, p => p.Name);
            var beverages = await _context.Beverages.ToDictionaryAsync(b => b.Id, b => b.Name);

            // Attach names dynamically to SpecialItems
            foreach (var special in specials)
            {
                Console.WriteLine($"\n🔹 SPECIAL: {special.Id} - {special.Title}");

                foreach (var item in special.SpecialItems)
                {
                    if (item.ItemType == "Pizza" && pizzas.ContainsKey(item.ItemId))
                        item.ItemName = pizzas[item.ItemId];
                    else if (item.ItemType == "Beverage" && beverages.ContainsKey(item.ItemId))
                        item.ItemName = beverages[item.ItemId];
                    else
                        item.ItemName = "Unknown";

                    // ✅ Log each item
                    Console.WriteLine($"   ▶ ItemType: {item.ItemType}, ItemId: {item.ItemId}, ItemName: {item.ItemName}, Qty: {item.Quantity}");
                }
            }

            Console.WriteLine("✅ Specials and their items loaded successfully.\n");

            return View(specials);
        }

        // GET: Admin/Specials/Details/5
        public async Task<IActionResult> Details(string? id)
        {

            var specials = await _context.Specials
        .Include(s => s.SpecialItems)
        .ToListAsync();

            var pizzas = await _context.Pizzas.ToDictionaryAsync(p => p.Id, p => p.Name);
            var beverages = await _context.Beverages.ToDictionaryAsync(b => b.Id, b => b.Name);

            // Attach names dynamically to SpecialItems
            foreach (var special in specials)
            {
                Console.WriteLine($"\n🔹 SPECIAL: {special.Id} - {special.Title}");

                foreach (var item in special.SpecialItems)
                {
                    if (item.ItemType == "Pizza" && pizzas.ContainsKey(item.ItemId))
                        item.ItemName = pizzas[item.ItemId];
                    else if (item.ItemType == "Beverage" && beverages.ContainsKey(item.ItemId))
                        item.ItemName = beverages[item.ItemId];
                    else
                        item.ItemName = "Unknown";

                    // ✅ Log each item
                    Console.WriteLine($"   ▶ ItemType: {item.ItemType}, ItemId: {item.ItemId}, ItemName: {item.ItemName}, Qty: {item.Quantity}");
                }
            }

            Console.WriteLine("✅ Specials and their items loaded successfully.\n");

            return View(specials);
        }
        
        // POST: Admin/Specials/Create
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Special special, List<SpecialItem> Items, IFormFile? ImageFile)
        {
           // Console.WriteLine("✅ Create() called");

            if (!ModelState.IsValid)
            {
                // Log errors to console
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("MODEL ERROR: " + error.ErrorMessage);
                }

                ViewBag.Pizzas = await _context.Pizzas.ToListAsync();
                ViewBag.Beverages = await _context.Beverages.ToListAsync();
                return PartialView("_AddSpecials", special);
            }
            if (ModelState.IsValid)
            {
                Console.WriteLine("1 called");
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine("2 called");
                    // Generate unique filename
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                    // Save to wwwroot/uploads/specials
                    var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "specials");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);
                    Console.WriteLine("3 called");
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    special.ImageName = fileName;
                }
                Console.WriteLine("4 called");
                special.Id = GenerateSpecialId();
                special.SpecialItems = Items ?? new List<SpecialItem>();

                 // Calculate total from items
                special.TotalPrice = special.SpecialItems.Sum(x => x.Subtotal);
                special.FinalPrice = special.TotalPrice;
                special.CreatedAt = DateTime.Now;
                _context.Specials.Add(special);
                await _context.SaveChangesAsync();

                // ✅ Redirect back to Dashboard with correct page
                return RedirectToAction("Index", "Dashboard", new { area = "Admin", page = "ViewSpecials" });
            }
            Console.WriteLine("5 called");
            // If validation fails, reload AddSpecials partial inside Dashboard
            return RedirectToAction("Index", "Dashboard", new { area = "Admin", page = "AddSpecials" });
        }
        // GET: Admin/Specials/Edit/5
            public async Task<IActionResult> Edit(string id)
        {
            var special = await _context.Specials
                .Include(s => s.SpecialItems)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (special == null)
                return NotFound();

            // ✅ Load dropdown data
            ViewBag.Pizzas = await _context.Pizzas.ToListAsync();
            ViewBag.Beverages = await _context.Beverages.ToListAsync();

            // ✅ Pass model to partial
            return View("_EditSpecials", special);
        }


        // POST: Admin/Specials/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(string id, Special special, IFormFile? ImageFile)
        // {
        //     Console.WriteLine("FIRST HERE");

        //     if (!ModelState.IsValid)
        //     {
        //          Console.WriteLine("FAILED HERE");
        //     }


        //     if (id != special.Id) return NotFound();

        //     var existingSpecial = await _context.Specials.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        //     if (existingSpecial == null) return NotFound();

        //     if (ModelState.IsValid)
        //     {
        //         // If a new image is uploaded, replace it
        //         if (ImageFile != null && ImageFile.Length > 0)
        //         {
        //             var fileName = Path.GetFileName(ImageFile.FileName);
        //             var uploads = Path.Combine(_environment.WebRootPath, "uploads", "specials");
        //             if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

        //             var filePath = Path.Combine(uploads, fileName);
        //             using (var stream = new FileStream(filePath, FileMode.Create))
        //             {
        //                 await ImageFile.CopyToAsync(stream);
        //             }
        //             special.ImageName = fileName;
        //         }
        //         else
        //         {
        //             // Keep the existing image if no new image uploaded
        //             special.ImageName = existingSpecial.ImageName;
        //         }

        //         _context.Update(special);
        //         await _context.SaveChangesAsync();

        //         // Redirect back to Dashboard with page=ViewSpecials
        //         // return RedirectToAction("Index", "Dashboard", new { area = "Admin", page = "ViewSpecials" });
        //         return Redirect("/Admin/Dashboard?page=ViewSpecials");
        //     }
        //     return Redirect("/Admin/Dashboard?page=ViewSpecials");
        //     // Reload partial if validation fails
        //     //    return PartialView("_EditSpecials", special);
        // }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Special special, IFormFile? ImageFile, List<SpecialItem> Items)
    {
        Console.WriteLine("FIRST HERE");
Items = Items.Where(i => !string.IsNullOrEmpty(i.ItemType) && !string.IsNullOrEmpty(i.ItemId)).ToList();

        if (id != special.Id) return NotFound();

        var existingSpecial = await _context.Specials
            .Include(s => s.SpecialItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSpecial == null) return NotFound();

        if (!ModelState.IsValid)
        {
            Console.WriteLine("FAILED HERE");
             foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            // ⚠️ Re-populate dropdown data before reloading view
            ViewBag.Pizzas = await _context.Pizzas.ToListAsync();
            ViewBag.Beverages = await _context.Beverages.ToListAsync();
            return PartialView("~/Areas/Admin/Views/Dashboard/_EditSpecials.cshtml", special);
        }

        // ✅ Handle image upload
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = Path.GetFileName(ImageFile.FileName);
            var uploads = Path.Combine(_environment.WebRootPath, "uploads", "specials");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }
            special.ImageName = fileName;
        }
        else
        {
            special.ImageName = existingSpecial.ImageName;
        }

        // ✅ Update fields
       if (!string.IsNullOrEmpty(special.Title))
    existingSpecial.Title = special.Title;

if (!string.IsNullOrEmpty(special.Description))
    existingSpecial.Description = special.Description;

if (special.TotalPrice.HasValue)
    existingSpecial.TotalPrice = special.TotalPrice.Value;

if (!string.IsNullOrEmpty(special.DiscountType))
    existingSpecial.DiscountType = special.DiscountType;

if (special.DiscountValue.HasValue)
    existingSpecial.DiscountValue = special.DiscountValue.Value;

if (special.FinalPrice.HasValue)
    existingSpecial.FinalPrice = special.FinalPrice.Value;

            // if (special.IsActive.HasValue)
            //     existingSpecial.IsActive = special.IsActive.Value;
// ✅ Update StartDate and EndDate
existingSpecial.StartDate = special.StartDate;
existingSpecial.EndDate = special.EndDate;

            if (!string.IsNullOrEmpty(special.ImageName))
                existingSpecial.ImageName = special.ImageName;

        // ✅ Update SpecialItems
        var itemsToRemove = existingSpecial.SpecialItems
            .Where(e => !Items.Any(i => i.ItemId == e.ItemId && i.ItemType == e.ItemType))
            .ToList();

        _context.SpecialItem.RemoveRange(itemsToRemove);

        foreach (var item in Items)
        {
            var existingItem = existingSpecial.SpecialItems
                .FirstOrDefault(si => si.ItemId == item.ItemId && si.ItemType == item.ItemType);

            if (existingItem != null)
            {
                existingItem.Quantity = item.Quantity;
            }
            else
            {
                item.SpecialId = existingSpecial.Id;
                existingSpecial.SpecialItems.Add(item);
            }
        }

        await _context.SaveChangesAsync();

        return Redirect("/Admin/Dashboard?page=ViewSpecials");
    }
        private string GenerateSpecialId()
        {
            // Get the last pizza ID from the database
            var lastSpecial = _context.Specials
                .OrderByDescending(p => p.Id)
                .FirstOrDefault();

            if (lastSpecial == null)
                return "SPC-0001";

            // Extract numeric part
            var lastNumber = int.Parse(lastSpecial.Id.Split('-')[1]);
            var newNumber = lastNumber + 1;

            return $"SPC-{newNumber:D4}"; // Format as PZ-0001, PZ-0002, etc.
        }


    // POST: Admin/Specials/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var special = await _context.Specials
            .Include(s => s.SpecialItems)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (special == null)
            return NotFound();

        try
        {
            // ✅ Delete related SpecialItems first
            if (special.SpecialItems != null && special.SpecialItems.Any())
            {
                _context.SpecialItem.RemoveRange(special.SpecialItems);
            }

            // ✅ Delete image from wwwroot/uploads/specials
            if (!string.IsNullOrEmpty(special.ImageName))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, "uploads", "specials", special.ImageName);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    Console.WriteLine($"🗑️ Deleted image: {imagePath}");
                }
            }

            // ✅ Remove the Special itself
            _context.Specials.Remove(special);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Special {id} deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error deleting special {id}: {ex.Message}");
            return BadRequest("Error occurred while deleting the special.");
        }

        // ✅ Redirect back to the specials list
        return RedirectToAction("Index", "Dashboard", new { area = "Admin", page = "ViewSpecials" });
    }

    }
}
