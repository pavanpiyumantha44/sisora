using Microsoft.EntityFrameworkCore;
using Sisora.API.Models.Entities;

namespace Sisora.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceRoute> ServiceRoutes => Set<ServiceRoute>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<ParentStudent> ParentStudents => Set<ParentStudent>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripEvent> TripEvents => Set<TripEvent>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- ParentStudent (join table) ---
        modelBuilder.Entity<ParentStudent>()
            .HasKey(ps => new { ps.ParentId, ps.StudentId });

        modelBuilder.Entity<ParentStudent>()
            .HasOne(ps => ps.Parent)
            .WithMany(p => p.ParentStudents)
            .HasForeignKey(ps => ps.ParentId);

        modelBuilder.Entity<ParentStudent>()
            .HasOne(ps => ps.Student)
            .WithMany(s => s.ParentStudents)
            .HasForeignKey(ps => ps.StudentId);

        // --- Driver ---
        modelBuilder.Entity<Driver>()
            .HasIndex(d => d.Phone)
            .IsUnique();

        modelBuilder.Entity<Driver>()
            .Property(d => d.Status)
            .HasConversion<string>();

        // --- Vehicle ---
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.VehicleType)
            .HasConversion<string>();

        // --- Student ---
        modelBuilder.Entity<Student>()
            .HasIndex(s => s.InviteCode)
            .IsUnique();

        // --- Parent ---
        modelBuilder.Entity<Parent>()
            .HasIndex(p => p.Phone)
            .IsUnique();

        // --- Trip ---
        modelBuilder.Entity<Trip>()
            .Property(t => t.TripType)
            .HasConversion<string>();

        modelBuilder.Entity<Trip>()
            .Property(t => t.Status)
            .HasConversion<string>();
            
        modelBuilder.Entity<Trip>()
            .HasOne(t => t.ServiceRoute)
            .WithMany(r => r.Trips)
            .HasForeignKey(t => t.ServiceRouteId);

        // --- TripEvent ---
        modelBuilder.Entity<TripEvent>()
            .Property(te => te.EventType)
            .HasConversion<string>();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}