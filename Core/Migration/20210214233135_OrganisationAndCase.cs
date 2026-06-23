using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class OrganisationAndCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Acronym",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotLine",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialMediaData",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfService",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivingOrganisationCode",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Acronym",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "HotLine",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "SocialMediaData",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "TypeOfService",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ReceivingOrganisationCode",
                table: "Cases");
        }
    }
}