using MediatR;

namespace Application.Features.Users.Commands.UpdateUserRoles
{
    public class UpdateUserRolesCommand : IRequest<Unit>
    {
        public string UserId { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
} 