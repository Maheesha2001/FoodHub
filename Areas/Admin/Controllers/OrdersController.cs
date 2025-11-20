using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FoodHub.Controllers.Admin
{
[Area("Admin")]
[Authorize(AuthenticationSchemes = "AdminScheme", Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly FoodHubContext _context;

        public OrdersController(FoodHubContext context)
        {
            _context = context;
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
                .Where(p => orderCodes.Contains(p.Code)) // assuming Payment has OrderCode
                .ToDictionaryAsync(p => p.Code);

            var deliveries = await _context.DeliveryInfo
                .Where(d => orderCodes.Contains(d.Code)) // assuming DeliveryInfo has OrderCode
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

            TempData["Message"] = $"Order #{order.Code} marked as {status}.";
            return RedirectToAction(nameof(Index));
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

        // ✅ Update Delivery Status
        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryStatus(string orderCode, string status)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Code == orderCode);
            if (order == null)
                return NotFound();

            var delivery = _context.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
            if (delivery != null)
            {
                delivery.DeliveryStatus = status;
                _context.Update(delivery);
                await _context.SaveChangesAsync();
            }

            TempData["Message"] = $"Delivery for Order #{order.Code} set to {status}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

