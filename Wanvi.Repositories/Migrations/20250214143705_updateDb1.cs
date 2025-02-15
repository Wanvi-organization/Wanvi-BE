using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class updateDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Tours_TourId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TourId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TourId",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TourId",
                table: "Bookings",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TourId",
                table: "Bookings",
                column: "TourId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Tours_TourId",
                table: "Bookings",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id");
        }
    }
}
