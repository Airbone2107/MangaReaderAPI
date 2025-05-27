using Application.Common.DTOs.Chapters;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Chapters.Queries.GetChapterById
{
    public class GetChapterByIdQuery : IRequest<ResourceObject<ChapterAttributesDto>?>
    {
        public Guid ChapterId { get; set; }
    }
} 