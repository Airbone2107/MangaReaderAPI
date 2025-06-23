using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<PagedResult<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }
            
            return new PagedResult<UserDto>(userDtos, totalUsers, request.Offset, request.Limit);
        }
    }
} 