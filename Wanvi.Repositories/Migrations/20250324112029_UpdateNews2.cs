using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNews2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Medias_MediaId",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_MediaId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "MediaId",
                table: "News",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_News_MediaId",
                table: "News",
                column: "MediaId",
                unique: true,
                filter: "[MediaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_News_Medias_MediaId",
                table: "News",
                column: "MediaId",
                principalTable: "Medias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Medias_MediaId",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_MediaId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "MediaId",
                table: "News",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_MediaId",
                table: "News",
                column: "MediaId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_News_Medias_MediaId",
                table: "News",
                column: "MediaId",
                principalTable: "Medias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
