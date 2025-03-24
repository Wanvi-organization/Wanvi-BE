using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "NewsDetails");

            migrationBuilder.AddColumn<string>(
                name: "MediaId",
                table: "NewsDetails",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaId",
                table: "News",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsDetails_MediaId",
                table: "NewsDetails",
                column: "MediaId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_NewsDetails_Medias_MediaId",
                table: "NewsDetails",
                column: "MediaId",
                principalTable: "Medias",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Medias_MediaId",
                table: "News");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsDetails_Medias_MediaId",
                table: "NewsDetails");

            migrationBuilder.DropIndex(
                name: "IX_NewsDetails_MediaId",
                table: "NewsDetails");

            migrationBuilder.DropIndex(
                name: "IX_News_MediaId",
                table: "News");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "NewsDetails");

            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "News");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "NewsDetails",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
