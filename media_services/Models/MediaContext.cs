using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace media_services.Models;

public partial class MediaContext : DbContext
{
    public MediaContext()
    {
    }

    public MediaContext(DbContextOptions<MediaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Medium> Media { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");

            entity.Property(e => e.LikeId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("like_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.CreateAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("create_at");
            entity.Property(e => e.LikeType).HasColumnName("like_type");
        });

        modelBuilder.Entity<Medium>(entity =>
        {
            entity.HasKey(e => e.MediaId);

            entity.ToTable("media");

            entity.Property(e => e.MediaId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("media_id");
            entity.Property(e => e.AssetId).HasColumnName("asset_id");
            entity.Property(e => e.CreateAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("create_at");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2048)
                .HasColumnName("media_url");
            entity.Property(e => e.ObjectKey)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("object_key");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
