using MediatR;

namespace Application.Features.Chapters.Commands.UpdateChapterPageDetails
{
    public class UpdateChapterPageDetailsCommand : IRequest<Unit>
    {
        public Guid PageId { get; set; } // ID của ChapterPage
        public int PageNumber { get; set; }
        // Các metadata khác của trang nếu có (ví dụ: chú thích,...)
    }
} 