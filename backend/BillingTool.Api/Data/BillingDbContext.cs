using BillingTool.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingTool.Api.Data;

public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<AutoPartEntity> AutoParts => Set<AutoPartEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AutoPartEntity>(entity =>
        {
            entity.ToTable("AutoParts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.HsnSac).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Rate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
        });
    }
}
