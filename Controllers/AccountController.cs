using FoodHub.Data;
using FoodHub.Models;
using FoodHub.ViewModels;
using FoodHub.ViewModels.Checkout;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly FoodHubContext _db;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        FoodHubContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // ✅ Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var err in result.Errors)
        {
            Console.WriteLine($"Code: {err.Code}, Description: {err.Description}");
            ModelState.AddModelError("", err.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var userId = user?.Id;

            // ✅ STEP 1: Read cart stored in session (for guest users)
            var sessionCartJson = HttpContext.Session.GetString("GuestCart");
            if (!string.IsNullOrEmpty(sessionCartJson))
            {
                var guestCart = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionCartJson);

                if (guestCart != null && guestCart.Count > 0 && userId != null)
                {
                    // ✅ Get or create an active cart
                    var cart = _db.Carts.FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
                    if (cart == null)
                    {
                        cart = new Cart
                        {
                            UserId = userId,
                            Status = "Active",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };
                        _db.Carts.Add(cart);
                        _db.SaveChanges();
                    }

                    // ✅ Merge guest cart items into CartItems table
                    foreach (var incoming in guestCart)
                    {
                        if (!int.TryParse(incoming.Id, out int pid))
                            continue;

                        // Look for existing item with same ProductId and Type
                        var existingItem = _db.CartItems.FirstOrDefault(i =>
                            i.ProductId == pid &&
                            i.Type == incoming.Type &&
                            i.Code == cart.Code);

                        if (existingItem != null)
                        {
                            existingItem.Quantity += incoming.Quantity;
                        }
                        else
                        {
                            _db.CartItems.Add(new CartItem
                            {
                                Code = cart.Code,
                                ProductId = pid,
                                ProductName = incoming.Name,
                                Type = incoming.Type,
                                Quantity = incoming.Quantity,
                                Price = incoming.Price,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    _db.SaveChanges();
                }

                // ✅ Clear guest session cart
                HttpContext.Session.Remove("GuestCart");
            }

            // ✅ Redirect after login
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Checkout", new { area = "Customer" });
        }

        ModelState.AddModelError("", "Invalid login attempt");
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}


// using FoodHub.Data;
// using FoodHub.Models;
// using FoodHub.ViewModels;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using FoodHub.ViewModels.Checkout;
// using Microsoft.EntityFrameworkCore; // ✅ Needed for Include()
// using System.Security.Claims; // ✅ Needed for User.FindFirstValue

// public class AccountController : Controller
// {
//     private readonly UserManager<ApplicationUser> _userManager;
//     private readonly SignInManager<ApplicationUser> _signInManager;
//     private readonly FoodHubContext _db;

//     public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,  FoodHubContext db)
//     {
//         _userManager = userManager;
//         _signInManager = signInManager;
//         _db = db;
//     }

//     [HttpGet]
//     public IActionResult Register() => View();

//     [HttpPost]
//     public async Task<IActionResult> Register(RegisterVM model)
//     {

//       if (!ModelState.IsValid)
//         return View(model);

//          // Check if email already exists
//         var existingUser = await _userManager.FindByEmailAsync(model.Email);
//         if (existingUser != null)
//         {
//             ModelState.AddModelError("Email", "An account with this email already exists.");
//             return View(model);
//         }


//         var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
//         var result = await _userManager.CreateAsync(user, model.Password);

//         if (result.Succeeded)
//         {

//             await _userManager.AddToRoleAsync(user, "Customer");
//             await _signInManager.SignInAsync(user, isPersistent: false);
//             return RedirectToAction("Index", "Home");
//         }

//         foreach (var err in result.Errors) {
//             Console.WriteLine($"Code: {err.Code}, Description: {err.Description}");
//             ModelState.AddModelError("", err.Description); }

//         return View(model);
//     }

//     [HttpGet]
//     public IActionResult Login() => View();

//     // [HttpPost]
//     // public async Task<IActionResult> Login(LoginVM model)
//     // {
//     //     if (!ModelState.IsValid) return View(model);

//     //     var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//     //     if (result.Succeeded) return RedirectToAction("Index", "Home");

//     //     ModelState.AddModelError("", "Invalid login attempt");
//     //     return View(model);
//     // }

//     [HttpPost]
// public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
// {
//     if (!ModelState.IsValid) return View(model);

//     var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
    
//     if (result.Succeeded)
//     {
//         // ✅ STEP 2: Read cart stored in Session before login
//         var sessionCartJson = HttpContext.Session.GetString("GuestCart");
//         if (!string.IsNullOrEmpty(sessionCartJson))
//         {
//             var guestCart = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(sessionCartJson);
//             var userId = (await _userManager.FindByEmailAsync(model.Email)).Id;

//             if (guestCart != null && guestCart.Count > 0)
//             {
//                 // ✅ Merge logic exactly like SaveCheckoutCart
//                 var cart = _db.Carts.Include(c => c.Items).FirstOrDefault(c => c.UserId == userId)
//                           ?? new Cart { UserId = userId, CreatedAt = DateTime.Now, Items = new List<CartItem>() };

//                 foreach (var incoming in guestCart)
//                 {
//                     if (!int.TryParse(incoming.Id, out int pid)) continue;

//                     var existing = cart.Items.FirstOrDefault(i => i.ProductId == pid && i.Type == incoming.Type);
//                     if (existing != null)
//                         existing.Quantity += incoming.Quantity;
//                     else
//                         cart.Items.Add(new CartItem {
//                             ProductId = pid,
//                             ProductName = incoming.Name,
//                             Quantity = incoming.Quantity,
//                             Type = incoming.Type,
//                             Price = incoming.Price
//                         });
//                 }

//                 if (cart.Id == 0) _db.Carts.Add(cart);
//                 _db.SaveChanges();
//             }

//             // ✅ Clear temp session & LocalStorage cart trigger
//             HttpContext.Session.Remove("GuestCart");
//         }

//         // ✅ Redirect properly after merge
//         if (!string.IsNullOrEmpty(returnUrl))
//             return Redirect(returnUrl);

//       return RedirectToAction("Index", "Checkout", new { area = "Customer" });

//     }

//     ModelState.AddModelError("", "Invalid login attempt");
//     return View(model);
// }

//     public async Task<IActionResult> Logout()
//     {
//         await _signInManager.SignOutAsync();
//         return RedirectToAction("Index", "Home");
//     }
// }
