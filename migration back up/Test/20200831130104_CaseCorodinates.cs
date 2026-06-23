using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CaseCorodinates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhoClosedTheCase",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "WhoClosedTheCase",
                table: "Cases");

          
        }
    }
}
