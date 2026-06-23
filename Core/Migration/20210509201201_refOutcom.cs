using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class refOutcom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralOutcome",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferralOutcome",
                table: "Cases");
        }
    }
}