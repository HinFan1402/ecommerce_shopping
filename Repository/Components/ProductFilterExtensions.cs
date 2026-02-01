// Repository/ProductFilterExtensions.cs
using ecommerce_shopping.Models;
using ecommerce_shopping.Models.ViewModel;
using ecommerce_shopping.Models.ViewModels;

namespace ecommerce_shopping.Repository
{
    public static class ProductFilterExtensions
    {
        public static IQueryable<ProductModel> ApplyFilters(
            this IQueryable<ProductModel> query,
            ProductFilterViewModel filter)
        {
            // Filter by price range
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Filter by category
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            // Filter by brand
            if (filter.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == filter.BrandId.Value);
            }

            // Filter by search query
            if (!string.IsNullOrEmpty(filter.SearchQuery))
            {
                query = query.Where(p =>
                    p.Name.Contains(filter.SearchQuery) ||
                    p.Description.Contains(filter.SearchQuery)
                );
            }

            // Apply sorting
            query = filter.SortBy switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "name-asc" => query.OrderBy(p => p.Name),
                "name-desc" => query.OrderByDescending(p => p.Name),
                "popular" => query.OrderByDescending(p => p.Sold),
                "newest" => query.OrderByDescending(p => p.Id),
                _ => query.OrderByDescending(p => p.Id) // Default
            };

            return query;
        }
    }
}