using FoodHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FoodHubContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Content")),
    RequestPath = "/Content"
});

app.UseRouting();
app.UseAuthorization();

// Redirect /Admin → /Admin/Dashboard
app.MapGet("/Admin", context =>
{
    context.Response.Redirect("/Admin/Dashboard");
    return Task.CompletedTask;
});

// Area route (must come first)
app.MapControllerRoute(
    name: "areas",
  //  pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}"
);

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();


// using FoodHub.Data;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.FileProviders;

// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// builder.Services.AddControllersWithViews();
// builder.Services.AddDbContext<FoodHubContext>(options =>
//     options.UseMySql(
//         builder.Configuration.GetConnectionString("DefaultConnection"),
//         new MySqlServerVersion(new Version(8, 0, 33))
//     )
// );

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Home/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

// app.UseHttpsRedirection();
// app.UseRouting();

// app.UseAuthorization();

// app.MapStaticAssets();

// // Enable wwwroot (default static files)
// app.UseStaticFiles();

// // Enable Content/images as static
// app.UseStaticFiles(new StaticFileOptions
// {
//     FileProvider = new PhysicalFileProvider(
//         Path.Combine(builder.Environment.ContentRootPath, "Content")),
//     RequestPath = "/Content"
// });


// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}")
//     .WithStaticAssets();

// app.MapControllerRoute(
//     name: "areas",
//     pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
// );

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");


// app.Run();
