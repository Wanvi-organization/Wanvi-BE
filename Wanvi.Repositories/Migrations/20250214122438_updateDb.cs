using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wanvi.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class updateDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostHashtag_Hashtags_HashtagId",
                table: "PostHashtag");

            migrationBuilder.DropForeignKey(
                name: "FK_PostHashtag_Posts_PostId",
                table: "PostHashtag");

            migrationBuilder.DropTable(
                name: "TourCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostHashtag",
                table: "PostHashtag");

            migrationBuilder.RenameTable(
                name: "PostHashtag",
                newName: "PostHashtags");

            migrationBuilder.RenameIndex(
                name: "IX_PostHashtag_HashtagId",
                table: "PostHashtags",
                newName: "IX_PostHashtags_HashtagId");

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "Medias",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Medias",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostHashtags",
                table: "PostHashtags",
                columns: new[] { "PostId", "HashtagId" });

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastUpdatedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DeletedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourActivities",
                columns: table => new
                {
                    TourId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActivityId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourActivities", x => new { x.TourId, x.ActivityId });
                    table.ForeignKey(
                        name: "FK_TourActivities_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourActivities_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourActivities_ActivityId",
                table: "TourActivities",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostHashtags_Hashtags_HashtagId",
                table: "PostHashtags",
                column: "HashtagId",
                principalTable: "Hashtags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostHashtags_Posts_PostId",
                table: "PostHashtags",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostHashtags_Hashtags_HashtagId",
                table: "PostHashtags");

            migrationBuilder.DropForeignKey(
                name: "FK_PostHashtags_Posts_PostId",
                table: "PostHashtags");

            migrationBuilder.DropTable(
                name: "TourActivities");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostHashtags",
                table: "PostHashtags");

            migrationBuilder.RenameTable(
                name: "PostHashtags",
                newName: "PostHashtag");

            migrationBuilder.RenameIndex(
                name: "IX_PostHashtags_HashtagId",
                table: "PostHashtag",
                newName: "IX_PostHashtag_HashtagId");

            migrationBuilder.UpdateData(
                table: "Medias",
                keyColumn: "TourId",
                keyValue: null,
                column: "TourId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "TourId",
                table: "Medias",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Medias",
                keyColumn: "PostId",
                keyValue: null,
                column: "PostId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PostId",
                table: "Medias",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostHashtag",
                table: "PostHashtag",
                columns: new[] { "PostId", "HashtagId" });

            migrationBuilder.CreateTable(
                name: "TourCategories",
                columns: table => new
                {
                    TourId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourCategories", x => new { x.TourId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_TourCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourCategories_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TourCategories_CategoryId",
                table: "TourCategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostHashtag_Hashtags_HashtagId",
                table: "PostHashtag",
                column: "HashtagId",
                principalTable: "Hashtags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostHashtag_Posts_PostId",
                table: "PostHashtag",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
