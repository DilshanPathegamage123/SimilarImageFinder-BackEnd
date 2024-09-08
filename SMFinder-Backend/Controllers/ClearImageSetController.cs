using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClearImageSetController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ClearImageSetController> _logger;

        public ClearImageSetController(IWebHostEnvironment environment, ILogger<ClearImageSetController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult ClearDataset()
        {
            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                _logger.LogError("WebRootPath is not set.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Web root path is not set.");
            }

            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");

            try
            {
                if (Directory.Exists(datasetPath))
                {
                    DirectoryInfo directory = new DirectoryInfo(datasetPath);
                    foreach (FileInfo file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while clearing dataset");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while clearing the dataset.");
            }

            return Ok("ImageSet cleared successfully.");
        }
    }
}
