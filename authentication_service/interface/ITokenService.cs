namespace authentication_service.Internal
{
    public interface ITokenService
    {
        Task<string> GenerateToken(string userId, string role);
    }
}
