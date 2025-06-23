using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic; // Cần cho List
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
// <<< THAY ĐỔI: Sử dụng thư viện mới và hiệu năng hơn >>>
using Microsoft.IdentityModel.JsonWebTokens;

namespace Infrastructure.Authentication
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SymmetricSecurityKey _key;
        // <<< THAY ĐỔI: Khai báo handler mới >>>
        private readonly JsonWebTokenHandler _tokenHandler;

        public TokenService(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            // <<< THAY ĐỔI: Khởi tạo handler mới >>>
            _tokenHandler = new JsonWebTokenHandler();
        }

        public async Task<string> CreateToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                // Các claims tiêu chuẩn của JWT
                new Claim(JwtRegisteredClaimNames.Sub, user.Id), // Subject (ID người dùng)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID (ID duy nhất cho mỗi token)
                
                // Các claims tiện ích của ASP.NET Core Identity
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            // <<< THÊM MỚI: Lấy và thêm vai trò của người dùng vào token >>>
            // Điều này sẽ rất hữu ích cho việc phân quyền [Authorize(Roles = "Admin")] sau này
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // <<< KẾT THÚC THÊM MỚI >>>

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature)
            };

            // JsonWebTokenHandler.CreateToken trả về thẳng chuỗi JWT, không cần qua đối tượng trung gian
            var token = _tokenHandler.CreateToken(tokenDescriptor);

            return token;
        }

        // Phương thức này không liên quan đến thư viện JWT nên không cần thay đổi
        public RefreshToken GenerateRefreshToken(string userId)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLInDays),
                Created = DateTime.UtcNow,
                ApplicationUserId = userId
            };
        }
    }
}