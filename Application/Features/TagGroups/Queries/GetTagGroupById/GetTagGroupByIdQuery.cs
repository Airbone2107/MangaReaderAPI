using Application.Common.DTOs.TagGroups;
using MediatR;

namespace Application.Features.TagGroups.Queries.GetTagGroupById
{
    public class GetTagGroupByIdQuery : IRequest<TagGroupDto?>
    {
        public Guid TagGroupId { get; set; }
        // TODO: [Improvement] Thêm tùy chọn bool IncludeTags để quyết định có load danh sách Tags con không.
        // public bool IncludeTags { get; set; } = false; 
    }
} 