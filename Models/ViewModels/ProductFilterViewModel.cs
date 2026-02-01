namespace ecommerce_shopping.Models.ViewModels
{
    public class ProductFilterViewModel
    {
        public string SortBy { get; set; } 
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string SearchQuery { get; set; }

        // Thông tin hiện tại để build URL
        public string CurrentController { get; set; }
        public string CurrentAction { get; set; }
        public Dictionary<string, string> RouteValues { get; set; } = new Dictionary<string, string>();
    }
}
