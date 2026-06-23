using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class CaseLgaVAlidation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LgaValidated",
                table: "Cases",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LgaValidatedAt",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LgaValidatedById",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LgaValidatedId",
                table: "Cases",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LgaValidatedById",
                table: "Cases",
                column: "LgaValidatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_AspNetUsers_LgaValidatedById",
                table: "Cases",
                column: "LgaValidatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_AspNetUsers_LgaValidatedById",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LgaValidatedById",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LgaValidated",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LgaValidatedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LgaValidatedById",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LgaValidatedId",
                table: "Cases");
        }
    }
}