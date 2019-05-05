using Microsoft.EntityFrameworkCore.Migrations;

namespace AlphaFacev2.Migrations
{
    public partial class UpdatedFace2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Accuracy",
                table: "Face",
                newName: "Confidence");

            migrationBuilder.AddColumn<bool>(
                name: "IsIdentical",
                table: "Face",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIdentical",
                table: "Face");

            migrationBuilder.RenameColumn(
                name: "Confidence",
                table: "Face",
                newName: "Accuracy");
        }
    }
}
