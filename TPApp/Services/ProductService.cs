using Microsoft.EntityFrameworkCore;
using TPApp.Data;
using TPApp.Entities;
using TPApp.Interfaces;

namespace TPApp.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SanPhamCNTB>> GetNewProductsAsync(int take)
        {
            return await _context.SanPhamCNTBs
                .Where(x => x.StatusId == 3
                         && x.LanguageId == 1
                         && x.bEffectiveDate <= DateTime.Now
                         && x.eEffectiveDate >= DateTime.Now)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take)
        {
            var cateKey = $";{catId};";
            return await _context.SanPhamCNTBs
                .Where(x => x.LanguageId == languageId 
                         && x.StatusId == 3 
                         && x.CategoryId != null 
                         && x.CategoryId.Contains(cateKey))
                .OrderByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoriesAsync(string categoryIds, int languageId, int take)
        {
             // Note: This logic mimics the original code's requirement to filter by multiple categories if needed
             // For now, implementing simplistic containment check as per original Logic in RelatedProducts
             
            if (string.IsNullOrEmpty(categoryIds)) return new List<SanPhamCNTB>();

            var cateIds = categoryIds.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            // This query might be expensive on large datasets due to client-side evaluation or complex LIKEs
            // Optimizing for simple containment matching
            var query = _context.SanPhamCNTBs
                .Where(x => x.LanguageId == languageId && x.StatusId == 3 && x.CategoryId != null);

            // Dynamically build OR clause is complex in LINQ without PredicateBuilder. 
            // Falling back to functional equivalent of original code logic: fetch candidates and filter in memory if complex, 
            // or use specific EF Core capabilities. 
            // Given the original code use AsEnumerable() in RelatedProducts, we will implement optimized database-side filtering if possible.
            // For now, let's use the exact pattern from original efficient query if possible, or Keep it simple.
            
            // Replicating original "RelatedProducts" logic which fetches candidates then filters
             return await query
                .Where(x => cateIds.Any(c => x.CategoryId.Contains(";" + c + ";")))
                .OrderByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<SanPhamCNTB?> GetProductByIdAsync(int id)
        {
            return await _context.SanPhamCNTBs
                .FirstOrDefaultAsync(x => x.ID == id && x.LanguageId == 1 && x.StatusId == 3);
        }

        public async Task<List<SanPhamCNTB>> GetRelatedProductsAsync(int productId, int languageId, int take)
        {
            var pp = await _context.SanPhamCNTBs.FirstOrDefaultAsync(x => x.ID == productId);
            if (pp == null || string.IsNullOrWhiteSpace(pp.CategoryId)) return new List<SanPhamCNTB>();

            var cateIds = pp.CategoryId.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            // logic from original: fetch candidates then filter. 
            // We can improve this in the future with Full Text Search or proper relationship tables.
            var candidates = await _context.SanPhamCNTBs
                .Where(x => x.LanguageId == languageId 
                         && x.StatusId == 3 
                         && x.ID != productId 
                         && x.CategoryId != null)
                .ToListAsync(); // Fetch eligible to memory to parse CategoryId string

            return candidates
                .Where(x => cateIds.Any(c => x.CategoryId != null && x.CategoryId.Contains(";" + c + ";")))
                .OrderByDescending(x => x.Created)
                .Take(take)
                .ToList();
        }

        public async Task<int> GetProductCountByCategoryAsync(int catId)
        {
            string cateToken = $";{catId};";
            return await _context.SanPhamCNTBs
                .CountAsync(x => x.StatusId == 3 && x.CategoryId != null && x.CategoryId.Contains(cateToken));
        }

        public async Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int page, int pageSize)
        {
            string cateToken = $";{catId};";
            return await _context.SanPhamCNTBs
                .Where(x => x.StatusId == 3 && x.CategoryId != null && x.CategoryId.Contains(cateToken))
                .OrderByDescending(x => x.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
