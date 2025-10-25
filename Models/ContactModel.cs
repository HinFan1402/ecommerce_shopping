using ecommerce_shopping.Views.Shared.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class ContactModel
    {

        [Required(ErrorMessage = "Nhập tên website")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Nhập mô tả website")]
        public string Description { get; set; }
        public string logoImgs { get; set; }= "noImage";
        [Required(ErrorMessage = "Nhập link định vị shop")]
        public string Maplink { get; set; }
        [Required(ErrorMessage = "Nhập địa chỉ shop")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Nhập số điện thoại shop")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Nhập email shop"),EmailAddress]
        public string Email { get; set; }
        [NotMapped]
        [FileExtension]

        public IFormFile logoImgUpload { get; set; }
    }
}
