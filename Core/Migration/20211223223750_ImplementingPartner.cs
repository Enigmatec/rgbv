using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class ImplementingPartner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_Name",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
                values: new object[] { "c2ee66f4-60e9-4983-9f9c-fddb2b57c768", "6050e15d-3352-47ef-aaa7-cb9aa4d722d3", "View only access to organisation's data", "Implementing Partner", "IMPLEMENTING PARTNER" });

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_CreatedAt",
                table: "ServicesProvided",
                column: "CreatedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_IncidentCode",
                table: "ServicesProvided",
                column: "IncidentCode")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_ModifiedAt",
                table: "ServicesProvided",
                column: "ModifiedAt")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_ServiceProvisionCode",
                table: "ServicesProvided",
                column: "ServiceProvisionCode",
                unique: true,
                filter: "[ServiceProvisionCode] IS NOT NULL")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_ServicesProvided_SexOfSurvivorOrVictim",
                table: "ServicesProvided",
                column: "SexOfSurvivorOrVictim")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Name",
                table: "Organisations",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases",
                column: "SerialNumber")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL")
                .Annotation("SqlServer:Clustered", false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServicesProvided_CreatedAt",
                table: "ServicesProvided");

            migrationBuilder.DropIndex(
                name: "IX_ServicesProvided_IncidentCode",
                table: "ServicesProvided");

            migrationBuilder.DropIndex(
                name: "IX_ServicesProvided_ModifiedAt",
                table: "ServicesProvided");

            migrationBuilder.DropIndex(
                name: "IX_ServicesProvided_ServiceProvisionCode",
                table: "ServicesProvided");

            migrationBuilder.DropIndex(
                name: "IX_ServicesProvided_SexOfSurvivorOrVictim",
                table: "ServicesProvided");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_Name",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c2ee66f4-60e9-4983-9f9c-fddb2b57c768");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_Name",
                table: "Organisations",
                column: "Name")
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_SerialNumber",
                table: "Cases",
                column: "SerialNumber",
                unique: true)
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Code",
                table: "AspNetUsers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email")
                .Annotation("SqlServer:Clustered", false);
        }
    }
}
