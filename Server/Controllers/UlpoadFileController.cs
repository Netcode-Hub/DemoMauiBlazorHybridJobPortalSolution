using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UlpoadFileController : ControllerBase
    {
        private readonly ILogger<UlpoadFileController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public UlpoadFileController(ILogger<UlpoadFileController> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpPost("logo")]
        [AllowAnonymous]
        public async Task<IActionResult> BusinessLogo()
        {
            try
            {
                // Store the content.
                var httpContent = HttpContext.Request;

                //check for null
                if (httpContent is null)
                    return BadRequest();

                // check if the context contains multiple file.
                if (httpContent.Form.Files.Count > 0)
                {
                    string fileName = string.Empty;
                    // loop through
                    foreach (var file in httpContent.Form.Files)
                    {
                        // get file path 
                        var filePath = Path.Combine(webHostEnvironment.ContentRootPath, "Files\\Logos");

                        // check if director exist; if NO then create.
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        Guid GuidStrings = Guid.NewGuid();
                        //copy the file to the folder
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            System.IO.File.WriteAllBytes(Path.Combine(filePath, GuidStrings + file.FileName), memoryStream.ToArray());
                        }
                        fileName = GuidStrings + file.FileName;
                    }
                    return Ok(fileName);
                }
                return BadRequest("No file selected");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new StatusCodeResult(500);
            }
        }

        [HttpPost("certificate")]
        [AllowAnonymous]
        public async Task<IActionResult> BusinessCertificate()
        {
            try
            {
                // Store the content.
                var httpContent = HttpContext.Request;

                //check for null
                if (httpContent is null)
                    return BadRequest();

                // check if the context contains multiple file.
                if (httpContent.Form.Files.Count > 0)
                {
                    string fileName = string.Empty;
                    // loop through
                    foreach (var file in httpContent.Form.Files)
                    {
                        // get file path 
                        var filePath = Path.Combine(webHostEnvironment.ContentRootPath, "Files\\Certificates");

                        // check if director exist; if NO then create.
                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        //copy the file to the folder
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            System.IO.File.WriteAllBytes(Path.Combine(filePath, file.FileName), memoryStream.ToArray());
                        }
                        fileName = file.FileName;
                    }
                    return Ok(fileName);
                }
                return BadRequest("No file selected");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return new StatusCodeResult(500);
            }



        }
    }
}
