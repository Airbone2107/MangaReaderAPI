using Application.Common.DTOs.Chapters;
using MediatR;

namespace Application.Features.Chapters.Queries.GetChapterById
{
    public class GetChapterByIdQuery : IRequest<ChapterDto?>
    {
        public Guid ChapterId { get; set; }
    }
} 