using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using media_services.Models.MLDb;

namespace media_services.Data.MLDb;

public partial class LogDbContext : DbContext
{
    public LogDbContext()
    {
    }

    public LogDbContext(DbContextOptions<LogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostComment> PostComments { get; set; }

    public virtual DbSet<PostMedium> PostMedia { get; set; }

    public virtual DbSet<PostShare> PostShares { get; set; }

  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.LikeId).HasName("PK__likes__992C793081D5FE5B");

            entity.ToTable("likes");

            entity.HasIndex(e => new { e.AccountId, e.PostId }, "UQ_Like_Post").IsUnique();

            entity.Property(e => e.LikeId).HasColumnName("like_id");
            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .HasColumnName("account_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.LikeType)
                .HasMaxLength(50)
                .HasColumnName("like_type");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Post).WithMany(p => p.Likes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__likes__post_id__46E78A0C");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__posts__3ED78766EBB70622");

            entity.ToTable("posts");

            entity.Property(e => e.PostId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("post_id");
            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.IsRemove)
                .HasDefaultValue(false)
                .HasColumnName("is_remove");
            entity.Property(e => e.PostType)
                .HasMaxLength(50)
                .HasColumnName("post_type");
            entity.Property(e => e.UpdateAt)
                .HasColumnType("datetime")
                .HasColumnName("update_at");
        });

        modelBuilder.Entity<PostComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__post_com__E795768782B1FBC0");

            entity.ToTable("post_comments");

            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .HasColumnName("account_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostComments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_comm__post___47DBAE45");
        });

        modelBuilder.Entity<PostMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("PK__post_med__D0A840F498E1AAC2");

            entity.ToTable("post_media");

            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.MediaType)
                .HasMaxLength(50)
                .HasColumnName("media_type");
            entity.Property(e => e.MediaUrl).HasColumnName("media_url");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_medi__post___48CFD27E");
        });

        modelBuilder.Entity<PostShare>(entity =>
        {
            entity.HasKey(e => e.PsId).HasName("PK__post_sha__5CFD143FAA78C8D0");

            entity.ToTable("post_shares");

            entity.Property(e => e.PsId).HasColumnName("ps_id");
            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .HasColumnName("account_id");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("create_at");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostShares)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__post_shar__post___49C3F6B7");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
