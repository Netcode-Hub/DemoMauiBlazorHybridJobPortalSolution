using Library.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.Data.Services
{
    public class ClientService : IClientService
    {

        private readonly AppDbContext appDbContext;
        private readonly IConfiguration config;
        public ClientService(AppDbContext appDbContext, IConfiguration config)
        {
            this.appDbContext = appDbContext;
            this.config = config;
        }

        // User Service
        public async Task<ServiceResponse> RegisterUserAsync(RegistrationModel model)
        {
            var userRole = new UserRole();
            //Check if admin already exist
            if (model.Email!.ToLower().StartsWith("admin"))
            {
                var chkIfAdminExist = await appDbContext.UserRoles.Where(_ => _.RoleName!.ToLower().Equals("admin")).FirstOrDefaultAsync();
                if (chkIfAdminExist != null) return new ServiceResponse() { Flag = false, Message = "Sorry Admin already exist, please change the email address" };

                userRole.RoleName = "Admin";
            }

            var checkIfUserAlreadyCreated = await appDbContext.Registrations.Where(_ => _.Email!.ToLower().Equals(model.Email!.ToLower())).FirstOrDefaultAsync();
            if (checkIfUserAlreadyCreated != null) return new ServiceResponse() { Flag = false, Message = $"Email: {model.Email} already registered" };


            var hashedPassword = HashPassword(model.Password!);
            var registeredEntity = appDbContext.Registrations.Add(new Registration()
            {
                Name = model.Name!,
                Email = model.Email,
                Password = hashedPassword,
                Phone = model.Phone.ToString()!,
            }).Entity;
            await appDbContext.SaveChangesAsync();


            if (string.IsNullOrEmpty(userRole.RoleName))
                userRole.RoleName = "User";

            userRole.UserId = registeredEntity.Id;
            appDbContext.UserRoles.Add(userRole);
            await appDbContext.SaveChangesAsync();
            return new ServiceResponse() { Flag = true, Message = "Account Created" };
        }


        // Encrypt user password
        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var randomGenerator = RandomNumberGenerator.Create())
            {
                randomGenerator.GetBytes(salt);
            }
            var rfcPassword = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassword.GetBytes(20);

            byte[] passwordHash = new byte[36];
            Array.Copy(salt, 0, passwordHash, 0, 16);
            Array.Copy(rfcPasswordHash, 0, passwordHash, 16, 20);
            return Convert.ToBase64String(passwordHash);
        }

        public async Task<ServiceResponse> LoginUserAsync(Login model)
        {
            var getUser = await appDbContext.Registrations.Where(_ => _.Email!.Equals(model.Email)).FirstOrDefaultAsync();
            if (getUser == null) return new ServiceResponse() { Flag = false, Message = "User not found" };

            var checkIfPasswordMatch = VerifyUserPassword(model.Password!, getUser.Password!);
            if (checkIfPasswordMatch)
            {
                //get user role from the roles table
                var getUserRole = await appDbContext.UserRoles.Where(_ => _.Id == getUser.Id).FirstOrDefaultAsync();

                //Generate token with the role, and username as email
                var token = GenerateToken(getUser.Id, getUser.Name, model.Email!, getUserRole!.RoleName!);

                var checkIfTokenExist = await appDbContext.TokenInfo.Where(_ => _.UserId == getUser.Id).FirstOrDefaultAsync();
                if (checkIfTokenExist == null)
                {
                    appDbContext.TokenInfo.Add(new TokenInfo() { Token = token, UserId = getUser.Id });
                    await appDbContext.SaveChangesAsync();
                    return new ServiceResponse() { Flag = true, Token = token };
                }
                checkIfTokenExist.Token = token;
                await appDbContext.SaveChangesAsync();
                return new ServiceResponse() { Flag = true, Token = token };
            }
            return new ServiceResponse() { Flag = false, Message = "Invalid email or password" };
        }

        //Decrypt user database password and encrypt user raw password and compare
        private static bool VerifyUserPassword(string rawPassword, string databasePassword)
        {
            byte[] dbPasswordHash = Convert.FromBase64String(databasePassword);
            byte[] salt = new byte[16];
            Array.Copy(dbPasswordHash, 0, salt, 0, 16);
            var rfcPassword = new Rfc2898DeriveBytes(rawPassword, salt, 1000, HashAlgorithmName.SHA1);
            byte[] rfcPasswordHash = rfcPassword.GetBytes(20);
            for (int i = 0; i < rfcPasswordHash.Length; i++)
            {
                if (dbPasswordHash[i + 16] != rfcPasswordHash[i])
                    return false;
            }
            return true;
        }

        private string GenerateToken(int userId, string name, string email, string roleName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.SerialNumber, userId.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, roleName)
            };
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }


        //job services
        public async Task<ServiceResponse> PostJobAsync(PostJob model)
        {
            var checkIfJobAlreadyPosted = await appDbContext.Jobs.Where(_ => _.ClientId == model.ClientId && _.Title!.ToLower().Equals(model.Title!.ToLower())).FirstOrDefaultAsync();
            if (checkIfJobAlreadyPosted != null) return new ServiceResponse() { Flag = false, Message = $"Job: {model.Title} already posted" };

            appDbContext.Jobs.Add(model);
            await appDbContext.SaveChangesAsync();
            return new ServiceResponse() { Flag = true, Message = "Job Posted" };
        }

        public async Task<List<GetJob>> GetAllJobsAsync(int categoryId, string filter)
        {
            var AllJobs = new List<GetJob>();
            List<PostJob> availableJobs = new();

            if (categoryId > 0)
                availableJobs = await appDbContext.Jobs.Where(_ => _.Active && _.CategoryId == categoryId).ToListAsync();
            else if (!string.IsNullOrEmpty(filter))
                availableJobs = await appDbContext.Jobs.Where(_ => _.Active && _.Title!.ToLower().Contains(filter.ToLower())).ToListAsync();
            else availableJobs = await appDbContext.Jobs.Where(_ => _.Active).ToListAsync();

               

            if (availableJobs is null) return null!;
            //get all companies
            var companyInfo = await appDbContext.CompanyInfo.ToListAsync();
            var registers = await appDbContext.Registrations.ToListAsync();

            //loop through jobs
            foreach (var job in availableJobs)
            {
                var getjobProvider = companyInfo.Where(_ => _.Id == job.ClientId).FirstOrDefault();
                var register = registers.Where(_ => _.Id == job.ClientId).FirstOrDefault();
                AllJobs.Add(new GetJob()
                {
                    Id = job.ClientId,
                    Title = job.Title,
                    Function = job.Function,
                    Description = job.Description,
                    MinSalaryRange = job.MinSalaryRange,
                    MaxSalaryRange = job.MaxSalaryRange,
                    JobMode = job.JobMode,
                    JobLocation = job.JobLocation,
                    Featured = job.Featured,
                    CompanyName = getjobProvider?.CompanyName,
                    CompanyAddress = getjobProvider?.CompanyAddress,
                    CompanyEmail = register?.Email,
                    Phone = register?.Phone,
                    CompanyLocation = getjobProvider?.CompanyLocation,
                    CompanyLogo = getjobProvider?.CompanyLogo,
                    DateAdded = job.DateAdded
                });
            }
            return AllJobs;
        }


        public async Task<List<GetJob>> GetJobWithCategoryIdAsync(int id)
        {
            var List = new List<GetJob>();
            var jobs = await appDbContext.Jobs.Where(_ => _.CategoryId == id).ToListAsync();
            var register = await appDbContext.Registrations.ToListAsync();
            var companyInfo = await appDbContext.CompanyInfo.ToListAsync();
            foreach (var job in jobs)
            {
                List.Add(new GetJob()
                {
                    Id = job.ClientId,
                    Title = job.Title,
                    Function = job.Function,
                    Description = job.Description,
                    MinSalaryRange = job.MinSalaryRange,
                    MaxSalaryRange = job.MaxSalaryRange,
                    JobMode = job.JobMode,
                    JobLocation = job.JobLocation,
                    Featured = job.Featured,
                    CompanyName = companyInfo.FirstOrDefault(_ => _.ClientId == job.ClientId)!.CompanyName,
                    CompanyAddress = companyInfo.FirstOrDefault(_ => _.ClientId == job.ClientId)!.CompanyAddress,
                    CompanyEmail = register.FirstOrDefault(_ => _.Id == job.ClientId)!.Email,
                    Phone = register.FirstOrDefault(_ => _.Id == job.ClientId)!.Phone,
                    CompanyLocation = companyInfo.FirstOrDefault(_ => _.ClientId == job.ClientId)!.CompanyLocation,
                    CompanyLogo = companyInfo.FirstOrDefault(_ => _.ClientId == job.ClientId)!.CompanyLogo,
                    DateAdded = job.DateAdded
                });
            }

            return List!;
        }

        public async Task<ServiceResponse> SetupCompanyAsync(CompanyInfo model)
        {
            var getUser = await appDbContext.Registrations.FirstOrDefaultAsync(_ => _.Id == model.ClientId);
            if (getUser == null) return new ServiceResponse() { Flag = false, Message = "User not found" };

            //check if data already submited
            var data = await appDbContext.CompanyInfo.FirstOrDefaultAsync(_ => _.ClientId == model.ClientId);
            if (data is not null)
            {
                data.CompanyCertificateName = model.CompanyCertificateName;
                data.CompanyLocation = model.CompanyLocation;
                data.CompanyLogo = model.CompanyLogo;
                data.CompanyAddress = model.CompanyAddress;
                data.CompanyName = model.CompanyName;
                await appDbContext.SaveChangesAsync();
                return new ServiceResponse() { Flag = true, Message = "Your data is updated" };
            }
            appDbContext.CompanyInfo.Add(model);
            await appDbContext.SaveChangesAsync();
            return new ServiceResponse() { Flag = true, Message = "Your data is submitted" };
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync(int clientId)
        {
            var result = await appDbContext.CompanyInfo.FirstOrDefaultAsync(_ => _.ClientId == clientId);
            return result!;
        }



        //Category Service
        public async Task<ServiceResponse> AddOrUpdateCategoryAsync(Category model)
        {
            if (model is null) return new ServiceResponse() { Flag = false, Message = "Model requires" };

            if (model.Id > 0)
            {
                var cat = await appDbContext.Categories.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (cat is null) return new ServiceResponse() { Flag = false, Message = "Category not found" };
                cat.Title = model.Title;
                cat.SubTitle = model.SubTitle;
                await appDbContext.SaveChangesAsync();
                return new ServiceResponse() { Flag = true, Message = $"Category {model.Title} updated!" };
            }

            // add as new cateogry by checking first if its already exist.
            var chk = await appDbContext.Categories.FirstOrDefaultAsync(_ => _.Title.ToLower().Equals(model.Title.ToLower()));
            if (chk is not null) return new ServiceResponse() { Flag = false, Message = $"Category {model.Title} already added" };

            appDbContext.Categories.Add(model);
            await appDbContext.SaveChangesAsync();
            return new ServiceResponse() { Flag = true, Message = $"Category {model.Title} saved!" };
        }
        public async Task<List<Category>> GetCategoriesAsync() => await appDbContext.Categories.ToListAsync();
        public async Task<ServiceResponse> DeleteCategoryAsync(int id)
        {
            var cat = await appDbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (cat is null) return new ServiceResponse() { Flag = false, Message = "Category not found" };
            appDbContext.Categories.Remove(cat);
            await appDbContext.SaveChangesAsync();
            return new ServiceResponse() { Flag = true, Message = $"{cat.Title} deleted!" };
        }

        public async Task<List<CategoryWithNumberOfJobs>> GetCategoryWithNumberOfJobs()
        {
            var list = new List<CategoryWithNumberOfJobs>();
            var jobs = await appDbContext.Jobs.ToListAsync();
            var categories = await GetCategoriesAsync();

            if (jobs is null) return null!;

            foreach (var cat in categories)
            {
                int getAllRelatedJobsCount = jobs.Where(_ => _.CategoryId == cat.Id).ToList().Count;
                list.Add(new CategoryWithNumberOfJobs() { Id = cat.Id, CategoryName = cat.Title, JobQuanities = getAllRelatedJobsCount });
            }
            return list;
        }
    }
}
