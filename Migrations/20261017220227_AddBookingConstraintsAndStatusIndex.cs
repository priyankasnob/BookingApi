using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingConstraintsAndStatusIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_Id",
                table: "Bookings",
                columns: new[] { "Status", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_BookingFee",
                table: "Bookings",
                sql: "\"BookingFee\" BETWEEN 0 AND 10000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_DiscountPercent",
                table: "Bookings",
                sql: "\"DiscountPercent\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_HourlyRate",
                table: "Bookings",
                sql: "\"HourlyRate\" > 0 AND \"HourlyRate\" <= 10000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Hours",
                table: "Bookings",
                sql: "\"Hours\" BETWEEN 1 AND 8");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings",
                sql: "\"Status\" IN ('Pending', 'Confirmed', 'Cancelled')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status_Id",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_BookingFee",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_DiscountPercent",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_HourlyRate",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Hours",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bookings_Status",
                table: "Bookings");
        }
    }
}
