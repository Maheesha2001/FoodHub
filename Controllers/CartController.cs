
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels.Checkout;
using System.Text.Json;
using FoodHub.Helpers;

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
                        Code = CodeGenerator.GenerateFexCode(),
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
                    // No need to parse int, ProductId can be string now
                    _db.CartItems.Add(new CartItem
                    {
                        Code = cart.Code,
                        ProductId = incoming.Id,       // <-- changed to string
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
                            Id = i.ProductId,         // <-- string
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
                        Code = CodeGenerator.GenerateFexCode(),
                        CreatedAt = DateTime.Now
                    };
                    _db.Carts.Add(cart);
                    _db.SaveChanges();
                }

                var existing = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId == item.Id && i.Type == item.Type);
                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                }
                else
                {
                    _db.CartItems.Add(new CartItem
                    {
                        Code = cart.Code,
                        ProductId = item.Id,
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

                var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId == productId && i.Type == type);
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

                var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId == productId && i.Type == type);
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
                        id = i.ProductId,   // string
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
// using System.Text.Json;
// using FoodHub.Helpers;

// namespace FoodHub.Controllers
// {
//     [Authorize]
//     public class CartController : Controller
//     {
//         private readonly FoodHubContext _db;

//         public CartController(FoodHubContext db)
//         {
//             _db = db;
//         }

//         // ✅ Go to Checkout page
//         public IActionResult Checkout()
//         {
//             return View("~/Areas/Customer/Views/Checkout/Index.cshtml");
//         }

//         // ✅ Save cart before checkout
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

//                 // Load or create active cart
//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null)
//                 {
//                     cart = new Cart
//                     {
//                         UserId = userId,
//                         Code = CodeGenerator.GenerateFexCode(),
//                         CreatedAt = DateTime.Now
//                     };
//                     _db.Carts.Add(cart);
//                     _db.SaveChanges();
//                 }

//                 // Remove existing items for this cart code (reset)
//                 var oldItems = _db.CartItems.Where(i => i.Code == cart.Code);
//                 _db.CartItems.RemoveRange(oldItems);

//                 // Add updated items
//                 foreach (var incoming in cartItems)
//                 {
//                     if (!int.TryParse(incoming.Id, out int productId)) continue;

//                     _db.CartItems.Add(new CartItem
//                     {
//                         Code = cart.Code,
//                         ProductId = productId,
//                         ProductName = incoming.Name,
//                         Type = incoming.Type,
//                         Quantity = incoming.Quantity,
//                         Price = incoming.Price
//                     });
//                 }

//                 _db.SaveChanges();

//                 // Update session
//                 HttpContext.Session.SetString("CheckoutCart", JsonSerializer.Serialize(cartItems));
//             return Json(new { success = true, code = cart.Code });
//               //  return Json(new { success = true });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Exception in SaveCheckoutCart: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         // ✅ Load cart items
//         [HttpGet]
//         public IActionResult GetCartItems()
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId))
//                     return Json(new { success = true, items = new List<object>() });

//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && (c.Status == "Active" || c.Status == "Pending"));
//                 if (cart != null)
//                 {
//                     var dbItems = _db.CartItems
//                         .Where(i => i.Code == cart.Code)
//                         .Select(i => new CartItemViewModel
//                         {
//                             Id = i.ProductId.ToString(),
//                             Name = i.ProductName,
//                             Type = i.Type,
//                             Quantity = i.Quantity,
//                             Price = i.Price
//                         }).ToList();

//                     HttpContext.Session.SetString("CheckoutCart", JsonSerializer.Serialize(dbItems));
//                     return Json(new { success = true, items = dbItems });
//                 }

//                 // Fallback: session
//                 var sessionJson = HttpContext.Session.GetString("CheckoutCart");
//                 if (!string.IsNullOrEmpty(sessionJson))
//                 {
//                     var sessionItems = JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionJson);
//                     return Json(new { success = true, items = sessionItems });
//                 }

//                 return Json(new { success = true, items = new List<object>() });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in GetCartItems: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         // ✅ Add item
//         [HttpPost]
//         public IActionResult AddToCart([FromBody] CartItemViewModel item)
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId)) return Unauthorized();

//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null)
//                 {
//                     cart = new Cart
//                     {
//                         UserId = userId,
//                         Code = CodeGenerator.GenerateFexCode(),
//                         CreatedAt = DateTime.Now
//                     };
//                     _db.Carts.Add(cart);
//                     _db.SaveChanges();
//                 }

//                 var existing = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == item.Id && i.Type == item.Type);
//                 if (existing != null)
//                 {
//                     existing.Quantity += item.Quantity;
//                 }
//                 else
//                 {
//                     _db.CartItems.Add(new CartItem
//                     {
//                         Code = cart.Code,
//                         ProductId = int.Parse(item.Id),
//                         ProductName = item.Name,
//                         Type = item.Type,
//                         Quantity = item.Quantity,
//                         Price = item.Price
//                     });
//                 }

//                 _db.SaveChanges();
//                 return Ok(new { success = true });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in AddToCart: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         // ✅ Update quantity
//         [HttpPost]
//         public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId)) return Unauthorized();

//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null) return NotFound();

//                 var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == productId && i.Type == type);
//                 if (item == null) return NotFound();

//                 item.Quantity = quantity;
//                 _db.SaveChanges();

//                 return Json(new { success = true, total = item.Price * item.Quantity });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in UpdateItemQuantity: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         // ✅ Remove item
//         [HttpPost]
//         public IActionResult RemoveItem(string productId, string type)
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId)) return Unauthorized();

//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null) return NotFound();

//                 var item = _db.CartItems.FirstOrDefault(i => i.Code == cart.Code && i.ProductId.ToString() == productId && i.Type == type);
//                 if (item != null)
//                 {
//                     _db.CartItems.Remove(item);
//                     _db.SaveChanges();
//                 }

//                 return Json(new { success = true });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in RemoveItem: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }

//         // ✅ Get full cart (for sidebar)
//         [HttpGet]
//         public IActionResult GetCart()
//         {
//             try
//             {
//                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                 if (string.IsNullOrEmpty(userId)) return Unauthorized();

//                 var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
//                 if (cart == null)
//                     return Json(new { success = true, items = new List<object>() });

//                 var items = _db.CartItems
//                     .Where(i => i.Code == cart.Code)
//                     .Select(i => new
//                     {
//                         id = i.ProductId,
//                         name = i.ProductName,
//                         type = i.Type,
//                         quantity = i.Quantity,
//                         price = i.Price
//                     }).ToList();

//                 return Json(new { success = true, items });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine("Error in GetCart: " + ex.Message);
//                 return StatusCode(500, ex.Message);
//             }
//         }
//     }
// }