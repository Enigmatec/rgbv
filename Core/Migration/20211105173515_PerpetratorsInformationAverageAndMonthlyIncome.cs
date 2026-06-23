using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class PerpetratorsInformationAverageAndMonthlyIncome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "AgeOfPerpetrator",
            //    table: "Cases");

            //migrationBuilder.DropColumn(
            //    name: "SexOfPerpetrator",
            //    table: "Cases");

            //migrationBuilder.DropColumn(
            //    name: "SurviorRelationWithPerpetrator",
            //    table: "Cases");

            migrationBuilder.AddColumn<string>(
                name: "PerpetratorsInformation",
                table: "Cases",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurvivorEstimatedAverageMonthlyIncome",
                table: "Cases",
                maxLength: 30,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerpetratorsInformation",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "SurvivorEstimatedAverageMonthlyIncome",
                table: "Cases");

            migrationBuilder.AddColumn<int>(
                name: "AgeOfPerpetrator",
                table: "Cases",
                type: "int",
                maxLength: 4,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SexOfPerpetrator",
                table: "Cases",
                type: "nvarchar(43)",
                maxLength: 43,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurviorRelationWithPerpetrator",
                table: "Cases",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);
        }
    }
}