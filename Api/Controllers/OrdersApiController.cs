using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly FoodHubContext _db;

        public OrdersApiController(FoodHubContext db)
        {
            _db = db;
        }

        // GET api/orders/123
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryInfo)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        // PUT api/orders/complete/123
        // Mark order as delivered + update payment status
        [HttpPut("complete/{id}")]
        public async Task<IActionResult> MarkDelivered(int id, [FromBody] bool paymentReceived)
        {
            var order = await _db.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Update order status
            order.Status = "Delivered";

            // Update payment if exists
            if (order.Payment != null)
            {
                order.Payment.PaymentStatus = paymentReceived ? "Completed" : "Pending";
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Order updated successfully" });
        }

        // GET api/orders/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Where(o => o.Status != "Delivered")
                .ToListAsync();

            return Ok(orders);
        }
    }
}
