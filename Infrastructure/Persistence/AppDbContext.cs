using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistence
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Campanya> Campanyes => Set<Campanya>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Campanya>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
                entity.Property(c => c.PressupostTotal).IsRequired();
                entity.Property(c => c.PressupostConsumit).IsRequired();
                entity.Property(c => c.DataInici).IsRequired();
                entity.Property(c => c.DataFi).IsRequired();
                entity.Property(c => c.Estat).IsRequired();
            });
        }
    }
}
