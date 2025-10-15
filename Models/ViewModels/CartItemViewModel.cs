namespace ecommerce_shopping.Models.ViewModel
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems {  get; set; }
        public decimal GrandTotal { get; set; }
    }
}
