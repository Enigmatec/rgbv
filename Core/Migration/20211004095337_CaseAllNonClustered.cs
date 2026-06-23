using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Migrations
{
    public partial class CaseAllNonClustered : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Organisations_Code",
            //    table: "Organisations");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Organisations_CreatedAt",
            //    table: "Organisations");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Organisations_Name",
            //    table: "Organisations");

            //migrationBuilder.DropIndex(
            //    name: "IX_Organisations_Code",
            //    table: "Organisations");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Cases_CreatedAt",
            //    table: "Cases");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Cases_IncidentCode",
            //    table: "Cases");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_Cases_SerialNumber",
            //    table: "Cases");

            //migrationBuilder.DropIndex(
            //    name: "IX_Cases_IncidentCode",
            //    table: "Cases");

            //migrationBuilder.DropIndex(
            //    name: "IX_Cases_SerialNumber",
            //    table: "Cases");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_AspNetUsers_CreatedAt",
            //    table: "AspNetUsers");

            //migrationBuilder.DropUniqueConstraint(
            //    name: "AK_AspNetUsers_Email",
            //    table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                maxLength: 6,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(6)",
                oldMaxLength: 6);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentCode",
                table: "Cases",
                maxLength: 15,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Organisations_Code",
            //    table: "Organisations",
            //    column: "Code",
            //    unique: true,
            //    filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ApprovedAt",
                table: "Cases",
                column: "ApprovedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CreatedAt",
                table: "Cases",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_DateOfIncident",
                table: "Cases",
                column: "DateOfIncident")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_DateReported",
                table: "Cases",
                column: "DateReported")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_HasCaseBeenClosed",
                table: "Cases",
                column: "HasCaseBeenClosed")
                .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Cases_IncidentCode",
            //    table: "Cases",
            //    column: "IncidentCode",
            //    unique: true,
            //    filter: "[IncidentCode] IS NOT NULL")
            //    .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_LgaValidatedAt",
                table: "Cases",
                column: "LgaValidatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ModifiedAt",
                table: "Cases",
                column: "ModifiedAt")
                .Annotation("SqlServer:Clustered", false);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Cases_SerialNumber",
            //    table: "Cases",
            //    column: "SerialNumber",
            //    unique: true)
            //    .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_SexOfSurvior",
                table: "Cases",
                column: "SexOfSurvior")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ValidatedAt",
                table: "Cases",
                column: "ValidatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_VulnerablePopulation",
                table: "Cases",
                column: "VulnerablePopulation")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_Code",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ApprovedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_CreatedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_DateOfIncident",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_DateReported",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_HasCaseBeenClosed",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_IncidentCode",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_LgaValidatedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ModifiedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_SexOfSurvior",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_ValidatedAt",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Cases_VulnerablePopulation",
                table: "Cases");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Organisations",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IncidentCode",
                table: "Cases",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 15,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Organisations_Code",
                table: "Organisations",
                column: "Code")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Organisations_CreatedAt",
                table: "Organisations",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Organisations_Name",
                table: "Organisations",
                column: "Name")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Cases_CreatedAt",
                table: "Cases",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Cases_IncidentCode",
                table: "Cases",
                column: "IncidentCode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Cases_SerialNumber",
                table: "Cases",
                column: "SerialNumber")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_CreatedAt",
                table: "AspNetUsers",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email")
                .Annotation("SqlServer:Clustered", false);

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

            migrationBuilder.CreateIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases",
                column: "SerialNumber",
                unique: true);
        }
    }
}