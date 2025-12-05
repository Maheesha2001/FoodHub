using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodHub.Models;


namespace FoodHub.Data
{
    public class FoodHubContext : IdentityDbContext<ApplicationUser>
    {
        public FoodHubContext(DbContextOptions<FoodHubContext> options) : base(options) { }

        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Special> Specials { get; set; }
        public DbSet<SpecialItem> SpecialItem { get; set; }
        public DbSet<Pizza> Pizzas { get; set; }
        public DbSet<Beverage> Beverages { get; set; }
        public DbSet<PizzaCrustCategory> PizzaCrustCategory { get; set; }
        public DbSet<PizzaPrice> PizzaPrices { get; set; } = null!;
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryInfo> DeliveryInfo { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<DeliveryPerson> DeliveryPerson { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .Ignore(o => o.OrderItems)
                .Ignore(o => o.DeliveryInfo)
                .Ignore(o => o.Payment);
        }

    }
}
