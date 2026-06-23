using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class SettingsUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowPrevMonthCases",
                table: "Settings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowPrevMonthCases",
                table: "Settings");
        }
    }
}