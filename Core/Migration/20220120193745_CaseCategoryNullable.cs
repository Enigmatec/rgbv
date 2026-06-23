using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class CaseCategoryNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_CaseCategories_CaseCategoryId",
                table: "Cases");

            migrationBuilder.AlterColumn<int>(
                name: "CaseCategoryId",
                table: "Cases",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_CaseCategories_CaseCategoryId",
                table: "Cases",
                column: "CaseCategoryId",
                principalTable: "CaseCategories",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_CaseCategories_CaseCategoryId",
                table: "Cases");

            migrationBuilder.AlterColumn<int>(
                name: "CaseCategoryId",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_CaseCategories_CaseCategoryId",
                table: "Cases",
                column: "CaseCategoryId",
                principalTable: "CaseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
