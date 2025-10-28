namespace FoodHub.ViewModels.Checkout
{
    public class DeliveryInfoViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? DeliveryNotes { get; set; }
    }
}
