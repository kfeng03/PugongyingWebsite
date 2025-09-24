using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.IO;
using System.Web;

namespace fyp
{
    public class CloudinaryHelper
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryHelper()
        {
            // Your Cloudinary credentials
            Account account = new Account(
                "dkzzqgxce",  // Replace with your cloud name
                "781414737836331",     // Replace with your API key
                "oMQ39AdA2m1bap8-GIhpUya5b00"   // Replace with your API secret
            );

            _cloudinary = new Cloudinary(account);
        }

        /// <summary>
        /// Uploads a file to Cloudinary
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folder">Optional folder in Cloudinary</param>
        /// <returns>URL of the uploaded file</returns>
        public string UploadFile(HttpPostedFile file, string folder = "profile_pictures")
        {
            if (file == null || file.ContentLength == 0)
                throw new ArgumentException("No file selected");

            try
            {
                // Determine the resource type based on file content type
                string contentType = file.ContentType.ToLower();

                // Generate a unique public ID
                string publicId = $"{folder}/{Guid.NewGuid()}";

                if (contentType.StartsWith("image/"))
                {
                    // Upload image
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
                else if (contentType.StartsWith("video/"))
                {
                    // Upload video
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    // Upload raw file (documents, etc.)
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, file.InputStream),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Uploads a file to Cloudinary directly from a file path
        /// </summary>
        /// <param name="filePath">The path to the file to upload</param>
        /// <param name="folder">Optional folder in Cloudinary</param>
        /// <returns>URL of the uploaded file</returns>
        public string UploadFileFromPath(string filePath, string folder = "certificate_templates")
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                throw new ArgumentException("Invalid file path or file does not exist");

            try
            {
                // Get file info
                var fileInfo = new FileInfo(filePath);

                // Determine the resource type based on file extension
                string contentType = MimeMapping.GetMimeMapping(filePath);

                // Generate a unique public ID
                string publicId = $"{folder}/{Guid.NewGuid()}";

                if (contentType.StartsWith("image/"))
                {
                    // Upload image
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(filePath),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
                else if (contentType.StartsWith("video/"))
                {
                    // Upload video
                    var uploadParams = new VideoUploadParams
                    {
                        File = new FileDescription(filePath),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
                else
                {
                    // Upload raw file (documents, etc.)
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(filePath),
                        PublicId = publicId,
                        Overwrite = true
                    };

                    var uploadResult = _cloudinary.Upload(uploadParams);
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }
    }
}