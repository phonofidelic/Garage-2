using Garage_2.Models;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Data
{
    public class GarageContext : DbContext
    {
        public GarageContext(DbContextOptions<GarageContext> options)
            : base(options)
        {
        }

        public DbSet<ParkedVehicle> ParkedVehicle { get; set; } = default!;
        public DbSet<VehicleSpot> VehicleSpots { get; set; } = default!;
        public DbSet<ParkingSpot> ParkingSpots { get; set; } = default!;

        // OnModelCreating() skapar relationen mellan ParkedVehicle och ParkingSpot, via join-tabell VehicleSpot
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Varje p-plats (ParkingSpot) har ett unikt SpotNumber (inte Id) 
            modelBuilder.Entity<ParkingSpot>()
                .HasIndex(s => s.SpotNumber)
                .IsUnique();

            // Ett fordon (med ett visst Id) ska inte kunna ha två rader mot samma p-plats (med ett visst Id)
            modelBuilder.Entity<VehicleSpot>()
                .HasIndex(vs => new { vs.ParkedVehicleId, vs.ParkingSpotId })
                .IsUnique();

            // 1:1 relation mellan VehicleSpots (rad i join-tabellen) och ParkedVehicle  
            // som i sin tur har 1:M relation med VehicleSpots (rader join-tabellen)
            modelBuilder.Entity<VehicleSpot>()
                .HasOne(vs => vs.ParkedVehicle)
                .WithMany(v => v.VehicleSpots)
                .HasForeignKey(vs => vs.ParkedVehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:1 relation mellan VehicleSpots och ParkingSpots 
            // som i sin tur har 1:M relation med VehicleSpots 
            modelBuilder.Entity<VehicleSpot>()
                .HasOne(vs => vs.ParkingSpot)
                .WithMany(s => s.VehicleSpots)
                .HasForeignKey(vs => vs.ParkingSpotId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
