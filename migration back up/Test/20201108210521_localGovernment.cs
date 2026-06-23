using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class localGovernment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocalGovernmentAreaId",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LocalGovernmentAreaId",
                table: "AspNetUsers",
                column: "LocalGovernmentAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_LocalGovernmentAreas_LocalGovernmentAreaId",
                table: "AspNetUsers",
                column: "LocalGovernmentAreaId",
                principalTable: "LocalGovernmentAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_LocalGovernmentAreas_LocalGovernmentAreaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LocalGovernmentAreaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LocalGovernmentAreaId",
                table: "AspNetUsers");
        }
    }
}
