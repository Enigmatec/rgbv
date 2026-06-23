using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CaseTimeOfDayIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cases_TimeOfDay",
                table: "Cases",
                column: "TimeOfDay")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cases_TimeOfDay",
                table: "Cases");
        }
    }
}