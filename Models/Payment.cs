using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodHub.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        // Link to Order
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Optional navigation property
        public Order? Order { get; set; }
    }
}
