using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReview3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewId",
                table: "Medias",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_ReviewId",
                table: "Medias",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Reviews_ReviewId",
                table: "Medias",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Reviews_ReviewId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_ReviewId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "Medias");
        }
    }
}
