using Application.Common.DTOs.Roles;
using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Roles.Queries.GetRolePermissions
{
    public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, RoleDetailsDto>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetRolePermissionsQueryHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<RoleDetailsDto> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException(nameof(IdentityRole), request.RoleId);
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            
            var roleDto = new RoleDetailsDto
            {
                Id = role.Id,
                Name = role.Name!,
                Permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList()
            };

            return roleDto;
        }
    }
} 