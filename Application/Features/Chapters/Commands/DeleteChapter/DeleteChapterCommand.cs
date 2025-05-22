using MediatR;

namespace Application.Features.Chapters.Commands.DeleteChapter
{
    public class DeleteChapterCommand : IRequest<Unit>
    {
        public Guid ChapterId { get; set; }
    }
} 