using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração Table Per Hierarchy (TPH) para User e suas subclasses
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // Configurar discriminator para TPH (Table Per Hierarchy)
        builder.HasDiscriminator<string>("user_type")
            .HasValue<IndividualEntity>("Individual")
            .HasValue<CorporateEntity>("Corporate");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        // Value Object: Email
        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("idx_users_email");
        });

        // Value Object: Password
        builder.OwnsOne(u => u.Password, password =>
        {
            password.Property(p => p.HashedValue)
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();
        });

        // Audit fields (BaseEntity)
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        // Ignorar Domain Events (não persiste no banco)
        builder.Ignore(u => u.DomainEvents);
    }
}

/// <summary>
/// Configuração específica para IndividualEntity
/// </summary>
public class IndividualEntityConfiguration : IEntityTypeConfiguration<IndividualEntity>
{
    public void Configure(EntityTypeBuilder<IndividualEntity> builder)
    {
        // Value Object: CPF
        builder.OwnsOne(i => i.Cpf, cpf =>
        {
            cpf.Property(c => c.Value)
                .HasColumnName("cpf")
                .HasMaxLength(11);
        });
    }
}

/// <summary>
/// Configuração específica para CorporateEntity
/// </summary>
public class CorporateEntityConfiguration : IEntityTypeConfiguration<CorporateEntity>
{
    public void Configure(EntityTypeBuilder<CorporateEntity> builder)
    {
        // Value Object: CNPJ
        builder.OwnsOne(c => c.Cnpj, cnpj =>
        {
            cnpj.Property(cn => cn.Value)
                .HasColumnName("cnpj")
                .HasMaxLength(14);
        });

        // CompanyName
        builder.Property(c => c.CompanyName)
            .HasColumnName("company_name")
            .HasMaxLength(255)
            .IsRequired();
    }
}
