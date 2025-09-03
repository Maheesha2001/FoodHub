using Microsoft.EntityFrameworkCore;
using FoodHub.Models;

namespace FoodHub.Data
{
    public class FoodHubContext : DbContext
    {
        public FoodHubContext(DbContextOptions<FoodHubContext> options) : base(options) { }

        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Special> Specials { get; set; }
    }
}
