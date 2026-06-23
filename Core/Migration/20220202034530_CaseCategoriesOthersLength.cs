using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class CaseCategoriesOthersLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurvivorName",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "CaseCategoriesOthers",
                table: "Cases",
                type: "nvarchar(350)",
                maxLength: 350,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CaseCategoriesOthers",
                table: "Cases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(350)",
                oldMaxLength: 350,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurvivorName",
                table: "Cases",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: true);
        }
    }
}
