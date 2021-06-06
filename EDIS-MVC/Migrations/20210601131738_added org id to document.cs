using Microsoft.EntityFrameworkCore.Migrations;

namespace EDIS_MVC.Migrations
{
    public partial class addedorgidtodocument : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationName",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Documents");

            migrationBuilder.AddColumn<string>(
                name: "OrganizationName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
