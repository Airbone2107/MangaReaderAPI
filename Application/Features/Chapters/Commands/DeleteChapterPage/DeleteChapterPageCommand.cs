using MediatR;

namespace Application.Features.Chapters.Commands.DeleteChapterPage
{
    public class DeleteChapterPageCommand : IRequest<Unit>
    {
        public Guid PageId { get; set; } // ID của ChapterPage cần xóa
    }
} 