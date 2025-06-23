using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(ApplicationUser user);
        RefreshToken GenerateRefreshToken(string userId);
    }
} 