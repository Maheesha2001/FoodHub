using System.ComponentModel.DataAnnotations.Schema;
namespace FoodHub.Models
{
    public class SpecialItem
    {
        public int Id { get; set; }

        public string? SpecialId { get; set; }
        public Special? Special { get; set; }

        public string? ItemType { get; set; } = ""; // e.g. "Pizza" or "Beverage"
        public string? ItemId { get; set; }            // e.g. Pizza.Id or Beverage.Id
        public int Quantity { get; set; } = 1;
        [NotMapped]
        public decimal Subtotal { get; set; }
        [NotMapped]
        public string? ItemName { get; set; }


    }
}
