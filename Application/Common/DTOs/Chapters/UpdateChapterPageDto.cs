namespace Application.Common.DTOs.Chapters
{
    public class UpdateChapterPageDto // Dùng cho command cập nhật chi tiết trang (không phải ảnh)
    {
        // PageId sẽ lấy từ route
        public int PageNumber { get; set; }
    }
} 