using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class ServiceProvisionCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SerialNumber",
                table: "ServicesProvided",
                maxLength: 10,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServiceProvisionCode",
                table: "ServicesProvided",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "ServiceProvisionCode",
                table: "ServicesProvided");
        }
    }
}