
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
                    Console.WriteLine("IT DOES");
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

        // [HttpPost]
        // public async Task<IActionResult> AddSpecialToCart([FromBody] CartSpecialDto dto)
        // {
        //     Console.WriteLine("HERE SPECIAL");
        //     Console.WriteLine("Received specialId: " + dto.SpecialId);

        //     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //     if (string.IsNullOrEmpty(userId))
        //         return Unauthorized("User not logged in");

        //     // 1. Get user's cart
        //     var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");
        //     if (cart == null)
        //     {
        //         cart = new Cart
        //         {
        //             UserId = userId,
        //             Code = CodeGenerator.GenerateFexCode(),
        //             Status = "Active",
        //             CreatedAt = DateTime.Now
        //         };
        //         _db.Carts.Add(cart);
        //         await _db.SaveChangesAsync();
        //     }

        //     // 2. Get special
        //     var special = await _db.Specials.FirstOrDefaultAsync(s => s.Id == dto.SpecialId && s.IsActive);
        //     if (special == null) return BadRequest("Special not found or inactive");
        //     CheckCartItem(special.Id, cart.Code);
        //     // 3. Add to cart
        //     var cartItem = new CartItem
        //     {
        //         Code = cart.Code,
        //         ProductId = special.Id,
        //         ProductName = special.Title,
        //         Type = "Special",
        //         Quantity = dto.Quantity,
        //         Price = special.FinalPrice ?? 0
        //     };
        //     _db.CartItems.Add(cartItem);
        //     await _db.SaveChangesAsync();

        //     return Ok(new { message = "Special added to cart", cartItem });
        // }

        // CheckCartItem(string itemId, string cartCode)
        // {
            
        // }

        [HttpPost]
public async Task<IActionResult> AddSpecialToCart([FromBody] CartSpecialDto dto)
{
    Console.WriteLine("HERE SPECIAL");
    Console.WriteLine("Received specialId: " + dto.SpecialId);

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
        return Unauthorized("User not logged in");

    // 1. Get user's cart
    var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");
    if (cart == null)
    {
        cart = new Cart
        {
            UserId = userId,
            Code = CodeGenerator.GenerateFexCode(),
            Status = "Active",
            CreatedAt = DateTime.Now
        };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
    }

    // 2. GET SPECIAL
    var special = await _db.Specials.FirstOrDefaultAsync(s => s.Id == dto.SpecialId && s.IsActive);
    if (special == null) return BadRequest("Special not found or inactive");

    // 3. Check if special is already in the cart
    var existingItem = await FindSpecialInCart(cart.Code, dto.SpecialId);

    if (existingItem != null)
    {
        // Increase quantity
        existingItem.Quantity += dto.Quantity;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Special quantity updated", cartItem = existingItem });
    }

    // 4. Add NEW cart item
    var cartItem = new CartItem
    {
        Code = cart.Code,
        ProductId = special.Id,
        ProductName = special.Title,
        Type = "Special",
        Quantity = dto.Quantity,
        Price = special.FinalPrice ?? 0
    };

    _db.CartItems.Add(cartItem);
    await _db.SaveChangesAsync();

    return Ok(new { message = "Special added to cart", cartItem });
}
private async Task<CartItem?> FindSpecialInCart(string cartCode, string specialId)
{
    return await _db.CartItems
        .FirstOrDefaultAsync(ci => 
            ci.Code == cartCode &&
            ci.ProductId == specialId &&
            ci.Type == "Special"
        );
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
        {Console.WriteLine("CAME HERE NOW");
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

//         [HttpPost]
// public async Task<IActionResult> Remove(string id, string type)
// {
//     Console.WriteLine("REMOVE CALLED");
//     Console.WriteLine("ID: " + id + " | Type: " + type);

//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//     if (string.IsNullOrEmpty(userId))
//         return Unauthorized();

//     // Find the user's active cart
//     var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");
//     if (cart == null) return NotFound("Cart not found");

//     // Find matching cart item
//     var cartItem = await _db.CartItems
//         .FirstOrDefaultAsync(i => i.Code == cart.Code && i.ProductId == id && i.Type == type);

//     if (cartItem == null)
//         return NotFound("Item not found");

//     _db.CartItems.Remove(cartItem);
//     await _db.SaveChangesAsync();

//     return Ok(new { message = "Item removed" });
// }

        [HttpPost]
public async Task<IActionResult> Remove(string id, string type)
{
    Console.WriteLine("REMOVE CALLED");
    Console.WriteLine("ID: " + id + " | Type: " + type);

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");
    if (cart == null) return NotFound("Cart not found");

    var cartItem = await _db.CartItems
        .FirstOrDefaultAsync(i => i.Code == cart.Code && i.ProductId == id && i.Type == type);

    if (cartItem == null)
        return NotFound("Item not found");

    _db.CartItems.Remove(cartItem);
    await _db.SaveChangesAsync();

    // 🔥 If cart becomes empty → delete cart
    bool hasItems = await _db.CartItems.AnyAsync(i => i.Code == cart.Code);

    if (!hasItems)
    {
        _db.Carts.Remove(cart);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Item removed & cart deleted", cartDeleted = true });
    }

    return Ok(new { message = "Item removed", cartDeleted = false });
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