using Library.Models;

namespace Server.Data.Services
{
    public interface IClientService
    {
        Task<ServiceResponse> RegisterUserAsync(RegistrationModel model);
        Task<ServiceResponse> LoginUserAsync(Login model);

        Task<ServiceResponse> SetupCompanyAsync(CompanyInfo model);
        Task<CompanyInfo> GetCompanyInfoAsync(int clientId);
        Task<ServiceResponse> PostJobAsync(PostJob model);
        Task<List<GetJob>> GetAllJobsAsync(int categoryId, string filter);
        Task<List<GetJob>> GetJobWithCategoryIdAsync(int id);

        //Category service
        Task<ServiceResponse> AddOrUpdateCategoryAsync(Category model);
        Task<List<Category>> GetCategoriesAsync();
        Task<ServiceResponse> DeleteCategoryAsync(int id);

        // Job Service
        Task<List<CategoryWithNumberOfJobs>> GetCategoryWithNumberOfJobs();
    }
}
