using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBooking.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableSeatsToFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvailableSeats",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSeats",
                table: "Flights");
        }
    }
}
