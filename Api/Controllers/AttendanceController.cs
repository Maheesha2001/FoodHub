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
        var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "Sri Lanka Standard Time"
                );

        var today = sriLankaTime.Date;

        var record = await _context.DeliveryAttendance
            .FirstOrDefaultAsync(x => x.DeliveryPersonId == driverId && x.Date == today);

        if (record == null)
        {
            _context.DeliveryAttendance.Add(new DeliveryAttendance
            {
                DeliveryPersonId = driverId,
                Date = today,
                IsPresent = true,
                CheckInTime = sriLankaTime
            });
        }
        else
        {
            record.IsPresent = true;
            record.CheckInTime = sriLankaTime;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut(string driverId)
    {
        var sriLankaTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "Sri Lanka Standard Time"
                );

        var today = sriLankaTime.Date;

        var record = await _context.DeliveryAttendance
            .FirstOrDefaultAsync(x => x.DeliveryPersonId == driverId && x.Date == today);

        if (record != null)
        {
            record.IsPresent = false;
            record.CheckOutTime = sriLankaTime;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }
}
}