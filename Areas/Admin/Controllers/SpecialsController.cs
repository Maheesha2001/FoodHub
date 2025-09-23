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
            var specials = await _context.Specials.ToListAsync();
            return View(specials);
        }

        // GET: Admin/Specials/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var special = await _context.Specials.FirstOrDefaultAsync(s => s.Id == id);
            if (special == null) return NotFound();

            return View(special);
        }

    // POST: Admin/Specials/Create
    [HttpPost]
   // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Special special, IFormFile? ImageFile)
    {
            Console.WriteLine("✅ Create() called");
             Console.WriteLine($"Title: {special.Title}, Desc: {special.Description}, Image: {ImageFile?.FileName}");
             
    if (!ModelState.IsValid)
    {
        // Log errors to console
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine("MODEL ERROR: " + error.ErrorMessage);
        }

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
        public async Task<IActionResult> Edit(int id)
        {
            var special = await _context.Specials.FindAsync(id);
            if (special == null) return NotFound();
            return View("_EditSpecials", special); // load the partial
        }

        // POST: Admin/Specials/Edit/5
       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, Special special, IFormFile? ImageFile)
{
    if (id != special.Id) return NotFound();

    var existingSpecial = await _context.Specials.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    if (existingSpecial == null) return NotFound();

            if (ModelState.IsValid)
            {
                // If a new image is uploaded, replace it
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
                    // Keep the existing image if no new image uploaded
                    special.ImageName = existingSpecial.ImageName;
                }

                _context.Update(special);
                await _context.SaveChangesAsync();

                // Redirect back to Dashboard with page=ViewSpecials
                // return RedirectToAction("Index", "Dashboard", new { area = "Admin", page = "ViewSpecials" });
         return Redirect("/Admin/Dashboard?page=ViewSpecials");
    }
  return Redirect("/Admin/Dashboard?page=ViewSpecials");
    // Reload partial if validation fails
//    return PartialView("_EditSpecials", special);
}


  }
}
