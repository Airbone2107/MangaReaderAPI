using Application.Common.DTOs.Authors;
using Application.Common.Models; // Cho ResourceObject
using MediatR;

namespace Application.Features.Authors.Queries.GetAuthorById
{
    public class GetAuthorByIdQuery : IRequest<ResourceObject<AuthorAttributesDto>?>
    {
        public Guid AuthorId { get; set; }
    }
} 