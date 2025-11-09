using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodHub.Data;
using FoodHub.ViewModels.Orders;

namespace FoodHub.Controllers
{
    [Authorize]
    public class MyOrdersController : Controller
    {
        private readonly FoodHubContext _db;

        public MyOrdersController(FoodHubContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = _db.Orders
     .Where(o => o.UserId == userId)
     .OrderByDescending(o => o.CreatedAt)
     .ToList();

            // Manually populate Payment and DeliveryInfo
            foreach (var order in orders)
            {
                order.Payment = _db.Payments.FirstOrDefault(p => p.Code == order.Code);
                order.DeliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
            }

            // Map to ViewModel
            var vm = orders.Select(o => new MyOrderViewModel
            {
                Id = o.Id,
                Code = o.Code,
                CreatedAt = o.CreatedAt,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "Not Available",
                DeliveryStatus = o.DeliveryInfo != null ? o.DeliveryInfo.DeliveryStatus : "Pending"
            }).ToList();

            return View(vm);

        }
        
        public IActionResult Details(string orderCode)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    // Load order by Code
    var order = _db.Orders.FirstOrDefault(o => o.Code == orderCode && o.UserId == userId);
    if (order == null)
        return NotFound();

    // Manually load related data
    order.OrderItems = _db.OrderItems.Where(oi => oi.Code == order.Code).ToList();
    order.Payment = _db.Payments.FirstOrDefault(p => p.Code == order.Code);
    order.DeliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
    order.User = _db.Users.FirstOrDefault(u => u.Id == order.UserId);

    return View(order);
}

 }
}

// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using FoodHub.Data;
// using FoodHub.ViewModels.Orders;

// namespace FoodHub.Controllers
// {
//     [Authorize]
//     public class MyOrdersController : Controller
//     {
//         private readonly FoodHubContext _db;

//         public MyOrdersController(FoodHubContext db)
//         {
//             _db = db;
//         }

//         public IActionResult Index()
//         {
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
          
//             var orders = _db.Orders
//                 .Include(o => o.Payment)
//                 .Where(o => o.UserId == userId)
//                 .OrderByDescending(o => o.CreatedAt)
//                 .Select(o => new MyOrderViewModel
//                 {
//                     Id = o.Id,
//                     CreatedAt = o.CreatedAt,
//                     TotalAmount = o.TotalAmount,
//                     Status = o.Status,
//                     PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "Pending"
//                 })
//                 .ToList();

//             return View(orders);
//         }

//         public IActionResult Details(int id)
//         {
//             // Optional: show detailed items of each order
//             // return RedirectToAction("Details", "Checkout", new { orderId = id });
//             return RedirectToAction("Details", "Checkout", new { area = "Customer", orderId = id });

//         }
        
//         [HttpGet]
// public IActionResult GetOrderDetails(int id)
// {
//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//     if (string.IsNullOrEmpty(userId))
//         return Unauthorized();

//     var order = _db.Orders
//         .Include(o => o.OrderItems)
//         .Include(o => o.DeliveryInfo)
//         .FirstOrDefault(o => o.Id == id && o.UserId == userId);

//     if (order == null)
//         return NotFound();

//     return PartialView("_OrderDetailsPartial", order);
// }

//     }
// }
