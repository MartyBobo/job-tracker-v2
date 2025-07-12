using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeTemplateConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_ResumeTemplates_TemplateId",
                table: "Resumes");

            migrationBuilder.CreateIndex(
                name: "IX_ResumeTemplates_UserId_Name",
                table: "ResumeTemplates",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_ResumeTemplates_TemplateId",
                table: "Resumes",
                column: "TemplateId",
                principalTable: "ResumeTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_ResumeTemplates_TemplateId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_ResumeTemplates_UserId_Name",
                table: "ResumeTemplates");

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_ResumeTemplates_TemplateId",
                table: "Resumes",
                column: "TemplateId",
                principalTable: "ResumeTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
