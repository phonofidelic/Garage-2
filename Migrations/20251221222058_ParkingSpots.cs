using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garage_2.Migrations
{
    /// <inheritdoc />
    public partial class ParkingSpots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParkingSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpotNumber = table.Column<int>(type: "int", nullable: false),
                    CapacityUnits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleSpots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkedVehicleId = table.Column<int>(type: "int", nullable: false),
                    ParkingSpotId = table.Column<int>(type: "int", nullable: false),
                    UnitsUsed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleSpots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleSpots_ParkedVehicle_ParkedVehicleId",
                        column: x => x.ParkedVehicleId,
                        principalTable: "ParkedVehicle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleSpots_ParkingSpots_ParkingSpotId",
                        column: x => x.ParkingSpotId,
                        principalTable: "ParkingSpots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpots_SpotNumber",
                table: "ParkingSpots",
                column: "SpotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleSpots_ParkedVehicleId_ParkingSpotId",
                table: "VehicleSpots",
                columns: new[] { "ParkedVehicleId", "ParkingSpotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleSpots_ParkingSpotId",
                table: "VehicleSpots",
                column: "ParkingSpotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleSpots");

            migrationBuilder.DropTable(
                name: "ParkingSpots");
        }
    }
}
