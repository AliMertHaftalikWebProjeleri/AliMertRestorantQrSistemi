using Microsoft.AspNetCore.Mvc;
using AliMertRestoran.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.IO;
using iText.Html2pdf;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace AliMertRestoran.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminCookies")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            // Günlük İstatistikler
            var todayOrders = await _context.Orders.Where(o => o.OrderDate.Date == today).ToListAsync();
            ViewBag.TotalOrdersToday = todayOrders.Count;
            ViewBag.TotalRevenueToday = todayOrders.Sum(o => o.TotalAmount);

            // Tüm zamanların toplam cirosu
            ViewBag.TotalRevenueAllTime = await _context.Orders.SumAsync(o => o.TotalAmount);

            // Grafik: Tüm zamanların en çok tercih edilen 5 ürünü
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new { ProductName = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            ViewBag.ChartLabels = topProducts.Select(x => x.ProductName).ToArray();
            ViewBag.ChartData = topProducts.Select(x => x.TotalQuantity).ToArray();

            return View();
        }

        // Ürünler sekmesi artık "Günlük Özet" işlevi görüyor
        public async Task<IActionResult> Products()
        {
            var today = DateTime.Today;

            // Günlük en çok tercih edilen yiyecekler
            var dailySummary = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order != null && oi.Order.OrderDate.Date == today)
                .GroupBy(oi => oi.Product != null ? oi.Product.Name : "Bilinmeyen Ürün")
                .Select(g => new DailyProductSummary
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();

            ViewBag.TodayDate = today.ToString("dd.MM.yyyy");
            return View(dailySummary);
        }

        public IActionResult QRCodes()
        {
            return View();
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var today = DateTime.Today;
            var dailySummary = await GetDailySummaryAsync(today);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Günlük Özet");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Tarih:";
                worksheet.Cell(currentRow, 2).Value = today.ToString("dd.MM.yyyy");
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Ürün Adı";
                worksheet.Cell(currentRow, 2).Value = "Satış Adedi";
                worksheet.Cell(currentRow, 3).Value = "Elde Edilen Gelir";

                // Başlık Stilleri
                var headerRange = worksheet.Range(currentRow, 1, currentRow, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                foreach (var item in dailySummary)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = item.ProductName;
                    worksheet.Cell(currentRow, 2).Value = item.TotalQuantity;
                    worksheet.Cell(currentRow, 3).Value = item.TotalRevenue;
                    worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00 ₺";
                }

                currentRow++;
                worksheet.Cell(currentRow, 2).Value = "Genel Toplam:";
                worksheet.Cell(currentRow, 2).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 3).Value = dailySummary.Sum(x => x.TotalRevenue);
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00 ₺";

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Gunluk_Ozet_{today:dd_MM_yyyy}.xlsx");
                }
            }
        }

        public async Task<IActionResult> ExportToPdf()
        {
            var today = DateTime.Today;
            var dailySummary = await GetDailySummaryAsync(today);

            decimal totalRevenue = dailySummary.Sum(x => x.TotalRevenue);

            var htmlContent = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                    h1 {{ color: #333; }}
                    .total {{ font-weight: bold; text-align: right; margin-top: 20px; font-size: 18px; }}
                </style>
            </head>
            <body>
                <h1>Günlük Satış Özeti</h1>
                <p><strong>Tarih:</strong> {today:dd.MM.yyyy}</p>
                <table>
                    <thead>
                        <tr>
                            <th>Ürün Adı</th>
                            <th>Satış Adedi</th>
                            <th>Elde Edilen Gelir</th>
                        </tr>
                    </thead>
                    <tbody>";

            foreach (var item in dailySummary)
            {
                htmlContent += $@"
                        <tr>
                            <td>{item.ProductName}</td>
                            <td>{item.TotalQuantity}</td>
                            <td>{item.TotalRevenue:C}</td>
                        </tr>";
            }

            htmlContent += $@"
                    </tbody>
                </table>
                <div class='total'>Genel Toplam: {totalRevenue:C}</div>
            </body>
            </html>";

            using (var stream = new MemoryStream())
            {
                HtmlConverter.ConvertToPdf(htmlContent, stream);
                return File(stream.ToArray(), "application/pdf", $"Gunluk_Ozet_{today:dd_MM_yyyy}.pdf");
            }
        }

        private async Task<List<DailyProductSummary>> GetDailySummaryAsync(DateTime date)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .Where(oi => oi.Order != null && oi.Order.OrderDate.Date == date)
                .GroupBy(oi => oi.Product != null ? oi.Product.Name : "Bilinmeyen Ürün")
                .Select(g => new DailyProductSummary
                {
                    ProductName = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToListAsync();
        }
    }

    public class DailyProductSummary
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
