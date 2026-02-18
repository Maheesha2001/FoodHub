using FoodHub.Data;
using FoodHub.Models;
using FoodHub.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// =======================
// WEB HOST (IMPORTANT)
// =======================
builder.WebHost.UseUrls("http://0.0.0.0:5187");
builder.Services.AddSignalR();
// =======================
// DATABASE
// =======================
builder.Services.AddDbContext<FoodHubContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);
// =======================
// cors
//========================

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// =======================
// IDENTITY
// =======================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<FoodHubContext>()
.AddDefaultTokenProviders();

// =======================
// AUTH – ADMIN vs CUSTOMER
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SmartScheme";
    options.DefaultChallengeScheme = "SmartScheme";
})
.AddCookie("AdminScheme", options =>
{
    options.Cookie.Name = "AdminAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Codespaces
    options.LoginPath = "/Admin/Account/LoginAdmin";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
})
.AddPolicyScheme("SmartScheme", "Smart auth", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        if (context.Request.Path.StartsWithSegments("/Admin"))
            return "AdminScheme";

        return IdentityConstants.ApplicationScheme;
    };
});

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

// =======================
// SESSION
// =======================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =======================
// ROLE SEEDER (YOUR CODE – CORRECT)
// =======================
builder.Services.AddHostedService<RoleSeeder>();

var app = builder.Build();

// =======================
// FORWARDED HEADERS (CRITICAL FOR CODESPACES)
// =======================
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// =======================
// MIDDLEWARE
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseCors("MobileApp");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// =======================
// ADMIN ROOT REDIRECT
// =======================
app.MapGet("/Admin", context =>
{
    context.Response.Redirect("/Admin/Dashboard");
    return Task.CompletedTask;
});

// =======================
// ROUTES
// =======================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();