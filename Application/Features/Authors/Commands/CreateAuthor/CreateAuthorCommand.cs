using MediatR;

namespace Application.Features.Authors.Commands.CreateAuthor
{
    public class CreateAuthorCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }
} 