using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerAddress",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "OrderCode",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "Payments",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerAddress",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "Payments");
        }
    }
}
