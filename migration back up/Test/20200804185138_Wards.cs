using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class Wards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IncidentWardId",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    LocalGovernmentAreaId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wards_LocalGovernmentAreas_LocalGovernmentAreaId",
                        column: x => x.LocalGovernmentAreaId,
                        principalTable: "LocalGovernmentAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentWardId",
                table: "Cases",
                column: "IncidentWardId");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_LocalGovernmentAreaId",
                table: "Wards",
                column: "LocalGovernmentAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Wards_IncidentWardId",
                table: "Cases",
                column: "IncidentWardId",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Wards_IncidentWardId",
                table: "Cases");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropIndex(
                name: "IX_Cases_IncidentWardId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IncidentWardId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IsActivated",
                table: "AspNetUsers");
        }
    }
}
