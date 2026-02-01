using ecommerce_shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce_shopping.Controllers
{
    [Area("Admin")]
    [Route("Admin/Dashboard")]
    [Authorize(Roles = "Admin")]

    public class DashboardController : Controller
    {
        private readonly IStatisticalService _statisticalService;

        public DashboardController(IStatisticalService statisticalService)
        {
            _statisticalService = statisticalService;
        }
        [Route("Index")]
        public async Task<IActionResult> Index(string filterType = "day", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var dashboard = await _statisticalService.GetDashboardDataAsync(filterType, fromDate, toDate);
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string filterType = "day", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var dashboard = await _statisticalService.GetDashboardDataAsync(filterType, fromDate, toDate);
            return Json(new
            {
                success = true,
                data = dashboard.SalesChart,
                summary = new
                {
                    totalOrders = dashboard.TotalOrders,
                    newOrders = dashboard.NewOrders,
                    approvedOrders = dashboard.ApprovedOrders,
                    deliveredOrders = dashboard.DeliveredOrders,
                    totalRevenue = dashboard.TotalRevenue,
                    totalProfit = dashboard.TotalProfit,
                    totalProductsSold = dashboard.TotalProductsSold
                },
                categoryStats = dashboard.CategoryStatistics,
                productStats = dashboard.ProductStatistics,
                topProducts = dashboard.TopSellingProducts
            });
        }
    }
}