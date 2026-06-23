using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class ComplaintFormRemovdeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplaintFormId",
                table: "ComplaintForms");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplaintFormId",
                table: "ComplaintForms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
