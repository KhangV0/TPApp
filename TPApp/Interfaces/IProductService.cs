using TPApp.Entities;
using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    public interface IProductService
    {
        Task<List<SanPhamCNTB>> GetNewProductsAsync(int take);
        Task<List<SanPhamCNTB>> GetProductsByCategoryAsync(int catId, int languageId, int take);
        Task<List<SanPhamCNTB>> GetProductsByCategoriesAsync(string categoryIds, int languageId, int take);
        Task<SanPhamCNTB?> GetProductByIdAsync(int id);
        Task<List<SanPhamCNTB>> GetRelatedProductsAsync(int productId, int languageId, int take);
        Task<int> GetProductCountByCategoryAsync(int catId);
        Task<List<SanPhamCNTB>> GetPagedProductsByCategoryAsync(int catId, int page, int pageSize);
    }
}
