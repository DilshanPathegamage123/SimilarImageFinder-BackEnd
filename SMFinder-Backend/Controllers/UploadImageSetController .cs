using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadImageSetController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadImageSetController> _logger;

        public UploadImageSetController(IWebHostEnvironment environment, ILogger<UploadImageSetController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult UploadImageSet([FromForm] List<IFormFile> dataset)
        {
            // Check if dataset is null or empty
            if (dataset == null || !dataset.Any())
            {
                return BadRequest("No files were uploaded.");
            }

            // Log WebRootPath for debugging
            _logger.LogInformation($"WebRootPath: {_environment.WebRootPath}");

            // Check if WebRootPath is set
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                _logger.LogError("WebRootPath is not set.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Web root path is not set.");
            }

            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");

            try
            {
                // Clear existing files in the dataset directory
                if (Directory.Exists(datasetPath))
                {
                    DirectoryInfo directory = new DirectoryInfo(datasetPath);
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                }
                else
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(datasetPath);
                }

                // Save each uploaded file
                foreach (var file in dataset)
                {
                    var filePath = Path.Combine(datasetPath, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while uploading dataset");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while uploading the dataset.");
            }

            return Ok("ImageSet uploaded successfully.");
        }
    }
}
