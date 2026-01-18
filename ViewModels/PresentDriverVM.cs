using System;

namespace FoodHub.ViewModels
{
    public class PresentDriverVM
    {
        public string DeliveryPersonId { get; set; }
        public string Name { get; set; }
        public string NIC { get; set; }
        public DateTime? CheckInTime { get; set; }
    }
}
