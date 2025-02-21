using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    public partial class UpdateDataBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột BookingId vào bảng Payments
            migrationBuilder.AddColumn<string>(
                name: "BookingId",
                table: "Payments",
                nullable: false,
                defaultValue: 0);

            // Tạo Index cho BookingId
            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            // Thêm khóa ngoại từ Payments → Bookings
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa khóa ngoại
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId",
                table: "Payments");

            // Xóa Index
            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId",
                table: "Payments");

            // Xóa cột BookingId
            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Payments");
        }
    }
}
