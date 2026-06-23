using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class CaseFormUpdates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CaseClosedDate",
                table: "Cases",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ContactChannelOrganisationIncidentCode",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherServiceProviderIncidentCode",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurviorDoesNotWantJusticeReasons",
                table: "Cases",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseClosedDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ContactChannelOrganisationIncidentCode",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "OtherServiceProviderIncidentCode",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "SurviorDoesNotWantJusticeReasons",
                table: "Cases");
        }
    }
}