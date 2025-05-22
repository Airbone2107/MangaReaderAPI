using Application.Common.DTOs.Users;
using System.Collections.Generic;

namespace Application.Common.DTOs.Chapters
{
    public class ChapterDto
    {
        public Guid ChapterId { get; set; }
        public Guid TranslatedMangaId { get; set; }
        public int UploadedByUserId { get; set; }
        public UserDto? Uploader { get; set; } // Thông tin user upload
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public int PagesCount { get; set; } // Số lượng trang
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ChapterPageDto> ChapterPages { get; set; } = new(); // Danh sách các trang
    }
} 