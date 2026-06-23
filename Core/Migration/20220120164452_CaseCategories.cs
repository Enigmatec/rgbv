using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class CaseCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "c2ee66f4-60e9-4983-9f9c-fddb2b57c768");

            migrationBuilder.AddColumn<string>(
                name: "CaseCategories",
                table: "Cases",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseCategoriesOthers",
                table: "Cases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseCategories",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CaseCategoriesOthers",
                table: "Cases");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "c2ee66f4-60e9-4983-9f9c-fddb2b57c768", "6050e15d-3352-47ef-aaa7-cb9aa4d722d3", "View only access to organisation's data", "Implementing Partner", "IMPLEMENTING PARTNER" });
        }
    }
}
