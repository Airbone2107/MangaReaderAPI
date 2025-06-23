using Application.Common.DTOs.Roles;
using MediatR;

namespace Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQuery : IRequest<List<RoleDto>>
    {
    }
} 