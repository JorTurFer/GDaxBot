using Microsoft.EntityFrameworkCore.Migrations;

namespace GDaxBot.Data.Migrations
{
    public partial class codigo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Productos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Productos");
        }
    }
}
