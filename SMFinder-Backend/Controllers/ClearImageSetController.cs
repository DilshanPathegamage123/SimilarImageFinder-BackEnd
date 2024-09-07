using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;  // For file operations

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClearImageSetController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;  // To access web root path

        public ClearImageSetController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public IActionResult ClearDataset()
        {
            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");

            // Clear old dataset files
            if (Directory.Exists(datasetPath))
            {
                DirectoryInfo directory = new DirectoryInfo(datasetPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();  // Delete each file
                }
            }

            return Ok("ImageSet cleared successfully.");  // Return success message
        }
    }
}
