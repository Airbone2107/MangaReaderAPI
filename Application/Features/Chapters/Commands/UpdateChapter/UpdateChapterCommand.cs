using MediatR;

namespace Application.Features.Chapters.Commands.UpdateChapter
{
    public class UpdateChapterCommand : IRequest<Unit>
    {
        public Guid ChapterId { get; set; } // Lấy từ route
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
        // UploadedByUserId không nên cho phép cập nhật qua command này
        // TranslatedMangaId cũng không nên thay đổi
    }
} 