using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FoodHub.Hubs
{
    public class DeliveryHub : Hub
    {
        // Driver joins a group using driverId
        public async Task JoinDriver(string driverId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, driverId);
        }

        public async Task LeaveDriver(string driverId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, driverId);
        }
    }
}
