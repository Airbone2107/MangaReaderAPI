using Application.Common.DTOs.CoverArts;
using MediatR;

namespace Application.Features.CoverArts.Queries.GetCoverArtById
{
    public class GetCoverArtByIdQuery : IRequest<CoverArtDto?>
    {
        public Guid CoverId { get; set; }
    }
} 