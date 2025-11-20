using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5187");

// Hosted service
builder.Services.AddHostedService<SpecialsStatusService>();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// MVC
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// DbContext
builder.Services.AddDbContext<FoodHubContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Identity for customers (default Identity uses its own cookie scheme: IdentityConstants.ApplicationScheme)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<FoodHubContext>()
.AddDefaultTokenProviders();

// Configure the default Identity cookie for customers
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "CustomerAuth";
    options.Cookie.Path = "/";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // allow http for local dev
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Add Admin cookie and a policy scheme that picks the scheme based on request path
builder.Services.AddAuthentication(options =>
{
    // We set a policy scheme below ("SmartScheme") as the default,
    // which will forward to the appropriate cookie scheme depending on the request path.
    options.DefaultAuthenticateScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme; // sign-ins from Identity still use Identity default
})
// Admin cookie (explicit)
.AddCookie("AdminScheme", options =>
{
    options.Cookie.Name = "AdminAuth";
    options.Cookie.Path = "/";                 // site-wide path avoids tricky path mismatch
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // allow http for localhost dev
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.LoginPath = "/Admin/Account/LoginAdmin";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})

// Policy scheme that chooses between AdminScheme and Identity.Application based on request path
.AddPolicyScheme("SmartScheme", "Smart auth scheme", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // If request is for admin area (path starts with /Admin), use AdminScheme,
        // otherwise use the default Identity application scheme for customers.
        var path = context.Request.Path;
        if (path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
            return "AdminScheme";

        return IdentityConstants.ApplicationScheme; // "Identity.Application"
    };
});


// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed roles (Admin, Customer)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "Admin", "Customer" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseCors("MobileApp");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Content")),
    RequestPath = "/Content"
});

app.UseRouting();
app.UseSession();

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// Redirect /Admin root to dashboard
app.MapGet("/Admin", context =>
{
    context.Response.Redirect("/Admin/Dashboard");
    return Task.CompletedTask;
});

// Area route (areas route should come before default)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
