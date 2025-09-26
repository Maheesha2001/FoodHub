using FoodHub.Models;
using FoodHub.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        Console.WriteLine("ffff");
        if (!ModelState.IsValid)
            {
                Console.WriteLine("not valid");
                foreach (var key in ModelState.Keys)
            {
                var errors = ModelState[key].Errors;
                foreach (var error in errors)
                {
                    Console.WriteLine($"{key}: {error.ErrorMessage}");
                }
            }
                return View(model);
            }

Console.WriteLine("1");
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await _userManager.CreateAsync(user, model.Password);
        Console.WriteLine("2");
        if (result.Succeeded)
        {
            Console.WriteLine("3");
            await _userManager.AddToRoleAsync(user, "Customer");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        Console.WriteLine("4");
        Console.WriteLine("CreateAsync failed:");
        foreach (var err in result.Errors) {
             Console.WriteLine($"Code: {err.Code}, Description: {err.Description}");
            ModelState.AddModelError("", err.Description); }
           Console.WriteLine("end");
        return View(model);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded) return RedirectToAction("Index", "Home");

        ModelState.AddModelError("", "Invalid login attempt");
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
