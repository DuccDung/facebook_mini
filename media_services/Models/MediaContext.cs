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
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<PostComment> PostComments { get; set; }
    public virtual DbSet<PostShare> PostShares { get; set; }

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
            entity.Property(e => e.AssetId)
                .HasMaxLength(200)
                .HasColumnName("asset_id");
            entity.Property(e => e.CreateAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("create_at");
            entity.Property(e => e.MediaType)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("media_type");
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(2048)
                .HasColumnName("media_url");
            entity.Property(e => e.ObjectKey)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("object_key");
            entity.Property(e => e.Size).HasColumnName("size");
        });
        // =============== POSTS ===============
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");

            entity.HasKey(e => e.PostId);

            entity.Property(e => e.PostId)
                .HasColumnName("post_id")
                .HasDefaultValueSql("(newsequentialid())");

            entity.Property(e => e.AccountId).HasColumnName("account_id");

            entity.Property(e => e.Content).HasColumnName("content");

            entity.Property(e => e.PostType).HasColumnName("post_type");

            entity.Property(e => e.CreateAt)
                .HasColumnName("create_at")
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.Property(e => e.UpdateAt)
                .HasColumnName("update_at")
                .HasPrecision(0);

            entity.Property(e => e.IsRemove)
                .HasColumnName("is_remove")
                .HasDefaultValue(false);

            entity.HasMany(e => e.PostShares)
                .WithOne(e => e.Post)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Comments)
                .WithOne(e => e.Post)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============== POST SHARES ===============
        modelBuilder.Entity<PostShare>(entity =>
        {
            entity.ToTable("post_shares");

            entity.HasKey(e => e.PsId);

            entity.Property(e => e.PsId)
                .HasColumnName("ps_id")
                .HasDefaultValueSql("(newsequentialid())");

            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
        });

        // =============== POST COMMENTS ===============
        modelBuilder.Entity<PostComment>(entity =>
        {
            entity.ToTable("post_comments");

            entity.HasKey(e => e.CommentId);

            entity.Property(e => e.CommentId)
                .HasColumnName("comment_id")
                .HasDefaultValueSql("(newsequentialid())");

            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");

            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");

            entity.Property(e => e.CreateAt)
                .HasColumnName("create_at")
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(e => e.ParentComment)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ParentCommentId);

            entity.HasOne(e => e.Post)
                .WithMany(e => e.Comments)
                .HasForeignKey(e => e.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
