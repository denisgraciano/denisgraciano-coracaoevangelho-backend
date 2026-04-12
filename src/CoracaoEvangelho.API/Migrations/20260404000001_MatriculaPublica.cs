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
            // MySQL não permite dropar um índice que suporta uma FK.
            // FK_Matriculas_Usuarios_UsuarioId usa IX_Matriculas_UsuarioId_CursoId
            // (primeiro campo da chave composta), então é preciso remover a FK antes.
            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Usuarios_UsuarioId",
                table: "Matriculas");

            // Com a FK removida, o índice composto pode ser excluído.
            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId_CursoId",
                table: "Matriculas");

            // Torna UsuarioId nullable: inscrição pública não exige conta.
            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "Matriculas",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: false);

            // Índice simples para suportar a FK reativada abaixo.
            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId");

            // Recria a FK com SET NULL — quando o usuário é deletado,
            // a matrícula mantém o histórico sem UsuarioId.
            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Usuarios_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Novo índice único: mesmo e-mail não pode se inscrever duas vezes
            // no mesmo curso (substitui a unicidade anterior por UsuarioId+CursoId).
            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_Email_CursoId",
                table: "Matriculas",
                columns: new[] { "Email", "CursoId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_Usuarios_UsuarioId",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_Email_CursoId",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_Usuarios_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
