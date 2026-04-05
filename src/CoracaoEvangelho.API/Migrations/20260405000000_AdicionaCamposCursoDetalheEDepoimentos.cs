using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoracaoEvangelho.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCamposCursoDetalheEDepoimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Novos campos em Cursos (página de detalhes públicos) ──────────
            migrationBuilder.AddColumn<string>(
                name: "Duracao",
                table: "Cursos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetivosJson",
                table: "Cursos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConteudoProgramaticoJson",
                table: "Cursos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequisitosJson",
                table: "Cursos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Certificacao",
                table: "Cursos",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Modalidade",
                table: "Cursos",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataInicio",
                table: "Cursos",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataFim",
                table: "Cursos",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Horario",
                table: "Cursos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Vagas",
                table: "Cursos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nivel",
                table: "Cursos",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "Cursos",
                type: "TEXT",
                nullable: true);

            // ── Nova tabela Depoimentos ───────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Depoimentos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    CursoId = table.Column<string>(type: "varchar(255)", nullable: false),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", nullable: false),
                    Nota = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depoimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depoimentos_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Depoimentos_CursoId",
                table: "Depoimentos",
                column: "CursoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Depoimentos");

            migrationBuilder.DropColumn(name: "Duracao", table: "Cursos");
            migrationBuilder.DropColumn(name: "ObjetivosJson", table: "Cursos");
            migrationBuilder.DropColumn(name: "ConteudoProgramaticoJson", table: "Cursos");
            migrationBuilder.DropColumn(name: "RequisitosJson", table: "Cursos");
            migrationBuilder.DropColumn(name: "Certificacao", table: "Cursos");
            migrationBuilder.DropColumn(name: "Modalidade", table: "Cursos");
            migrationBuilder.DropColumn(name: "DataInicio", table: "Cursos");
            migrationBuilder.DropColumn(name: "DataFim", table: "Cursos");
            migrationBuilder.DropColumn(name: "Horario", table: "Cursos");
            migrationBuilder.DropColumn(name: "Vagas", table: "Cursos");
            migrationBuilder.DropColumn(name: "Nivel", table: "Cursos");
            migrationBuilder.DropColumn(name: "TagsJson", table: "Cursos");
        }
    }
}
