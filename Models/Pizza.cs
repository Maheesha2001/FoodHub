namespace FoodHub.Models
{
    public class Pizza
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageName { get; set; }
        public decimal BasePrice { get; set; }
        // public string? CrustCategory { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<PizzaPrice> PizzaPrices { get; set; } = new List<PizzaPrice>();
    }
}
