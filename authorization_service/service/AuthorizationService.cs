using authorization_service.Internal;
using authorization_service.Models;
using infrastructure.caching;
using Microsoft.EntityFrameworkCore;

namespace authorization_service.service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationContext _db;
        private readonly ICacheService _cache;
        public AuthorizationService(AuthorizationContext db, ICacheService cache)
        {
            _db = db;
            _cache = cache;
        }
        public async Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(int userId, int assetId, CancellationToken ct = default)
        {
            var key = $"perms:{userId}:{assetId}";
            var cached = await _cache.GetAsync<string[]>(key);
            if (cached is not null) return cached;

            var list = await (from uar in _db.AccountAssetRoles
                              where uar.UserId == userId && uar.AssetId == assetId
                              join r in _db.Roles on uar.RoleId equals r.RoleId
                              join rp in _db.RolePermissions on r.RoleId equals rp.RoleId
                              join p in _db.Permissions on rp.PermissionId equals p.PermissionId
                              join a in _db.Assets on uar.AssetId equals a.AssetId
                              where r.AssetTypeId == a.AssetTypeId
                              select p.Code)
                              .Distinct()
                              .ToArrayAsync(ct);

            await _cache.SetAsync(key, list);
            return list;
        }

        public async Task<IReadOnlyCollection<string>> GetUserRolesOnAssetAsync(int userId, int assetId, CancellationToken ct = default)
        {
            var roles = await (from uar in _db.AccountAssetRoles
                               where uar.UserId == userId && uar.AssetId == assetId
                               join r in _db.Roles on uar.RoleId equals r.RoleId
                               select r.Code)
                          .ToListAsync(ct);
            return roles;
        }

        public async Task GrantRoleAsync(int grantedBy, int userId, int assetId, int roleId, CancellationToken ct = default)
        {
            var exists = await _db.AccountAssetRoles
                .AnyAsync(x => x.UserId == userId && x.AssetId == assetId && x.RoleId == roleId, ct);

            if (!exists)
            {
                var entity = new AccountAssetRole
                {
                    UserId = userId,
                    AssetId = assetId,
                    RoleId = roleId,
                    GrantedBy = grantedBy
                };

                _db.AccountAssetRoles.Add(entity);
                await _db.SaveChangesAsync(ct);
            }

            await _cache.RemoveAsync($"perms:{userId}:{assetId}");
        }


        public async Task<bool> HasPermissionAsync(int userId, int assetId, string code, CancellationToken ct = default)
        {
            var perms = await GetEffectivePermissionsAsync(userId, assetId, ct);
            return perms.Contains(code, StringComparer.OrdinalIgnoreCase);
        }

        public async Task RevokeRoleAsync(int userId, int assetId, int roleId, CancellationToken ct = default)
        {
            var item = await _db.AccountAssetRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AssetId == assetId && x.RoleId == roleId, ct);
            if (item != null)
            {
                _db.AccountAssetRoles.Remove(item);
                await _db.SaveChangesAsync(ct);
            }
            await _cache.RemoveAsync($"perms:{userId}:{assetId}");
            await Task.CompletedTask;
        }
    }
}
