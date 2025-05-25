using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using MediatR;

namespace Application.Features.TagGroups.Queries.GetTagGroups
{
    public class GetTagGroupsQuery : IRequest<PagedResult<TagGroupDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? NameFilter { get; set; }
        public string OrderBy { get; set; } = "Name";
        public bool Ascending { get; set; } = true;
        // TODO: [Improvement] Thêm tùy chọn bool IncludeTags để có thể lấy danh sách tags con trong kết quả phân trang (cần cẩn thận về performance)
    }
} 