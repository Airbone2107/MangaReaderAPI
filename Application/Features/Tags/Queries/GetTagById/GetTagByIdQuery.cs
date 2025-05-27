using Application.Common.DTOs.Tags;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Tags.Queries.GetTagById
{
    public class GetTagByIdQuery : IRequest<ResourceObject<TagAttributesDto>?>
    {
        public Guid TagId { get; set; }
    }
} 