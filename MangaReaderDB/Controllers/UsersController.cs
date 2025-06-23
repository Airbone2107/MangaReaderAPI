using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.UpdateUserRoles;
using Application.Features.Users.Queries.GetUsers;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực cho toàn bộ controller
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permissions.Users.View)]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] GetUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Users.Create)]
        public async Task<IActionResult> CreateUser(CreateUserRequestDto request)
        {
            var command = new CreateUserCommand
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = request.Password,
                Roles = request.Roles
            };

            var userId = await _mediator.Send(command);
            // Có thể trả về thông tin user vừa tạo bằng cách gọi GetUserById query
            return CreatedAtAction(nameof(GetUsers), new { id = userId }, new { userId });
        }

        [HttpPut("{id}/roles")]
        [Authorize(Policy = Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateUserRoles(string id, UpdateUserRolesRequestDto request)
        {
            var command = new UpdateUserRolesCommand
            {
                UserId = id,
                Roles = request.Roles
            };
            await _mediator.Send(command);
            return NoContent();
        }
    }
} 