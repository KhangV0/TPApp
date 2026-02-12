using TPApp.ViewModel;

namespace TPApp.Interfaces
{
    public interface IProjectService
    {
        Task<List<MyProjectVm>> GetMyProjectsAsync(int userId);
        Task<int> GetProjectCountAsync(int userId);
    }
}
