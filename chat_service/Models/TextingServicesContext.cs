using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace chat_service.Models;

public partial class TextingServicesContext : DbContext
{
    public TextingServicesContext()
    {
    }

    public TextingServicesContext(DbContextOptions<TextingServicesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<ConversationMember> ConversationMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("PK__conversa__311E7E9AAEBDC03F");

            entity.ToTable("conversations");

            entity.Property(e => e.ConversationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsGroup).HasColumnName("is_group");
            entity.Property(e => e.Remove)
                .HasDefaultValue(false)
                .HasColumnName("remove");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<ConversationMember>(entity =>
        {
            entity.HasKey(e => e.ConversationMemberId).HasName("PK_conversation_members");
            entity.ToTable("conversation_members");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("joined_at");
            entity.Property(e => e.Remove)
                .HasDefaultValue(false)
                .HasColumnName("remove");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Conversation).WithMany()
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_conversation_members_conversation");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__messages__0BBF6EE6C206A6A2");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.IsRemove).HasColumnName("is_remove");
            entity.Property(e => e.LikeId).HasColumnName("like_id");
            entity.Property(e => e.MessageType).HasColumnName("message_type");
            entity.Property(e => e.ParentMessageId).HasColumnName("parent_message_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("FK_messages_conversation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
