using Garage_2.Models;

namespace Garage_2.Data
{
    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder appBuilder)
        {
            // ApplicationBuilder-objektet är ett objekt som representerar appen och ger access till DbContext(via Services),
            // då DI ej kan användas här (en statisk Helper-klass som saknar konstruktor kan ej skapas med DI av Service-containern.)
            // och DbContext SKA skapas via Services och inget annat så kan den inte skickas in här.

            GarageContext context = appBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<GarageContext>();

            // Fyll bara på med seed-data om db är tom (dvs ingen överskrivning av befintligt data)

            if (!context.ParkedVehicle.Any())
                context.ParkedVehicle.AddRange(parkedVehiclesList);

            if (!context.ParkingSpots.Any())
                context.ParkingSpots.AddRange(ParkingSpotsList());

            context.SaveChanges();

            // Endast om data finns i både ParkedVehicle och ParkingSpots kan join-tabellen fyllas med relationer
            if (context.ParkingSpots.Any() && context.ParkedVehicle.Any())
            {
                SeedVehicleSpots(context);
            }
        }


        // Seed-data – Lista (property) av vehicles av alla typer i applikationen (Car, Motorcycle, Bus, Boat)
        // Olika parkeringsdatum (ArrivalTime) för att kunna få ut lite olika priser vid uthämtning av fordonet
        public static List<ParkedVehicle> parkedVehiclesList
        {
            get
            {
                DateTime now = DateTime.Now;
                var vehicles = new List<ParkedVehicle>();

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Red",
                    Make = "Volvo",
                    Model = "XC60",
                    NumberOfWheels = 4,
                    RegistrationNumber = "ABC123",
                    ArrivalTime = now.AddMinutes(-30)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Black",
                    Make = "BMW",
                    Model = "320i",
                    NumberOfWheels = 4,
                    RegistrationNumber = "DEF456",
                    ArrivalTime = now.AddHours(-1)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "White",
                    Make = "Tesla",
                    Model = "Model 3",
                    NumberOfWheels = 4,
                    RegistrationNumber = "GHI789",
                    ArrivalTime = now.AddHours(-5)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Motorcycle,
                    Color = "Black",
                    Make = "Yamaha",
                    Model = "MT-07",
                    NumberOfWheels = 2,
                    RegistrationNumber = "JKL321",
                    ArrivalTime = now.AddHours(-12)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Motorcycle,
                    Color = "Blue",
                    Make = "Honda",
                    Model = "CBR600RR",
                    NumberOfWheels = 2,
                    RegistrationNumber = "MNO654",
                    ArrivalTime = now.AddHours(-24)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Silver",
                    Make = "Audi",
                    Model = "A6",
                    NumberOfWheels = 4,
                    RegistrationNumber = "PQR987",
                    ArrivalTime = now.AddDays(-2)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Green",
                    Make = "Volkswagen",
                    Model = "Golf",
                    NumberOfWheels = 4,
                    RegistrationNumber = "STU159",
                    ArrivalTime = now.AddDays(-14)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Bus,
                    Color = "Yellow",
                    Make = "Scania",
                    Model = "Citywide",
                    NumberOfWheels = 6,
                    RegistrationNumber = "VWX753",
                    ArrivalTime = now.AddMonths(-1)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Bus,
                    Color = "Blue",
                    Make = "Volvo",
                    Model = "7900 Electric",
                    NumberOfWheels = 6,
                    RegistrationNumber = "YZA852",
                    ArrivalTime = now
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Boat,
                    Color = "White",
                    Make = "Nimbus",
                    Model = "27 Nova",
                    NumberOfWheels = 0,
                    RegistrationNumber = "BCD246",
                    ArrivalTime = now.AddYears(-1)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Boat,
                    Color = "Blue",
                    Make = "Yamarin",
                    Model = "63 DC",
                    NumberOfWheels = 0,
                    RegistrationNumber = "EFG369",
                    ArrivalTime = now.AddMinutes(-25)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Gray",
                    Make = "Toyota",
                    Model = "Corolla",
                    NumberOfWheels = 4,
                    RegistrationNumber = "HIJ741",
                    ArrivalTime = now.AddMinutes(-45)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Blue",
                    Make = "Ford",
                    Model = "Focus",
                    NumberOfWheels = 4,
                    RegistrationNumber = "KLM963",
                    ArrivalTime = now.AddHours(-3.5)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Motorcycle,
                    Color = "Red",
                    Make = "Ducati",
                    Model = "Monster",
                    NumberOfWheels = 2,
                    RegistrationNumber = "NOP147",
                    ArrivalTime = now.AddHours(-4.5)
                });

                vehicles.Add(new ParkedVehicle
                {
                    Type = VehicleType.Car,
                    Color = "Black",
                    Make = "Mercedes-Benz",
                    Model = "C220",
                    NumberOfWheels = 4,
                    RegistrationNumber = "QRS258",
                    ArrivalTime = now.AddHours(-36)
                });

                return vehicles;
            }
        }


        // Seed-data – Lista av p-platser (ParkingSpot) 1-30 (hårdkodat) med initial kapacitet 3, dvs tomma parkeringsplatser
        private static List<ParkingSpot> ParkingSpotsList()
        {
            var spots = new List<ParkingSpot>();

            // Obs! Hårdkodat antal p-platser i garaget här.
            // TODO: Borde fixas, hämtas från config istället (?)
            const int totalSpots = 30;

            for (int i = 1; i <= totalSpots; i++)
            {
                spots.Add(new ParkingSpot { SpotNumber = i, CapacityUnits = 3 });
            }

            return spots;

        }

        private static void SeedVehicleSpots(GarageContext context)
        {
            if (!context.VehicleSpots.Any())
            {
                var spots = context.ParkingSpots
                    .OrderBy(s => s.SpotNumber)
                    .ToList();

                var vehicles = context.ParkedVehicle
                    .OrderBy(v => v.Id)
                    .ToList();

                int spotIndex = 0;
                int unitsRequired;

                foreach (var vehicle in vehicles)
                {
                    unitsRequired = 0;

                    switch (vehicle.Type)
                    {
                        case VehicleType.Motorcycle:
                            unitsRequired = 1;
                            break;
                        case VehicleType.Car:
                            unitsRequired = 3;
                            break;
                        case VehicleType.Bus:
                            unitsRequired = 6;
                            break;
                        case VehicleType.Boat:
                            unitsRequired = 9;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    int unitsLeft = unitsRequired;

                    while (unitsLeft > 0)
                    {
                        var spot = spots[spotIndex];

                        context.VehicleSpots.Add(new VehicleSpot
                        {
                            ParkedVehicleId = vehicle.Id,
                            ParkingSpotId = spot.Id,
                            UnitsUsed = Math.Min(3, unitsLeft)
                        });

                        unitsLeft -= 3;
                        spotIndex++;
                    }
                }

                context.SaveChanges();
            }
        }
    }


}



