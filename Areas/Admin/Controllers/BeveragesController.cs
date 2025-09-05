using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BeveragesController : Controller
    {
        private readonly FoodHubContext _context;
        private readonly IWebHostEnvironment _env;

        public BeveragesController(FoodHubContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Admin/Beverages
        public async Task<IActionResult> Index()
        {
            var beverages = await _context.Beverages.ToListAsync();
            return View(beverages); // can include _ViewBeverages partial
        }

        // GET: Admin/Beverages/Create
        public IActionResult Create() => View();

        // POST: Admin/Beverages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Beverage beverage, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var uploads = Path.Combine(_env.WebRootPath, "uploads/beverages");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                    var filePath = Path.Combine(uploads, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    beverage.ImageName = fileName;
                }

                beverage.CreatedAt = DateTime.Now;
                _context.Add(beverage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(beverage);
        }

        // GET: Admin/Beverages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var beverage = await _context.Beverages.FindAsync(id);
            if (beverage == null) return NotFound();
            return View(beverage);
        }

        // POST: Admin/Beverages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Beverage beverage, IFormFile? ImageFile)
        {
            if (id != beverage.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var uploads = Path.Combine(_env.WebRootPath, "uploads/beverages");
                        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                        var filePath = Path.Combine(uploads, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        beverage.ImageName = fileName;
                    }

                    _context.Update(beverage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Beverages.Any(e => e.Id == beverage.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(beverage);
        }

        // GET: Admin/Beverages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var beverage = await _context.Beverages.FindAsync(id);
            if (beverage == null) return NotFound();

            _context.Beverages.Remove(beverage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
