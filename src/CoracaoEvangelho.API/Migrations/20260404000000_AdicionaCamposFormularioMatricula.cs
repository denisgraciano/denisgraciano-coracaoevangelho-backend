using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoracaoEvangelho.API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCamposFormularioMatricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AceitaTermos",
                table: "Matriculas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Matriculas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cep",
                table: "Matriculas",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Matriculas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Matriculas",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Matriculas",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataNascimento",
                table: "Matriculas",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Matriculas",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Matriculas",
                type: "varchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logradouro",
                table: "Matriculas",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeCompleto",
                table: "Matriculas",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Matriculas",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Matriculas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceberEmails",
                table: "Matriculas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Matriculas",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AceitaTermos",    table: "Matriculas");
            migrationBuilder.DropColumn(name: "Bairro",          table: "Matriculas");
            migrationBuilder.DropColumn(name: "Cep",             table: "Matriculas");
            migrationBuilder.DropColumn(name: "Cidade",          table: "Matriculas");
            migrationBuilder.DropColumn(name: "Complemento",     table: "Matriculas");
            migrationBuilder.DropColumn(name: "Cpf",             table: "Matriculas");
            migrationBuilder.DropColumn(name: "DataNascimento",  table: "Matriculas");
            migrationBuilder.DropColumn(name: "Email",           table: "Matriculas");
            migrationBuilder.DropColumn(name: "Estado",          table: "Matriculas");
            migrationBuilder.DropColumn(name: "Logradouro",      table: "Matriculas");
            migrationBuilder.DropColumn(name: "NomeCompleto",    table: "Matriculas");
            migrationBuilder.DropColumn(name: "Numero",          table: "Matriculas");
            migrationBuilder.DropColumn(name: "Observacoes",     table: "Matriculas");
            migrationBuilder.DropColumn(name: "ReceberEmails",   table: "Matriculas");
            migrationBuilder.DropColumn(name: "Telefone",        table: "Matriculas");
        }
    }
}
