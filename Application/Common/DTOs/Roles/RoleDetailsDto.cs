namespace Application.Common.DTOs.Roles
{
    public class RoleDetailsDto : RoleDto
    {
        public List<string> Permissions { get; set; } = new List<string>();
    }
} 