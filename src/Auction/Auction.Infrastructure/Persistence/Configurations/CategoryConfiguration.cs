using Auction.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auction.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        // Audit fields (BaseEntity)
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        // Índice único no nome
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("idx_categories_name");

        // Ignorar Domain Events (não persiste no banco)
        builder.Ignore(c => c.DomainEvents);

        // Seed de categorias iniciais (opcional)
        builder.HasData(
            Category.Create("Eletrônicos", "Dispositivos eletrônicos e gadgets"),
            Category.Create("Veículos", "Carros, motos e outros veículos"),
            Category.Create("Imóveis", "Casas, apartamentos e terrenos"),
            Category.Create("Arte", "Obras de arte e colecionáveis"),
            Category.Create("Joias", "Joias e pedras preciosas"),
            Category.Create("Antiguidades", "Itens antigos e vintage"),
            Category.Create("Outros", "Outras categorias")
        );
    }
}
