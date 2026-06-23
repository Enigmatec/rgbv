using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CaseApproved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Cases",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Cases",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ApprovedById",
                table: "Cases",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_AspNetUsers_ApprovedById",
                table: "Cases",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_AspNetUsers_ApprovedById",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ApprovedById",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Cases");
        }
    }
}
