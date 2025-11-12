using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;

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

        [HttpGet("/Customer/Checkout/{code?}")]
        public IActionResult Index(string code)
        {
            List<CartItemViewModel> vm;

            if (!string.IsNullOrEmpty(code))
            {
                var cartItems = _db.CartItems
                    .Where(c => c.Code == code)
                    .Select(i => new CartItemViewModel
                    {
                        Id = i.ProductId.ToString(),
                        Name = i.ProductName,
                        Type = i.Type,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Code = i.Code
                    }).ToList();

                vm = cartItems;
                HttpContext.Session.SetString("CheckoutCart", JsonConvert.SerializeObject(cartItems));

                TempData["FrozenOrderCode"] = code;
                TempData.Keep("FrozenOrderCode"); // ✅ Keep it for next request
            }
            else
            {
                var cartJson = HttpContext.Session.GetString("CheckoutCart");
                vm = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItemViewModel>()
                    : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);

                if (vm.Any())
                {
                    var firstCartCode = _db.CartItems.FirstOrDefault(i => i.ProductId.ToString() == vm[0].Id)?.Code;
                    if (!string.IsNullOrEmpty(firstCartCode))
                    {
                        TempData["FrozenOrderCode"] = firstCartCode;
                        TempData.Keep("FrozenOrderCode");
                    }
                }
            }

            return View(vm);
        }

        // ✅ Check if cart is frozen (Delivery flow)
        [HttpGet]
        public IActionResult IsCartFrozen()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { frozen = false });

            bool frozen = TempData.ContainsKey("IsFrozen") && TempData["IsFrozen"] is bool f && f;
            if (frozen)
            {
                TempData.Keep("IsFrozen");
                return Json(new { frozen = true });
            }

            TempData.Remove("IsFrozen");
            return Json(new { frozen = false });
        }

        // 🧮 Update Cart Quantities (no FK between Cart and CartItem)
        [HttpPost]
        public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
            if (cart == null) return NotFound();

            var item = _db.CartItems.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
            if (item == null) return NotFound();

            item.Quantity = quantity;
            _db.SaveChanges();

            return Json(new { success = true, total = item.Price * item.Quantity });
        }

        //  🚚 Step 2: Delivery Info

        [HttpGet("/Customer/Checkout/DeliveryInfo")]
        public async Task<IActionResult> DeliveryInfo(string code)
        {
            Console.WriteLine($"🎯 Reached DeliveryInfo() with code: {code}");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            TempData["IsFrozen"] = true;

            var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active" && c.Code == code);
            if (cart == null)
            {
                var existingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Code == code);
                if (existingOrder == null)
                    return RedirectToAction("Index", "Checkout");

                TempData["FrozenOrderCode"] = existingOrder.Code;
                return View(new DeliveryInfoViewModel());
            }

            var cartItems = _db.CartItems.Where(i => i.Code == cart.Code).ToList();
            if (!cartItems.Any())
                return RedirectToAction("Index", "Checkout");

            var order = new Order
            {
                UserId = userId,
                Code = cart.Code,
                TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                _db.OrderItems.Add(new OrderItem
                {
                    Code = cart.Code,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductType = item.Type,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                });
            }

            cart.Status = "Inactive";
            await _db.SaveChangesAsync();

            TempData["FrozenOrderCode"] = order.Code;

           // Console.WriteLine($"✅ Redirecting to DeliveryInfo with Code = {order.Code}");

            //return View(new DeliveryInfoViewModel());
           
            var model = new DeliveryInfoViewModel
{
    OrderCode = order.Code
};
Console.WriteLine($"✅ Passing OrderCode to view: {model.OrderCode}");

return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryInfo(DeliveryInfoViewModel model)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("model.OrderCode is here" + model.OrderCode);
            // Find the pending order by Code instead of Id
            // var pendingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending" && o.Code == orderCode);
            var pendingOrder = _db.Orders.FirstOrDefault(o =>
     o.UserId == userId && o.Status == "Pending" && o.Code == model.OrderCode);
            if (pendingOrder == null)

                return RedirectToAction("Cart");


            // Check existing DeliveryInfo by order Code
            var existing = _db.DeliveryInfo.FirstOrDefault(d => d.Code == pendingOrder.Code);
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
                _db.DeliveryInfo.Add(new DeliveryInfo
                {
                    Code = pendingOrder.Code, // use Code
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    DeliveryNotes = model.DeliveryNotes,
                    DeliveryStatus = "Pending"
                });
            }

            _db.SaveChanges();
            return RedirectToAction("Payment", new { orderCode = pendingOrder.Code });
        }

        // Step 3: Payment
        [HttpGet("/Customer/Checkout/Payment")]
        public IActionResult Payment(string orderCode)
        {
            Console.WriteLine("Order code in the payment is --> " + orderCode);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get order by Code
            var order = _db.Orders.FirstOrDefault(o => o.Code == orderCode && o.UserId == userId);
            if (order == null)
                return RedirectToAction("Index");

            // Get order items manually by Code
            var orderItems = _db.OrderItems.Where(i => i.Code == order.Code).ToList();

            // Get DeliveryInfo by Code
            var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);

            return View("~/Areas/Customer/Views/Checkout/Payment.cshtml", new PaymentViewModel
            {
                Order = order,
                OrderItems = orderItems,
                DeliveryInfo = deliveryInfo,
                OrderCode = order.Code
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Payment(PaymentViewModel vm)
        {
            Console.WriteLine("HERE HERE HERE");
            if (!ModelState.IsValid)
            {
                        Console.WriteLine("END END END");
                // Log each model state error
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"ModelState Error for '{key}': {error.ErrorMessage}");
                    }
                }
                return View(vm);
            }

            var order = _db.Orders.FirstOrDefault(o => o.Code == vm.OrderCode);
            if (order == null) return RedirectToAction("Index", "Checkout");

            string paymentMethod = vm.PaymentMethod;
            string transactionId = paymentMethod == "Card" 
                ? $"TRX_{Guid.NewGuid().ToString()[..8].ToUpper()}" 
                : "Not Required";

            var payment = new Payment
            {
                Code = order.Code,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                Amount = order.TotalAmount,
                PaymentStatus = paymentMethod == "Card" ? "Paid" : "Pending",
                CreatedAt = DateTime.Now
            };

            _db.Payments.Add(payment);
            _db.SaveChanges();

            var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
            var user = _db.Users.FirstOrDefault(u => u.Id == order.UserId);

            var customerEmail = deliveryInfo?.Email ?? user?.Email ?? "";
            var customerName = deliveryInfo?.Name ?? user?.FullName ?? "Valued Customer";

            SendOrderConfirmationEmail(customerEmail, customerName, order);

            return RedirectToAction("Success", new { orderCode = order.Code });
        }

        private void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
        {
            try
            {
                string fromEmail = "foodhubwork2025@gmail.com";
                string fromPassword = "vptl evpa tvte udke";
                string subject = $"Order Confirmation - Order {order.Code}";

                // string orderUrl = $"http://localhost:5187/MyOrders/Details/{order.Code}"; // ✅ use Code
                string orderUrl = $"http://localhost:5187/MyOrders/Details?orderCode={order.Code}"; // ✅ use Code with query string

                string body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding: 30px;'>
                    <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; 
                                box-shadow: 0 4px 10px rgba(0,0,0,0.08); overflow: hidden;'>
                        <div style='background-color:#F39C12; padding:20px; text-align:center; color:white;'>
                            <h2 style='margin:0;'>FoodHub</h2>
                            <p style='margin:0; font-size:14px;'>Order Confirmation</p>
                        </div>

                        <div style='padding: 25px 30px; color: #333;'>
                            <h3 style='margin-top:0;'>Hi {customerName},</h3>
                            <p>Thank you for your order! We're preparing your food with care. 🍕</p>

                            <h4 style='margin-top:25px; color:#F39C12;'>🧾 Order Summary:</h4>
                            <table style='width:100%; border-collapse: collapse; font-size: 15px;'>
                                <tr>
                                    <td style='padding: 8px 0;'><strong>Order Code:</strong></td>
                                    <td>#{order.Code}</td>
                                </tr>
                                <tr style='background-color:#f6f6f6;'>
                                    <td style='padding: 8px 0;'><strong>Total Amount:</strong></td>
                                    <td>$. {order.TotalAmount:N2}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0;'><strong>Status:</strong></td>
                                    <td>{order.Status}</td>
                                </tr>
                                <tr style='background-color:#f6f6f6;'>
                                    <td style='padding: 8px 0;'><strong>Payment Status:</strong></td>
                                    <td>{order.Payment?.PaymentStatus ?? "Pending"}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 0;'><strong>Order Date:</strong></td>
                                    <td>{order.CreatedAt:dd-MMM-yyyy hh:mm tt}</td>
                                </tr>
                            </table>

                            <div style='text-align:center; margin: 30px 0;'>
                                <a href='{orderUrl}'
                                style='background-color:#F39C12; color:white; padding:14px 28px; 
                                        text-decoration:none; border-radius:8px; font-weight:bold;
                                        display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.2);'>
                                    🔍 View Your Order
                                </a>
                            </div>

                            <p>Your order is being processed. We'll notify you when it’s on its way! 🚚</p>

                            <p style='font-size:14px; color:#555; margin-top:25px;'>
                                Warm regards,<br/>
                                <strong>The FoodHub Team</strong>
                            </p>
                        </div>

                        <div style='background-color:#f1f1f1; text-align:center; padding:15px; font-size:12px; color:#888;'>
                            © {DateTime.Now.Year} FoodHub. All rights reserved.
                        </div>
                    </div>
                </body>
                </html>";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "FoodHub"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                smtpClient.Send(mailMessage);

                Console.WriteLine($"✅ Order confirmation email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email sending failed: {ex.Message}");
            }
        }
        
        // Step 4: Success
        [HttpGet("/Customer/Checkout/Success")]
        public IActionResult Success(string orderCode)
        {
            HttpContext.Session.Clear();
            TempData.Clear();

            var order = _db.Orders.FirstOrDefault(o => o.Code == orderCode);
            if (order == null)
                return RedirectToAction("Index");

            order.OrderItems = _db.OrderItems.Where(i => i.Code == order.Code).ToList();
            order.DeliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);

            return View(order);
        }

    }
}