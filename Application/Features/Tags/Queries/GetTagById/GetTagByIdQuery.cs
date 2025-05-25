using Application.Common.DTOs.Tags;
using MediatR;

namespace Application.Features.Tags.Queries.GetTagById
{
    public class GetTagByIdQuery : IRequest<TagDto?>
    {
        public Guid TagId { get; set; }
    }
} 