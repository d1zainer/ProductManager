using Microsoft.EntityFrameworkCore;
using Web.Models.Entities;

namespace Web.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("product");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
        });
        
        base.OnModelCreating(modelBuilder);
    }
}