using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class OrderDetailModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public int ProductId { get;set; }
        public decimal Price { get;set; }
        public int Quantity { get; set; }
        public decimal Total =>Price* Quantity;

        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
