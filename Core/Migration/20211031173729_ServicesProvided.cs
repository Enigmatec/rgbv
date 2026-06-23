using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Core.Migrations
{
    public partial class ServicesProvided : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UploadCategory",
                table: "FilesUploads",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfReferralServiceRequired",
                table: "Cases",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SexOfPerpetrator",
                table: "Cases",
                maxLength: 43,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ServicesProvided",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(maxLength: 27, nullable: false),
                    ModifiedAt = table.Column<DateTime>(maxLength: 27, nullable: true),
                    AgeOfSurvivorInYears = table.Column<int>(nullable: false),
                    SexOfSurvivorOrVictim = table.Column<string>(maxLength: 6, nullable: true),
                    TypeOfClient = table.Column<string>(maxLength: 50, nullable: true),
                    HasSurvivorReceivedServiceFromAnotherOrganisation = table.Column<string>(maxLength: 15, nullable: false),
                    IncidentCodeFromReferringOrganisation = table.Column<string>(maxLength: 30, nullable: true),
                    ReferralCode = table.Column<string>(maxLength: 30, nullable: true),
                    TypeOfServiceReceivedAnotherOrganisation = table.Column<string>(maxLength: 1000, nullable: true),
                    TypeOfServiceNeeded = table.Column<string>(maxLength: 1000, nullable: true),
                    TypeOfServiceProvided = table.Column<string>(maxLength: 1000, nullable: true),
                    TypeOfServiceReferredFor = table.Column<string>(maxLength: 1000, nullable: true),
                    NameOfServiceProviderOrCsoReferredTo = table.Column<string>(maxLength: 200, nullable: true),
                    ReferralOutcome = table.Column<string>(maxLength: 50, nullable: true),
                    OrganisationId = table.Column<int>(nullable: false),
                    StateId = table.Column<int>(nullable: false),
                    Longitude = table.Column<string>(maxLength: 20, nullable: true),
                    Latitude = table.Column<string>(maxLength: 20, nullable: true),
                    DateOfServiceProvision = table.Column<DateTime>(maxLength: 27, nullable: true),
                    Status = table.Column<string>(maxLength: 9, nullable: false),
                    CreatedById = table.Column<string>(nullable: true),
                    SpOrCsoApprovalById = table.Column<string>(nullable: true),
                    SpOrCsoApprovalDate = table.Column<DateTime>(maxLength: 27, nullable: true),
                    StateApprovalById = table.Column<string>(nullable: true),
                    StateApprovalDate = table.Column<DateTime>(maxLength: 27, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicesProvided", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_AspNetUsers_SpOrCsoApprovalById",
                        column: x => x.SpOrCsoApprovalById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_AspNetUsers_StateApprovalById",
                        column: x => x.StateApprovalById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServicesProvided_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_CreatedById",
                table: "ServicesProvided",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_OrganisationId",
                table: "ServicesProvided",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_SpOrCsoApprovalById",
                table: "ServicesProvided",
                column: "SpOrCsoApprovalById");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_StateApprovalById",
                table: "ServicesProvided",
                column: "StateApprovalById");

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_StateId",
                table: "ServicesProvided",
                column: "StateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicesProvided");

            migrationBuilder.AlterColumn<int>(
                name: "UploadCategory",
                table: "FilesUploads",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "TypeOfReferralServiceRequired",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SexOfPerpetrator",
                table: "Cases",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 43,
                oldNullable: true);
        }
    }
}