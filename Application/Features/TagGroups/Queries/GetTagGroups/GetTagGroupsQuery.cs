using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using Application.Common.Models;
using MediatR;

namespace Application.Features.TagGroups.Queries.GetTagGroups
{
    public class GetTagGroupsQuery : IRequest<PagedResult<ResourceObject<TagGroupAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 100;
        public string? NameFilter { get; set; }
        public string OrderBy { get; set; } = "Name";
        public bool Ascending { get; set; } = true;
        // TODO: [Improvement] Thêm tùy chọn bool IncludeTags để có thể lấy danh sách tags con trong kết quả phân trang (cần cẩn thận về performance)
    }
} 