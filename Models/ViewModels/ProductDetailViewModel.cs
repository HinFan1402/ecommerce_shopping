namespace ecommerce_shopping.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public ProductModel Product { get; set; }
        public IEnumerable<RatingModel> Ratings { get; set; }
        public RatingModel NewRating { get; set; } 
    }
}
