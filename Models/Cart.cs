using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}