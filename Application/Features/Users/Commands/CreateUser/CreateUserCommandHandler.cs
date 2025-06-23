using Application.Exceptions;
using Domain.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, string>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<string> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new ValidationException(string.Join("\n", result.Errors.Select(e => e.Description)));
            }
            
            // Gán vai trò User mặc định
            if(!request.Roles.Any())
            {
                await _userManager.AddToRoleAsync(user, AppRoles.User);
            }
            else
            {
                foreach(var roleName in request.Roles)
                {
                    if(!await _roleManager.RoleExistsAsync(roleName))
                    {
                        throw new NotFoundException($"Role '{roleName}' not found.");
                    }
                }
                await _userManager.AddToRolesAsync(user, request.Roles);
            }

            return user.Id;
        }
    }
} 