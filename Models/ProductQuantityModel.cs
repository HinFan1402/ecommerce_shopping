using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class ProductQuantityModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Nhập mã sản phẩm")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Nhập số lượng sản phẩm")]
        public int Quantity { get; set; }
        public DateTime DateCreated { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
