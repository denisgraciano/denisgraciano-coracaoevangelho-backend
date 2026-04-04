using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoracaoEvangelho.API.Migrations
{
    /// <inheritdoc />
    public partial class MatriculaPublica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice único (UsuarioId, CursoId) — UsuarioId vai ser nullable
            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId_CursoId",
                table: "Matriculas");

            // Torna UsuarioId nullable: inscrição pública não exige conta
            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Matriculas",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: false);

            // Novo índice único: mesmo e-mail não pode se inscrever duas vezes no mesmo curso
            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_Email_CursoId",
                table: "Matriculas",
                columns: new[] { "Email", "CursoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Matriculas_Email_CursoId",
                table: "Matriculas");

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Matriculas",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_UsuarioId_CursoId",
                table: "Matriculas",
                columns: new[] { "UsuarioId", "CursoId" },
                unique: true);
        }
    }
}
