using Microsoft.AspNetCore.Mvc;
using FoodHub.Models;
using FoodHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
     [Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class DeliveryPersonController : Controller
    {
        private readonly FoodHubContext _context;
        public DeliveryPersonController(FoodHubContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var deliveryPersons = await _context.DeliveryPerson.ToListAsync();
            return View(deliveryPersons);
        }

        // GET: Admin/DeliveryPerson/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(DeliveryPerson model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.DeliveryPerson
                    .FirstOrDefaultAsync(dp => dp.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Hash the password using ASP.NET Core Identity PasswordHasher
                var passwordHasher = new PasswordHasher<DeliveryPerson>();
                model.Password = passwordHasher.HashPassword(model, model.Password);

                // Set created date and default values
                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true; // default active status

                _context.DeliveryPerson.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Delivery person registered successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Admin/DeliveryPerson/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var deliveryPerson = await _context.DeliveryPerson.FindAsync(id);
            if (deliveryPerson == null)
            {
                return NotFound();
            }

            return View(deliveryPerson);
        }

        // POST: Admin/DeliveryPerson/Edit/5
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, DeliveryPerson model)
{
    Console.WriteLine($"Edit POST called for Id: {id}");

    if (id != model.Id)
    {
        Console.WriteLine("ID mismatch!");
        return BadRequest();
    }

    // if (!ModelState.IsValid)
    // {
    //     Console.WriteLine("Model state invalid!");
    //     return View(model);
    // }

    if (!ModelState.IsValid)
    {
        foreach (var key in ModelState.Keys)
        {
            var errors = ModelState[key].Errors;
            foreach (var error in errors)
            {
                Console.WriteLine($"Validation failed for {key}: {error.ErrorMessage}");
            }
        }
        return View(model);
    }


    var existingUser = await _context.DeliveryPerson.FindAsync(id);
    if (existingUser == null)
    {
        Console.WriteLine("Existing user not found in DB!");
        return NotFound();
    }

    Console.WriteLine("Existing user loaded successfully.");

    // Update only fields that have changed & are not null
    if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existingUser.Name)
    {
        existingUser.Name = model.Name;
        Console.WriteLine($"Name updated to: {model.Name}");
    }

    if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != existingUser.Email)
    {
        existingUser.Email = model.Email;
        Console.WriteLine($"Email updated to: {model.Email}");
    }

    if (!string.IsNullOrWhiteSpace(model.NIC) && model.NIC != existingUser.NIC)
    {
        existingUser.NIC = model.NIC;
        Console.WriteLine($"NIC updated to: {model.NIC}");
    }

    if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber != existingUser.PhoneNumber)
    {
        existingUser.PhoneNumber = model.PhoneNumber;
        Console.WriteLine($"PhoneNumber updated to: {model.PhoneNumber}");
    }

    // Boolean fields: always take the submitted value
    existingUser.FingerprintEnabled = model.FingerprintEnabled;
    Console.WriteLine($"FingerprintEnabled set to: {model.FingerprintEnabled}");

    existingUser.IsActive = model.IsActive;
    Console.WriteLine($"IsActive set to: {model.IsActive}");

    // Timestamp
    existingUser.UpdatedAt = DateTime.UtcNow;
    Console.WriteLine($"UpdatedAt set to: {existingUser.UpdatedAt}");

    await _context.SaveChangesAsync();
    Console.WriteLine("Changes saved to DB successfully.");

    TempData["SuccessMessage"] = "Delivery person updated successfully!";
    return RedirectToAction("Index");
}




        // Details / Delete methods...
    }
}
