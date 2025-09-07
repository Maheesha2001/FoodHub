using System;
using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class Beverage
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string? ImageName { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
