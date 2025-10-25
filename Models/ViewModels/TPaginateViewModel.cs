namespace ecommerce_shopping.Models.ViewModels
{
    public class TPaginateViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public Paginate Pager { get; set; }
    }
}
