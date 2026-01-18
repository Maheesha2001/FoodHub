using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

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




        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine("🔵 Login API called");
            Console.WriteLine($"📩 Email received: {request.Email}");

            // Get the delivery person by email only
            var deliveryPerson = await _db.DeliveryPerson
                .FirstOrDefaultAsync(d => d.Email == request.Email && d.IsActive);

            if (deliveryPerson == null)
            {
                Console.WriteLine("❌ Login failed – deliveryPerson is NULL");
                return Unauthorized(new {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            // Verify the password
            var passwordHasher = new PasswordHasher<DeliveryPerson>();
            var result = passwordHasher.VerifyHashedPassword(deliveryPerson, deliveryPerson.Password, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("❌ Login failed – password mismatch");
                return Unauthorized(new {
                    success = false,
                    message = "Invalid email or password"
                });
            }

            Console.WriteLine("✅ Login success");
            Console.WriteLine($"🆔 DeliveryPerson.Id (RAW) = '{deliveryPerson.Id}'");
            Console.WriteLine($"🧑 Name = '{deliveryPerson.Name}'");

            return Ok(new {
                success = true,
                message = "Login successful",
                deliveryPersonId = deliveryPerson.Id,
                name = deliveryPerson.Name
            });
        }

        // // GET api/delivery/pending
        // [HttpGet("pending")]
        // public async Task<IActionResult> GetPendingOrders()
        // {
        //     var orders = await _db.Orders
        //         .Include(o => o.OrderItems)
        //         .Include(o => o.Payment)
        //         .Include(o => o.DeliveryInfo)
        //         .Where(o => o.Status != "Delivered") // Only pending or confirmed
        //         .ToListAsync();

        //     return Ok(orders);
        // }

// // GET api/delivery/pending
// [HttpGet("pending")]
// public async Task<IActionResult> GetPendingOrders()
// {
//     // Fetch pending orders
//     var orders = await _db.Orders
//         .Where(o => o.Status == "Pending")
//         .Select(o => new
//         {
//             o.Id,
//             o.Code,
//             o.TotalAmount,
//             o.Status,
//             o.CreatedAt,
//             o.DeliveryStatus
//         })
//         .ToListAsync();

//     // Fetch all items for these orders
//     var orderCodes = orders.Select(o => o.Code).ToList();
//     var items = await _db.OrderItems
//         .Where(oi => orderCodes.Contains(oi.Code))
//         .Select(oi => new
//         {
//             oi.Code,
//             oi.ProductName,
//             oi.Quantity,
//             oi.UnitPrice,
//             TotalPrice = oi.Quantity * oi.UnitPrice
//         })
//         .ToListAsync();

//     // Attach items to each order
//     var result = orders.Select(o => new
//     {
//         o.Id,
//         o.Code,
//         o.TotalAmount,
//         o.Status,
//         o.CreatedAt,
//         o.DeliveryStatus,
//         Items = items.Where(i => i.Code == o.Code).ToList()
//     });

//     return Ok(result);
// }

// GET api/delivery/pending
[HttpGet("pending")]
public async Task<IActionResult> GetPendingOrders()
{
    var orders = await (
        from o in _db.Orders
        join u in _db.Users on o.UserId equals u.Id
        join d in _db.DeliveryInfo on o.Code equals d.Code into deliveryJoin
        from d in deliveryJoin.DefaultIfEmpty() // LEFT JOIN (important)
        where o.Status == "Pending"
        select new
        {
            code = o.Code,
            userName = u.FullName,
            dilveryName = d.Name,
            address = d != null ? d.Address : "Address not available"
        }
    ).ToListAsync();

    return Ok(orders);
}

// [HttpGet("order/{code}")]
// public async Task<IActionResult> GetOrderByCode(string code)
// {
//     var order = await _db.Orders
//         .Where(o => o.Code == code)
//         .Select(o => new 
//         {
//             o.Id,
//             o.Code,
//             o.TotalAmount,
//             o.Status,
//             o.CreatedAt,
//             DeliveryStatus = o.DeliveryStatus,
//             Items = _db.OrderItems
//                         .Where(i => i.Code == o.Code)
//                         .Select(i => new {
//                             i.ProductName,
//                             i.Quantity,
//                             i.UnitPrice,
//                             TotalPrice = i.Quantity * i.UnitPrice
//                         }).ToList()
//         })
//         .FirstOrDefaultAsync();

//     if (order == null)
//         return NotFound();

//     return Ok(order);
// }

[HttpGet("order/{code}")]
public async Task<IActionResult> GetOrderByCode(string code)
{
    Console.WriteLine($"[GetOrderByCode] Called with code: {code}");

    var order = await _db.Orders
        .Where(o => o.Code == code)
        .Select(o => new 
        {
            o.Id,
            o.Code,
            o.TotalAmount,
            o.Status,
            o.CreatedAt,
            DeliveryStatus = o.DeliveryStatus,
            Items = _db.OrderItems
                        .Where(i => i.Code == o.Code)
                        .Select(i => new {
                            i.ProductName,
                            i.Quantity,
                            i.UnitPrice,
                            TotalPrice = i.Quantity * i.UnitPrice
                        }).ToList()
        })
        .FirstOrDefaultAsync();

    if (order == null)
    {
        Console.WriteLine($"[GetOrderByCode] Order not found for code: {code}");
        return NotFound();
    }

    Console.WriteLine($"[GetOrderByCode] Order found: Id={order.Id}, Code={order.Code}, TotalAmount={order.TotalAmount}");
    Console.WriteLine($"[GetOrderByCode] Items count: {order.Items.Count}");
    foreach (var item in order.Items)
    {
        Console.WriteLine($"    Item: {item.ProductName}, Qty: {item.Quantity}, UnitPrice: {item.UnitPrice}, TotalPrice: {item.TotalPrice}");
    }

    return Ok(order);
}


// // GET api/delivery/order/{code}
// [HttpGet("order/{code}")]
// public async Task<IActionResult> GetOrderByCode(string code)
// {
//     var order = await _db.Orders
//         .Where(o => o.Code == code)
//         .Select(o => new
//         {
//             o.Id,
//             o.Code,
//             o.TotalAmount,
//             o.Status,
//             Items = _db.OrderItems
//                 .Where(i => i.Code == o.Code)
//                 .Select(i => new
//                 {
//                     i.ProductName,
//                     i.Quantity,
//                     i.UnitPrice,
//                     TotalPrice = i.Quantity * i.UnitPrice
//                 })
//                 .ToList()
//         })
//         .FirstOrDefaultAsync();

//     if (order == null)
//         return NotFound();

//     return Ok(order);
// }


// POST api/delivery/mark-delivered/{code}
[HttpPost("mark-delivered/{code}")]
public async Task<IActionResult> MarkDelivered(string code)
{
    var order = await _db.Orders.FirstOrDefaultAsync(o => o.Code == code);

    if (order == null)
        return NotFound();

    order.Status = "Delivered";
    order.DeliveryStatus = "Completed";
    order.DeliveredAt = DateTime.UtcNow;

    await _db.SaveChangesAsync();

    return Ok();
}


//===================================================================================
    
//         // GET api/delivery/pending
// [HttpGet("pending")]
// public async Task<IActionResult> GetPendingOrders()
// {
//     // Fetch only the orders needed for delivery list
//     var orders = await _db.Orders
//         //.Where(o => o.Status != "Delivered" && o.DeliveryPersonId == null) // unassigned orders
//         .Where(o => o.Status == "Pending")
//         .Select(o => new
//         {
//             o.Id,
//             o.Code,
//             o.TotalAmount,
//             o.Status,
//             o.CreatedAt,        
//             o.DeliveryStatus
//             // Add more fields you need, but avoid navigation properties
//         })
//         .ToListAsync();

//     return Ok(orders);
// }


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

       
       [HttpPost("attendance")]
        public async Task<IActionResult> MarkAttendance(
            [FromHeader(Name = "deliveryPersonId")] string deliveryPersonId,
            [FromBody] string date)
        {
            if (string.IsNullOrWhiteSpace(deliveryPersonId))
                return BadRequest("Missing deliveryPersonId header");

            // ✅ Id is STRING
            var deliveryPerson = await _db.DeliveryPerson
                .FirstOrDefaultAsync(dp => dp.Id == deliveryPersonId);

            if (deliveryPerson == null)
                return NotFound("Delivery person not found");

            var parsedDate = DateTime.Parse(date);

            // ✅ Compare DATE ONLY (important)
            var existing = await _db.DeliveryAttendance
                .FirstOrDefaultAsync(a =>
                    a.DeliveryPersonId == deliveryPersonId &&
                    a.Date.Date == parsedDate.Date);

            if (existing != null)
                return BadRequest("Attendance already marked");

            var attendance = new DeliveryAttendance
            {
                DeliveryPersonId = deliveryPersonId, // ✅ string
                Date = parsedDate,
                IsPresent = true,
                CheckInTime = DateTime.Now
            };

            _db.DeliveryAttendance.Add(attendance);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Attendance marked successfully" });
        }

    
    }
}
