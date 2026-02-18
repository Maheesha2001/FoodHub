using Microsoft.AspNetCore.Mvc;
using FoodHub.Models;
using FoodHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FoodHub.ViewModels;

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
            ModelState.Remove(nameof(model.Id));

            if (ModelState.IsValid)
            {
                if (await _context.DeliveryPerson.AnyAsync(dp => dp.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Hash password
                var passwordHasher = new PasswordHasher<DeliveryPerson>();
                model.Password = passwordHasher.HashPassword(model, model.Password);

                // Get existing EmpDel IDs
                var existingIds = await _context.DeliveryPerson
                    .Where(dp => dp.Id.StartsWith("EmpDel"))
                    .Select(dp => dp.Id)
                    .ToListAsync();

                int maxNumber = 0;

                if (existingIds.Any())
                {
                    maxNumber = existingIds
                        .Select(id => int.Parse(id.Substring(6)))
                        .Max();
                }

                model.Id = "EmpDel" + (maxNumber + 1).ToString("D3");

                model.CreatedAt = DateTime.UtcNow;
                model.IsActive = true;

                _context.DeliveryPerson.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Delivery person registered successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        private string GenerateCustomId()
        {
            // Get the last numeric part
            var lastId = _context.DeliveryPerson
                .OrderByDescending(dp => dp.Id)   // Order by Id
                .Select(dp => dp.Id)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(lastId))
                return "EmpDel001";

            // Extract number part after "EmpDel"
            string numberPart = lastId.Substring(6);
            int number = int.Parse(numberPart);

            number++;  // Increment

            return "EmpDel" + number.ToString("D3");
        }

                
        // GET: Admin/DeliveryPerson/Edit/EmpDel001
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var deliveryPerson = await _context.DeliveryPerson.FindAsync(id);
            if (deliveryPerson == null)
            {
                return NotFound();
            }

            return View(deliveryPerson);
        }

        // POST: Admin/DeliveryPerson/Edit/EmpDel001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DeliveryPerson model)
        {
            if (id != model.Id)
            {
                Console.WriteLine("ID mismatch!");
                return BadRequest();
            }

            var existingUser = await _context.DeliveryPerson.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(model.Name) && model.Name != existingUser.Name)
                existingUser.Name = model.Name;

            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != existingUser.Email)
                existingUser.Email = model.Email;

            if (!string.IsNullOrWhiteSpace(model.NIC) && model.NIC != existingUser.NIC)
                existingUser.NIC = model.NIC;

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber != existingUser.PhoneNumber)
                existingUser.PhoneNumber = model.PhoneNumber;

            existingUser.FingerprintEnabled = model.FingerprintEnabled;
            existingUser.IsActive = model.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Delivery person updated successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Attendance()
        {
            return View();
        }

        public async Task<IActionResult> PresentDrivers()
        {
                //var today = DateTime.Today;
                var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "Sri Lanka Standard Time"
                );

                var today = sriLankaTime.Date;

                var attendance = await _context.DeliveryAttendance
                    .Where(a => a.Date == today && a.IsPresent)
                    .ToListAsync();

                var drivers = await _context.DeliveryPerson.ToListAsync();

                var result = attendance.Select(a => new PresentDriverVM
                {
                    DeliveryPersonId = a.DeliveryPersonId,
                    Name = drivers.First(d => d.Id == a.DeliveryPersonId).Name,
                    NIC = drivers.First(d => d.Id == a.DeliveryPersonId).NIC,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    Date = a.Date
                }).ToList();

                return View("Attendance", result);
        }   


        // public async Task<IActionResult> PresentDriversHistory()
        // {
        //     var attendance = await _context.DeliveryAttendance
        //         .OrderByDescending(a => a.Date) 
        //         .ToListAsync();

        //     var drivers = await _context.DeliveryPerson.ToListAsync();

        //     var result = attendance.Select(a => new PresentDriverVM
        //         {
        //             DeliveryPersonId = a.DeliveryPersonId,
        //             Name = drivers.First(d => d.Id == a.DeliveryPersonId).Name,
        //             NIC = drivers.First(d => d.Id == a.DeliveryPersonId).NIC,
        //             CheckInTime = a.CheckInTime,
        //              CheckOutTime = a.CheckOutTime,
        //             Date = a.Date
        //         }).ToList();

        //         return View("AttendanceHistory", result);
        // }      
           
        public async Task<IActionResult> PresentDriversHistory()
        {
            var result = await _context.DeliveryAttendance
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.CheckInTime)
                .Join(_context.DeliveryPerson,
                    attendance => attendance.DeliveryPersonId,
                    driver => driver.Id,
                    (attendance, driver) => new PresentDriverVM
                    {
                        DeliveryPersonId = attendance.DeliveryPersonId,
                        Name = driver.Name,
                        NIC = driver.NIC,
                        CheckInTime = attendance.CheckInTime,
                        CheckOutTime = attendance.CheckOutTime,
                        Date = attendance.Date
                    })
                .ToListAsync();

            return View("AttendanceHistory", result);
        }



        public async Task<IActionResult> DeliveryAssign()
        {
               var assignments = await _context.DeliveryOrderAssignments
                .OrderByDescending(a => a.AssignedAt)
                .ToListAsync();

                return View("DeliveryOrderAssignments", assignments);
        }

    
    }

    
}
