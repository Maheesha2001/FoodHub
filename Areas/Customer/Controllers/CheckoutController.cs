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

        // 🛒 Step 1: Cart Review
        //     [HttpGet("/Customer/Checkout/{code?}")]
        //     public IActionResult Index(string code)
        //     {
        //         // var cartJson = HttpContext.Session.GetString("CheckoutCart");
        //         // var vm = string.IsNullOrEmpty(cartJson)
        //         //     ? new List<CartItemViewModel>()
        //         //     : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);

        //         // return View(vm);

        //          List<CartItemViewModel> vm;

        // if (!string.IsNullOrEmpty(code))
        // {
        //     // Fetch cart items from DB by Code
        //     var cartItems = _db.CartItems
        //         .Where(c => c.Code == code)
        //         .Select(i => new CartItemViewModel
        //         {
        //             Id = i.ProductId.ToString(),
        //             Name = i.ProductName,
        //             Type = i.Type,
        //             Quantity = i.Quantity,
        //             Price = i.Price
        //         }).ToList();

        //     vm = cartItems;
        //             // Optionally, store in session
        //             HttpContext.Session.SetString("CheckoutCart", JsonConvert.SerializeObject(cartItems));

        //         TempData["FrozenOrderCode"] = code;
        //     TempData.Keep("FrozenOrderCode");
        // }
        // else
        // {
        //     // Fallback: load from session
        //     var cartJson = HttpContext.Session.GetString("CheckoutCart");
        //     vm = string.IsNullOrEmpty(cartJson)
        //         ? new List<CartItemViewModel>()
        //         : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);
        // }

        // return View(vm);
        //     }


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

        // // 🚚 Step 2: Delivery Info
        // [HttpGet]
        // public async Task<IActionResult> DeliveryInfo(int? orderId)
        // {
        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     if (string.IsNullOrEmpty(userId))
        //         return RedirectToAction("Index", "Home");

        //     TempData["IsFrozen"] = true;

        //     var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
        //     if (cart == null)
        //     {
        //         var existingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Id == orderId);
        //         if (existingOrder == null)
        //             return RedirectToAction("Index", "Checkout");

        //         TempData["FrozenOrderId"] = existingOrder.Id;
        //         return View(new DeliveryInfoViewModel());
        //     }

        //     // Fetch items from CartItems table
        //     var cartItems = _db.CartItems.ToList();
        //     if (!cartItems.Any())
        //         return RedirectToAction("Index", "Checkout");

        //     // ✅ Create order
        //     var order = new Order
        //     {
        //         UserId = userId,
        //         TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
        //         Status = "Pending",
        //         CreatedAt = DateTime.Now
        //     };

        //     _db.Orders.Add(order);
        //     await _db.SaveChangesAsync();

        //     // ✅ Copy to OrderItems
        //     foreach (var item in cartItems)
        //     {
        //         _db.OrderItems.Add(new OrderItem
        //         {
        //             OrderId = order.Id,
        //             ProductId = item.ProductId,
        //             ProductName = item.ProductName,
        //             ProductType = item.Type,
        //             Quantity = item.Quantity,
        //             UnitPrice = item.Price
        //         });
        //     }

        //     // ✅ Mark cart inactive
        //     cart.Status = "Inactive";
        //     await _db.SaveChangesAsync();

        //     // ✅ Store FrozenOrderId
        //     TempData["FrozenOrderId"] = order.Id;

        //     return RedirectToAction("DeliveryInfo", new { orderId = order.Id });
        // }

        // 🚚 Step 2: Delivery Info
        // [HttpGet]
        // public async Task<IActionResult> DeliveryInfo(string code)
        // {
        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     if (string.IsNullOrEmpty(userId))
        //         return RedirectToAction("Index", "Home");

        //     TempData["IsFrozen"] = true;

        //     // Try to get active cart by code
        //     var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active" && c.Code == code);

        //     if (cart == null)
        //     {
        //         // If no active cart, try existing order by code
        //         var existingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Code == code);
        //         if (existingOrder == null)
        //             return RedirectToAction("Index", "Checkout");

        //         TempData["FrozenOrderCode"] = existingOrder.Code;
        //         return View(new DeliveryInfoViewModel());
        //     }

        //     // Fetch items from CartItems by cart code
        //     var cartItems = _db.CartItems.Where(i => i.Code == cart.Code).ToList();
        //     if (!cartItems.Any())
        //         return RedirectToAction("Index", "Checkout");

        //     // ✅ Create order with same Code
        //     var order = new Order
        //     {
        //         UserId = userId,
        //         Code = cart.Code, // Use the same code
        //         TotalAmount = cartItems.Sum(i => i.Price * i.Quantity),
        //         Status = "Pending",
        //         CreatedAt = DateTime.Now
        //     };

        //     _db.Orders.Add(order);
        //     await _db.SaveChangesAsync();

        //     // ✅ Copy items to OrderItems using same Code
        //     foreach (var item in cartItems)
        //     {
        //         _db.OrderItems.Add(new OrderItem
        //         {
        //             //OrderId = order.Id,
        //             Code = cart.Code, // same code for traceability
        //             ProductId = item.ProductId,
        //             ProductName = item.ProductName,
        //             ProductType = item.Type,
        //             Quantity = item.Quantity,
        //             UnitPrice = item.Price
        //         });
        //     }

        //     // ✅ Mark cart as inactive
        //     cart.Status = "Inactive";
        //     await _db.SaveChangesAsync();

        //     // ✅ Store FrozenOrderCode for view
        //     TempData["FrozenOrderCode"] = order.Code;

        //     // ✅ Redirect to DeliveryInfo using Code instead of orderId
        //     return RedirectToAction("DeliveryInfo", new { code = order.Code });
        // }

        //         [HttpGet]
        // public async Task<IActionResult> DeliveryInfo(string code)
        // {
        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     if (string.IsNullOrEmpty(userId))
        //         return RedirectToAction("Index", "Home");

        //     TempData["IsFrozen"] = true;

        //     if (string.IsNullOrEmpty(code) && TempData["FrozenOrderCode"] != null)
        //     {
        //         code = TempData["FrozenOrderCode"].ToString();
        //         TempData.Keep("FrozenOrderCode");
        //     }

        //     var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active" && c.Code == code);
        //     if (cart == null)
        //     {
        //         var existingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Code == code);
        //         if (existingOrder == null)
        //             return RedirectToAction("Index", "Checkout");

        //         TempData["FrozenOrderCode"] = existingOrder.Code;
        //         return View(new DeliveryInfoViewModel());
        //     }

        //     // Create order logic here...
        //     TempData["FrozenOrderCode"] = cart.Code;
        //     TempData.Keep("FrozenOrderCode");

        //     return View(new DeliveryInfoViewModel());
        // }
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


        //         [HttpPost]
        //         [ValidateAntiForgeryToken]
        //         public IActionResult DeliveryInfo(int orderId, DeliveryInfoViewModel model)
        //         {
        //             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //             var pendingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
        //             if (pendingOrder == null)
        //                 return RedirectToAction("Cart");

        //             var existing = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == pendingOrder.Id);
        //             if (existing != null)
        //             {
        //                 existing.Name = model.Name;
        //                 existing.Email = model.Email;
        //                 existing.Phone = model.Phone;
        //                 existing.Address = model.Address;
        //                 existing.DeliveryNotes = model.DeliveryNotes;
        //             }
        //             else
        //             {
        //                 _db.DeliveryInfo.Add(new DeliveryInfo
        //                 {
        //                     OrderId = pendingOrder.Id,
        //                     Name = model.Name,
        //                     Email = model.Email,
        //                     Phone = model.Phone,
        //                     Address = model.Address,
        //                     DeliveryNotes = model.DeliveryNotes,
        //                     DeliveryStatus = "Pending"
        //                 });
        //             }

        //             _db.SaveChanges();
        //             return RedirectToAction("Payment", new { orderId = pendingOrder.Id });
        //         }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeliveryInfo(DeliveryInfoViewModel model)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine("model.OrderCode is here" +model.OrderCode);
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

        //    [HttpGet("/Customer/Checkout/Payment")]
        //     public IActionResult Payment(string orderCode)
        //     {
        //         Console.WriteLine("Order code in the payment is --> "+orderCode );
        //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //         // Find the order by Code instead of Id
        //         var order = _db.Orders
        //             .Include(o => o.OrderItems)
        //             .FirstOrDefault(o => o.Code == orderCode && o.UserId == userId);

        //         if (order == null)
        //             return RedirectToAction("Index");

        //         // Get DeliveryInfo by Code
        //         var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);

        //        return View("~/Areas/Customer/Views/Checkout/Payment.cshtml", new PaymentViewModel
        //         {
        //             Order = order,
        //             OrderItems = order.OrderItems.ToList(),
        //             DeliveryInfo = deliveryInfo
        //         });
        //     }

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


        //         [HttpPost]
        // public IActionResult Payment(PaymentViewModel vm)
        // {
        //     if (!ModelState.IsValid)
        //         return View(vm);

        //     // Fetch order by Code instead of Id
        //     var order = _db.Orders
        //         .Include(o => o.OrderItems)
        //         .Include(o => o.DeliveryInfo)
        //         .Include(o => o.User)
        //         .FirstOrDefault(o => o.Code == vm.Order.Code);

        //     if (order == null)
        //         return RedirectToAction("Index", "Checkout");

        //     string paymentMethod = Request.Form["PaymentMethod"];
        //     if (string.IsNullOrEmpty(paymentMethod))
        //         paymentMethod = "Card";

        //     string transactionId = paymentMethod == "Card"
        //         ? $"TRX_{Guid.NewGuid().ToString()[..8].ToUpper()}"
        //         : "Not Required";

        //     var payment = new Payment
        //     {
        //         Code = order.Code, // ✅ use Code
        //         PaymentMethod = paymentMethod,
        //         TransactionId = transactionId,
        //         Amount = order.TotalAmount,
        //         PaymentStatus = paymentMethod == "Card" ? "Paid" : "Pending",
        //         CreatedAt = DateTime.Now
        //     };

        //     _db.Payments.Add(payment);
        //     _db.SaveChanges();

        //     var customerEmail = !string.IsNullOrEmpty(order.DeliveryInfo?.Email)
        //         ? order.DeliveryInfo.Email
        //         : order.User?.Email ?? string.Empty;

        //     var customerName = !string.IsNullOrEmpty(order.DeliveryInfo?.Name)
        //         ? order.DeliveryInfo.Name
        //         : order.User?.FullName ?? "Valued Customer";

        //     Console.WriteLine($"[Payment Debug] Customer Name: {customerName}, Email: {customerEmail}");

        //     // Send email using Code instead of Id
        //     SendOrderConfirmationEmail(customerEmail, customerName, order);

        //     return RedirectToAction("Success", new { orderCode = order.Code });
        // }

// [HttpPost]
// [ValidateAntiForgeryToken]
// public IActionResult Payment(PaymentViewModel vm)
//         {
//             Console.WriteLine("HERE HERE HERE");
//     if (!ModelState.IsValid)
//             {
//                   Console.WriteLine("END END END");
//                 return View(vm);
                

//             }
        
//     // Fetch order by Code
//     var order = _db.Orders.FirstOrDefault(o => o.Code == vm.Order.Code);
//     if (order == null)
//     {
//         Console.WriteLine("⚠️ Order not found for Code: " + vm.Order?.Code);
//         return RedirectToAction("Index", "Checkout");
//     }

//     // Determine payment method
//     string paymentMethod = vm.PaymentMethod ?? "Card";

//     // Create Transaction ID
//     string transactionId = paymentMethod == "Card"
//         ? $"TRX_{Guid.NewGuid().ToString()[..8].ToUpper()}"
//         : "Not Required";

//     var payment = new Payment
//     {
//         Code = order.Code,
//         PaymentMethod = paymentMethod,
//         TransactionId = transactionId,
//         Amount = order.TotalAmount,
//         PaymentStatus = paymentMethod == "Card" ? "Paid" : "Pending",
//         CreatedAt = DateTime.Now
//     };

//     _db.Payments.Add(payment);
//     _db.SaveChanges();

//     // Optional email sending
//     var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.Code == order.Code);
//     var user = _db.Users.FirstOrDefault(u => u.Id == order.UserId);

//     var customerEmail = deliveryInfo?.Email ?? user?.Email ?? "";
//     var customerName = deliveryInfo?.Name ?? user?.FullName ?? "Valued Customer";

//     Console.WriteLine($"[Payment Debug] Saved Payment for {customerName} ({customerEmail})");

//     SendOrderConfirmationEmail(customerEmail, customerName, order);

//     return RedirectToAction("Success", new { orderCode = order.Code });
// }
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
                string subject = $"Order Confirmation - Order #{order.Code}";

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


        //         // 💳 Step 3: Payment
        //         [HttpGet]
        //         public IActionResult Payment(int orderId)
        //         {
        //             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //             var order = _db.Orders
        //                 .Include(o => o.OrderItems)
        //                 .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

        //             if (order == null)
        //                 return RedirectToAction("Index");

        //             var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == order.Id);

        //             return View(new PaymentViewModel
        //             {
        //                 Order = order,
        //                 OrderItems = order.OrderItems.ToList(),
        //                 DeliveryInfo = deliveryInfo
        //             });
        //         }

        //         [HttpPost]
        //         public IActionResult Payment(PaymentViewModel vm)
        //         {
        //             if (!ModelState.IsValid)
        //                 return View(vm);

        //             var order = _db.Orders
        //             .Include(o => o.OrderItems)
        //                 .Include(o => o.DeliveryInfo)  // ✅ Include delivery info
        //                 .Include(o => o.User)          // ✅ Include user
        //             .FirstOrDefault(o => o.Id == vm.Order.Id);
        //             if (order == null)
        //                 return RedirectToAction("Index", "Checkout");

        //             string paymentMethod = Request.Form["PaymentMethod"];
        //             if (string.IsNullOrEmpty(paymentMethod))
        //                 paymentMethod = "Card";

        //             string transactionId = paymentMethod == "Card"
        //                 ? $"TRX_{Guid.NewGuid().ToString()[..8].ToUpper()}"
        //                 : "Not Required";

        //             var payment = new Payment
        //             {
        //                 OrderId = order.Id,
        //                 PaymentMethod = paymentMethod,
        //                 TransactionId = transactionId,
        //                 Amount = order.TotalAmount,
        //                 PaymentStatus = paymentMethod == "Card" ? "Paid" : "Pending",
        //                 CreatedAt = DateTime.Now
        //             };

        //             _db.Payments.Add(payment);
        //             _db.SaveChanges();

        //           var customerEmail = !string.IsNullOrEmpty(order.DeliveryInfo?.Email)
        //     ? order.DeliveryInfo.Email
        //     : order.User?.Email ?? string.Empty;

        // var customerName = !string.IsNullOrEmpty(order.DeliveryInfo?.Name)
        //     ? order.DeliveryInfo.Name
        //     : order.User?.FullName ?? "Valued Customer";

        // // ✅ Debug output
        // Console.WriteLine($"[Payment Debug] Customer Name: {customerName}, Email: {customerEmail}");

        //             // ✅ Send confirmation email
        //             SendOrderConfirmationEmail(customerEmail, customerName, order);


        //             return RedirectToAction("Success", new { orderId = order.Id });
        //         }

        //         // 📧 Email
        //         // private void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
        //         // {
        //         //     try
        //         //     {
        //         //         string fromEmail = "foodhubwork2025@gmail.com";
        //         //         string fromPassword = "vptl evpa tvte udke";

        //         //         string orderUrl = $"http://localhost:5187/MyOrders/Details/{order.Id}";

        //         //         string body = $@"
        //         //         <html><body style='font-family:Arial'>
        //         //         <h3>Hi {customerName},</h3>
        //         //         <p>Your order #{order.Id} has been received successfully.</p>
        //         //         <p>Total: Rs. {order.TotalAmount:N2}</p>
        //         //         <p><a href='{orderUrl}'>View Order Details</a></p>
        //         //         </body></html>";

        //         //         var smtp = new SmtpClient("smtp.gmail.com")
        //         //         {
        //         //             Port = 587,
        //         //             Credentials = new NetworkCredential(fromEmail, fromPassword),
        //         //             EnableSsl = true
        //         //         };

        //         //         var msg = new MailMessage(fromEmail, toEmail, $"Order Confirmation #{order.Id}", body)
        //         //         {
        //         //             IsBodyHtml = true
        //         //         };

        //         //         smtp.Send(msg);
        //         //         Console.WriteLine($"✅ Email sent to {toEmail}");
        //         //     }
        //         //     catch (Exception ex)
        //         //     {
        //         //         Console.WriteLine($"❌ Email failed: {ex.Message}");
        //         //     }
        //         // }
        //  private void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
        //         {
        //             try
        //             {
        //                 string fromEmail = "foodhubwork2025@gmail.com"; // sender email
        //                 string fromPassword = "vptl evpa tvte udke";    // app-specific password
        //                 string subject = $"Order Confirmation - Order #{order.Id}";

        //                 // ✅ Local Admin Order Details URL
        //                 string orderUrl = $"http://localhost:5187/MyOrders/Details/{order.Id}";


        //                 // ✅ Styled Email Body
        //                 string body = $@"
        //                 <html>
        //                 <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding: 30px;'>
        //                     <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; 
        //                                 box-shadow: 0 4px 10px rgba(0,0,0,0.08); overflow: hidden;'>
        //                         <div style='background-color:#F39C12; padding:20px; text-align:center; color:white;'>
        //                             <h2 style='margin:0;'>FoodHub</h2>
        //                             <p style='margin:0; font-size:14px;'>Order Confirmation</p>
        //                         </div>

        //                         <div style='padding: 25px 30px; color: #333;'>
        //                             <h3 style='margin-top:0;'>Hi {customerName},</h3>
        //                             <p>Thank you for your order! We're preparing your food with care. 🍕</p>

        //                             <h4 style='margin-top:25px; color:#F39C12;'>🧾 Order Summary:</h4>
        //                             <table style='width:100%; border-collapse: collapse; font-size: 15px;'>
        //                                 <tr>
        //                                     <td style='padding: 8px 0;'><strong>Order ID:</strong></td>
        //                                     <td>#{order.Id}</td>
        //                                 </tr>
        //                                 <tr style='background-color:#f6f6f6;'>
        //                                     <td style='padding: 8px 0;'><strong>Total Amount:</strong></td>
        //                                     <td>$. {order.TotalAmount:N2}</td>
        //                                 </tr>
        //                                 <tr>
        //                                     <td style='padding: 8px 0;'><strong>Status:</strong></td>
        //                                     <td>{order.Status}</td>
        //                                 </tr>
        //                                 <tr style='background-color:#f6f6f6;'>
        //                                     <td style='padding: 8px 0;'><strong>Payment Status:</strong></td>
        //                                     <td>{order.Payment?.PaymentStatus ?? "Pending"}</td>
        //                                 </tr>
        //                                 <tr>
        //                                     <td style='padding: 8px 0;'><strong>Order Date:</strong></td>
        //                                     <td>{order.CreatedAt:dd-MMM-yyyy hh:mm tt}</td>
        //                                 </tr>
        //                             </table>

        //                             <div style='text-align:center; margin: 30px 0;'>
        //                                 <a href='{orderUrl}'
        //                                 style='background-color:#F39C12; color:white; padding:14px 28px; 
        //                                         text-decoration:none; border-radius:8px; font-weight:bold;
        //                                         display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.2);'>
        //                                     🔍 View Your Order
        //                                 </a>
        //                             </div>

        //                             <p>Your order is being processed. We'll notify you when it’s on its way! 🚚</p>

        //                             <p style='font-size:14px; color:#555; margin-top:25px;'>
        //                                 Warm regards,<br/>
        //                                 <strong>The FoodHub Team</strong>
        //                             </p>
        //                         </div>

        //                         <div style='background-color:#f1f1f1; text-align:center; padding:15px; font-size:12px; color:#888;'>
        //                             © {DateTime.Now.Year} FoodHub. All rights reserved.
        //                         </div>
        //                     </div>
        //                 </body>
        //                 </html>
        //                 ";

        //                 // ✅ SMTP client setup
        //                 var smtpClient = new SmtpClient("smtp.gmail.com")
        //                 {
        //                     Port = 587,
        //                     Credentials = new NetworkCredential(fromEmail, fromPassword),
        //                     EnableSsl = true,
        //                 };

        //                 var mailMessage = new MailMessage
        //                 {
        //                     From = new MailAddress(fromEmail, "FoodHub"),
        //                     Subject = subject,
        //                     Body = body,
        //                     IsBodyHtml = true,
        //                 };

        //                 mailMessage.To.Add(toEmail);

        //                 smtpClient.Send(mailMessage);
        //                 Console.WriteLine($"✅ Order confirmation email sent to {toEmail}");
        //             }
        //             catch (Exception ex)
        //             {
        //                 Console.WriteLine($"❌ Email sending failed: {ex.Message}");
        //             }
        //         }


        //         // ✅ Step 4: Success Page
        //         public IActionResult Success(int orderId)
        //         {
        //             HttpContext.Session.Clear();
        //             TempData.Clear();

        //             var order = _db.Orders
        //                 .Include(o => o.OrderItems)
        //                 .Include(o => o.DeliveryInfo)
        //                 .FirstOrDefault(o => o.Id == orderId);

        //             if (order == null)
        //                 return RedirectToAction("Index");

        //             return View(order);
        //         }

    }
}



// using FoodHub.Data;
// using FoodHub.Models;
// using FoodHub.ViewModels.Checkout;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using Newtonsoft.Json;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
// using System.Net;
// using System.Net.Mail;


// namespace FoodHub.Areas.Customer.Controllers
// {
//     [Area("Customer")]
//     [Authorize]
//     public class CheckoutController : Controller
//     {
//         private readonly FoodHubContext _db;

//         public CheckoutController(FoodHubContext db)
//         {
//             _db = db;
//         }

//         // 🛒 Step 1: Cart Review
//         public IActionResult Index()
//         {
//             var cartJson = HttpContext.Session.GetString("CheckoutCart");
//             var vm = string.IsNullOrEmpty(cartJson)
//                 ? new List<CartItemViewModel>()
//                 : JsonConvert.DeserializeObject<List<CartItemViewModel>>(cartJson);

//             return View(vm);
//         }

//         //    [HttpGet]
//         //     public IActionResult IsCartFrozen()
//         //     {
//         //         var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         //         if (string.IsNullOrEmpty(userId)) return Json(new { frozen = false });

//         //         // // Check if there's already an Order with Pending status
//         //         // var pendingOrder = _db.Orders.Any(o => o.UserId == userId && o.Status == "Pending");

//         //         // return Json(new { frozen = pendingOrder });

//         //           // ✅ Check if there's an order that has a DeliveryInfo record with status "Pending" or "Out for Delivery"
//         // var hasDeliveryInProgress = _db.Orders
//         //     .Any(o => o.UserId == userId &&
//         //               o.DeliveryInfo != null &&
//         //               (o.DeliveryInfo.DeliveryStatus == "Pending" || 
//         //                o.DeliveryInfo.DeliveryStatus == "Out for Delivery"));

//         // return Json(new { frozen = hasDeliveryInProgress });
//         //     }

//         // [HttpGet]
//         // public IActionResult IsCartFrozen()
//         // {
//         //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         //     if (string.IsNullOrEmpty(userId))
//         //         return Json(new { frozen = false });

//         //     // ✅ If DeliveryInfo has been reached, mark frozen = true
//         //     if (TempData["IsFrozen"] != null)
//         //     {
//         //         TempData.Keep("IsFrozen"); // Keep for next request
//         //         return Json(new { frozen = true });
//         //     }

//         //     // (Optional fallback check — only runs if IsFrozen not set)
//         //     var hasDeliveryInProgress = _db.Orders.Any(o => o.UserId == userId &&
//         //                                                     o.DeliveryInfo != null &&
//         //                                                     (o.DeliveryInfo.DeliveryStatus == "Pending" ||
//         //                                                      o.DeliveryInfo.DeliveryStatus == "Out for Delivery"));

//         //     return Json(new { frozen = hasDeliveryInProgress });
//         // }


//         public IActionResult IsCartFrozen()
// {
//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//     if (string.IsNullOrEmpty(userId))
//         return Json(new { frozen = false });

//     // ✅ Return true ONLY if user actually reached DeliveryInfo in this session
//     if (TempData.ContainsKey("IsFrozen") && TempData["IsFrozen"] is bool frozen && frozen)
//     {
//         TempData.Keep("IsFrozen"); // Keep alive for next requests in checkout flow
//         return Json(new { frozen = true });
//     }

//     // ✅ If not explicitly set, make sure it's cleared
//     TempData.Remove("IsFrozen");

//     // ❌ Otherwise: Not frozen
//     return Json(new { frozen = false });
// }


//         // 🧮 Update Cart Quantities
//         [HttpPost]
//         public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
//         {

//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             if (userId == null) return Unauthorized();

//             var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId);
//             if (cart == null) return NotFound();

//             var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
//             if (item == null) return NotFound();

//             item.Quantity = quantity;
//             _db.SaveChanges();

//             return Json(new { success = true, total = item.Price * item.Quantity });
//         }
//         //-----------------------------------------------------------------------------------------------------------------------------
//         // 🚚 Step 2: Delivery Info
//         // // GET: Delivery Info (auto-create order)
//         // [HttpGet]
//         // public async Task<IActionResult> DeliveryInfo(int? orderId)
//         // {
//         //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//         //      if (string.IsNullOrEmpty(userId))
//         // return RedirectToAction("Index", "Home");

//         //     var cart = _db.Carts.Include(c => c.Items)
//         //                         .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

//         //         Order order = null;
//         //     if (cart != null && cart.Items.Any())
//         //     {
//         //         // ✅ Step 1: Create Order
//         //          order = new Order
//         //         {
//         //             UserId = userId,
//         //             TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
//         //             Status = "Pending",
//         //             CreatedAt = DateTime.Now
//         //         };
//         //         _db.Orders.Add(order);
//         //         await _db.SaveChangesAsync();

//         //         // ✅ Step 2: Copy CartItems to OrderItems
//         //         foreach (var item in cart.Items)
//         //         {
//         //             _db.OrderItems.Add(new OrderItem
//         //             {
//         //                 OrderId = order.Id,
//         //                 ProductId = item.ProductId,
//         //                 ProductName = item.ProductName,
//         //                 ProductType = item.Type,
//         //                 Quantity = item.Quantity,
//         //                 UnitPrice = item.Price
//         //             });
//         //         }

//         //         // ✅ Step 3: Mark Cart as Inactive
//         //         cart.Status = "Inactive";
//         //         await _db.SaveChangesAsync();

//         //         // ✅ Step 4: Clear all relevant session data
//         //         HttpContext.Session.Remove("CheckoutCart");
//         //         HttpContext.Session.Remove("SomeOtherCartRelatedKey"); // optional if you store more
//         //     }
//         //     else
//         //     {
//         //         // ✅ If orderId exists, retrieve that order
//         //         order = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Id == orderId);
//         //     }

//         //     // ✅ Step 5: Store FrozenOrderId for DeliveryInfo flow
//         //     var frozenOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
//         //     TempData["FrozenOrderId"] = frozenOrder != null ? frozenOrder.Id : 0;

//         //     //return View(new DeliveryInfoViewModel()); 
//         //     // ✅ Redirect with orderId in URL
//         //     return RedirectToAction("DeliveryInfo", new { orderId = order.Id });
//         // }

//         [HttpGet]
// public async Task<IActionResult> DeliveryInfo(int? orderId)
//         {
    
//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             if (string.IsNullOrEmpty(userId))
//                 return RedirectToAction("Index", "Home");
        
//             TempData["IsFrozen"] = true;
    
//     var cart = _db.Carts
//         .Include(c => c.Items)
//         .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

//     Order order = null;

//     // ✅ Step 1: Create new order if user has an active cart
//     if (cart != null && cart.Items.Any())
//     {
//         order = new Order
//         {
//             UserId = userId,
//             TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
//             Status = "Pending",
//             CreatedAt = DateTime.Now
//         };
//         _db.Orders.Add(order);
//         await _db.SaveChangesAsync();

//         // ✅ Step 2: Copy items into OrderItems
//         foreach (var item in cart.Items)
//         {
//             _db.OrderItems.Add(new OrderItem
//             {
//                 OrderId = order.Id,
//                 ProductId = item.ProductId,
//                 ProductName = item.ProductName,
//                 ProductType = item.Type,
//                 Quantity = item.Quantity,
//                 UnitPrice = item.Price
//             });
//         }

//         // ✅ Step 3: Mark cart inactive and save
//         cart.Status = "Inactive";
//         await _db.SaveChangesAsync();

//         // ✅ Step 4: Clear session-based checkout cart if any
//         HttpContext.Session.Remove("CheckoutCart");
//         HttpContext.Session.Remove("SomeOtherCartRelatedKey");

//         // ✅ Step 5: Store FrozenOrderId for DeliveryInfo flow
//         var frozenOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
//         TempData["FrozenOrderId"] = frozenOrder != null ? frozenOrder.Id : 0;

//         // ✅ Redirect only once (after order creation)
//         return RedirectToAction("DeliveryInfo", new { orderId = order.Id });
//     }
//     else
//     {
//         // ✅ If no active cart, check if order already exists
//         order = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Id == orderId);
//     }

//     if (order == null)
//         return RedirectToAction("Index", "Checkout");

//     // ✅ Step 5 (keep this line even for returning users)
//     var existingFrozenOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
//     TempData["FrozenOrderId"] = existingFrozenOrder != null ? existingFrozenOrder.Id : 0;

//     // ✅ Return the view for delivery info input
//     return View(new DeliveryInfoViewModel());
// }

        
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public IActionResult DeliveryInfo(int orderId, DeliveryInfoViewModel model)
//         {

//             // Log form input
//             Console.WriteLine($"Model: Name={model.Name}, Email={model.Email}, Phone={model.Phone}, Address={model.Address}, DeliveryNotes={model.DeliveryNotes}");

//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
//             var pendingOrder = _db.Orders.FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");
//             if (pendingOrder == null)
//                 return RedirectToAction("Cart");

//             var existing = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == pendingOrder.Id);

//             if (existing != null)
//             {
//                 existing.Name = model.Name;
//                 existing.Email = model.Email;
//                 existing.Phone = model.Phone;
//                 existing.Address = model.Address;
//                 existing.DeliveryNotes = model.DeliveryNotes;
//             }
//             else
//             {
//                 var deliveryInfo = new DeliveryInfo
//                 {
//                     OrderId = pendingOrder.Id,
//                     Name = model.Name,
//                     Email = model.Email,
//                     Phone = model.Phone,
//                     Address = model.Address,
//                     DeliveryNotes = model.DeliveryNotes,
//                     DeliveryStatus = "Pending"
//                 };
//                 _db.DeliveryInfo.Add(deliveryInfo);
//             }

           

//             _db.SaveChanges();
//             Console.WriteLine("✅ DeliveryInfo saved successfully.");

//             // return RedirectToAction("Payment");
//             // return RedirectToAction("Payment", new { orderId = pendingOrder.Id });
//             //return RedirectToAction("Payment", "Checkout", new { area = "Customer", orderId = pendingOrder.Id });
//             return RedirectToAction("Payment", new { orderId = pendingOrder.Id });


//         }

//         [HttpGet]
//         public IActionResult Payment(int orderId)
//         {
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
//             // 1️⃣ Find the pending order for this user
//             var order = _db.Orders
//                 .Include(o => o.OrderItems)
//                 .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);
//                // .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

//             if (order == null)
//                 return RedirectToAction("Index");

//             // 2️⃣ Find delivery info for this order
//             var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == order.Id);

//             // 3️⃣ Prepare ViewModel
//             var viewModel = new PaymentViewModel
//             {
//                 Order = order,
//                 OrderItems = order.OrderItems.ToList(),
//                 DeliveryInfo = deliveryInfo
//             };

//             // 4️⃣ Pass to the view
//             return View(viewModel);
//         }

//         // [HttpPost]
//         // public IActionResult Payment(PaymentViewModel vm)
//         // {
//         //     if (!ModelState.IsValid)
//         //         return View(vm);

//         //     var cartJson = HttpContext.Session.GetString("CheckoutCart");
//         //     var cart = string.IsNullOrEmpty(cartJson)
//         //         ? new List<CartItemViewModel>()
//         //         : System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);

//         //     var deliveryJson = TempData["DeliveryInfo"] as string ?? "{}";
//         //     var deliveryInfoVm = System.Text.Json.JsonSerializer.Deserialize<DeliveryInfoViewModel>(deliveryJson);

//         //     if (cart == null || cart.Count == 0 || deliveryInfoVm == null)
//         //         return RedirectToAction("Index");


//         //     // 💳 Determine payment method
//         //     string paymentMethod = Request.Form["PaymentMethod"];
//         //     if (string.IsNullOrEmpty(paymentMethod))
//         //         paymentMethod = "Card"; // default fallback

//         //     string transactionId = "Not Required";
//         //     if (paymentMethod == "Card")        // ⚙️ Simulate Transaction ID
//         //         transactionId = $"TRX_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

//         //         // 💰 Log before creating payment record
//         //         Console.WriteLine($"[Payment Debug] OrderId: {vm?.Order?.Id}, Amount: {vm?.Order?.TotalAmount}, Method: {paymentMethod}, Transaction: {transactionId}");


//         //     // 💰 Create payment record
//         //     var payment = new Payment
//         //     {
//         //         OrderId = vm.Order!.Id, // Use the Order from the ViewModel
//         //         PaymentMethod = paymentMethod,
//         //         TransactionId = transactionId,
//         //         Amount = vm.Order.TotalAmount,
//         //         PaymentStatus = paymentMethod == "Card" ? "Success" : "Pending",
//         //         CreatedAt = DateTime.Now
//         //     };

//         //             _db.Payments.Add(payment);

//         //             // ✅ Update order status to Completed
//         //             var order = _db.Orders.FirstOrDefault(o => o.Id == vm.Order.Id);
//         //             if (order != null)
//         //             {
//         //                 order.Status = "Completed";
//         //                 _db.Orders.Update(order);
//         //             }


//         //             _db.SaveChanges();


//         //     return RedirectToAction("Success", new { orderId = vm.Order.Id });
//         // }

//         [HttpPost]
//         public IActionResult Payment(PaymentViewModel vm)
//         {
//             if (!ModelState.IsValid)
//                 return View(vm);

//             // 🧾 1️⃣ Load Order directly from DB using the OrderId from the form
//             var order = _db.Orders
//                 .Include(o => o.OrderItems)
//                 .FirstOrDefault(o => o.Id == vm.Order.Id);

//             if (order == null)
//             {
//                 Console.WriteLine("[Payment Error] Order not found.");
//                 return RedirectToAction("Index", "Checkout");
//             }

//             // 🚚 2️⃣ Ensure delivery info exists
//             var deliveryInfo = _db.DeliveryInfo.FirstOrDefault(d => d.OrderId == order.Id);
//             if (deliveryInfo == null)
//             {
//                 Console.WriteLine("[Payment Error] Delivery info missing.");
//                 return RedirectToAction("DeliveryInfo", new { orderId = order.Id });
//             }

//             // 💳 3️⃣ Determine payment method (from hidden field)
//             string paymentMethod = Request.Form["PaymentMethod"];
//             if (string.IsNullOrEmpty(paymentMethod))
//                 paymentMethod = "Card"; // fallback to card

//             // 🧾 4️⃣ Simulate Transaction ID (for card payments)
//             string transactionId = paymentMethod == "Card"
//                 ? $"TRX_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"
//                 : "Not Required";

//             Console.WriteLine($"[Payment Debug] OrderId: {order.Id}, Amount: {order.TotalAmount}, Method: {paymentMethod}, Transaction: {transactionId}");

//             // 💰 5️⃣ Create and save payment record
//             var payment = new Payment
//             {
//                 OrderId = order.Id,
//                 PaymentMethod = paymentMethod,
//                 TransactionId = transactionId,
//                 Amount = order.TotalAmount,
//                 PaymentStatus = paymentMethod == "Card" ? "Paid" : "Pending",
//                 CreatedAt = DateTime.Now
//             };

//             _db.Payments.Add(payment);

//             // 🟢 6️⃣ Update order status
//             // order.Status = "Completed";
//             // _db.Orders.Update(order);

//             // 🧹 7️⃣ Mark cart as inactive (if applicable)
//             var cart = _db.Carts.FirstOrDefault(c => c.UserId == order.UserId && c.Status == "Active");
//             if (cart != null)
//             {
//                 cart.Status = "Inactive";
//                 _db.Carts.Update(cart);
//             }

//             // 💾 8️⃣ Save all changes
//             _db.SaveChanges();

//             // 🧼 9️⃣ Clear checkout session completely
//             HttpContext.Session.Remove("CheckoutCart");

//             // Safely get the email and name from DeliveryInfo first, otherwise fallback to User
//             var customerEmail = !string.IsNullOrEmpty(order.DeliveryInfo?.Email)
//                 ? order.DeliveryInfo.Email
//                 : order.User?.Email ?? string.Empty;

//             var customerName = !string.IsNullOrEmpty(order.DeliveryInfo?.Name)
//                 ? order.DeliveryInfo.Name
//                 : order.User?.FullName ?? "Valued Customer";

//             // Send the email
//             SendOrderConfirmationEmail(customerEmail, customerName, order);

//             // ✅ 10️⃣ Redirect to success page
//             return RedirectToAction("Success", new { orderId = order.Id });
//         }
//         private void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
//         {
//             try
//             {
//                 string fromEmail = "foodhubwork2025@gmail.com"; // sender email
//                 string fromPassword = "vptl evpa tvte udke";    // app-specific password
//                 string subject = $"Order Confirmation - Order #{order.Id}";

//                 // ✅ Local Admin Order Details URL
//                 string orderUrl = $"http://localhost:5187/MyOrders/Details/{order.Id}";


//                 // ✅ Styled Email Body
//                 string body = $@"
//                 <html>
//                 <body style='font-family: Arial, sans-serif; background-color:#f9f9f9; padding: 30px;'>
//                     <div style='max-width: 600px; margin: 0 auto; background: white; border-radius: 10px; 
//                                 box-shadow: 0 4px 10px rgba(0,0,0,0.08); overflow: hidden;'>
//                         <div style='background-color:#F39C12; padding:20px; text-align:center; color:white;'>
//                             <h2 style='margin:0;'>FoodHub</h2>
//                             <p style='margin:0; font-size:14px;'>Order Confirmation</p>
//                         </div>

//                         <div style='padding: 25px 30px; color: #333;'>
//                             <h3 style='margin-top:0;'>Hi {customerName},</h3>
//                             <p>Thank you for your order! We're preparing your food with care. 🍕</p>

//                             <h4 style='margin-top:25px; color:#F39C12;'>🧾 Order Summary:</h4>
//                             <table style='width:100%; border-collapse: collapse; font-size: 15px;'>
//                                 <tr>
//                                     <td style='padding: 8px 0;'><strong>Order ID:</strong></td>
//                                     <td>#{order.Id}</td>
//                                 </tr>
//                                 <tr style='background-color:#f6f6f6;'>
//                                     <td style='padding: 8px 0;'><strong>Total Amount:</strong></td>
//                                     <td>$. {order.TotalAmount:N2}</td>
//                                 </tr>
//                                 <tr>
//                                     <td style='padding: 8px 0;'><strong>Status:</strong></td>
//                                     <td>{order.Status}</td>
//                                 </tr>
//                                 <tr style='background-color:#f6f6f6;'>
//                                     <td style='padding: 8px 0;'><strong>Payment Status:</strong></td>
//                                     <td>{order.Payment?.PaymentStatus ?? "Pending"}</td>
//                                 </tr>
//                                 <tr>
//                                     <td style='padding: 8px 0;'><strong>Order Date:</strong></td>
//                                     <td>{order.CreatedAt:dd-MMM-yyyy hh:mm tt}</td>
//                                 </tr>
//                             </table>

//                             <div style='text-align:center; margin: 30px 0;'>
//                                 <a href='{orderUrl}'
//                                 style='background-color:#F39C12; color:white; padding:14px 28px; 
//                                         text-decoration:none; border-radius:8px; font-weight:bold;
//                                         display:inline-block; box-shadow:0 3px 6px rgba(0,0,0,0.2);'>
//                                     🔍 View Your Order
//                                 </a>
//                             </div>

//                             <p>Your order is being processed. We'll notify you when it’s on its way! 🚚</p>

//                             <p style='font-size:14px; color:#555; margin-top:25px;'>
//                                 Warm regards,<br/>
//                                 <strong>The FoodHub Team</strong>
//                             </p>
//                         </div>

//                         <div style='background-color:#f1f1f1; text-align:center; padding:15px; font-size:12px; color:#888;'>
//                             © {DateTime.Now.Year} FoodHub. All rights reserved.
//                         </div>
//                     </div>
//                 </body>
//                 </html>
//                 ";

//                 // ✅ SMTP client setup
//                 var smtpClient = new SmtpClient("smtp.gmail.com")
//                 {
//                     Port = 587,
//                     Credentials = new NetworkCredential(fromEmail, fromPassword),
//                     EnableSsl = true,
//                 };

//                 var mailMessage = new MailMessage
//                 {
//                     From = new MailAddress(fromEmail, "FoodHub"),
//                     Subject = subject,
//                     Body = body,
//                     IsBodyHtml = true,
//                 };

//                 mailMessage.To.Add(toEmail);

//                 smtpClient.Send(mailMessage);
//                 Console.WriteLine($"✅ Order confirmation email sent to {toEmail}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Email sending failed: {ex.Message}");
//             }
//         }


//         // private void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
//         // {
//         //     try
//         //     {
//         //         string fromEmail = "foodhubwork2025@gmail.com"; // replace with your sender email
//         //         string fromPassword = "vptl evpa tvte udke";     // app-specific password (not your main password)
//         //         string subject = $"Order Confirmation - Order #{order.Id}";

//         //         // ✅ Build message body
//         //         string body = $@"
//         //             <h2>Thank you for your order, {customerName}!</h2>
//         //             <p>Your order has been successfully placed and is being processed.</p>

//         //             <h4>Order Details:</h4>
//         //             <ul>
//         //                 <li><strong>Order ID:</strong> {order.Id}</li>
//         //                 <li><strong>Total Amount:</strong> Rs. {order.TotalAmount:N2}</li>
//         //                 <li><strong>Status:</strong> {order.Status}</li>
//         //                 <li><strong>Payment Status:</strong> {order.Payment?.PaymentStatus ?? "Pending"}</li>
//         //                 <li><strong>Order Date:</strong> {order.CreatedAt:dd-MMM-yyyy hh:mm tt}</li>
//         //             </ul>

//         //             <p>We'll notify you once your order is on its way! 🚚</p>
//         //             <br/>
//         //             <p>– The FoodHub Team</p>
//         //         ";

//         //         // ✅ SMTP client setup
//         //         var smtpClient = new SmtpClient("smtp.gmail.com")
//         //         {
//         //             Port = 587,
//         //             Credentials = new NetworkCredential(fromEmail, fromPassword),
//         //             EnableSsl = true,
//         //         };

//         //         var mailMessage = new MailMessage
//         //         {
//         //             From = new MailAddress(fromEmail, "FoodHub"),
//         //             Subject = subject,
//         //             Body = body,
//         //             IsBodyHtml = true,
//         //         };

//         //         mailMessage.To.Add(toEmail);

//         //         smtpClient.Send(mailMessage);
//         //         Console.WriteLine($"✅ Order confirmation email sent to {toEmail}");
//         //     }
//         //     catch (Exception ex)
//         //     {
//         //         Console.WriteLine($"❌ Email sending failed: {ex.Message}");
//         //     }
//         // }

//         //==================================================================================================================

//         // ✅ Step 4: Success Page
//         public IActionResult Success(int orderId)
//         {
//             HttpContext.Session.Clear();
//             TempData.Clear();

//             var order = _db.Orders
//                 .Include(o => o.OrderItems)
//                 .Include(o => o.DeliveryInfo)
//                 .FirstOrDefault(o => o.Id == orderId);

//             if (order == null)
//                 return RedirectToAction("Index");

//             return View(order);
//         }
//     }
// }
