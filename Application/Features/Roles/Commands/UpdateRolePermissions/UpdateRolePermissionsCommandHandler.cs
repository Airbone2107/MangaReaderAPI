using Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Application.Features.Roles.Commands.UpdateRolePermissions
{
    public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, Unit>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public UpdateRolePermissionsCommandHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<Unit> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId);
            if (role == null)
            {
                throw new NotFoundException(nameof(IdentityRole), request.RoleId);
            }

            var currentClaims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = currentClaims.Where(c => c.Type == "permission").ToList();

            // Remove claims that are no longer in the request
            foreach (var claim in permissionClaims)
            {
                if (!request.Permissions.Contains(claim.Value))
                {
                    await _roleManager.RemoveClaimAsync(role, claim);
                }
            }

            // Add new claims from the request
            foreach (var permission in request.Permissions)
            {
                if (!permissionClaims.Any(c => c.Value == permission))
                {
                    await _roleManager.AddClaimAsync(role, new Claim("permission", permission));
                }
            }

            return Unit.Value;
        }
    }
} 