// using FoodHub.Data;
// using FoodHub.Models;
// using FoodHub.ViewModels;
// using FoodHub.ViewModels.Checkout;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authorization;

// namespace FoodHub.Areas.Admin.Controllers
// {
//      [Area("Admin")]
//         public class AccountController : Controller
//     {
//         private readonly UserManager<ApplicationUser> _userManager;
//         private readonly SignInManager<ApplicationUser> _signInManager;
//         private readonly FoodHubContext _db;

//         public AccountController(
//             UserManager<ApplicationUser> userManager,
//             SignInManager<ApplicationUser> signInManager,
//             FoodHubContext db)
//         {
//             _userManager = userManager;
//             _signInManager = signInManager;
//             _db = db;
//         }
 
//         [HttpGet]
//         public IActionResult RegisterAdmin() => View();

//         [HttpPost]
//         public async Task<IActionResult> RegisterAdmin(RegisterVM model)
//         {
//             if (!ModelState.IsValid)
//                 return View(model);

//             var existing = await _userManager.FindByEmailAsync(model.Email);
//             if (existing != null)
//             {
//                 ModelState.AddModelError("", "Email already exists.");
//                 return View(model);
//             }

//             var user = new ApplicationUser
//             {
//                 UserName = model.Email,
//                 Email = model.Email,
//                 FullName = model.FullName
//             };

//             var result = await _userManager.CreateAsync(user, model.Password);

//             if (result.Succeeded)
//             {
//                 await _userManager.AddToRoleAsync(user, "Admin");

//                 // NEVER SIGN IN AS CUSTOMER
//                 return RedirectToAction("LoginAdmin");
//             }

//             foreach (var err in result.Errors)
//                 ModelState.AddModelError("", err.Description);

//             return View(model);
//         }

//         [HttpGet]
//         public IActionResult LoginAdmin() => View();

//         [HttpPost]
//         public async Task<IActionResult> LoginAdmin(LoginVM model)
//         {
//             if (!ModelState.IsValid)
//                 return View(model);

//             var user = await _userManager.FindByEmailAsync(model.Email);

//             if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
//             {
//                 ModelState.AddModelError("", "Invalid login attempt.");
//                 return View(model);
//             }

//             var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

//             if (!result.Succeeded)
//             {
//                 ModelState.AddModelError("", "Invalid login attempt.");
//                 return View(model);
//             }

//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id),
//                 new Claim(ClaimTypes.Name, user.UserName),
//                 new Claim(ClaimTypes.Email, user.Email),
//                 new Claim(ClaimTypes.Role, "Admin")
//             };

//             var identity = new ClaimsIdentity(claims, "AdminScheme");
//             var principal = new ClaimsPrincipal(identity);

//             await HttpContext.SignInAsync("AdminScheme", principal);

//             return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> LogoutAdmin()
//         {
//             await HttpContext.SignOutAsync("AdminScheme");
//             Response.Cookies.Delete("AdminAuth", new CookieOptions { Path = "/Admin" });
//             return RedirectToAction("LoginAdmin");
//         }


//     }

// }

// using FoodHub.Data;
// using FoodHub.Models;
// using FoodHub.ViewModels;
// using FoodHub.ViewModels.Checkout;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using System.Security.Claims;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authorization;

// namespace FoodHub.Areas.Admin.Controllers
//  {
// [Area("Admin")]
// public class AccountController : Controller
// {
//     private readonly UserManager<ApplicationUser> _userManager;
//     private readonly SignInManager<ApplicationUser> _signInManager;

//     public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
//     {
//         _userManager = userManager;
//         _signInManager = signInManager;
//     }

//     [HttpGet]
//     public IActionResult LoginAdmin() => View();

//     // [HttpPost]
//     // public async Task<IActionResult> LoginAdmin(LoginVM model)
//     // {
//     //     Console.WriteLine("login admin 1");
//     //     if (!ModelState.IsValid ){
//     //          Console.WriteLine("login admin end");
//     //         return View(model);
            
//     //     }

//     //     var user = await _userManager.FindByEmailAsync(model.Email);
//     //     if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
//     //     {
//     //         ModelState.AddModelError("", "Invalid login attempt.");
//     //         return View(model);
//     //     }

//     //     var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
//     //     if (!result.Succeeded)
//     //     {
//     //         ModelState.AddModelError("", "Invalid login attempt.");
//     //         return View(model);
//     //     }

//     //     var claims = new List<Claim>
//     //     {
//     //         new Claim(ClaimTypes.NameIdentifier, user.Id),
//     //         new Claim(ClaimTypes.Name, user.UserName),
//     //         new Claim(ClaimTypes.Email, user.Email),
//     //         new Claim(ClaimTypes.Role, "Admin")
//     //     };

//     //     var identity = new ClaimsIdentity(claims, "AdminScheme");
//     //     var principal = new ClaimsPrincipal(identity);

//     //     await HttpContext.SignInAsync("AdminScheme", principal);

//     //     return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
//     // }

// [HttpPost]
// public async Task<IActionResult> LoginAdmin(LoginVM model)
// {
//     Console.WriteLine("LoginAdmin: Start");

//     // Check model validation
//     if (!ModelState.IsValid)
//     {
//         Console.WriteLine("LoginAdmin: ModelState is invalid");
//         return View(model);
//     }
//     Console.WriteLine($"LoginAdmin: ModelState is valid. Email={model.Email}");

//     // Find user
//     var user = await _userManager.FindByEmailAsync(model.Email);
//     if (user == null)
//     {
//         Console.WriteLine("LoginAdmin: User not found");
//         ModelState.AddModelError("", "Invalid login attempt.");
//         return View(model);
//     }
//     Console.WriteLine($"LoginAdmin: User found. UserId={user.Id}");

//     // Check role
//     bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
//     if (!isAdmin)
//     {
//         Console.WriteLine("LoginAdmin: User is not in Admin role");
//         ModelState.AddModelError("", "Invalid login attempt.");
//         return View(model);
//     }
//     Console.WriteLine("LoginAdmin: User is in Admin role");

//     // Check password
//     var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
//     if (!result.Succeeded)
//     {
//         Console.WriteLine("LoginAdmin: Password check failed");
//         ModelState.AddModelError("", "Invalid login attempt.");
//         return View(model);
//     }
//     Console.WriteLine("LoginAdmin: Password check succeeded");

//     // Create claims
//     var claims = new List<Claim>
//     {
//         new Claim(ClaimTypes.NameIdentifier, user.Id),
//         new Claim(ClaimTypes.Name, user.UserName),
//         new Claim(ClaimTypes.Email, user.Email),
//         new Claim(ClaimTypes.Role, "Admin")
//     };
//     Console.WriteLine("LoginAdmin: Claims created");

//     // Create identity and principal
//     var identity = new ClaimsIdentity(claims, "AdminScheme");
//     var principal = new ClaimsPrincipal(identity);
//     Console.WriteLine("LoginAdmin: ClaimsIdentity and ClaimsPrincipal created");

//     // Sign in
//     await HttpContext.SignInAsync("AdminScheme", principal);
//     Console.WriteLine("LoginAdmin: Signed in with AdminScheme");

//     Console.WriteLine("LoginAdmin: Redirecting to Admin Dashboard");
//     return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
// }

//     [HttpPost]
//     [ValidateAntiForgeryToken]
//     public async Task<IActionResult> LogoutAdmin()
//     {
//         await HttpContext.SignOutAsync("AdminScheme");
//         Response.Cookies.Delete("AdminAuth", new CookieOptions { Path = "/Admin" });
//         return RedirectToAction("LoginAdmin");
//     }
// }
// }

using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodHub.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult RegisterAdmin() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(RegisterVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("", "Email already exists.");
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
                await _userManager.AddToRoleAsync(user, "Admin");

                // NEVER SIGN IN AS CUSTOMER
                return RedirectToAction("LoginAdmin");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult LoginAdmin(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAdmin(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "AdminScheme");
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync("AdminScheme", principal, authProperties);

            return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboard", new { area = "Admin" }));
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> LogoutAdmin()
        // {
        //     await HttpContext.SignOutAsync("AdminScheme");
        //     Response.Cookies.Delete("AdminAuth", new CookieOptions { Path = "/" });
        //     return RedirectToAction("LoginAdmin");
        // }

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> LogoutAdmin()
{
    Console.WriteLine("LogoutAdmin: Start");

    // Sign out the Admin scheme
    await HttpContext.SignOutAsync("AdminScheme");
    Console.WriteLine("LogoutAdmin: SignOutAsync(AdminScheme) done");

    // Explicitly expire the cookie (cover path / domain mismatch)
    var cookieOptions = new CookieOptions
    {
        Path = "/",                 // match what you set when creating the cookie
        HttpOnly = true,
        Expires = DateTimeOffset.UtcNow.AddDays(-1)
    };

    Response.Cookies.Append("AdminAuth", "", cookieOptions);
    Console.WriteLine("LogoutAdmin: Response.Cookies.Append(AdminAuth expired) done");

    // As a safety net, attempt to delete with Delete() too
    Response.Cookies.Delete("AdminAuth", new CookieOptions { Path = "/" });
    Console.WriteLine("LogoutAdmin: Response.Cookies.Delete(AdminAuth) called");

    return RedirectToAction("LoginAdmin");
}

    }
}
