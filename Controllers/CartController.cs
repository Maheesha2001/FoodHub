using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels.Checkout;
using System.Text.Json;

namespace FoodHub.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly FoodHubContext _db;

        public CartController(FoodHubContext db)
        {
            _db = db;
        }

        // ✅ Go to Checkout page
        public IActionResult Checkout()
        {
            return View("~/Areas/Customer/Views/Checkout/Index.cshtml");
        }

        // ✅ Save cart before checkout
        [HttpPost]
        public IActionResult SaveCheckoutCart([FromBody] List<CartItemViewModel> cartItems)
        {
            try
            {
                if (cartItems == null || !cartItems.Any())
                    return BadRequest("Cart items are missing.");

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not logged in.");

                // Load or create active cart
                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Code = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now
                    };
                    _db.Carts.Add(cart);
                    _db.SaveChanges();
                }

                // Remove existing items for this cart code (reset)
                var oldItems = _db.CartItems.Where(i => i.Code == cart.Code);
                _db.CartItems.RemoveRange(oldItems);

                // Add updated items
                foreach (var incoming in cartItems)
                {
                    if (!int.TryParse(incoming.Id, out int productId)) continue;

                    _db.CartItems.Add(new CartItem
                    {
                        Code = cart.Code,
                        ProductId = productId,
                        ProductName = incoming.Name,
                        Type = incoming.Type,
                        Quantity = incoming.Quantity,
                        Price = incoming.Price
                    });
                }

                _db.SaveChanges();

                // Update session
                HttpContext.Session.SetString("CheckoutCart", JsonSerializer.Serialize(cartItems));
            return Json(new { success = true, code = cart.Code });
              //  return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in SaveCheckoutCart: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ Load cart items
        [HttpGet]
        public IActionResult GetCartItems()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = true, items = new List<object>() });

                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && (c.Status == "Active" || c.Status == "Pending"));
                if (cart != null)
                {
                    var dbItems = _db.CartItems
                        .Where(i => i.Code == cart.Code)
                        .Select(i => new CartItemViewModel
                        {
                            Id = i.ProductId.ToString(),
                            Name = i.ProductName,
                            Type = i.Type,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList();

                    HttpContext.Session.SetString("CheckoutCart", JsonSerializer.Serialize(dbItems));
                    return Json(new { success = true, items = dbItems });
                }

                // Fallback: session
                var sessionJson = HttpContext.Session.GetString("CheckoutCart");
                if (!string.IsNullOrEmpty(sessionJson))
                {
                    var sessionItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionJson);
                    return Json(new { success = true, items = sessionItems });
                }

                return Json(new { success = true, items = new List<object>() });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCartItems: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ Add item
        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItemViewModel item)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Code = Guid.NewGuid().ToString("N"),
                        CreatedAt = DateTime.Now
                    };
                    _db.Carts.Add(cart);
                    _db.SaveChanges();
                }

                var existing = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == item.Id && i.Type == item.Type);
                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                }
                else
                {
                    _db.CartItems.Add(new CartItem
                    {
                        Code = cart.Code,
                        ProductId = int.Parse(item.Id),
                        ProductName = item.Name,
                        Type = item.Type,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in AddToCart: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ Update quantity
        [HttpPost]
        public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null) return NotFound();

                var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == productId && i.Type == type);
                if (item == null) return NotFound();

                item.Quantity = quantity;
                _db.SaveChanges();

                return Json(new { success = true, total = item.Price * item.Quantity });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateItemQuantity: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ Remove item
        [HttpPost]
        public IActionResult RemoveItem(string productId, string type)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null) return NotFound();

                var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == productId && i.Type == type);
                if (item != null)
                {
                    _db.CartItems.Remove(item);
                    _db.SaveChanges();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RemoveItem: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        // ✅ Get full cart (for sidebar)
        [HttpGet]
        public IActionResult GetCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null)
                    return Json(new { success = true, items = new List<object>() });

                var items = _db.CartItems
                    .Where(i => i.Code == cart.Code)
                    .Select(i => new
                    {
                        id = i.ProductId,
                        name = i.ProductName,
                        type = i.Type,
                        quantity = i.Quantity,
                        price = i.Price
                    }).ToList();

                return Json(new { success = true, items });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCart: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}




// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using FoodHub.Data;
// using FoodHub.Models;
// using FoodHub.ViewModels.Checkout;
// using Newtonsoft.Json;

// namespace FoodHub.Controllers
// {

//     public class CartController : Controller
//     {
//         private readonly FoodHubContext _db;

//         public CartController(FoodHubContext db)
//         {
//             _db = db;
//         }
//         [Authorize]
//         public IActionResult Checkout()
//         {
//             Console.WriteLine("Authenticated? " + User.Identity?.IsAuthenticated);
//             Console.WriteLine("User: " + User.Identity?.Name);
//            // return View();
//            return View("~/Areas/Customer/Views/Checkout/Index.cshtml");
//         }

//         [HttpPost]
//         public IActionResult SaveCheckoutCart([FromBody] List<CartItemViewModel> cartItems)
//         {
//             try
//             {
//                 if (cartItems == null || !cartItems.Any())
//                     return BadRequest("Cart items are missing.");

//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId))
//                     return Unauthorized("User not logged in.");

//                 // Load or create cart
//                 var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null)
//                 {
//                     cart = new Cart
//                     {
//                         UserId = userId,
//                         CreatedAt = DateTime.Now,
//                         Items = new List<CartItem>()
//                     };
//                     _db.Carts.Add(cart);
//                     _db.SaveChanges();
//                 }

//                 if (cart.Items == null)
//                     cart.Items = new List<CartItem>();

//                 // Merge incoming cart items with DB cart
//                 foreach (var incoming in cartItems)
//                 {
//                     if (!int.TryParse(incoming.Id, out int productId))
//                         continue;

//                     var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Type == incoming.Type);
//                     if (existing != null)
//                     {
//                         // Merge quantities if item already exists
//                         existing.Quantity = incoming.Quantity;
//                         existing.Price = incoming.Price; // optionally update price
//                     }
//                     else
//                     {
//                         cart.Items.Add(new CartItem
//                         {
//                             Code = cart.Code,
//                             ProductId = productId,
//                             ProductName = incoming.Name,
//                             Type = incoming.Type,
//                             Quantity = incoming.Quantity,
//                             Price = incoming.Price
//                         });
//                     }
//                 }

//                 _db.SaveChanges();

//                 // Update session for frontend
//                 HttpContext.Session.SetString(
//                     "CheckoutCart",
//                     System.Text.Json.JsonSerializer.Serialize(
//                         cart.Items.Select(i => new CartItemViewModel
//                         {
//                             Id = i.ProductId.ToString(),
//                             Name = i.ProductName,
//                             Type = i.Type,
//                             Quantity = i.Quantity,
//                             Price = i.Price
//                         }).ToList()
//                     )
//                 );

//                 return Json(new { success = true });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Exception in SaveCheckoutCart: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         [HttpGet]
//         public IActionResult GetCartItems()
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId))
//                     return Json(new { success = true, items = new List<object>() });

//                 // 1️⃣ Load cart from DB first
//                 var dbCart = _db.Carts
//                 .Include(c => c.Items)
//                 .FirstOrDefault(c => c.UserId == userId && (c.Status == "Active" || c.Status == "Pending"));

//                 if (dbCart != null && dbCart.Items.Any())
//                 {
//                     var cartItems = dbCart.Items.Select(i => new CartItemViewModel
//                     {
//                         Id = i.ProductId.ToString(),
//                         Name = i.ProductName,
//                         Type = i.Type,
//                         Price = i.Price,
//                         Quantity = i.Quantity
//                     }).ToList();

//                     HttpContext.Session.SetString("CheckoutCart",
//                         System.Text.Json.JsonSerializer.Serialize(cartItems));

//                     return Json(new { success = true, items = cartItems });
//                 }

//                 // 2️⃣ Fallback: session cart
//                 var sessionCartJson = HttpContext.Session.GetString("CheckoutCart");
//                 if (!string.IsNullOrEmpty(sessionCartJson))
//                 {
//                     var sessionCartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionCartJson);
//                     return Json(new { success = true, items = sessionCartItems });
//                 }

//                 // 3️⃣ Both DB and session empty → check frozen order
//                 var frozenOrder = _db.Orders
//                     .Include(o => o.OrderItems)
//                     .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

//                 if (frozenOrder != null && frozenOrder.OrderItems.Any())
//                 {
//                     var orderItems = frozenOrder.OrderItems.Select(i => new CartItemViewModel
//                     {
//                         Id = i.ProductId.ToString(),
//                         Name = i.ProductName,
//                         Type = i.ProductType,
//                         Price = i.UnitPrice,
//                         Quantity = i.Quantity
//                     }).ToList();

//                     // Optional: mark session as frozen
//                     HttpContext.Session.SetString("CheckoutCart",
//                         System.Text.Json.JsonSerializer.Serialize(orderItems));

//                     return Json(new { success = true, items = orderItems });
//                 }

//                 // 4️⃣ Nothing found anywhere
//                 return Json(new { success = true, items = new List<object>() });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in GetCartItems: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }


//         // [HttpGet]
//         // public IActionResult IsLoggedIn()
//         // {
//         //     if (User.Identity != null && User.Identity.IsAuthenticated)
//         //         return Json(new { isLoggedIn = true });

//         //     return Json(new { isLoggedIn = false });
//         // }

//         // ✅ Remove a specific item from cart
//         [HttpPost]
//         public IActionResult Remove(string id, string type)
//         {
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             var cart = _db.Carts
//                 .Include(c => c.Items)
//                 .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

//             if (cart == null)
//                 return NotFound();

//             var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == id && i.Type == type);
//             if (item != null)
//             {
//                 _db.CartItems.Remove(item);
//                 _db.SaveChanges();
//             }

//             return Ok(new { success = true });
//         }

//         [HttpPost]
//         public IActionResult AddToCart([FromBody] CartItemViewModel item)
//         {
//             Console.WriteLine("EXECUTED 1");
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

//             if (cart == null)
//             {
//                 Console.WriteLine("EXECUTED 2");
//                 cart = new Cart { UserId = userId };
//                 _db.Carts.Add(cart);
//             }

//             var existingItem = cart.Items
//                 .FirstOrDefault(i => i.ProductId.ToString() == item.Id && i.Type == item.Type);
//             if (existingItem != null)
//             {
//                 Console.WriteLine("EXECUTED 3");
//                 existingItem.Quantity += item.Quantity;
//             }
//             else
//             {
//                 Console.WriteLine("EXECUTED 4");
//                 cart.Items.Add(new CartItem
//                 {
//                     ProductId = int.Parse(item.Id),
//                     ProductName = item.Name,
//                     Type = item.Type,
//                     Quantity = item.Quantity,
//                     Price = item.Price
//                 });
//             }
//             Console.WriteLine("EXECUTED 5");

//             _db.SaveChanges();
//             return Ok(new { success = true });
//         }


//          // ✅ Update quantity (from Checkout page)
//         [HttpPost]
//         public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
// {

//             Console.WriteLine($"[UpdateItemQuantity] ProductId: {productId}, Type: {type}, Qty: {quantity}");

//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//     if (userId == null) return Unauthorized();

//     var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//     if (cart == null) return NotFound();

//     var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
//     if (item == null) return NotFound();

//     item.Quantity = quantity;
//     _db.SaveChanges();

//     return Json(new { success = true, total = item.Price * item.Quantity });
// }

//         // ✅ Remove item (from Checkout page)
//         [HttpPost]
//         public IActionResult RemoveItem(string productId, string type)
//         {
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             if (userId == null)
//                 return Unauthorized();

//             var cart = _db.Carts
//                 .Include(c => c.Items)
//                 .FirstOrDefault(c => c.UserId == userId  && c.Status == "Active");

//             if (cart == null)
//                 return NotFound();

//             // Find by both productId and type
//             var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
//             if (item == null)
//                 return NotFound();

//             _db.CartItems.Remove(item);
//             _db.SaveChanges();

//             return Json(new { success = true, message = "Item removed from cart." });
//         }

//         // ✅ Load updated cart (used by sidebar or checkout page)
//         [HttpGet]
//         public IActionResult GetCart()
//         {
//             var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//             if (userId == null)
//                 return Unauthorized();


//             var cart = _db.Carts
//                 .Include(c => c.Items)
//                 .FirstOrDefault(c => c.UserId == userId && c.Status == "Active" );

//             if (cart == null)
//                 return Json(new { items = new List<object>() });

//             var items = cart.Items.Select(i => new
//             {
//                 id = i.ProductId,
//                 name = i.ProductName,
//                 type = i.Type,
//                 quantity = i.Quantity,
//                 price = i.Price
//             });

//             return Json(new { success = true, items });
//         }
    
    

//     }
// }