using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModels;
using ecommerce_shopping.Repository;
using Microsoft.EntityFrameworkCore;
using System;

namespace ecommerce_shopping.Services
{
    public interface IStatisticalService
    {
        Task<StatisticModel> CreateStatisticalAsync(OrderDetailModel orderDetail, int orderStatus = 0, string couponCode = null, decimal discountAmount = 0);
        Task<DashboardViewModel> GetDashboardDataAsync(string filterType, DateTime? fromDate, DateTime? toDate);
        Task UpdateOrderStatusAsync(string orderCode, int newStatus);
    }

    public class StatisticalService : IStatisticalService
    {
        private readonly DataContext _context;

        public StatisticalService(DataContext context)
        {
            _context = context;
        }

        public async Task<StatisticModel> CreateStatisticalAsync(OrderDetailModel orderDetail, int orderStatus = 0, string couponCode = null, decimal discountAmount = 0)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);

            if (product == null) return null;

            var statistical = new StatisticModel
            {
                OrderCode = orderDetail.OrderCode,
                UserName = orderDetail.UserName,
                ProductId = orderDetail.ProductId,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                BrandId = product.BrandId,
                BrandName = product.Brand.Name,
                Quantity = orderDetail.Quantity,
                SellingPrice = orderDetail.Price,
                CostPrice = product.InPrice,
                DiscountAmount = discountAmount,
                CouponCode = couponCode,
                OrderDate = DateTime.Now,
                OrderStatus = orderStatus
            };

            _context.Statistics.Add(statistical);
            await _context.SaveChangesAsync();

            return statistical;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string filterType, DateTime? fromDate, DateTime? toDate)
        {
            // Xác định khoảng thời gian filter
            var (startDate, endDate) = GetDateRange(filterType, fromDate, toDate);

            // Thống kê cố định
            var totalCategories = await _context.Categories.CountAsync();
            var totalBrands = await _context.Brands.CountAsync();
            var totalProducts = await _context.Products.CountAsync();

            // Lấy dữ liệu statistical theo filter
            var statistics = await _context.Statistics
                .Include(s => s.Product)
                .Where(s => s.OrderDate >= startDate && s.OrderDate <= endDate && s.OrderStatus != 4)
                .ToListAsync();

            // Thống kê đơn hàng
            var orderCodes = statistics.Select(s => s.OrderCode).Distinct().ToList();
            var totalOrders = orderCodes.Count;
            var newOrders = statistics.Where(s => s.OrderStatus == 0).Select(s => s.OrderCode).Distinct().Count();
            var approvedOrders = statistics.Where(s => s.OrderStatus == 1).Select(s => s.OrderCode).Distinct().Count();
            var deliveredOrders = statistics.Where(s => s.OrderStatus == 3).Select(s => s.OrderCode).Distinct().Count();

            // Thống kê doanh thu
            var totalRevenue = statistics.Sum(s => s.NetRevenue);
            var totalProfit = statistics.Sum(s => s.Profit);
            var totalDiscount = statistics.Sum(s => s.DiscountAmount);
            var totalProductsSold = statistics.Sum(s => s.Quantity);

            // Biểu đồ doanh số
            var salesChart = GetSalesChartData(statistics, filterType, startDate, endDate);

            // Thống kê theo danh mục
            var categoryStats = statistics
                .GroupBy(s => new { s.CategoryId, s.CategoryName })
                .Select(g => new CategoryStatistic
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    TotalProducts = g.Select(x => x.ProductId).Distinct().Count(),
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.NetRevenue),
                    Profit = g.Sum(x => x.Profit)
                })
                .OrderByDescending(c => c.Revenue)
                .ToList();

            // Thống kê theo sản phẩm
            var productStats = statistics
                .GroupBy(s => new { s.ProductId, s.ProductName, s.CategoryName, s.BrandName })
                .Select(g => new ProductStatistic
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    CategoryName = g.Key.CategoryName,
                    BrandName = g.Key.BrandName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.NetRevenue),
                    Profit = g.Sum(x => x.Profit)
                })
                .OrderByDescending(p => p.QuantitySold)
                .ToList();

            // Top sản phẩm bán chạy
            var topProducts = statistics
                .GroupBy(s => new { s.ProductId, s.ProductName, s.Product.Image })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    ProductImage = g.Key.Image,
                    QuantitySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.NetRevenue),
                    Profit = g.Sum(x => x.Profit)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(10)
                .ToList();

            return new DashboardViewModel
            {
                TotalCategories = totalCategories,
                TotalBrands = totalBrands,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                NewOrders = newOrders,
                ApprovedOrders = approvedOrders,
                DeliveredOrders = deliveredOrders,
                TotalRevenue = totalRevenue,
                TotalProfit = totalProfit,
                TotalDiscount = totalDiscount,
                TotalProductsSold = totalProductsSold,
                SalesChart = salesChart,
                CategoryStatistics = categoryStats,
                ProductStatistics = productStats,
                TopSellingProducts = topProducts,
                FilterType = filterType,
                FromDate = startDate,
                ToDate = endDate
            };
        }

        public async Task UpdateOrderStatusAsync(string orderCode, int newStatus)
        {
            var statistics = await _context.Statistics
                .Where(s => s.OrderCode == orderCode)
                .ToListAsync();

            foreach (var stat in statistics)
            {
                stat.OrderStatus = newStatus;
            }

            await _context.SaveChangesAsync();
        }

        private (DateTime startDate, DateTime endDate) GetDateRange(string filterType, DateTime? fromDate, DateTime? toDate)
        {
            var now = DateTime.Now;
            DateTime startDate, endDate;

            if (fromDate.HasValue && toDate.HasValue)
            {
                return (fromDate.Value.Date, toDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            switch (filterType.ToLower())
            {
                case "week":
                    startDate = now.AddDays(-(int)now.DayOfWeek).Date;
                    endDate = startDate.AddDays(7).AddSeconds(-1);
                    break;
                case "month":
                    startDate = new DateTime(now.Year, now.Month, 1);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                    break;
                case "year":
                    startDate = new DateTime(now.Year, 1, 1);
                    endDate = startDate.AddYears(1).AddSeconds(-1);
                    break;
                default: // day
                    startDate = now.Date;
                    endDate = now.Date.AddDays(1).AddSeconds(-1);
                    break;
            }

            return (startDate, endDate);
        }

        private List<SalesChartData> GetSalesChartData(List<StatisticModel> statistics, string filterType, DateTime startDate, DateTime endDate)
        {
            switch (filterType.ToLower())
            {
                case "day":
                    return statistics
                        .GroupBy(s => s.OrderDate.Hour)
                        .Select(g => new SalesChartData
                        {
                            Label = $"{g.Key}:00",
                            Revenue = g.Sum(x => x.NetRevenue),
                            Profit = g.Sum(x => x.Profit),
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.Label)
                        .ToList();

                case "week":
                    return statistics
                        .GroupBy(s => s.OrderDate.Date)
                        .Select(g => new SalesChartData
                        {
                            Label = g.Key.ToString("dd/MM"),
                            Revenue = g.Sum(x => x.NetRevenue),
                            Profit = g.Sum(x => x.Profit),
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.Label)
                        .ToList();

                case "month":
                    return statistics
                        .GroupBy(s => s.OrderDate.Date)
                        .Select(g => new SalesChartData
                        {
                            Label = g.Key.Day.ToString(),
                            Revenue = g.Sum(x => x.NetRevenue),
                            Profit = g.Sum(x => x.Profit),
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => int.Parse(x.Label))
                        .ToList();

                case "year":
                    return statistics
                        .GroupBy(s => s.OrderDate.Month)
                        .Select(g => new SalesChartData
                        {
                            Label = $"Tháng {g.Key}",
                            Revenue = g.Sum(x => x.NetRevenue),
                            Profit = g.Sum(x => x.Profit),
                            Quantity = g.Sum(x => x.Quantity)
                        })
                        .OrderBy(x => x.Label)
                        .ToList();

                default:
                    return new List<SalesChartData>();
            }
        }
    }
}