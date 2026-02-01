using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class StatisticModel
    {
        [Key]
        public int Id { get; set; }

        // Thông tin đơn hàng
        [Required]
        public string OrderCode { get; set; }

        // Thông tin khách hàng
        [Required]
        public string UserName { get; set; }

        // Thông tin sản phẩm
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        public string BrandName { get; set; }

        [Required]
        public int Quantity { get; set; }

        // Thông tin giá
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // Giá bán

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPrice { get; set; } // Giá nhập (vốn)

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0; // Số tiền giảm giá

        public string CouponCode { get; set; }

        // Tính toán tự động
        [NotMapped]
        public decimal TotalRevenue => SellingPrice * Quantity; // Doanh thu

        [NotMapped]
        public decimal TotalCost => CostPrice * Quantity; // Tổng vốn

        [NotMapped]
        public decimal NetRevenue => TotalRevenue - DiscountAmount; // Doanh thu sau giảm giá

        [NotMapped]
        public decimal Profit => (SellingPrice - CostPrice) * Quantity - DiscountAmount; // Lợi nhuận

        // Thông tin thời gian
        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Trạng thái đơn hàng: 0=Mới, 1=Đã duyệt, 2=Đang giao, 3=Đã giao, 4=Hủy
        [Required]
        public int OrderStatus { get; set; }

        // Navigation Properties
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}