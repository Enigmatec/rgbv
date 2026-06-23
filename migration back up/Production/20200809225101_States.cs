using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class States : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "OrganisationType",
                table: "Organisations",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.CreateTable(
                name: "FollowUps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    CaseId = table.Column<int>(nullable: false),
                    HasClientReceivedjustice = table.Column<string>(nullable: false),
                    JusticeReceivedDate = table.Column<DateTime>(nullable: true),
                    FinalOutcome = table.Column<string>(nullable: true),
                    HasCaseBeenClosed = table.Column<string>(nullable: false),
                    CreatedById = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowUps_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FollowUps_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ValidatedById",
                table: "Cases",
                column: "ValidatedById");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_CaseId",
                table: "FollowUps",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_CreatedById",
                table: "FollowUps",
                column: "CreatedById");

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

            migrationBuilder.DropTable(
                name: "FollowUps");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ValidatedById",
                table: "Cases");

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

            migrationBuilder.AlterColumn<int>(
                name: "OrganisationType",
                table: "Organisations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

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
