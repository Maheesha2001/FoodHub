using System.Collections.Generic;
using FoodHub.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FoodHub.ViewModels.Checkout
{
    public class PaymentViewModel
    {
        public string OrderCode { get; set; } = string.Empty;
        // Order details
        [BindNever]
        public Order? Order { get; set; }
         [BindNever]
        public List<OrderItem>? OrderItems { get; set; }

        // Delivery details
         [BindNever]
        public DeliveryInfo? DeliveryInfo { get; set; }

        // Payment details
        [Required]
        public string PaymentMethod { get; set; } = "Card";
    }
}
