using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FoodHub.Data;

namespace FoodHub.Services
{
    public class RoleSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public RoleSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = scope.ServiceProvider.GetRequiredService<FoodHubContext>();

            // Wait for DB to be ready
            var retries = 10;
            while (retries > 0)
            {
                try
                {
                    if (await context.Database.CanConnectAsync(cancellationToken))
                        break;
                }
                catch { }

                retries--;
                if (retries == 0) 
                    throw new Exception("Database not ready for seeding");

                Console.WriteLine("Waiting for database...");
                await Task.Delay(2000, cancellationToken);
            }

            string[] roles = new[] { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            Console.WriteLine("Roles seeded successfully.");
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
