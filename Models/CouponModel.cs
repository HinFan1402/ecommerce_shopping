using System.ComponentModel.DataAnnotations;

namespace ecommerce_shopping.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mã khuyến mãi!")]
        public string CouponCode { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập thông tin giảm giá!")]
        public string Description { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập giá trị mã giảm giá!")]
        public decimal PriceCoupon { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập số lượng mã giảm giá!")]
        public int Quantity { get; set; }
        public int Status { get; set; }
    }
}
