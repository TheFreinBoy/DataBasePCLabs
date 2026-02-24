using Microsoft.EntityFrameworkCore;
using DBPCLabs.Models;


namespace DBPCLabs.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Laboratory> Laboratories { get; set; }
    public DbSet<Computer> Computers { get; set; }
    public DbSet<Software> Softwares { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Computer>()
            .HasMany(c => c.InstalledSoftware)
            .WithMany(s => s.Computers)
            .UsingEntity(j => j.ToTable("ComputerSoftware"));
        
        modelBuilder.Entity<Computer>()
            .HasIndex(c => c.InventoryNumber)
            .IsUnique();
        
        modelBuilder.Entity<Computer>()
            .HasOne(c => c.Laboratory)
            .WithMany(l => l.Computers)
            .HasForeignKey(c => c.LaboratoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}