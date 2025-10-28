using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FoodHub.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly FoodHubContext _db;

        public CheckoutController(FoodHubContext db)
        {
            _db = db;
        }

        // 🛒 Step 1: Cart Review
        public IActionResult Index()
        {
            var cartJson = HttpContext.Session.GetString("CheckoutCart");
            var vm = string.IsNullOrEmpty(cartJson)
                ? new List<CartItemViewModel>()
                : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);

            return View(vm);
        }

       [HttpGet]
        public IActionResult IsCartFrozen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Json(new { frozen = false });

            // Check if there's already an Order with Pending status
            var pendingOrder = _db.Orders.Any(o => o.UserId == userId && o.Status == "Pending");

            return Json(new { frozen = pendingOrder });
        }

       
        // 🧮 Update Cart Quantities
        [HttpPost]
        public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
            if (cart == null) return NotFound();

            var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
            if (item == null) return NotFound();

            item.Quantity = quantity;
            _db.SaveChanges();

            return Json(new { success = true, total = item.Price * item.Quantity });
        }

        // 🚚 Step 2: Delivery Info
        // GET: Delivery Info (auto-create order)
        [HttpGet]
        public async Task<IActionResult> DeliveryInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var cart = _db.Carts.Include(c => c.Items)
                                .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

            if (cart != null && cart.Items.Any())
            {
                // ✅ Step 1: Create Order
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };
                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                // ✅ Step 2: Copy CartItems to OrderItems
                foreach (var item in cart.Items)
                {
                    _db.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        ProductType = item.Type,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });
                }

                // ✅ Step 3: Mark Cart as Inactive
                cart.Status = "Inactive";
                await _db.SaveChangesAsync();

                // ✅ Step 4: Clear all relevant session data
                HttpContext.Session.Remove("CheckoutCart");
                HttpContext.Session.Remove("SomeOtherCartRelatedKey"); // optional if you store more
            }

            // ✅ Step 5: Store FrozenOrderId for DeliveryInfo flow
            var frozenOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
            TempData["FrozenOrderId"] = frozenOrder != null ? frozenOrder.Id : 0;

            return View(new DeliveryInfoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryInfo(DeliveryInfoViewModel model)
        {
            Console.WriteLine("POST: DeliveryInfo triggered");

            // Log form input
            Console.WriteLine($"Model: Name={model.Name}, Email={model.Email}, Phone={model.Phone}, Address={model.Address}, DeliveryNotes={model.DeliveryNotes}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var pendingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
            if (pendingOrder == null)
                return RedirectToAction("Cart");

            var existing = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == pendingOrder.Id);

            if (existing != null)
            {
                existing.Name = model.Name;
                existing.Email = model.Email;
                existing.Phone = model.Phone;
                existing.Address = model.Address;
                existing.DeliveryNotes = model.DeliveryNotes;
            }
            else
            {
                var deliveryInfo = new DeliveryInfo
                {
                    OrderId = pendingOrder.Id,
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    DeliveryNotes = model.DeliveryNotes
                };
                _db.DeliveryInfo.Add(deliveryInfo);
            }

            _db.SaveChanges();
            Console.WriteLine("✅ DeliveryInfo saved successfully.");

            return RedirectToAction("Payment");
        }

        [HttpGet]
        public IActionResult Payment()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            // 1️⃣ Find the pending order for this user
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

            if (order == null)
                return RedirectToAction("Cart");

            // 2️⃣ Find delivery info for this order
            var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == order.Id);

            // 3️⃣ Prepare ViewModel
            var viewModel = new PaymentViewModel
            {
                Order = order,
                OrderItems = order.OrderItems.ToList(),
                DeliveryInfo = deliveryInfo
            };

            // 4️⃣ Pass to the view
            return View(viewModel);
        }
      
        [HttpPost]
        public IActionResult Payment(PaymentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var cartJson = HttpContext.Session.GetString("CheckoutCart");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItemViewModel>()
                : System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);

            var deliveryJson = TempData["DeliveryInfo"] as string ?? "{}";
            var deliveryInfoVm = System.Text.Json.JsonSerializer.Deserialize<DeliveryInfoViewModel>(deliveryJson);

            if (cart == null || cart.Count == 0 || deliveryInfoVm == null)
                return RedirectToAction("Index");

            
            // 💳 Determine payment method
            string paymentMethod = Request.Form["PaymentMethod"];
            if (string.IsNullOrEmpty(paymentMethod))
                paymentMethod = "Card"; // default fallback

            string transactionId = "Not Required";
            if (paymentMethod == "Card")        // ⚙️ Simulate Transaction ID
                transactionId = $"TRX_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

                // 💰 Log before creating payment record
                Console.WriteLine($"[Payment Debug] OrderId: {vm?.Order?.Id}, Amount: {vm?.Order?.TotalAmount}, Method: {paymentMethod}, Transaction: {transactionId}");

            
            // 💰 Create payment record
            var payment = new Payment
            {
                OrderId = vm.Order!.Id, // Use the Order from the ViewModel
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                Amount = vm.Order.TotalAmount,
                PaymentStatus = paymentMethod == "Card" ? "Success" : "Pending",
                CreatedAt = DateTime.Now
            };

                    _db.Payments.Add(payment);
            
                    // ✅ Update order status to Completed
                    var order = _db.Orders.FirstOrDefault(o => o.Id == vm.Order.Id);
                    if (order != null)
                    {
                        order.Status = "Completed";
                        _db.Orders.Update(order);
                    }


                    _db.SaveChanges();


            return RedirectToAction("Success", new { orderId = vm.Order.Id });
        }

        // ✅ Step 4: Success Page
        public IActionResult Success(int orderId)
        {
             HttpContext.Session.Clear();
            TempData.Clear();

            var order = _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryInfo)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return RedirectToAction("Index");

            return View(order);
        }
    }
}
