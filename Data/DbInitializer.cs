using Garage_2.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Garage_2.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<GarageContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1) Se till att DB + migrations är på plats (valfritt men praktiskt i dev)
            await context.Database.MigrateAsync();

            // 2) Seed roller + admin 
            await SeedRolesAndAdminAsync(roleManager, userManager);

            // 3) Seed VehicleTypes
            await SeedVehicleTypesAsync(context);

            // 4) Seed ParkingSpots (tomma)
            await SeedParkingSpotsAsync(context);

            // 5) Seed:a Fordon för adminanvändaren 
            await SeedDemoVehiclesForAdminAsync(context, userManager);

        }

        private static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            string[] roles = ["Admin", "User"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            const string adminEmail = "admin@garage.se";
            const string adminPassword = "Admin123!"; // byt i riktig miljö

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "Adminsson",
                    SSN = "19810101-1234",
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    // Krass: faila tidigt så du ser vad som är fel (password policy osv)
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        private static async Task SeedVehicleTypesAsync(GarageContext context)
        {
            if (await context.VehicleTypes.AnyAsync())
                return;

            var types = new List<VehicleType>
            {
                new() { Name = "Motorcycle", SizeInUnits = 1 },
                new() { Name = "Car",        SizeInUnits = 3 },
                new() { Name = "Bus",        SizeInUnits = 6 },
                new() { Name = "Boat",       SizeInUnits = 9 }
            };

            context.VehicleTypes.AddRange(types);
            await context.SaveChangesAsync();
        }

        private static async Task SeedParkingSpotsAsync(GarageContext context)
        {
            if (await context.ParkingSpotV2.AnyAsync())
                return;

            const int totalSpots = 50;

            var spots = Enumerable.Range(1, totalSpots)
                .Select(i => new ParkingSpotV2
                {
                    SpotNumber = i,
                    CapacityUnits = 3,
                    IsBlocked = false
                })
                .ToList();

            context.ParkingSpotV2.AddRange(spots);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDemoVehiclesForAdminAsync(
    GarageContext context,
    UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@garage.se";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
                return; // admin seed avstängt eller misslyckades -> hoppa över demo vehicles

            // Skapa bara demo vehicles om admin inte redan har några (så vi inte spammar vid varje start)
            bool adminHasVehicles = await context.Vehicles.AnyAsync(v => v.ApplicationUserId == admin.Id);
            if (adminHasVehicles)
                return;

            // Hämta VehicleTypeId för varje typ (Names ska matcha dina seedade VehicleTypes)
            var typeIds = await context.VehicleTypes
                .AsNoTracking()
                .ToDictionaryAsync(t => t.Name, t => t.Id);

            int CarId = typeIds["Car"];
            int MotorcycleId = typeIds["Motorcycle"];
            int BusId = typeIds["Bus"];
            int BoatId = typeIds["Boat"];

            // Registrerade fordon: ingen parkering, inga sessions.
            var vehicles = new List<Vehicle>
    {
        new()
        {
            RegistrationNumber = "ABC123",
            Make = "Volvo",
            Model = "XC60",
            NumberOfWheels = 4,
            Color = "Red",
            VehicleTypeId = CarId,
            ApplicationUserId = admin.Id
        },
        new()
        {
            RegistrationNumber = "DEF456",
            Make = "BMW",
            Model = "320i",
            NumberOfWheels = 4,
            Color = "Black",
            VehicleTypeId = CarId,
            ApplicationUserId = admin.Id
        },
        new()
        {
            RegistrationNumber = "JKL321",
            Make = "Yamaha",
            Model = "MT-07",
            NumberOfWheels = 2,
            Color = "Black",
            VehicleTypeId = MotorcycleId,
            ApplicationUserId = admin.Id
        },
        new()
        {
            RegistrationNumber = "VWX753",
            Make = "Scania",
            Model = "Citywide",
            NumberOfWheels = 6,
            Color = "Yellow",
            VehicleTypeId = BusId,
            ApplicationUserId = admin.Id
        },
        new()
        {
            RegistrationNumber = "BCD246",
            Make = "Nimbus",
            Model = "27 Nova",
            NumberOfWheels = 0,
            Color = "White",
            VehicleTypeId = BoatId,
            ApplicationUserId = admin.Id
        }
    };

            // DB har unik index på RegistrationNumber -> skydda mot krock om du ändrar logiken senare
            var regNumbers = vehicles.Select(v => v.RegistrationNumber).ToList();
            var existingRegs = await context.Vehicles
                .AsNoTracking()
                .Where(v => regNumbers.Contains(v.RegistrationNumber))
                .Select(v => v.RegistrationNumber)
                .ToListAsync();

            vehicles.RemoveAll(v => existingRegs.Contains(v.RegistrationNumber));

            if (vehicles.Count == 0)
                return;

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }

    }
}
