using Microsoft.AspNetCore.Mvc;
using FoodHub.Data;
using FoodHub.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodHub.Api.Controllers
{
[Route("api/attendance")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly FoodHubContext _context;
    public AttendanceController(FoodHubContext ctx) { _context = ctx; }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn(string driverId)
    {
        var today = DateTime.UtcNow.Date;

        var record = await _context.DeliveryAttendance
            .FirstOrDefaultAsync(x => x.DeliveryPersonId == driverId && x.Date == today);

        if (record == null)
        {
            _context.DeliveryAttendance.Add(new DeliveryAttendance
            {
                DeliveryPersonId = driverId,
                Date = today,
                IsPresent = true,
                CheckInTime = DateTime.UtcNow
            });
        }
        else
        {
            record.IsPresent = true;
            record.CheckInTime = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut(string driverId)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.DeliveryAttendance
            .FirstOrDefaultAsync(x => x.DeliveryPersonId == driverId && x.Date == today);

        if (record != null)
        {
            record.IsPresent = false;
            record.CheckOutTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}
}