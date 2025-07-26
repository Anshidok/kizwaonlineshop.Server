using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace kizwaonlineshop.Server.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var publicId = $"{fileNameWithoutExtension}";

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Transformation = new Transformation().Width(500).Height(500).Crop("fill")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception($"Cloudinary upload error: {uploadResult.Error.Message}");

            return uploadResult.SecureUrl.ToString();
        }


        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;
            try
            {
                var uri = new Uri(imageUrl);
                var fileName = uri.Segments.Last(); // e.g. "profile.png"
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                var fullPublicId = $"{fileNameWithoutExtension}"; // e.g. "Images/Products/profile"

                var deletionParams = new DeletionParams(fullPublicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                return deletionResult.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cloudinary delete error: {ex.Message}");
                return false;
            }
        }
    }
}
