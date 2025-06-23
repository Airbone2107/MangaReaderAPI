using Application.Common.DTOs.Roles;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public GetRolesQueryHandler(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleDto { Id = r.Id, Name = r.Name! })
                .ToListAsync(cancellationToken);
                
            return roles;
        }
    }
} 