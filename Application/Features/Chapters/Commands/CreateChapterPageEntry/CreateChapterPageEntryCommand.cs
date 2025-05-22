using MediatR;

namespace Application.Features.Chapters.Commands.CreateChapterPageEntry
{
    public class CreateChapterPageEntryCommand : IRequest<Guid> // Trả về PageId
    {
        public Guid ChapterId { get; set; }
        public int PageNumber { get; set; } 
        // PublicId sẽ được cập nhật sau khi upload ảnh bằng một command khác (UploadChapterPageImageCommand)
    }
} 