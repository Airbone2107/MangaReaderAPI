namespace Application.Common.DTOs.Roles
{
    public class UpdateRolePermissionsRequestDto
    {
        public List<string> Permissions { get; set; } = new List<string>();
    }
} 