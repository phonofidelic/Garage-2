using Garage_2.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VehicleType = Garage_2.Models.Entities.VehicleType;

namespace Garage_2.Data
{
    public class GarageContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public GarageContext(DbContextOptions<GarageContext> options)
            : base(options)
        {
        }

        //public DbSet<ParkedVehicle> ParkedVehicle { get; set; } = default!;
        //public DbSet<VehicleSpot> VehicleSpots { get; set; } = default!;

        public DbSet<ParkingSpotV2> ParkingSpotV2 { get; set; } = default!;
        public DbSet<VehicleType> VehicleTypes { get; set; } = default!;
        public DbSet<Vehicle> Vehicles { get; set; } = default!;
        public DbSet<ParkingSession> ParkingSessions { get; set; } = default!;
        public DbSet<VehicleParking> VehicleParkings { get; set; } = default!;


        // OnModelCreating() skapar relationen mellan ParkedVehicle och ParkingSpot, via join-tabell VehicleSpot
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // APPLICATIONUSER
            // -------------------------------
            // Personnummer (SSN) är unikt
            modelBuilder.Entity<ApplicationUser>().HasIndex(u => u.SSN).IsUnique();

            // Personnummer måsre finnas
            modelBuilder.Entity<ApplicationUser>().Property(u => u.SSN).IsRequired();


            // VEHICLETYPE
            // ----------------------
            modelBuilder.Entity<VehicleType>().Property(t => t.Name).IsRequired();

            // Undvik dubletter av fordonstyper
            modelBuilder.Entity<VehicleType>().HasIndex(t => t.Name).IsUnique();


            // VEHICLE
            // ----------------
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(v => v.RegistrationNumber).IsRequired();
                entity.Property(v => v.Make).IsRequired();
                entity.Property(v => v.Model).IsRequired();
                entity.Property(v => v.Color).IsRequired();
            });

            // RegNr unikt
            modelBuilder.Entity<Vehicle>().HasIndex(v => v.RegistrationNumber).IsUnique();

            // Vehicle har M:1 relation med VehicleType 
            modelBuilder.Entity<Vehicle>().HasOne(v => v.VehicleType)
                      .WithMany(t => t.Vehicles)
                      .HasForeignKey(v => v.VehicleTypeId)
                      .OnDelete(DeleteBehavior.Restrict);   // Restrict viktigt: Tar ej bort VehicleType om Vehicle tas bort

            // Vehicle har M:1 relation med ApplicationUser (ägare). 
            modelBuilder.Entity<Vehicle>().HasOne(v => v.User)
                      .WithMany(u => u.Vehicles)
                      .HasForeignKey(v => v.ApplicationUserId)
                      .OnDelete(DeleteBehavior.Cascade);   // Obs! Cascade här? Ta användare bort, försvinner deras fordon och all historik via ParkingSessions. Rätt eller fel?


            // PARKINGSPOTV2
            // --------------------

            // SpotNumber (p-plats nr) är unikt
            modelBuilder.Entity<ParkingSpotV2>().HasIndex(s => s.SpotNumber).IsUnique();

            // Har alltid 3 enheter
            modelBuilder.Entity<ParkingSpotV2>().Property(s => s.CapacityUnits).HasDefaultValue(3);

            // Optional DB-regel: capacity måste vara 3
            modelBuilder.Entity<ParkingSpotV2>().ToTable(tb => tb.HasCheckConstraint("CK_ParkingSpotV2_CapacityUnits", "[CapacityUnits] = 3"));



            // PARKINGSESSION
            // -------------------------------
            modelBuilder.Entity<ParkingSession>().Property(ps => ps.ArrivalTime).IsRequired();

            // M:1 relation mellan ParkingSession och Vehicle 
            modelBuilder.Entity<ParkingSession>().HasOne(ps => ps.Vehicle)
                      .WithMany(v => v.ParkingSessions)
                      .HasForeignKey(ps => ps.VehicleId)
                      .OnDelete(DeleteBehavior.Cascade);

            // Max 1 aktiv session per Vehicle - I SQL Server: filtered unique index
            modelBuilder.Entity<ParkingSession>().HasIndex(ps => ps.VehicleId).IsUnique().HasFilter("[DepartureTime] IS NULL");


            // VEHICLEPARKING (join-tabell mellan ParkingSession och ParkingSpotV2)
            // --------------------------------------------------------------------
            // 1:1 relation mellan en rad i VehicleParking (join-tabellen) och en rad i ParkingSession  
            // som i sin tur har 1:M relation med VehicleParking (rader join-tabellen)
            modelBuilder.Entity<VehicleParking>()
                .HasOne(vp => vp.ParkingSession)
                .WithMany(ps => ps.VehicleParkings)
                .HasForeignKey(vp => vp.ParkingSessionId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade - om en ParkingSession tas bort försvinner även alla VehicleParkings-rader på den sessionen

            // 1:1 relation mellan en rad i VehicleParking och en rad i ParkingSpotV2 
            // som i sin tur har 1:M relation med VehicleParking
            modelBuilder.Entity<VehicleParking>()
                .HasOne(vp => vp.ParkingSpotV2)
                .WithMany(ps => ps.VehicleParkings)
                .HasForeignKey(vp => vp.ParkingSpotV2Id)
                .OnDelete(DeleteBehavior.Restrict); // Restrict för att inte radera historik om någon tar bort en spot

            // DB-skydd: UnitsUsed måste vara 1..3
            modelBuilder.Entity<VehicleParking>().ToTable(tb => tb.HasCheckConstraint("CK_VehicleParking_UnitsUsed", "[UnitsUsed] BETWEEN 1 AND 3"));

            // Ingen kompositnyckel skapas, man vill ha eget Id för att tabellen har eget data (UnitsUsed) och ska kunna ses som en egen tabell också
            // Däremot skydd mot att samma session får två rader mot samma spot i VehicleParking, genom att använda unikt index på de två FK
            modelBuilder.Entity<VehicleParking>().HasIndex(vp => new { vp.ParkingSessionId, vp.ParkingSpotV2Id }).IsUnique();

        }

    }
}
