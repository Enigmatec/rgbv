using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class NonClusteredIndexes2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_Code",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentCode",
                table: "Cases",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Organisations_Code",
            //    table: "Organisations",
            //    column: "Code")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Organisations_CreatedAt",
            //    table: "Organisations",
            //    column: "CreatedAt")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Organisations_Name",
            //    table: "Organisations",
            //    column: "Name")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Cases_CreatedAt",
            //    table: "Cases",
            //    column: "CreatedAt")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Cases_IncidentCode",
            //    table: "Cases",
            //    column: "IncidentCode")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_Cases_SerialNumber",
            //    table: "Cases",
            //    column: "SerialNumber")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_AspNetUsers_CreatedAt",
            //    table: "AspNetUsers",
            //    column: "CreatedAt")
            //    .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.AddUniqueConstraint(
            //    name: "AK_AspNetUsers_Email",
            //    table: "AspNetUsers",
            //    column: "Email")
            //    .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Code",
                table: "Organisations",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases",
                column: "IncidentCode",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Organisations_Code",
                table: "Organisations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Organisations_CreatedAt",
                table: "Organisations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Organisations_Name",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_Code",
                table: "Organisations");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Cases_CreatedAt",
                table: "Cases");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Cases_IncidentCode",
                table: "Cases");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Cases_SerialNumber",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentCode",
                table: "Cases",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Code",
                table: "Organisations",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases",
                column: "IncidentCode",
                unique: true,
                filter: "[IncidentCode] IS NOT NULL");
        }
    }
}