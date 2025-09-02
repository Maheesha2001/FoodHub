using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required]
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public List<OrderItem> OrderItems { get; set; }
    }
}
