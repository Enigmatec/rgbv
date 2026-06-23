using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CaseEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "States",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    IncidentCode = table.Column<string>(nullable: true),
                    SerialNumber = table.Column<int>(nullable: false),
                    ContactChannel = table.Column<string>(nullable: true),
                    WhoReportedIncident = table.Column<string>(nullable: true),
                    AgeOfSurvior = table.Column<int>(nullable: false),
                    SexOfSurvior = table.Column<string>(nullable: true),
                    MaritalStatus = table.Column<string>(nullable: true),
                    EmploymentStatus = table.Column<string>(nullable: true),
                    EmploymentStatusOfParentOrGuardian = table.Column<string>(nullable: true),
                    VulnerablePopulation = table.Column<string>(nullable: true),
                    Education = table.Column<string>(nullable: true),
                    DoesSurviorLiveAlone = table.Column<string>(nullable: false),
                    WhoDoesSurviorLiveWith = table.Column<string>(nullable: true),
                    ActualLocationOfIncident = table.Column<string>(nullable: true),
                    DateOfIncident = table.Column<DateTime>(nullable: false),
                    DateReported = table.Column<DateTime>(nullable: false),
                    TimeOfDay = table.Column<string>(nullable: false),
                    HasSurviorReceivedService = table.Column<string>(nullable: false),
                    WasViolenceFatal = table.Column<string>(nullable: false),
                    FollowUpActionByCSO = table.Column<string>(nullable: true),
                    TypeOfServiceReceivedBySurvior = table.Column<string>(nullable: true),
                    TypeOfReferralServiceRequired = table.Column<string>(nullable: true),
                    TypeOfServiceProvidedToSurvior = table.Column<string>(nullable: true),
                    ActualReferralServiceReceived = table.Column<string>(nullable: true),
                    NameOfServiceProviderReferredTo = table.Column<string>(nullable: true),
                    OutcomeOfServiceorReferral = table.Column<string>(nullable: true),
                    SexOfPerpetrator = table.Column<string>(nullable: true),
                    AgeOfPerpetrator = table.Column<int>(nullable: false),
                    SurviorRelationWithPerpetrator = table.Column<string>(nullable: true),
                    IsSurviorContinuousThreat = table.Column<string>(nullable: false),
                    NumberOfPerpetrators = table.Column<string>(nullable: true),
                    DoestheSurviorWantJustice = table.Column<string>(nullable: false),
                    GBV_COVID19_Question1 = table.Column<string>(nullable: true),
                    GBV_COVID19_Question2 = table.Column<string>(nullable: true),
                    GBV_COVID19_Question3 = table.Column<string>(nullable: true),
                    GBV_COVID19_Question4 = table.Column<string>(nullable: true),
                    HasCaseBeenClosed = table.Column<string>(nullable: false),
                    CanBeEdited = table.Column<bool>(nullable: false),
                    CaseCategoryId = table.Column<int>(nullable: false),
                    IncidentStateId = table.Column<int>(nullable: false),
                    IncidentLGAId = table.Column<int>(nullable: false),
                    CreatedById = table.Column<string>(nullable: true),
                    OrganisationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_CaseCategories_CaseCategoryId",
                        column: x => x.CaseCategoryId,
                        principalTable: "CaseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cases_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cases_LocalGovernmentAreas_IncidentLGAId",
                        column: x => x.IncidentLGAId,
                        principalTable: "LocalGovernmentAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Cases_States_IncidentStateId",
                        column: x => x.IncidentStateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Cases_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    Field = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseCategoryId",
                table: "Cases",
                column: "CaseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CreatedById",
                table: "Cases",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases",
                column: "IncidentCode",
                unique: true,
                filter: "[IncidentCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentLGAId",
                table: "Cases",
                column: "IncidentLGAId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentStateId",
                table: "Cases",
                column: "IncidentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_OrganisationId",
                table: "Cases",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases",
                column: "SerialNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "States");
        }
    }
}
