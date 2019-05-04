using Microsoft.EntityFrameworkCore.Migrations;

namespace AlphaFacev2.Migrations
{
    public partial class UpdatedProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IpAdress",
                table: "Profile",
                newName: "IpAddress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "Profile",
                newName: "IpAdress");
        }
    }
}
