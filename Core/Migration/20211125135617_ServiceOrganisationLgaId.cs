using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class ServiceOrganisationLgaId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganisationLgaId",
                table: "ServicesProvided",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganisationLgaId",
                table: "ServicesProvided");
        }
    }
}