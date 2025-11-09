using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
public class CartItem
    {
        public int Id { get; set; }
        //public int CartId { get; set; }
        public string Code { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
    }
}