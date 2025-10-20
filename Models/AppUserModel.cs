using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecommerce_shopping.Models
{
    public class AppUserModel: IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { get; set; }
        [NotMapped]
        [DataType(DataType.Password)]
        public string PassWordNoHash { get; set; }
    }
}
