using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUploadEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Uploads_UploadType",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "UploadType",
                table: "Uploads");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "Uploads",
                newName: "ContentType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Uploads",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DocumentType",
                table: "Uploads",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Uploads");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Uploads");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Uploads",
                newName: "MimeType");

            migrationBuilder.AddColumn<string>(
                name: "UploadType",
                table: "Uploads",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_UploadType",
                table: "Uploads",
                column: "UploadType");
        }
    }
}
