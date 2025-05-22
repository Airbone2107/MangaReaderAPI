using MediatR;

namespace Application.Features.Authors.Commands.UpdateAuthor
{
    public class UpdateAuthorCommand : IRequest<Unit> // Hoặc IRequest<AuthorDto> nếu muốn trả về author đã cập nhật
    {
        public Guid AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
    }
} 