using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace authentication_service.Models;

public partial class AuthenticationContext : DbContext
{
    public AuthenticationContext()
    {
    }

    public AuthenticationContext(DbContextOptions<AuthenticationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Friendship> Friendships { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__account__46A222CDC83752DC");

            entity.ToTable("account");

            entity.HasIndex(e => e.Email, "UQ__account__AB6E616404563141").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AccountName)
                .HasMaxLength(500)
                .HasColumnName("account_name");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(500)
                .HasColumnName("photo_url");
        });

        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.FriendshipId).HasName("PK_friendships");

            entity.ToTable("friendships");

            entity.Property(e => e.FriendshipId)
                .HasColumnName("friendship_id")
                .HasDefaultValueSql("NEWSEQUENTIALID()");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FriendId).HasColumnName("friend_id");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("SYSDATETIME()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("SYSDATETIME()");

            // ====== RELATIONSHIPS ======
            entity.HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_friend_user");

            entity.HasOne(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_friend_friend");

            // ====== UNIQUE (user_id, friend_id) ======
            entity.HasIndex(e => new { e.UserId, e.FriendId })
                  .IsUnique()
                  .HasDatabaseName("uq_friendship");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
