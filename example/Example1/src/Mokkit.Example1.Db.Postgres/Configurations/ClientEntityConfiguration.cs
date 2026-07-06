using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mokkit.Example1.Domain.Entities;

namespace Mokkit.Example1.Db.Postgres.Configurations;

public class ClientEntityConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable(DatabaseConstants.ClientsTableName);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(255)")
            .IsRequired();
        
        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(255)")
            .IsRequired();
        
        builder.Property(t => t.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasColumnType("int")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasKey(t => new { t.Id })
            .HasName("pk_clients_id");

        builder.HasIndex(t => t.Email)
            .HasDatabaseName("ix_clients_email")
            .IsUnique();
    }
}