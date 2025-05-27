using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Chapters.Queries.GetChapterPages
{
    public class GetChapterPagesQuery : IRequest<PagedResult<ResourceObject<ChapterPageAttributesDto>>>
    {
        public Guid ChapterId { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20; // Số trang mỗi lần lấy
        // TODO: [Improvement] Thêm OrderBy nếu cần (thường chỉ cần PageNumber)
    }
} 