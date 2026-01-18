namespace FoodHub.Models  // Or FoodHub.ViewModels
{public class DeliveryAttendance
{
    public int Id { get; set; }
    public string DeliveryPersonId { get; set; }
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}
}
