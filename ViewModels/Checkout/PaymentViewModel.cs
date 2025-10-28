using System.Collections.Generic;
using FoodHub.Models;
using System.ComponentModel.DataAnnotations;

namespace FoodHub.ViewModels.Checkout
{
    public class PaymentViewModel
    {
        // Order details
        public Order? Order { get; set; }
        public List<OrderItem>? OrderItems { get; set; }

        // Delivery details
        public DeliveryInfo? DeliveryInfo { get; set; }

        // Payment details
        [Required]
        public string PaymentMethod { get; set; } = "Card";
    }
}
