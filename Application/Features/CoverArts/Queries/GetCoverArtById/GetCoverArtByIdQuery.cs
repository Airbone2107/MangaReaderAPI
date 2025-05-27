using Application.Common.DTOs.CoverArts;
using Application.Common.Models;
using MediatR;

namespace Application.Features.CoverArts.Queries.GetCoverArtById
{
    public class GetCoverArtByIdQuery : IRequest<ResourceObject<CoverArtAttributesDto>?>
    {
        public Guid CoverId { get; set; }
    }
} 