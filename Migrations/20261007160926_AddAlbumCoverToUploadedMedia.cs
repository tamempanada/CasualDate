using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbumCoverToUploadedMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlbumCoverMediaId",
                table: "UploadedMedia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedMedia_AlbumCoverMediaId",
                table: "UploadedMedia",
                column: "AlbumCoverMediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedMedia_UploadedMedia_AlbumCoverMediaId",
                table: "UploadedMedia",
                column: "AlbumCoverMediaId",
                principalTable: "UploadedMedia",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedMedia_UploadedMedia_AlbumCoverMediaId",
                table: "UploadedMedia");

            migrationBuilder.DropIndex(
                name: "IX_UploadedMedia_AlbumCoverMediaId",
                table: "UploadedMedia");

            migrationBuilder.DropColumn(
                name: "AlbumCoverMediaId",
                table: "UploadedMedia");
        }
    }
}
