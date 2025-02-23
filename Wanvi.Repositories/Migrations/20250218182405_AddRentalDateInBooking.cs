using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddRentalDateInBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RentalDate",
                table: "Bookings",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RentalDate",
                table: "Bookings");
        }
    }
}
