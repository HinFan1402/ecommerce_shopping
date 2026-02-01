using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan (không đổi theo filter)
        public int TotalCategories { get; set; }
        public int TotalBrands { get; set; }
        public int TotalProducts { get; set; }

        // Thống kê đơn hàng (theo filter)
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int ApprovedOrders { get; set; }
        public int DeliveredOrders { get; set; }

        // Thống kê doanh thu và lợi nhuận
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalRevenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalProfit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; }

        public int TotalProductsSold { get; set; }

        // Biểu đồ doanh số
        public List<SalesChartData> SalesChart { get; set; }

        // Bảng thống kê chi tiết
        public List<CategoryStatistic> CategoryStatistics { get; set; }
        public List<ProductStatistic> ProductStatistics { get; set; }

        // Sản phẩm bán chạy
        public List<TopProductViewModel> TopSellingProducts { get; set; }

        // Filter
        public string FilterType { get; set; } = "day"; // day, week, month, year
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class SalesChartData
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public decimal Profit { get; set; }
        public int Quantity { get; set; }
    }

    public class CategoryStatistic
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int TotalProducts { get; set; }
        public int QuantitySold { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
    }

    public class ProductStatistic
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public int QuantitySold { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int QuantitySold { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit { get; set; }
    }
}