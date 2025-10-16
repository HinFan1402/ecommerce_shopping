using System;
using System.ComponentModel.DataAnnotations;

namespace ecommerce_shopping.Views.Shared.Validations
{
    public class FileExtensionAttribute :ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string[] extensions = { ".jpg", ".png", ".jpeg" };
                
                bool result= extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Allowes extensions are JPG| PNG|JPEG");
                }
            }
            return ValidationResult.Success;
            
        }
    }
}
