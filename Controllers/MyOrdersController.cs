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
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new MyOrderViewModel
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    PaymentStatus = o.Payment != null ? o.Payment.PaymentStatus : "Pending"
                })
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryInfo)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

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
