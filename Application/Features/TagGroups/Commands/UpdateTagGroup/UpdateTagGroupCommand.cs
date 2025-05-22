using MediatR;

namespace Application.Features.TagGroups.Commands.UpdateTagGroup
{
    public class UpdateTagGroupCommand : IRequest<Unit>
    {
        public Guid TagGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
} 