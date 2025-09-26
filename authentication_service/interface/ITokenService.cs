using authentication_service.Dtos;

namespace authentication_service.Internal
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateToken(int userId);
        Task<TokenResponse> GenerateTokenTemporarily(int userId);
        Task<string> GenerateRefreshTokenAsync();
    }
}
