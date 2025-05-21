using Application.Common.Interfaces;
using Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Photos
{
    public class PhotoAccessor : IPhotoAccessor
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoAccessor> _logger;

        public PhotoAccessor(IOptions<CloudinarySettings> config, ILogger<PhotoAccessor> logger)
        {
            _logger = logger;
            // Kiểm tra null cho các tham số cấu hình quan trọng
            if (string.IsNullOrWhiteSpace(config.Value.CloudName) ||
                string.IsNullOrWhiteSpace(config.Value.ApiKey) ||
                string.IsNullOrWhiteSpace(config.Value.ApiSecret))
            {
                _logger.LogError("Cloudinary configuration is missing or incomplete. Please check CloudName, ApiKey, and ApiSecret.");
                // Bạn có thể throw exception ở đây nếu muốn dừng ứng dụng nếu cấu hình thiếu
                // throw new ArgumentNullException(nameof(config), "Cloudinary configuration is incomplete.");
                _cloudinary = new Cloudinary(); // Khởi tạo rỗng để tránh null reference, nhưng sẽ không hoạt động
            }
            else
            {
                var account = new Account(
                    config.Value.CloudName,
                    config.Value.ApiKey,
                    config.Value.ApiSecret
                );
                _cloudinary = new Cloudinary(account);
            }
        }

        public async Task<PhotoUploadResult?> UploadPhotoAsync(Stream stream, string fileName, string? folderName = null)
        {
            if (stream == null || stream.Length == 0)
            {
                _logger.LogWarning("UploadPhotoAsync: Stream is null or empty.");
                return null;
            }

            // Nếu Cloudinary chưa được cấu hình đúng, không thực hiện upload
            if (string.IsNullOrWhiteSpace(_cloudinary.Api.Account.Cloud))
            {
                 _logger.LogError("Cloudinary client is not properly configured. Upload aborted.");
                 return null;
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                // Có thể thêm các transformation ở đây nếu muốn áp dụng mặc định khi upload
                // Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = folderName // Chỉ định thư mục trên Cloudinary
            };

            try
            {
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError(uploadResult.Error.Message, "Cloudinary upload failed for file {FileName}.", fileName);
                    return null;
                }

                return new PhotoUploadResult
                {
                    PublicId = uploadResult.PublicId,
                    Url = uploadResult.SecureUrl.ToString() // Hoặc Url.ToString() nếu bạn không dùng HTTPS
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during Cloudinary upload for file {FileName}.", fileName);
                return null;
            }
        }

        public async Task<string?> DeletePhotoAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                _logger.LogWarning("DeletePhotoAsync: PublicId is null or empty.");
                return null;
            }
            
            // Nếu Cloudinary chưa được cấu hình đúng, không thực hiện xóa
            if (string.IsNullOrWhiteSpace(_cloudinary.Api.Account.Cloud))
            {
                 _logger.LogError("Cloudinary client is not properly configured. Deletion aborted for PublicId {PublicId}.", publicId);
                 return null;
            }

            var deleteParams = new DeletionParams(publicId);
            try
            {
                var result = await _cloudinary.DestroyAsync(deleteParams);
                if (result.Result == "ok")
                {
                    return result.Result;
                }
                else
                {
                    _logger.LogWarning("Cloudinary deletion failed for PublicId {PublicId}. Result: {DeletionResult}", publicId, result.Result);
                    // Nếu Cloudinary trả về "not found", bạn có thể coi đó là thành công tùy theo logic nghiệp vụ
                    // if (result.Result == "not found") return "ok"; 
                    return result.Result; // Trả về thông báo lỗi từ Cloudinary
                }
            }
            catch (System.Exception ex)
            {
                 _logger.LogError(ex, "An exception occurred during Cloudinary deletion for PublicId {PublicId}.", publicId);
                return null;
            }
        }
    }
} 