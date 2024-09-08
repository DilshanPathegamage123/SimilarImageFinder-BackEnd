using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FindSimilarImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FindSimilarImagesController> _logger;

        public FindSimilarImagesController(IWebHostEnvironment environment, ILogger<FindSimilarImagesController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult FindSimilarImages([FromForm] IFormFile singleImage)
        {
            if (singleImage == null || singleImage.Length == 0)
            {
                return BadRequest("No image file or an empty file was uploaded.");
            }

            if (string.IsNullOrEmpty(_environment.WebRootPath))
            {
                _logger.LogError("WebRootPath is not set.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Web root path is not set.");
            }

            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

            try
            {
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uploadedImagePath = Path.Combine(uploadsPath, singleImage.FileName);
                using (var stream = new FileStream(uploadedImagePath, FileMode.Create))
                {
                    singleImage.CopyTo(stream);
                }

                if (!Directory.Exists(datasetPath) || Directory.GetFiles(datasetPath).Length == 0)
                {
                    _logger.LogError("Dataset directory is missing or empty.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Dataset directory is missing or empty.");
                }

                var similarImages = FindSimilarImages(uploadedImagePath, datasetPath);
                return Ok(similarImages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while finding similar images");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the image.");
            }
        }

        private List<string> FindSimilarImages(string uploadedImagePath, string datasetPath)
        {
            List<string> similarImages = new List<string>();
            Mat uploadedImage = Cv2.ImRead(uploadedImagePath, ImreadModes.Color);

            foreach (var file in Directory.GetFiles(datasetPath))
            {
                Mat datasetImage = Cv2.ImRead(file, ImreadModes.Color);
                double similarity = CompareImages(uploadedImage, datasetImage);

                if (similarity > 0.9) // Adjust similarity threshold as needed
                {
                    similarImages.Add(file.Replace(_environment.WebRootPath, "").Replace("\\", "/"));
                }
            }

            return similarImages;
        }

        private double CompareImages(Mat img1, Mat img2)
        {
            Mat grayImg1 = new Mat();
            Mat grayImg2 = new Mat();
            Cv2.CvtColor(img1, grayImg1, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(img2, grayImg2, ColorConversionCodes.BGR2GRAY);

            Mat hist1 = new Mat();
            Mat hist2 = new Mat();
            Cv2.CalcHist(new Mat[] { grayImg1 }, new int[] { 0 }, null, hist1, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });
            Cv2.CalcHist(new Mat[] { grayImg2 }, new int[] { 0 }, null, hist2, 1, new int[] { 256 }, new Rangef[] { new Rangef(0, 256) });

            Cv2.Normalize(hist1, hist1, 0, 1, NormTypes.MinMax);
            Cv2.Normalize(hist2, hist2, 0, 1, NormTypes.MinMax);

            double result = Cv2.CompareHist(hist1, hist2, HistCompMethods.Correl);

            return result;
        }
    }
}
