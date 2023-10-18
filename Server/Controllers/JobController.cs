using Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Services;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IClientService service;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly AppDbContext appDbContext;

        public JobController(IClientService service, IHostingEnvironment hostingEnvironment, AppDbContext appDbContext)
        {
            this.service = service;
            this.hostingEnvironment = hostingEnvironment;
            this.appDbContext = appDbContext;
        }

        [HttpPost("post-job")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse>> PostJobAsync(PostJob model)
        {
            var result = await service.PostJobAsync(model);
            return Ok(result);
        }

        [HttpGet("all-jobs")]
        [AllowAnonymous]
        public async Task<ActionResult<GetJob>> GetJobsAsync(int categoryId = 0, string filter = null!)
        {
            var jobs = await service.GetAllJobsAsync(categoryId, filter);
            return Ok(jobs);
        }

        [HttpGet("all-jobs-with-categoryId/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GetJob>>> GetJobWithCategoryIdAsync(int id)
        {
            var job = await service.GetJobWithCategoryIdAsync(id);
            return Ok(job);
        }


        [HttpPost("submit-business-data")]
        [Authorize]
        public async Task<ActionResult<ServiceResponse>> SetupCompanyAsync(CompanyInfo model)
        {
            var result = await service.SetupCompanyAsync(model);
            return Ok(result);
        }

        [HttpGet("get-business-data/{clientId:int}")]
        [Authorize]
        public async Task<ActionResult<CompanyInfo>> GetCompanyInfoAsync(int clientId)
        {
            var result = await service.GetCompanyInfoAsync(clientId);
            return result is null ? NotFound(result) : Ok(result);
        }

        [HttpGet("get-logo/{filename}")]
        [AllowAnonymous]
        public async Task<IActionResult> FetchLogo(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return BadRequest("Filename required");

            var company = await appDbContext.CompanyInfo.Where(_ => _.CompanyLogo!.ToLower().Equals(filename.ToLower())).FirstOrDefaultAsync();
            if (company is null) return BadRequest("Invalid filename");

            string contentType = string.Empty;
            if (company!.CompanyLogo!.ToLower().Contains("png"))
                contentType = "image/png";

            if (company!.CompanyLogo!.ToLower().Contains("jpg"))
                contentType = "image/jpg";

            if (company!.CompanyLogo!.ToLower().Contains("jpeg"))
                contentType = "image/jpeg";

            var filePath = Path.Combine(hostingEnvironment.ContentRootPath, "Files\\Logos", $"{company.CompanyLogo}");

            if (!System.IO.File.Exists(filePath))
            {
                contentType = string.Empty;
                filePath = "default_notfound_image_path_here";
            }
            return PhysicalFile(filePath, contentType);
        }

        // Category Action Method
        [HttpPost("add-update-category")]
        public async Task<ActionResult<ServiceResponse>> AddOrUpdateCategoryAsync(Category model)
        {
            var result = await service.AddOrUpdateCategoryAsync(model);
            return Ok(result);
        }

        [HttpDelete("delete-category/{id:int}")]
        public async Task<ActionResult<ServiceResponse>> DeleteCategoryAsync(int id)
        {
            var results = await service.DeleteCategoryAsync(id);
            return Ok(results);
        }

        [HttpGet("get-categories")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceResponse>> GetCategoriesAsync()
        {
            var results = await service.GetCategoriesAsync();
            return Ok(results);
        }


        //Job service
        [HttpGet("get-cat-with-Job-quantities")]
        public async Task<ActionResult<List<CategoryWithNumberOfJobs>>> GetCategoryWithNumberOfJobs()
        {
            return await service.GetCategoryWithNumberOfJobs();
        }
    }
}
