using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CodeChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "Code", table: "States", newName: "Key", null);

            migrationBuilder.RenameColumn(name: "Code", table: "LocalGovernmentAreas", newName: "Key", null);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);


            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Code",
                table: "Organisations",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_Code",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "States");

            migrationBuilder.RenameColumn(name: "Key", table: "States", newName: "Code", null);

            migrationBuilder.RenameColumn(name: "Key", table: "LocalGovernmentAreas", newName: "Code", null);

            

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);


         
        }
    }
}
