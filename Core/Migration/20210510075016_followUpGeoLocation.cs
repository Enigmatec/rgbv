using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class followUpGeoLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "FollowUps",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "FollowUps",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "FollowUps");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "FollowUps");
        }
    }
}