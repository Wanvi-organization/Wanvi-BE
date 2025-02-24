using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsDetails_Medias_MediaId",
                table: "NewsDetails");

            migrationBuilder.DropIndex(
                name: "IX_NewsDetails_MediaId",
                table: "NewsDetails");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "NewsDetails");

            migrationBuilder.AddColumn<double>(
                name: "MinDeposit",
                table: "Schedules",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "NewsDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "NewsDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinDeposit",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "NewsDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "NewsDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "MediaId",
                table: "NewsDetails",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsDetails_MediaId",
                table: "NewsDetails",
                column: "MediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsDetails_Medias_MediaId",
                table: "NewsDetails",
                column: "MediaId",
                principalTable: "Medias",
                principalColumn: "Id");
        }
    }
}
