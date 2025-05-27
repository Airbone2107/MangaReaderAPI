using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Models; // Cho ResourceObject
using MediatR;

namespace Application.Features.Authors.Queries.GetAuthors
{
    public class GetAuthorsQuery : IRequest<PagedResult<ResourceObject<AuthorAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public string? NameFilter { get; set; }
        public string OrderBy { get; set; } = "Name"; 
        public bool Ascending { get; set; } = true;
    }
} 