using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using MediatR;

namespace Application.Features.Chapters.Queries.GetChapterPages
{
    public class GetChapterPagesQuery : IRequest<PagedResult<ChapterPageDto>>
    {
        public Guid ChapterId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 30; // Số trang mỗi lần lấy
        // TODO: [Improvement] Thêm OrderBy nếu cần (thường chỉ cần PageNumber)
    }
} 