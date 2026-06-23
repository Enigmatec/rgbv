using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class columnchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AlterColumn<string>(
                name: "HasClientReceivedjustice",
                table: "FollowUps",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "HasCaseBeenClosed",
                table: "FollowUps",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HasClientReceivedjustice",
                table: "FollowUps",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "HasCaseBeenClosed",
                table: "FollowUps",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

        }
    }
}
