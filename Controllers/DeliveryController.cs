using FoodHub.Data;
using FoodHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Controllers
{
    [ApiController]
    [Route("api/delivery")]
    public class DeliveryController : ControllerBase
    {
        private readonly FoodHubContext _context;

        public DeliveryController(FoodHubContext context)
        {
            _context = context;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingOrders()
        {
            // Fetch pending orders
            var ordersFromDb = await _context.Orders
                .Where(o => o.Status == "Pending")
                .ToListAsync();

            // Fetch all order items (to filter by Code)
            var allItems = await _context.OrderItems.ToListAsync();

            // Build the response manually
            var orders = ordersFromDb.Select(o => new
            {
                o.Id,
                o.Code,
                o.UserId,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,

                Items = allItems
                    .Where(i => i.Code == o.Code) // join by Code
                    .Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList(),

                Delivery = o.DeliveryInfo, // optional
                Payment = o.Payment        // optional
            }).ToList();

            return Ok(orders);
        }
    
    
    [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var user = await _context.DeliveryPerson
        .FirstOrDefaultAsync(u => u.Email == request.Email);

    if (user == null)
        return Unauthorized(new LoginResponse { Success = false, Message = "Invalid email" });

    // Verify password
    var passwordHasher = new PasswordHasher<DeliveryPerson>();
    var result = passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

    if (result == PasswordVerificationResult.Failed)
        return Unauthorized(new LoginResponse { Success = false, Message = "Invalid password" });

    // Generate token (JWT or similar)
    string token = "YOUR_TOKEN_HERE";

    return Ok(new LoginResponse { Success = true, Token = token, Message = "Login successful" });
}

    }
}
