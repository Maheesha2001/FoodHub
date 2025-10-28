using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Foreign key to ApplicationUser (AspNetUsers)
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Delivered

        public DateTime CreatedAt { get; set; } = DateTime.Now;
       // public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<OrderItem>? OrderItems { get; set; }
        public DeliveryInfo? DeliveryInfo { get; set; }
    }


}
