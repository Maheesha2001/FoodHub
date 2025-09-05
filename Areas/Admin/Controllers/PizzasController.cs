using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var pizzas = await _context.Pizzas.ToListAsync();
            return View(pizzas); // Index.cshtml can include _ViewPizzas partial
        }

        // GET: Admin/Pizzas/PartialList
        public async Task<IActionResult> PartialList()
        {
            var pizzas = await _context.Pizzas.ToListAsync();
            return PartialView("_ViewPizzas", pizzas); // partial for dashboard
        }

        // GET: Admin/Pizzas/Create
        public IActionResult Create() => View();

        // POST: Admin/Pizzas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pizza pizza, IFormFile? ImageFile)
        {
            if (ModelState.IsValid)
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

                pizza.CreatedAt = DateTime.Now;
                _context.Pizzas.Add(pizza);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(pizza);
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
    }
}
