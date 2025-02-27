using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDRequestInBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Request",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Request",
                table: "Bookings");
        }
    }
}
