using MediatR;

namespace Application.Features.Authors.Commands.DeleteAuthor
{
    public class DeleteAuthorCommand : IRequest<Unit>
    {
        public Guid AuthorId { get; set; }
    }
} 