using Auction.Domain.Entities;
using Auction.Domain.Entities.Base;
using Auction.Infrastructure.Persistence.Seeders;
using Auction.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Auction.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ILogger<AppDbContext> _logger;

    public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<Domain.Entities.Auction> Auctions => Set<Domain.Entities.Auction>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Suprimir warning sobre modelo não-determinístico
        // Causado por HasData com DateTime estático em SeedCategories()
        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas as configurações do assembly atual
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Configuração global de precisão para decimais (PostgreSQL)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // Seed de dados de exemplo (Desabilitado - usando SQL direto nas migrations)
        // DatabaseSeeder.SeedData(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Atualizar timestamps e versão (Optimistic Concurrency Control)
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity)
                {
                    var versionProperty = entry.Property("Version");
                    var updatedAtProperty = entry.Property("UpdatedAt");

                    if (entry.State == EntityState.Added)
                    {
                        versionProperty.CurrentValue = 1L;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        var currentVersion = (long)(versionProperty.CurrentValue ?? 0L);
                        versionProperty.CurrentValue = currentVersion + 1;
                        updatedAtProperty.CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict detected");
            throw new InvalidOperationException(
                "O registro foi modificado por outro processo. Por favor, tente novamente.", ex);
        }
    }
}
