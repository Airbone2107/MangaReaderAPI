using MediatR;

namespace Application.Features.Tags.Commands.CreateTag
{
    public class CreateTagCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
} 