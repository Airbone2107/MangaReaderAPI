using MediatR;

namespace Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsCommand : IRequest<Unit>
    {
        public string RoleId { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }
} 