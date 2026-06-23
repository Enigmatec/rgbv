using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class organisationtype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OrganisationType",
                table: "Organisations",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "OrganisationType",
                table: "Organisations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
