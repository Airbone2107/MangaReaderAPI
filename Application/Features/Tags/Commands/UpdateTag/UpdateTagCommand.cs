using MediatR;

namespace Application.Features.Tags.Commands.UpdateTag
{
    public class UpdateTagCommand : IRequest<Unit>
    {
        public Guid TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
} 