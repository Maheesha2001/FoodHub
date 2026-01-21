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
        //     var orders = await (
        //         from o in _db.Orders
        //         join u in _db.Users on o.UserId equals u.Id
        //         join d in _db.DeliveryInfo on o.Code equals d.Code into deliveryJoin
        //         from d in deliveryJoin.DefaultIfEmpty() // LEFT JOIN (important)
        //         where o.Status == "Pending"
        //         select new
        //         {
        //             code = o.Code,
        //             userName = u.FullName,
        //             dilveryName = d.Name,
        //             address = d != null ? d.Address : "Address not available"
        //         }
        //     ).ToListAsync();

        //     return Ok(orders);
        // }

        [HttpGet("assigned/today/{driverId}")]
    public async Task<IActionResult> GetTodaysAssignedOrders(string driverId)
    {
        // Log the received driverId
        Console.WriteLine($"[DEBUG] GetTodaysAssignedOrders called with driverId = {driverId}");

        var today = DateTime.Today;
        Console.WriteLine($"[DEBUG] Today's date = {today.ToShortDateString()}");

        var orders = await (
            from d in _db.DeliveryOrderAssignments
            join o in _db.Orders on d.OrderCode equals o.Code
            join u in _db.Users on o.UserId equals u.Id
            join p in _db.DeliveryInfo on o.Code equals p.Code
            where d.DeliveryPersonId == driverId
                && d.AssignedAt.Date == today
            select new
            {
                orderCode = o.Code,
                customerName = u.FullName,
                dilveryName = p.Name,
                address = p.Address,
                assignedAt = d.AssignedAt,
                pickedUpAt = d.PickedUpAt,
                deliveredAt = d.DeliveredAt,
                status = d.Status
            }
        ).ToListAsync();

        // Log how many orders were retrieved
        Console.WriteLine($"[DEBUG] Orders retrieved = {orders.Count}");

        // Optionally, log each order for debugging
        foreach (var order in orders)
        {
            Console.WriteLine($"[DEBUG] Order: {order.orderCode}, Customer: {order.customerName}, Address: {order.address}, Status: {order.status}, FullName: {order.dilveryName}");
        }

        return Ok(orders);
    }


        // [HttpGet("assigned/today/{driverId}")]
        // public async Task<IActionResult> GetTodaysAssignedOrders(string driverId)
        // {
        //     var today = DateTime.Today; // get current day

        //     var orders = await (
        //         from d in _db.DeliveryOrderAssignments
        //         join o in _db.Orders on d.OrderCode equals o.Code
        //         join u in _db.Users on o.UserId equals u.Id
        //         join p in _db.DeliveryInfo on o.Code equals p.Code
        //         where d.DeliveryPersonId == driverId
        //             && d.AssignedAt.Date == today // only orders assigned today
        //         select new
        //         {
        //             orderCode = o.Code,
        //             customerName = u.FullName,
        //             address = p.Address,
        //             assignedAt = d.AssignedAt,
        //             pickedUpAt = d.PickedUpAt,
        //             deliveredAt = d.DeliveredAt,
        //             status = d.Status
        //         }
        //     ).ToListAsync();

        //     return Ok(orders);
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

        // POST api/delivery/mark-delivered/{code}
        [HttpPost("mark-delivered/{code}")]
        public async Task<IActionResult> MarkDelivered(string code)
        {
            var order = await _db.Orders
                .FirstOrDefaultAsync(o => o.Code == code);

            if (order == null)
                return NotFound("Order not found");

            var deliveryInfo = await _db.DeliveryInfo
                .FirstOrDefaultAsync(d => d.Code == code);

            if (deliveryInfo == null)
                return NotFound("Delivery info not found");

            var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.Code == code);
            

            // Update Order
            order.Status = "Completed";
            order.DeliveryStatus = "Completed";
            order.DeliveredAt = DateTime.UtcNow;

            // Update DeliveryInfo
            deliveryInfo.DeliveryStatus = "Delivered";
            deliveryInfo.DeliveredAt = DateTime.UtcNow; // optional

            // Payments
                payment.PaymentStatus = "Paid";
            

            await _db.SaveChangesAsync();

            return Ok(new { message = "Order marked as delivered" });
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
