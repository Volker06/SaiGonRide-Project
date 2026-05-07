using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaigonRide.Migrations
{
    /// <inheritdoc />
    public partial class InitFresh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Rentals_RentalID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Vehicles_VehicleID",
                table: "Rentals");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Rentals_RentalID",
                table: "Payments",
                column: "RentalID",
                principalTable: "Rentals",
                principalColumn: "RentalID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Vehicles_VehicleID",
                table: "Rentals",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Rentals_RentalID",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Vehicles_VehicleID",
                table: "Rentals");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Rentals_RentalID",
                table: "Payments",
                column: "RentalID",
                principalTable: "Rentals",
                principalColumn: "RentalID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Vehicles_VehicleID",
                table: "Rentals",
                column: "VehicleID",
                principalTable: "Vehicles",
                principalColumn: "VehicleID");
        }
    }
}
