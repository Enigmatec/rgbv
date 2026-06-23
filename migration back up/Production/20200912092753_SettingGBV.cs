using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class SettingGBV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGBVQuestionsEnabled",
                table: "Settings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGBVQuestionsEnabled",
                table: "Settings");
        }
    }
}
