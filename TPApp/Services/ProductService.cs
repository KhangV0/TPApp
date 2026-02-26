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
            // NOTE: bEffectiveDate/eEffectiveDate range filter removed — it caused a full table scan
            // (no index on those columns). StatusId + LanguageId filter is sufficient for homepage.
            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.StatusId == 3 && x.LanguageId == 1)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take)
        {
            // Join with SanPhamCNTBCategory table instead of string parsing
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == catId
                                && p.LanguageId == languageId
                                && p.StatusId == 3
                          orderby p.Created descending
                          select p)
                          .Distinct()
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
            // Get category IDs for current product from junction table
            var catIds = await _context.SanPhamCNTBCategories
                .Where(x => x.SanPhamCNTBId == productId)
                .Select(x => x.CatId)
                .ToListAsync();

            if (!catIds.Any()) return new List<SanPhamCNTB>();

            // Find products in the same categories
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where catIds.Contains(c.CatId)
                                && p.LanguageId == languageId
                                && p.StatusId == 3
                                && p.ID != productId
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Take(take)
                          .ToListAsync();
        }

        public async Task<int> GetProductCountByCategoryAsync(int catId)
        {
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == catId && p.StatusId == 3
                          select p.ID)
                          .Distinct()
                          .CountAsync();
        }

        public async Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int page, int pageSize)
        {
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == catId && p.StatusId == 3
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();
        }

        // ── ProductType-scoped queries ─────────────────────────────────────────────

        public async Task<List<SanPhamCNTB>> GetNewProductsByProductTypeAsync(int productType, int take)
        {
            return await _context.SanPhamCNTBs
                .AsNoTracking()
                .Where(x => x.ProductType == productType && x.StatusId == 3)
                .OrderByDescending(x => x.Modified)
                .ThenByDescending(x => x.Created)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<SanPhamCNTB>> GetProductsByCategoryAndProductTypeAsync(
            int cateId, int productType, int languageId, int take)
        {
            return await (from p in _context.SanPhamCNTBs
                          join c in _context.SanPhamCNTBCategories on p.ID equals c.SanPhamCNTBId
                          where c.CatId == cateId
                                && p.ProductType == productType
                                && p.StatusId == 3
                                && p.LanguageId == languageId
                          orderby p.Created descending
                          select p)
                          .Distinct()
                          .Take(take)
                          .ToListAsync();
        }
    }
}
