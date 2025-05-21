using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IPhotoAccessor
    {
        /// <summary>
        /// Upload một file ảnh lên dịch vụ lưu trữ.
        /// </summary>
        /// <param name="stream">Stream của file ảnh.</param>
        /// <param name="fileName">Tên file gốc.</param>
        /// <param name="folderName">Tên thư mục trên Cloudinary (tùy chọn, ví dụ: "cover_arts", "chapter_pages").</param>
        /// <returns>Kết quả upload chứa PublicId và Url.</returns>
        Task<PhotoUploadResult?> UploadPhotoAsync(Stream stream, string fileName, string? folderName = null);

        /// <summary>
        /// Xóa một ảnh khỏi dịch vụ lưu trữ dựa trên PublicId.
        /// </summary>
        /// <param name="publicId">PublicId của ảnh cần xóa.</param>
        /// <returns>Thông báo kết quả (ví dụ: "ok" nếu thành công).</returns>
        Task<string?> DeletePhotoAsync(string publicId);
    }
} 