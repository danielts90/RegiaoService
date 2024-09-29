using Microsoft.EntityFrameworkCore;
using Namotion.Reflection;
using RegiaoApi.Models;

namespace RegiaoApi.Context;
public class RegiaoDb : DbContext
{
    public RegiaoDb(DbContextOptions<RegiaoDb> options)
        : base(options) { }

    public DbSet<Regiao> Regioes => Set<Regiao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Regiao>()
            .ToTable("regiao") 
            .HasKey(c => c.Id);

        builder.Entity<Regiao>()
            .Property(o => o.Id)
            .ValueGeneratedOnAdd();
        
        builder.Entity<Regiao>()
            .Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100); 
    }
}