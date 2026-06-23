using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class StateChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_LocalGovernmentAreas_LocalGovernmentAreaId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_States_StateId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_LocalGovernmentAreaId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_StateId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LocalGovernmentAreaId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "Organisations");

            migrationBuilder.AddColumn<bool>(
                name: "IsSoftDeleted",
                table: "Organisations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "States",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "Cases",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatedById",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSoftDeleted",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "216c3efe-9226-468c-ab5c-a2169a924794", "041e0cb3-df4b-4aae-a0b6-8fbb2244612c", "Supervisor with rights to data of an organisation", "Organisation Supervisor", "ORGANISATION SUPERVISOR" });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ValidatedById",
                table: "Cases",
                column: "ValidatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_AspNetUsers_ValidatedById",
                table: "Cases",
                column: "ValidatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_AspNetUsers_ValidatedById",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ValidatedById",
                table: "Cases");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "216c3efe-9226-468c-ab5c-a2169a924794");

            migrationBuilder.DropColumn(
                name: "IsSoftDeleted",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "States",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ValidatedById",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IsSoftDeleted",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "LocalGovernmentAreaId",
                table: "Organisations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StateId",
                table: "Organisations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LocalGovernmentAreaId",
                table: "Organisations",
                column: "LocalGovernmentAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_StateId",
                table: "Organisations",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_LocalGovernmentAreas_LocalGovernmentAreaId",
                table: "Organisations",
                column: "LocalGovernmentAreaId",
                principalTable: "LocalGovernmentAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_States_StateId",
                table: "Organisations",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
