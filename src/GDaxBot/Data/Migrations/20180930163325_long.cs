using Microsoft.EntityFrameworkCore.Migrations;

namespace GDaxBot.Data.Migrations
{
    public partial class @long : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "IdTelegram",
                table: "Sesiones",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "IdTelegram",
                table: "Sesiones",
                nullable: false,
                oldClrType: typeof(long));
        }
    }
}
