using ecommerce_shopping.Models;
using Microsoft.EntityFrameworkCore;

namespace ecommerce_shopping.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel Laptop = new CategoryModel { Name = "Laptop", Slug = "laptop", Description = "Suitable for every task", Status = 1 };
                CategoryModel SmartPhone = new CategoryModel { Name = "SmartPhone", Slug = "smartphone", Description = "Flexible, multitasking ", Status = 1 };
                BrandModel Apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Discover the innovative world of Apple", Status = "1" };
                BrandModel SamSung = new BrandModel { Name = "SamSung", Slug = "samsung", Description = "Do What You Can't.", Status = "1" };
                _context.Products.AddRange(
                    new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is the best", Image = "1.jpg", Category = Laptop, Price = 1234, Brand = Apple },
                    new ProductModel { Name = "Galaxy S24", Slug = "galaxys24", Description = "Excellent battery life, lasts all day.", Image = "2.jpg", Category = SmartPhone, Price = 1111, Brand = SamSung }
                );
                _context.SaveChanges();
            }
        }
    }
}
