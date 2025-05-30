using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Tags.Queries.GetTags
{
    public class GetTagsQuery : IRequest<PagedResult<ResourceObject<TagAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 100;
        public Guid? TagGroupId { get; set; }
        public string? NameFilter { get; set; }
        public string OrderBy { get; set; } = "Name"; // Name, TagGroupName
        public bool Ascending { get; set; } = true;
    }
} 