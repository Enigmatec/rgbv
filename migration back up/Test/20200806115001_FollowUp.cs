using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class FollowUp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "FollowUps",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: true),
                    CaseId = table.Column<int>(nullable: false),
                    HasClientReceivedjustice = table.Column<int>(nullable: false),
                    JusticeReceivedDate = table.Column<DateTime>(nullable: true),
                    FinalOutcome = table.Column<string>(nullable: true),
                    HasCaseBeenClosed = table.Column<int>(nullable: false),
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
                        onDelete: ReferentialAction.NoAction);
                });



            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_CaseId",
                table: "FollowUps",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_CreatedById",
                table: "FollowUps",
                column: "CreatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUps");

        }
    }
}
