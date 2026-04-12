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
            // ── Novos campos na tabela Cursos ─────────────────────────────────
            // SQL direto para evitar incompatibilidade do Pomelo com FK em
            // CreateTable com tipo anônimo.
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Duracao` varchar(100) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `ObjetivosJson` LONGTEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `ConteudoProgramaticoJson` LONGTEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `RequisitosJson` LONGTEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Certificacao` varchar(200) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Modalidade` varchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `DataInicio` varchar(10) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `DataFim` varchar(10) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Horario` varchar(100) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Vagas` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Nivel` varchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `TagsJson` LONGTEXT NULL;");

            // ── Nova tabela Depoimentos ───────────────────────────────────────
            // Pomelo cria os varchar(255) com o mesmo charset/collation do banco,
            // garantindo compatibilidade com Cursos.Id sem especificar charset manualmente.
            migrationBuilder.CreateTable(
                name: "Depoimentos",
                columns: table => new
                {
                    Id        = table.Column<string>(type: "varchar(255)", nullable: false),
                    CursoId   = table.Column<string>(type: "varchar(255)", nullable: false),
                    Nome      = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Comentario = table.Column<string>(type: "LONGTEXT", nullable: false),
                    Nota      = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    // FK adicionada via Sql() separado para evitar bug de geração
                    // do Pomelo com ForeignKey em tipo anônimo (errava key columns).
                    table.PrimaryKey("PK_Depoimentos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Depoimentos_CursoId",
                table: "Depoimentos",
                column: "CursoId");

            // FK adicionada depois da criação da tabela — colunas já têm o
            // charset correto do Pomelo, evitando incompatibilidade de collation.
            migrationBuilder.Sql(@"
                ALTER TABLE `Depoimentos`
                ADD CONSTRAINT `FK_Depoimentos_Cursos_CursoId`
                FOREIGN KEY (`CursoId`) REFERENCES `Cursos` (`Id`)
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `Depoimentos`;");

            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Duracao`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `ObjetivosJson`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `ConteudoProgramaticoJson`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `RequisitosJson`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Certificacao`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Modalidade`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `DataInicio`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `DataFim`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Horario`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Vagas`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `Nivel`;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` DROP COLUMN `TagsJson`;");
        }
    }
}
