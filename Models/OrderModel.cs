namespace ecommerce_shopping.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreateDate { get; set; }
        public int status { get; set; }

        public string ShippingProvince { get; set; }  // Tên tỉnh/thành
        public string ShippingDistrict { get; set; }  // Quận/huyện
        public string ShippingWard { get; set; }      // Phường/xã
        public string ShippingAddress { get; set; }   // Địa chỉ chi tiết
        public string ShippingPhone { get; set; }     // Số điện thoại
        public string ShippingName { get; set; }      // Tên người nhận
        public decimal ShippingCost { get; set; }     // Phí ship
        public string CouponCode { get; set; }
        public decimal PriceCoupon { get; set; }
        public decimal FinalTotal { get; set; }

        public string PaymentMethod { get; set; }        
        public PaymentStatus PaymentStatus { get; set; } 
        public string PaymentTransactionId { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }
}
