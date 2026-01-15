using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Garage_2.Migrations
{
    /// <inheritdoc />
    public partial class GarageEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleSpots_ParkedVehicle_ParkedVehicleId",
                table: "VehicleSpots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkedVehicle",
                table: "ParkedVehicle");

            migrationBuilder.RenameTable(
                name: "ParkedVehicle",
                newName: "ParkedVehicles");

            migrationBuilder.AlterColumn<string>(
                name: "SSN",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkedVehicles",
                table: "ParkedVehicles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ParkingSpotsV2",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpotNumber = table.Column<int>(type: "int", nullable: false),
                    CapacityUnits = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSpotsV2", x => x.Id);
                    table.CheckConstraint("CK_ParkingSpotV2_CapacityUnits", "[CapacityUnits] = 3");
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SizeInUnits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Make = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfWheels = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkingSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleId = table.Column<int>(type: "int", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkingSessions_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleParkings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkingSessionId = table.Column<int>(type: "int", nullable: false),
                    ParkingSpotV2Id = table.Column<int>(type: "int", nullable: false),
                    UnitsUsed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleParkings", x => x.Id);
                    table.CheckConstraint("CK_VehicleParking_UnitsUsed", "[UnitsUsed] BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_VehicleParkings_ParkingSessions_ParkingSessionId",
                        column: x => x.ParkingSessionId,
                        principalTable: "ParkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleParkings_ParkingSpotsV2_ParkingSpotV2Id",
                        column: x => x.ParkingSpotV2Id,
                        principalTable: "ParkingSpotsV2",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SSN",
                table: "AspNetUsers",
                column: "SSN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSessions_VehicleId",
                table: "ParkingSessions",
                column: "VehicleId",
                unique: true,
                filter: "[DepartureTime] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ParkingSpotsV2_SpotNumber",
                table: "ParkingSpotsV2",
                column: "SpotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParkings_ParkingSessionId_ParkingSpotV2Id",
                table: "VehicleParkings",
                columns: new[] { "ParkingSessionId", "ParkingSpotV2Id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleParkings_ParkingSpotV2Id",
                table: "VehicleParkings",
                column: "ParkingSpotV2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ApplicationUserId",
                table: "Vehicles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_RegistrationNumber",
                table: "Vehicles",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleTypeId",
                table: "Vehicles",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_Name",
                table: "VehicleTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleSpots_ParkedVehicles_ParkedVehicleId",
                table: "VehicleSpots",
                column: "ParkedVehicleId",
                principalTable: "ParkedVehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleSpots_ParkedVehicles_ParkedVehicleId",
                table: "VehicleSpots");

            migrationBuilder.DropTable(
                name: "VehicleParkings");

            migrationBuilder.DropTable(
                name: "ParkingSessions");

            migrationBuilder.DropTable(
                name: "ParkingSpotsV2");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SSN",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ParkedVehicles",
                table: "ParkedVehicles");

            migrationBuilder.RenameTable(
                name: "ParkedVehicles",
                newName: "ParkedVehicle");

            migrationBuilder.AlterColumn<string>(
                name: "SSN",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ParkedVehicle",
                table: "ParkedVehicle",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleSpots_ParkedVehicle_ParkedVehicleId",
                table: "VehicleSpots",
                column: "ParkedVehicleId",
                principalTable: "ParkedVehicle",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
