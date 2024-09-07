using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO; // For file handling

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadImageSetController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment; // To access web root path

        public UploadImageSetController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public IActionResult UploadImageSet([FromForm] IFormFileCollection dataset)
        {
            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");

            // Clear the old dataset files
            if (Directory.Exists(datasetPath))
            {
                DirectoryInfo directory = new DirectoryInfo(datasetPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();  // Delete each file
                }
            }
            else
            {
                Directory.CreateDirectory(datasetPath);  // Create directory if it does not exist
            }

            // Save new dataset files
            foreach (var file in dataset)
            {
                var filePath = Path.Combine(datasetPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);  // Save file to server
                }
            }

            return Ok("ImageSet uploaded successfully.");
        }
    }
}
