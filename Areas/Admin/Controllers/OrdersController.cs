using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using FoodHub.Hubs;



namespace FoodHub.Areas.Admin.Controllers
{
[Area("Admin")]
[Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly FoodHubContext _context;
        private readonly IHubContext<DeliveryHub> _hubContext;

               
        public OrdersController(
            FoodHubContext context,
            IHubContext<DeliveryHub> hubContext
        )
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            // Get all orders first
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

                    // Manually load related data
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var orderCodes = orders.Select(o => o.Code).ToList();
            var paymentIds = orders.Select(o => o.Id).ToList(); // Payment linked by OrderId
            var deliveryIds = orders.Select(o => o.Id).ToList(); // DeliveryInfo linked by OrderId

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var payments = await _context.Payments
                .Where(p => orderCodes.Contains(p.Code))
                .ToDictionaryAsync(p => p.Code);

            var deliveries = await _context.DeliveryInfo
                .Where(d => orderCodes.Contains(d.Code)) 
                .ToDictionaryAsync(d => d.Code);

            // Attach related entities manually
            foreach (var order in orders)
            {
                if (users.TryGetValue(order.UserId, out var user))
                    order.User = user;

                if (payments.TryGetValue(order.Code, out var payment))
                    order.Payment = payment;

                if (deliveries.TryGetValue(order.Code, out var delivery))
                    order.DeliveryInfo = delivery;
            }

            // Custom sorting based on status
            orders = orders
                .OrderBy(o =>
                {
                    // Define a numeric priority: lower number = higher priority
                    if (o.Status == "Pending" || o.DeliveryInfo?.DeliveryStatus == "Pending")
                        return 1;
                    else if (o.DeliveryInfo?.DeliveryStatus == "Out for Delivery")
                        return 2;
                    else if (o.Status == "Completed" || o.DeliveryInfo?.DeliveryStatus == "Delivered")
                        return 3;
                    else
                        return 4; // Any other status
                })
                .ThenByDescending(o => o.CreatedAt) // Within same status, newest first
                .ToList();

   
            return View(orders);
        }

    
        // GET: Admin/Orders/Details?orderCode=xxxx
        public async Task<IActionResult> Details(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
                return BadRequest();

            var order = _context.Orders.FirstOrDefault(o => o.Code == orderCode);
            if (order == null)
                return NotFound();

            // Manually get related entities
            order.OrderItems = _context.OrderItems.Where(i => i.Code == order.Code).ToList();
            order.Payment = _context.Payments.FirstOrDefault(p => p.Code == order.Code);
            order.DeliveryInfo = _context.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
            order.User = _context.Users.FirstOrDefault(u => u.Id == order.UserId);

            return View(order);
        }

        // ✅ Update Order Status
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(string orderCode, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Code == orderCode);
            if (order == null)
                return NotFound();

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            if (status == "Completed")
            {
                await AssignDeliveryPerson(order.Code);
            }


            TempData["Message"] = $"Order #{order.Code} marked as {status}.";
            return RedirectToAction(nameof(Index));
        }

        
        // private async Task AssignDeliveryPerson(string orderCode)
        // {
        //     var today = DateTime.Today;

        //     if (await _context.DeliveryOrderAssignments
        //         .AnyAsync(a => a.OrderCode == orderCode))
        //         return;

        //     var presentDrivers = await _context.DeliveryAttendance
        //         .Where(a => a.Date == today && a.IsPresent)
        //         .Select(a => a.DeliveryPersonId)
        //         .ToListAsync();

        //         if (!presentDrivers.Any())
        //             return;

        //     var driverLoad = await _context.DeliveryOrderAssignments
        //         .Where(a =>
        //             presentDrivers.Contains(a.DeliveryPersonId) &&
        //             a.Status != "Delivered"
        //         )
        //         .GroupBy(a => a.DeliveryPersonId)
        //         .Select(g => new
        //         {
        //             DriverId = g.Key,
        //             Count = g.Count()
        //         })
        //         .ToListAsync();

        //     var selectedDriver = presentDrivers
        //         .Select(d => new
        //         {
        //             DriverId = d,
        //             Count = driverLoad.FirstOrDefault(x => x.DriverId == d)?.Count ?? 0
        //         })
        //         .OrderBy(x => x.Count)
        //         .First();

        //     // ✅ Assign delivery person
        //     var assignment = new DeliveryOrderAssignment
        //     {
        //         OrderCode = orderCode,
        //         DeliveryPersonId = selectedDriver.DriverId,
        //         AssignedAt = DateTime.Today,
        //         Status = "Assigned"
        //     };

        //     _context.DeliveryOrderAssignments.Add(assignment);

        //     // ✅ AUTO update delivery status
        //     var delivery = await _context.DeliveryInfo
        //         .FirstOrDefaultAsync(d => d.Code == orderCode);

        //     if (delivery != null)
        //     {
        //         delivery.DeliveryStatus = "Out for Delivery";
        //         _context.Update(delivery);
        //     }

        //     await _context.SaveChangesAsync();
        // }

        private async Task AssignDeliveryPerson(string orderCode)
        {
            var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "Sri Lanka Standard Time"
                );

            var today = sriLankaTime.Date;

            // ❌ If already assigned → do nothing
            if (await _context.DeliveryOrderAssignments
                .AnyAsync(a => a.OrderCode == orderCode))
                return;

            // var presentDrivers = await _context.DeliveryAttendance
            //     .Where(a => a.Date == today && a.IsPresent)
            //     .Select(a => a.DeliveryPersonId)
            //     .ToListAsync();

            var presentDrivers = await _context.DeliveryAttendance
            .Where(a =>
                a.Date == today &&
                a.IsPresent &&
                a.CheckOutTime == null)   // 🔥 must not be checked out
            .Select(a => a.DeliveryPersonId)
            .ToListAsync();

            var delivery = await _context.DeliveryInfo
                .FirstOrDefaultAsync(d => d.Code == orderCode);

            // ❗ If no drivers present → mark waiting
            if (!presentDrivers.Any())
            {
                if (delivery != null)
                {
                    delivery.DeliveryStatus = "Waiting for Driver";
                    _context.Update(delivery);
                    await _context.SaveChangesAsync();
                }

                return;
            }

            // 🔥 Auto load balancing
            var driverLoad = await _context.DeliveryOrderAssignments
                .Where(a =>
                    presentDrivers.Contains(a.DeliveryPersonId) &&
                    a.Status != "Delivered")
                .GroupBy(a => a.DeliveryPersonId)
                .Select(g => new
                {
                    DriverId = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var selectedDriver = presentDrivers
                .Select(d => new
                {
                    DriverId = d,
                    Count = driverLoad.FirstOrDefault(x => x.DriverId == d)?.Count ?? 0
                })
                .OrderBy(x => x.Count)
                .First();

            var assignment = new DeliveryOrderAssignment
            {
                OrderCode = orderCode,
                DeliveryPersonId = selectedDriver.DriverId,
                AssignedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                                DateTime.UtcNow,
                                "Sri Lanka Standard Time"
                            ),
                Status = "Assigned"
            };

            _context.DeliveryOrderAssignments.Add(assignment);

            if (delivery != null)
            {
                delivery.DeliveryStatus = "Out for Delivery";
                _context.Update(delivery);
            }

            await _context.SaveChangesAsync();
        }

        // ✅ Update Payment Status
        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(string orderCode, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Code == orderCode);
            if (order == null)
                return NotFound();

            var payment = _context.Payments.FirstOrDefault(p => p.Code == order.Code);
            if (payment != null)
            {
                payment.PaymentStatus = status;
                _context.Update(payment);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = $"Payment for Order #{order.Code} updated to {status}.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Update Delivery Status (Auto + Manual)
        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryStatus(string orderCode, string status)
        {
            var delivery = await _context.DeliveryInfo
                .FirstOrDefaultAsync(d => d.Code == orderCode);

            if (delivery == null)
                return NotFound();

            delivery.DeliveryStatus = status;
            _context.Update(delivery);
            await _context.SaveChangesAsync();

            TempData["Message"] =
                $"Delivery for Order #{orderCode} set to {status}.";

            return RedirectToAction(nameof(Index));
        }


       // ✅ Update Delivery Status
        private async Task SetDeliveryStatus(string orderCode, string status)
        {
            var delivery = await _context.DeliveryInfo
                .FirstOrDefaultAsync(d => d.Code == orderCode);

            if (delivery == null)
                return;

            delivery.DeliveryStatus = status;
            _context.Update(delivery);
            await _context.SaveChangesAsync();
        }

        // public async Task<IActionResult> AssignDriver(string orderCode)
        // {
        //      var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
        //             DateTime.UtcNow,
        //             "Sri Lanka Standard Time"
        //         );

        //     var today = sriLankaTime.Date;

        //     // var presentDriverIds = await _context.DeliveryAttendance
        //     //     .Where(a => a.Date == today && a.IsPresent)
        //     //     .Select(a => a.DeliveryPersonId)
        //     //     .ToListAsync();

        //     var presentDrivers = await _context.DeliveryAttendance
        //     .Where(a =>
        //         a.Date == today &&
        //         a.IsPresent &&
        //         a.CheckOutTime == null)   // 🔥 must not be checked out
        //     .Select(a => a.DeliveryPersonId)
        //     .ToListAsync();

        //     var drivers = await _context.DeliveryPerson
        //         .Where(d => presentDriverIds.Contains(d.Id))
        //         .ToListAsync();

        //     ViewBag.OrderCode = orderCode;

        //     return View(drivers);
        // }

        public async Task<IActionResult> AssignDriver(string orderCode)
        {
            var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                DateTime.UtcNow,
                "Sri Lanka Standard Time"
            );

            var today = sriLankaTime.Date;

            var presentDrivers = await _context.DeliveryAttendance
                .Where(a =>
                    a.Date == today &&
                    a.IsPresent &&
                    a.CheckOutTime == null)
                .Select(a => a.DeliveryPersonId)
                .ToListAsync();

            var drivers = await _context.DeliveryPerson
                .Where(d => presentDrivers.Contains(d.Id))
                .ToListAsync();

            ViewBag.OrderCode = orderCode;

            return View(drivers);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAssignDriver(string orderCode, string driverId)
        {
            var alreadyAssigned = await _context.DeliveryOrderAssignments
                .AnyAsync(a => a.OrderCode == orderCode);

            if (alreadyAssigned)
            {
                TempData["Message"] = "Order already assigned.";
                return RedirectToAction(nameof(Index));
            }

            var assignment = new DeliveryOrderAssignment
            {
                OrderCode = orderCode,
                DeliveryPersonId = driverId,
                AssignedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                                DateTime.UtcNow,
                                "Sri Lanka Standard Time"
                            ),
                Status = "Assigned"
            };

            _context.DeliveryOrderAssignments.Add(assignment);

            var delivery = await _context.DeliveryInfo
                .FirstOrDefaultAsync(d => d.Code == orderCode);

            if (delivery != null)
            {
                delivery.DeliveryStatus = "Out for Delivery";
                _context.Update(delivery);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Driver assigned successfully.";

            return RedirectToAction(nameof(Index));
        }

    }
}

