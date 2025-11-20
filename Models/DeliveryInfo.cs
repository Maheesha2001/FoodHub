namespace FoodHub.Models
{
    public class DeliveryInfo
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string DeliveryStatus { get; set; } = string.Empty;

        public string? DeliveryNotes { get; set; }

           public DateTime? DeliveredAt { get; set; } 
    }
}
