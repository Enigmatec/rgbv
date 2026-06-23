using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class GbvQuestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GbvCovid19Question1",
                table: "ServicesProvided",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GbvCovid19Question2",
                table: "ServicesProvided",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GbvCovid19Question3",
                table: "ServicesProvided",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GbvCovid19Question4",
                table: "ServicesProvided",
                maxLength: 250,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GbvCovid19Question1",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "GbvCovid19Question2",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "GbvCovid19Question3",
                table: "ServicesProvided");

            migrationBuilder.DropColumn(
                name: "GbvCovid19Question4",
                table: "ServicesProvided");
        }
    }
}