using MediatR;

namespace Application.Features.Tags.Commands.DeleteTag
{
    public class DeleteTagCommand : IRequest<Unit>
    {
        public Guid TagId { get; set; }
    }
} 