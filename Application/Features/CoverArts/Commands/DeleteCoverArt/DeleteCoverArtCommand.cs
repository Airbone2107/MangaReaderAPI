using MediatR;

namespace Application.Features.CoverArts.Commands.DeleteCoverArt
{
    public class DeleteCoverArtCommand : IRequest<Unit>
    {
        public Guid CoverId { get; set; }
    }
} 