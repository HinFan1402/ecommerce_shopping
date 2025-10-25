using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập nội dung đánh giá!")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email!")]
        public string Email { get; set; }
        
        public string rating { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }

    }
}
