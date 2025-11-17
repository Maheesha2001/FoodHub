using System.ComponentModel.DataAnnotations;
namespace FoodHub.Models
{
    public class Special
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TotalPrice { get; set; }  // make nullable

    public string? DiscountType { get; set; }  // make nullable
    [Range(0, 100)]
    public decimal? DiscountValue { get; set; }  // make nullable

    public decimal? FinalPrice { get; set; }  // make nullable
    public string? ImageName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<SpecialItem> SpecialItems { get; set; } = new List<SpecialItem>();
}

}
