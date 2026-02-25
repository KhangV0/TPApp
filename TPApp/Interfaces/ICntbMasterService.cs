using TPApp.Areas.Cms.Models;

namespace TPApp.Interfaces
{
    public interface ICntbMasterService
    {
        Task<List<LookupDto>> GetXuatXuAsync();
        Task<List<LookupDto>> GetMucDoAsync();
        Task<List<LookupDto>> GetLinhVucAsync();
        Task<List<LookupDto>> GetRootSitesAsync();
        Task<List<LookupDto>> GetStatusesAsync();
        Task<List<LookupDto>> GetNhaCungUngAsync();
        Task<List<LookupDto>> GetDichVuAsync();
    }
}
