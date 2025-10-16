using ecommerce_shopping.Views.Shared.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập tên Sản phẩm!")]
        public string Name { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập mô tả sản phẩm!")]
        public string Description { get; set; }
        public string Slug { get; set; }
        [Required,Range(0.001,double.MaxValue, ErrorMessage = "Yêu cầu nhập giá sản phẩm!")]
        public decimal Price { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn 1 thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn 1 Danh mục")]
        public int CategoryId { get; set; }
        
        public CategoryModel Category { get; set; }
       
        public BrandModel Brand { get; set; }
        [Required]
        public string Image { get; set; } = "noimage.jpg";
        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload {  get; set; }
    }
}
