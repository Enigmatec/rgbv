using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class outcomeOfProsecution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutcomeOfProsecution",
                table: "Cases",
                maxLength: 36,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutcomeOfProsecution",
                table: "Cases");
        }
    }
}