namespace Application.Common.DTOs.Chapters
{
    public class UpdateChapterDto
    {
        // ChapterId sẽ lấy từ route
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
        // UploadedByUserId không nên cho phép cập nhật qua DTO này
    }
} 