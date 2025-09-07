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

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
