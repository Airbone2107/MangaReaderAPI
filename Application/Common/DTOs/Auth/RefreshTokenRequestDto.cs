using System.ComponentModel.DataAnnotations;

namespace Application.Common.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
} 