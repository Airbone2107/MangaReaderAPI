using MediatR;

namespace Application.Features.TagGroups.Commands.CreateTagGroup
{
    public class CreateTagGroupCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }
} 