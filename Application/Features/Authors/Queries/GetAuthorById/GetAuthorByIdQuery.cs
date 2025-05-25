using Application.Common.DTOs.Authors;
using MediatR;

namespace Application.Features.Authors.Queries.GetAuthorById
{
    public class GetAuthorByIdQuery : IRequest<AuthorDto?>
    {
        public Guid AuthorId { get; set; }
    }
} 