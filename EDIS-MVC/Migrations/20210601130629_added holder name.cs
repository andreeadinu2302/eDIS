using Microsoft.EntityFrameworkCore.Migrations;

namespace EDIS_MVC.Migrations
{
    public partial class addedholdername : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HolderName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HolderName",
                table: "Documents");
        }
    }
}
