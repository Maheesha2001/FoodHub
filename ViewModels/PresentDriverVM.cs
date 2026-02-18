using System;

namespace FoodHub.ViewModels
{
    public class PresentDriverVM
    {
        public string DeliveryPersonId { get; set; }
        public string Name { get; set; }
        public string NIC { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public DateTime? Date { get; set; }
    }
}
