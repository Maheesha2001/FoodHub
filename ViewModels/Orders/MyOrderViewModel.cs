using System.Text.Json.Serialization;
using FoodHub.Models;

namespace FoodHub.ViewModels.Orders
{
    public class MyOrderViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        // ✅ Add this line
        public virtual Payment? Payment { get; set; }
    }
}
