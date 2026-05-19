using Microsoft.AspNetCore.Mvc;
using AliMertRestoran.Models;
using Microsoft.EntityFrameworkCore;
using AliMertRestoran.Extensions;

namespace AliMertRestoran.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string tableNumber)
        {
            if (!string.IsNullOrEmpty(tableNumber))
            {
                HttpContext.Session.SetString("TableNumber", tableNumber);
            }

            var currentTable = HttpContext.Session.GetString("TableNumber");
            if (string.IsNullOrEmpty(currentTable))
            {
                return View("ScanQR");
            }

            ViewBag.CurrentTable = currentTable;
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        [HttpPost]
        public IActionResult LeaveTable()
        {
            HttpContext.Session.Remove("TableNumber");
            HttpContext.Session.Remove("Cart"); // Sepeti de temizle
            return Ok();
        }
    }
}
