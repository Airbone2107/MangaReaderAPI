namespace Application.Common.DTOs.Users
{
    public class UpdateUserRolesRequestDto
    {
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
} 