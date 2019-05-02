using Microsoft.EntityFrameworkCore.Migrations;

namespace AlphaFacev2.Migrations
{
    public partial class IsUserLoggedInColumnInHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUserLoggedIn",
                table: "History",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUserLoggedIn",
                table: "History");
        }
    }
}
