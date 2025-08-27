namespace authorization_service.Internal
{
    public interface IAuthorizationService
    {
        // Kiểm tra quyền trên 1 asset cụ thể
        Task<bool> HasPermissionAsync(int userId, int assetId, string permissionCode, CancellationToken ct = default);

        // Liệt kê toàn bộ permission hiệu lực của user trên 1 asset
        Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(int userId, int assetId, CancellationToken ct = default);

        // Liệt kê role của user trên asset
        Task<IReadOnlyCollection<string>> GetUserRolesOnAssetAsync(int userId, int assetId, CancellationToken ct = default);

        // Gán / thu hồi role cho user trên asset
        Task GrantRoleAsync(int grantedBy, int userId, int assetId, int roleId, CancellationToken ct = default);
        Task RevokeRoleAsync(int userId, int assetId, int roleId, CancellationToken ct = default);
    }
}
