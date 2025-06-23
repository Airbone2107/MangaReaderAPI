using Application.Common.DTOs.Roles;
using Application.Features.Roles.Commands.UpdateRolePermissions;
using Application.Features.Roles.Queries.GetRolePermissions;
using Application.Features.Roles.Queries.GetRoles;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<ActionResult<List<RoleDto>>> GetRoles()
        {
            return await _mediator.Send(new GetRolesQuery());
        }

        [HttpGet("{id}/permissions")]
        [Authorize(Policy = Permissions.Roles.View)]
        public async Task<ActionResult<RoleDetailsDto>> GetRolePermissions(string id)
        {
            var query = new GetRolePermissionsQuery { RoleId = id };
            return await _mediator.Send(query);
        }

        [HttpPut("{id}/permissions")]
        [Authorize(Policy = Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateRolePermissions(string id, UpdateRolePermissionsRequestDto request)
        {
            var command = new UpdateRolePermissionsCommand
            {
                RoleId = id,
                Permissions = request.Permissions
            };
            await _mediator.Send(command);
            return NoContent();
        }
    }
} 