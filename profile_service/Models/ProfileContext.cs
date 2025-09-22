using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace profile_service.Models;

public partial class ProfileContext : DbContext
{
    public ProfileContext()
    {
    }

    public ProfileContext(DbContextOptions<ProfileContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profile");

            entity.Property(e => e.ProfileId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("profile_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.AvartaUrl)
                .HasMaxLength(512)
                .HasColumnName("avarta_url");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.CreateAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("create_at");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UpdateAt)
                .HasPrecision(3)
                .HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
