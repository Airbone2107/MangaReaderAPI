using Application.Common.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQuery : IRequest<RoleDetailsDto>
    {
        public string RoleId { get; set; } = string.Empty;
    }
} 