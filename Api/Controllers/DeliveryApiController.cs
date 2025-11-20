using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryApiController : ControllerBase
    {
        private readonly FoodHubContext _db;

        public DeliveryApiController(FoodHubContext db)
        {
            _db = db;
        }

        // GET api/delivery/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.DeliveryInfo)
                .Where(o => o.Status != "Delivered") // Only pending or confirmed
                .ToListAsync();

            return Ok(orders);
        }

        // PUT api/delivery/complete/123
        [HttpPut("complete/{id}")]
        public async Task<IActionResult> MarkDelivered(int id, [FromBody] bool paymentReceived)
        {
            var order = await _db.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Mark as delivered
            order.Status = "Delivered";

            // Update payment status if exists
            if (order.Payment != null)
                order.Payment.PaymentStatus = paymentReceived ? "Completed" : "Pending";

            order.DeliveryInfo ??= new DeliveryInfo(); // Ensure delivery info exists
            order.DeliveryInfo.DeliveredAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Order delivered and payment updated successfully" });
        }
    }
}
