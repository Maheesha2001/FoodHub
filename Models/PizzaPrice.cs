using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodHub.Models
{
    public class PizzaPrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Pizza")]
        public int PizzaId { get; set; }

        [ForeignKey("PizzaId")]
        public Pizza? Pizza { get; set; } = null!;

        [Required]
        [Display(Name = "Crust")]
        public int CrustId { get; set; }

        [ForeignKey("CrustId")]
        public PizzaCrustCategory? Crust { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
