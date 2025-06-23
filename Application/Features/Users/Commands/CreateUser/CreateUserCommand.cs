using MediatR;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<string>
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
} 