using System;
using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class Beverage
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [Required, StringLength(200)]
        public string ImageName { get; set; }

        [Range(0.01, 9999.99)]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
