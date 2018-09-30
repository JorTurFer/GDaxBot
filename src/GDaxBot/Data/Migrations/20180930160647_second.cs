using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GDaxBot.Data.Migrations
{
    public partial class second : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessage",
                table: "Usuarios",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    IdProducto = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.IdProducto);
                });

            migrationBuilder.CreateTable(
                name: "AjustesProductos",
                columns: table => new
                {
                    IdAjuste = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUsuario = table.Column<int>(nullable: false),
                    IdProducto = table.Column<int>(nullable: false),
                    ValorMarcado = table.Column<decimal>(nullable: false),
                    UmbralInferior = table.Column<decimal>(nullable: false),
                    UmbralSuperior = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesProductos", x => x.IdAjuste);
                    table.ForeignKey(
                        name: "FK_AjustesProductos_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AjustesProductos_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registros",
                columns: table => new
                {
                    IdRegistro = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdProducto = table.Column<int>(nullable: false),
                    Valor = table.Column<decimal>(nullable: false),
                    Fecha = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registros", x => x.IdRegistro);
                    table.ForeignKey(
                        name: "FK_Registros_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AjustesProductos_IdProducto",
                table: "AjustesProductos",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesProductos_IdUsuario",
                table: "AjustesProductos",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Registros_IdProducto",
                table: "Registros",
                column: "IdProducto");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjustesProductos");

            migrationBuilder.DropTable(
                name: "Registros");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropColumn(
                name: "LastMessage",
                table: "Usuarios");
        }
    }
}
