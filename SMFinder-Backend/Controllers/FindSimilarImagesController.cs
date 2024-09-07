using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;  // For image processing
using System.IO;  // For file handling
using System.Collections.Generic;  // For list of results

namespace SMFinder_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FindSimilarImagesController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;  // To access web root path

        public FindSimilarImagesController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost]
        public IActionResult FindSimilarImages([FromForm] IFormFile singleImage)
        {
            var datasetPath = Path.Combine(_environment.WebRootPath, "dataset");
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

            // Clear previous single image
            if (Directory.Exists(uploadsPath))
            {
                DirectoryInfo directory = new DirectoryInfo(uploadsPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();  // Delete each file
                }
            }
            else
            {
                Directory.CreateDirectory(uploadsPath);  // Create directory if it does not exist
            }

            // Save the new single image
            var uploadedImagePath = Path.Combine(uploadsPath, singleImage.FileName);
            using (var stream = new FileStream(uploadedImagePath, FileMode.Create))
            {
                singleImage.CopyTo(stream);  // Save image to server
            }

            // Find similar images in the dataset
            var similarImages = FindSimilarImages(uploadedImagePath, datasetPath);

            return Ok(similarImages);  // Return the list of similar images
        }

        private List<string> FindSimilarImages(string uploadedImagePath, string datasetPath)
        {
            List<string> similarImages = new List<string>();
            Mat uploadedImage = Cv2.ImRead(uploadedImagePath, ImreadModes.Color);  // Read the uploaded image

            foreach (var file in Directory.GetFiles(datasetPath))  // Iterate through the dataset
            {
                Mat datasetImage = Cv2.ImRead(file, ImreadModes.Color);  // Read each image in the dataset

                double similarity = CompareImages(uploadedImage, datasetImage);  // Compare images

                if (similarity > 0.9)  // Threshold for similarity
                {
                    similarImages.Add(file.Replace(_environment.WebRootPath, ""));  // Add relative path to results
                }
            }

            return similarImages;  // Return list of similar images
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

            return result;  // Return similarity score
        }
    }
}
