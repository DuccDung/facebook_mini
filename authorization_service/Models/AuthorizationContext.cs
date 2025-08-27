using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace authorization_service.Models;

public partial class AuthorizationContext : DbContext
{
    public AuthorizationContext()
    {
    }

    public AuthorizationContext(DbContextOptions<AuthorizationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccountAssetRole> AccountAssetRoles { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetType> AssetTypes { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountAssetRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.AssetId, e.RoleId }).HasName("PK__account___B9E08B0B25CC10D6");

            entity.ToTable("account_asset_roles");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.GrantedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("granted_at");
            entity.Property(e => e.GrantedBy).HasColumnName("granted_by");

            entity.HasOne(d => d.Asset).WithMany(p => p.AccountAssetRoles)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK_uar_assets");

            entity.HasOne(d => d.Role).WithMany(p => p.AccountAssetRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_uar_roles");
        });

        modelBuilder.Entity<RolePermission>()
          .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__assets__D28B561DE01EF998");

            entity.ToTable("assets");

            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.AssetTypeId).HasColumnName("asset_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.MetaJson).HasColumnName("meta_json");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");

            entity.HasOne(d => d.AssetType).WithMany(p => p.Assets)
                .HasForeignKey(d => d.AssetTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_assets_asset_types");
        });

        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.HasKey(e => e.AssetTypeId).HasName("PK__asset_ty__95A1E2BCA5F16400");

            entity.ToTable("asset_types");

            entity.HasIndex(e => e.Code, "UQ__asset_ty__357D4CF96C9181BD").IsUnique();

            entity.Property(e => e.AssetTypeId).HasColumnName("asset_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__permissi__E5331AFA21EA743D");

            entity.ToTable("permissions");

            entity.HasIndex(e => e.Code, "UQ__permissi__357D4CF9C5892B7F").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CCDD1E5D23");

            entity.ToTable("roles");

            entity.HasIndex(e => new { e.AssetTypeId, e.Code }, "UQ_roles_assettype_code").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.AssetTypeId).HasColumnName("asset_type_id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(true)
                .HasColumnName("is_system");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.AssetType).WithMany(p => p.Roles)
                .HasForeignKey(d => d.AssetTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_roles_asset_types");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("FK_role_permissions_permissions"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_role_permissions_roles"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("PK__role_per__C85A54637233C67B");
                        j.ToTable("role_permissions");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<int>("PermissionId").HasColumnName("permission_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
