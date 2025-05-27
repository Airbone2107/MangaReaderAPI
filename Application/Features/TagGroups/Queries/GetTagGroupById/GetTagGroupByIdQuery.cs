using Application.Common.DTOs.TagGroups;
using Application.Common.Models;
using MediatR;

namespace Application.Features.TagGroups.Queries.GetTagGroupById
{
    public class GetTagGroupByIdQuery : IRequest<ResourceObject<TagGroupAttributesDto>?>
    {
        public Guid TagGroupId { get; set; }
        public bool IncludeTags { get; set; } = false;
    }
} 