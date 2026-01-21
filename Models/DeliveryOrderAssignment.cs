using System;
using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class DeliveryOrderAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string OrderCode { get; set; }

        [Required]
        public string DeliveryPersonId { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? PickedUpAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        public string Status { get; set; } // Assigned, PickedUp, Delivered
    }
}
