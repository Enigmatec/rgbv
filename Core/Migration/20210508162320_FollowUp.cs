using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class FollowUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CaseClosedDate",
                table: "FollowUps",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhoClosedTheCase",
                table: "FollowUps",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseClosedDate",
                table: "FollowUps");

            migrationBuilder.DropColumn(
                name: "WhoClosedTheCase",
                table: "FollowUps");
        }
    }
}