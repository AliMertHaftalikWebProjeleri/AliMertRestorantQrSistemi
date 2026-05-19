using Microsoft.AspNetCore.Mvc;
using AliMertRestoran.Models;
using AliMertRestoran.Extensions;

namespace AliMertRestoran.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            var existingItem = cart.FirstOrDefault(c => c.Product.Id == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }

            HttpContext.Session.Set("Cart", cart);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cart == null || !cart.Any()) return RedirectToAction("Index");

            var tableNumber = HttpContext.Session.GetString("TableNumber") ?? "Bilinmeyen Masa";

            var order = new Order
            {
                TableNumber = tableNumber,
                TotalAmount = cart.Sum(c => c.TotalPrice),
                OrderItems = cart.Select(c => new OrderItem
                {
                    ProductId = c.Product.Id,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            TempData["SuccessMessage"] = "Siparişiniz başarıyla alındı!";
            return RedirectToAction("Index", "Home");
        }
    }
}
