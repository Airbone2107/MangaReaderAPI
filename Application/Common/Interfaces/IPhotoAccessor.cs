using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IPhotoAccessor
    {
        /// <summary>
        /// Upload một file ảnh lên dịch vụ lưu trữ với một public_id được chỉ định.
        /// </summary>
        /// <param name="stream">Stream của file ảnh.</param>
        /// <param name="desiredPublicId">PublicId mong muốn cho ảnh trên Cloudinary. Phải đảm bảo tính duy nhất.</param>
        /// <param name="originalFileNameForUpload">Tên file gốc, có thể cần thiết cho một số API upload, nhưng không dùng để tạo public_id.</param>
        /// <param name="folderName">Tên thư mục trên Cloudinary (tùy chọn). Nếu desiredPublicId đã bao gồm cấu trúc thư mục, tham số này có thể không cần thiết hoặc dùng để bổ sung.</param>
        /// <returns>Kết quả upload chứa PublicId (sẽ giống desiredPublicId nếu thành công) và Url. Trả về null nếu có lỗi.</returns>
        Task<PhotoUploadResult?> UploadPhotoAsync(Stream stream, string desiredPublicId, string originalFileNameForUpload, string? folderName = null);

        /// <summary>
        /// Xóa một ảnh khỏi dịch vụ lưu trữ dựa trên PublicId.
        /// </summary>
        /// <param name="publicId">PublicId của ảnh cần xóa.</param>
        /// <returns>Thông báo kết quả (ví dụ: "ok" nếu thành công). Trả về null nếu có lỗi.</returns>
        Task<string?> DeletePhotoAsync(string publicId);
    }
} 