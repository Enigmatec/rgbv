using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class added_donor_users : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonorId",
                table: "Organisations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DonorId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Acronym = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", maxLength: 27, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", maxLength: 27, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_DonorId",
                table: "Organisations",
                column: "DonorId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DonorId",
                table: "AspNetUsers",
                column: "DonorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Donors_DonorId",
                table: "AspNetUsers",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_Donors_DonorId",
                table: "Organisations",
                column: "DonorId",
                principalTable: "Donors",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Donors_DonorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_Donors_DonorId",
                table: "Organisations");

            migrationBuilder.DropTable(
                name: "Donors");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_DonorId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DonorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DonorId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "DonorId",
                table: "AspNetUsers");
        }
    }
}
