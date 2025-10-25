using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace ecommerce_shopping.Models
{
    public class CompareModel
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string User { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel Product { get; set; }
    }
}
