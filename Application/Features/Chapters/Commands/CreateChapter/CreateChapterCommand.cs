using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapter
{
    public class CreateChapterCommand : IRequest<Guid>
    {
        public Guid TranslatedMangaId { get; set; }
        public int UploadedByUserId { get; set; } // Sẽ lấy từ user context ở Controller
        public string? Volume { get; set; }
        public string? ChapterNumber { get; set; }
        public string? Title { get; set; }
        public DateTime PublishAt { get; set; }
        public DateTime ReadableAt { get; set; }
    }
} 