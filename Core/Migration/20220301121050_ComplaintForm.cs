using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class ComplaintForm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplaintFormId",
                table: "FilesUploads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ComplaintForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintFormId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    ComplaintCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ComplaintType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", maxLength: 27, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", maxLength: 27, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", maxLength: 27, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintForms_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplaintForms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilesUploads_ComplaintFormId",
                table: "FilesUploads",
                column: "ComplaintFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintForms_ResolvedByUserId",
                table: "ComplaintForms",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintForms_UserId",
                table: "ComplaintForms",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FilesUploads_ComplaintForms_ComplaintFormId",
                table: "FilesUploads",
                column: "ComplaintFormId",
                principalTable: "ComplaintForms",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FilesUploads_ComplaintForms_ComplaintFormId",
                table: "FilesUploads");

            migrationBuilder.DropTable(
                name: "ComplaintForms");

            migrationBuilder.DropIndex(
                name: "IX_FilesUploads_ComplaintFormId",
                table: "FilesUploads");

            migrationBuilder.DropColumn(
                name: "ComplaintFormId",
                table: "FilesUploads");
        }
    }
}
