namespace Application.Common.DTOs.Chapters
{
    public class CreateChapterDto
    {
        public Guid TranslatedMangaId { get; set; }
        public int UploadedByUserId { get; set; } // ID của user upload, sẽ lấy từ context user đã xác thực
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
    }
} 