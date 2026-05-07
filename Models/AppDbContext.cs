using Microsoft.EntityFrameworkCore;

namespace SaigonRide.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            // STATION → VEHICLE
            
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Station)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(v => v.StationID)
                .OnDelete(DeleteBehavior.Cascade);

            
            // VEHICLE → RENTAL
            
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleID)
                .OnDelete(DeleteBehavior.Cascade);

            
            // RENTAL → PAYMENT
           
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Rental)
                .WithMany()
                .HasForeignKey(p => p.RentalID)
                .OnDelete(DeleteBehavior.Cascade);

            
            // RENTAL → STATION (KHÔNG CASCADE)
         
            modelBuilder.Entity<Rental>()
                .HasOne(r => r.StartStation)
                .WithMany()
                .HasForeignKey(r => r.StartStationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rental>()
                .HasOne(r => r.ReturnStation)
                .WithMany()
                .HasForeignKey(r => r.ReturnStationID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}