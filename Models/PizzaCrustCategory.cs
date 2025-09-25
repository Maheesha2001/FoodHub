using System.ComponentModel.DataAnnotations;

namespace FoodHub.Models
{
    public class PizzaCrustCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Crust Category")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Extra Charge (Fixed)")]
        public decimal? ExtraCharge { get; set; }

        [Display(Name = "Percentage Increase (0.20 = 20%)")]
        public decimal? PercentageIncrease { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
