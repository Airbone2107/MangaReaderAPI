using Application.Common.Interfaces;
using Application.Common.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

        public async Task<PhotoUploadResult?> UploadPhotoAsync(Stream stream, string desiredPublicId, string originalFileNameForUpload, string? folderName = null)
        {
            if (stream == null || stream.Length == 0)
            {
                _logger.LogWarning("UploadPhotoAsync: Stream is null or empty.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(desiredPublicId))
            {
                _logger.LogWarning("UploadPhotoAsync: Desired PublicId is null or empty.");
                return null; // Hoặc throw ArgumentNullException
            }

            // Nếu Cloudinary chưa được cấu hình đúng, không thực hiện upload
            if (string.IsNullOrWhiteSpace(_cloudinary.Api.Account.Cloud))
            {
                 _logger.LogError("Cloudinary client is not properly configured. Upload aborted.");
                 return null;
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(originalFileNameForUpload, stream),
                PublicId = desiredPublicId, // Sử dụng public_id được chỉ định
                Overwrite = true, // Cho phép ghi đè nếu public_id đã tồn tại (quan trọng)
                UniqueFilename = false, // Không tự động sinh tên file duy nhất nếu đã cung cấp PublicId
                Folder = folderName 
                // Nếu desiredPublicId đã chứa cấu trúc thư mục (ví dụ: "manga_reader/chapters/xxx/pages/yyy")
                // thì Folder có thể không cần thiết hoặc chỉ định thêm 1 cấp nữa.
                // Cloudinary sẽ gộp Folder và phần thư mục trong PublicId.
                // Ví dụ: Folder = "my_app", PublicId = "folder1/image1" -> kết quả là "my_app/folder1/image1"
                // Ví dụ: Folder = null, PublicId = "folder1/image1" -> kết quả là "folder1/image1"
            };

            try
            {
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed for file {OriginalFileName} with desired PublicId {DesiredPublicId}. Error: {ErrorMessage}", originalFileNameForUpload, desiredPublicId, uploadResult.Error.Message);
                    return null;
                }

                // PublicId trả về từ Cloudinary nên giống với desiredPublicId nếu thành công và Overwrite=true
                return new PhotoUploadResult
                {
                    PublicId = uploadResult.PublicId, 
                    Url = uploadResult.SecureUrl.ToString()
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during Cloudinary upload for file {OriginalFileName} with desired PublicId {DesiredPublicId}.", originalFileNameForUpload, desiredPublicId);
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