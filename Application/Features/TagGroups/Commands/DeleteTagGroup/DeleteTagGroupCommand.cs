using MediatR;

namespace Application.Features.TagGroups.Commands.DeleteTagGroup
{
    public class DeleteTagGroupCommand : IRequest<Unit>
    {
        public Guid TagGroupId { get; set; }
    }
} 