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
            // Usamos SQL direto para evitar incompatibilidade do Pomelo ao
            // gerar DDL a partir de MigrationBuilder com tipos anônimos.
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Duracao` varchar(100) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `ObjetivosJson` TEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `ConteudoProgramaticoJson` TEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `RequisitosJson` TEXT NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Certificacao` varchar(200) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Modalidade` varchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `DataInicio` varchar(10) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `DataFim` varchar(10) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Horario` varchar(100) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Vagas` int NOT NULL DEFAULT 0;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `Nivel` varchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE `Cursos` ADD COLUMN `TagsJson` TEXT NULL;");

            // ── Nova tabela Depoimentos ───────────────────────────────────────
            migrationBuilder.Sql(@"
                CREATE TABLE `Depoimentos` (
                    `Id`        varchar(255) NOT NULL,
                    `CursoId`   varchar(255) NOT NULL,
                    `Nome`      varchar(150) NOT NULL,
                    `Comentario` TEXT NOT NULL,
                    `Nota`      int NOT NULL,
                    PRIMARY KEY (`Id`),
                    INDEX `IX_Depoimentos_CursoId` (`CursoId`),
                    CONSTRAINT `FK_Depoimentos_Cursos_CursoId`
                        FOREIGN KEY (`CursoId`) REFERENCES `Cursos` (`Id`)
                        ON DELETE CASCADE
                );
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
