using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResumeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "Resumes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Resumes",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileFormat",
                table: "Resumes",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "Resumes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Resumes",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResumeData",
                table: "Resumes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Resumes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Resumes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_ApplicationId",
                table: "Resumes",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_UserId",
                table: "Resumes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_UserId_Name_Version",
                table: "Resumes",
                columns: new[] { "UserId", "Name", "Version" });

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_JobApplications_ApplicationId",
                table: "Resumes",
                column: "ApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Resumes_Users_UserId",
                table: "Resumes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_JobApplications_ApplicationId",
                table: "Resumes");

            migrationBuilder.DropForeignKey(
                name: "FK_Resumes_Users_UserId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_ApplicationId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_UserId",
                table: "Resumes");

            migrationBuilder.DropIndex(
                name: "IX_Resumes_UserId_Name_Version",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "FileFormat",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "ResumeData",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Resumes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Resumes");
        }
    }
}
