namespace FoodHub.Models
{
  public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public string ProductType { get; set; } = string.Empty; // Pizza, Beverage, etc.
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        //public string? CrustType { get; set; } // optional
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }

}
