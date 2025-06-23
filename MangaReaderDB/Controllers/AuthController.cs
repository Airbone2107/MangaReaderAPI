using Application.Common.DTOs.Auth;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new ApplicationUser { UserName = registerDto.Username, Email = registerDto.Email };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                return Ok(new AuthResponseDto { IsSuccess = true, Message = "User registered successfully!" });
            }
            return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(" | ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid username or password." });
            }

            var accessToken = await _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(CancellationToken.None);
            
            return Ok(new AuthResponseDto 
            { 
                IsSuccess = true, 
                Message = "Login successful!",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var storedToken = await _context.RefreshTokens.Include(rt => rt.ApplicationUser)
                                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired refresh token." });
            }
            
            var user = storedToken.ApplicationUser;
            if (user == null)
            {
                 return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "User not found for this token." });
            }

            // Tạo token mới
            var newAccessToken = await _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
            
            // Thu hồi token cũ và thêm token mới
            storedToken.Revoked = DateTime.UtcNow;
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Token refreshed successfully!",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var storedToken = await _context.RefreshTokens
                                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
            
            // User chỉ có thể thu hồi token của chính mình
            if (storedToken == null || storedToken.ApplicationUserId != userId)
            {
                 return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid token." });
            }

            storedToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Token revoked successfully." });
        }
    }
} 