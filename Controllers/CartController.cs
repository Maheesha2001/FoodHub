using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels.Checkout;
using Newtonsoft.Json;

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

        public IActionResult Checkout()
        {
            return View();
        }

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

                // Load or create cart
                var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        Items = new List<CartItem>()
                    };
                    _db.Carts.Add(cart);
                    _db.SaveChanges();
                }

                if (cart.Items == null)
                    cart.Items = new List<CartItem>();

                // Merge incoming cart items with DB cart
                foreach (var incoming in cartItems)
                {
                    if (!int.TryParse(incoming.Id, out int productId))
                        continue;

                    var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId && i.Type == incoming.Type);
                    if (existing != null)
                    {
                        // Merge quantities if item already exists
                        existing.Quantity = incoming.Quantity;
                        existing.Price = incoming.Price; // optionally update price
                    }
                    else
                    {
                        cart.Items.Add(new CartItem
                        {
                            CartId = cart.Id,
                            ProductId = productId,
                            ProductName = incoming.Name,
                            Type = incoming.Type,
                            Quantity = incoming.Quantity,
                            Price = incoming.Price
                        });
                    }
                }

                _db.SaveChanges();

                // Update session for frontend
                HttpContext.Session.SetString(
                    "CheckoutCart",
                    System.Text.Json.JsonSerializer.Serialize(
                        cart.Items.Select(i => new CartItemViewModel
                        {
                            Id = i.ProductId.ToString(),
                            Name = i.ProductName,
                            Type = i.Type,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList()
                    )
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in SaveCheckoutCart: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetCartItems()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = true, items = new List<object>() });

                // 1️⃣ Load cart from DB first
                var dbCart = _db.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.UserId == userId && (c.Status == "Active" || c.Status == "Pending"));

                if (dbCart != null && dbCart.Items.Any())
                {
                    var cartItems = dbCart.Items.Select(i => new CartItemViewModel
                    {
                        Id = i.ProductId.ToString(),
                        Name = i.ProductName,
                        Type = i.Type,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList();

                    HttpContext.Session.SetString("CheckoutCart",
                        System.Text.Json.JsonSerializer.Serialize(cartItems));

                    return Json(new { success = true, items = cartItems });
                }

                // 2️⃣ Fallback: session cart
                var sessionCartJson = HttpContext.Session.GetString("CheckoutCart");
                if (!string.IsNullOrEmpty(sessionCartJson))
                {
                    var sessionCartItems = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionCartJson);
                    return Json(new { success = true, items = sessionCartItems });
                }

                // 3️⃣ Both DB and session empty → check frozen order
                var frozenOrder = _db.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefault(o => o.UserId == userId && o.Status == "Pending");

                if (frozenOrder != null && frozenOrder.OrderItems.Any())
                {
                    var orderItems = frozenOrder.OrderItems.Select(i => new CartItemViewModel
                    {
                        Id = i.ProductId.ToString(),
                        Name = i.ProductName,
                        Type = i.ProductType,
                        Price = i.UnitPrice,
                        Quantity = i.Quantity
                    }).ToList();

                    // Optional: mark session as frozen
                    HttpContext.Session.SetString("CheckoutCart",
                        System.Text.Json.JsonSerializer.Serialize(orderItems));

                    return Json(new { success = true, items = orderItems });
                }

                // 4️⃣ Nothing found anywhere
                return Json(new { success = true, items = new List<object>() });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetCartItems: " + ex.Message);
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet]
        public IActionResult IsLoggedIn()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return Json(new { isLoggedIn = true });

            return Json(new { isLoggedIn = false });
        }

        // ✅ Remove a specific item from cart
        [HttpPost]
        public IActionResult Remove(string id, string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = _db.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

            if (cart == null)
                return NotFound();

            var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == id && i.Type == type);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                _db.SaveChanges();
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartItemViewModel item)
        {
            Console.WriteLine("EXECUTED 1");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

            if (cart == null)
            {
                Console.WriteLine("EXECUTED 2");
                cart = new Cart { UserId = userId };
                _db.Carts.Add(cart);
            }

            var existingItem = cart.Items
                .FirstOrDefault(i => i.ProductId.ToString() == item.Id && i.Type == item.Type);
            if (existingItem != null)
            {
                Console.WriteLine("EXECUTED 3");
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Console.WriteLine("EXECUTED 4");
                cart.Items.Add(new CartItem
                {
                    ProductId = int.Parse(item.Id),
                    ProductName = item.Name,
                    Type = item.Type,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }
            Console.WriteLine("EXECUTED 5");

            _db.SaveChanges();
            return Ok(new { success = true });
        }


         // ✅ Update quantity (from Checkout page)
        [HttpPost]
        public IActionResult UpdateItemQuantity(string productId, int quantity, string type)
{


    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId == null) return Unauthorized();

    var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
    if (cart == null) return NotFound();

    var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
    if (item == null) return NotFound();

    item.Quantity = quantity;
    _db.SaveChanges();

    return Json(new { success = true, total = item.Price * item.Quantity });
}

        // ✅ Remove item (from Checkout page)
        [HttpPost]
        public IActionResult RemoveItem(string productId, string type)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var cart = _db.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
                return NotFound();

            // Find by both productId and type
            var item = cart.Items.FirstOrDefault(i => i.ProductId.ToString() == productId && i.Type == type);
            if (item == null)
                return NotFound();

            _db.CartItems.Remove(item);
            _db.SaveChanges();

            return Json(new { success = true, message = "Item removed from cart." });
        }

        // ✅ Load updated cart (used by sidebar or checkout page)
        [HttpGet]
        public IActionResult GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();


            var cart = _db.Carts
                .Include(c => c.Items)
                .FirstOrDefault(c => c.UserId == userId && c.Status == "Active" );

            if (cart == null)
                return Json(new { items = new List<object>() });

            var items = cart.Items.Select(i => new
            {
                id = i.ProductId,
                name = i.ProductName,
                type = i.Type,
                quantity = i.Quantity,
                price = i.Price
            });

            return Json(new { success = true, items });
        }
    
    

    }
}