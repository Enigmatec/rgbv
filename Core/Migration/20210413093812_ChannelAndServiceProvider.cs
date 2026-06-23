using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class ChannelAndServiceProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactChannelOrganisation",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactChannelOrganisationService",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherServiceProviderAddress",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherServiceProviderName",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactChannelOrganisation",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ContactChannelOrganisationService",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "OtherServiceProviderAddress",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "OtherServiceProviderName",
                table: "Cases");
        }
    }
}