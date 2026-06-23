using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class OrganisationLgaId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganisationLgaId",
                table: "Cases",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganisationLgaId",
                table: "Cases");
        }
    }
}