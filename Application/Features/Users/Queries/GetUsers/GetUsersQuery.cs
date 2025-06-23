using Application.Common.DTOs;
using Application.Common.DTOs.Users;
using MediatR;

namespace Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<PagedResult<UserDto>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
    }
} 