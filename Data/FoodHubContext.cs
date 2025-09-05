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
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Beverage> Beverages { get; set; }


    }
}
