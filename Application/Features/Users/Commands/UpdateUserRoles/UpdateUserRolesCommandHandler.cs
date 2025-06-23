using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Users.Commands.UpdateUserRoles
{
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateUserRolesCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Unit> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), request.UserId);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!result.Succeeded)
            {
                throw new ValidationException("Failed to remove existing user roles.");
            }

            result = await _userManager.AddToRolesAsync(user, request.Roles);
            if (!result.Succeeded)
            {
                throw new ValidationException("Failed to add new roles to user.");
            }
            
            return Unit.Value;
        }
    }
} 