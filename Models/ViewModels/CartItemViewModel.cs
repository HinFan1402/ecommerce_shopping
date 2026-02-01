namespace ecommerce_shopping.Models.ViewModel
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems {  get; set; }
        public decimal GrandTotal { get; set; }
        public decimal ShippingCost { get; set; } = 0;
       

        // Thông tin giao hàng
        public string ShippingName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingProvince { get; set; }
        public string ShippingDistrict { get; set; }
        public string ShippingWard { get; set; }
        public string ShippingAddress { get; set; }
        // Thông tin coupom
        public string CouponCode { get; set; }
        public decimal PriceCoupon { get;set; }
        public decimal FinalTotal { get; set; }

        public string PaymentMethod { get; set; } = "Cash";
    }
}
