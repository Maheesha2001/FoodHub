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

  }
}
