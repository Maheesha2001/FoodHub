using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodHub.Data;
using FoodHub.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FoodHub.Controllers.Admin
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        private readonly FoodHubContext _context;

        public OrdersController(FoodHubContext context)
        {
            _context = context;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)   // ✅ Include payment info
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryInfo)
                .Include(o => o.Payment)   // ✅ Include payment info
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}
