using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using MediatR;
using System; // Cần cho Guid?

namespace Application.Features.Tags.Queries.GetTags
{
    public class GetTagsQuery : IRequest<PagedResult<TagDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? TagGroupId { get; set; }
        public string? NameFilter { get; set; }
        public string OrderBy { get; set; } = "Name"; // Name, TagGroupName
        public bool Ascending { get; set; } = true;
    }
} 