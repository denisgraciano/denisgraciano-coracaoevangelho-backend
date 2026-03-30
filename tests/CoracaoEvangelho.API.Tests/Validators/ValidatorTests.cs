using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace CoracaoEvangelho.API.Tests.Validators;

public class ValidatorTests
{
    // ── RegisterRequestValidator ──────────────────────────────

    private readonly RegisterRequestValidator _registerValidator = new();

    [Fact]
    public void Register_DadosValidos_SemErros()
    {
        var dto    = new RegisterRequestDto("João Silva", "joao@email.com", "Senha123");
        var result = _registerValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Jo")]          // muito curto
    [InlineData("")]            // vazio
    public void Register_NomeInvalido_ComErro(string nome)
    {
        var dto    = new RegisterRequestDto(nome, "joao@email.com", "Senha123");
        var result = _registerValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Theory]
    [InlineData("nao-e-email")]
    [InlineData("")]
    [InlineData("@semdominio")]
    public void Register_EmailInvalido_ComErro(string email)
    {
        var dto    = new RegisterRequestDto("João Silva", email, "Senha123");
        var result = _registerValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("curta1A")]     // 7 chars — mínimo é 8
    [InlineData("semmaiuscula1")]
    [InlineData("SemNumero")]
    [InlineData("")]
    public void Register_SenhaFraca_ComErro(string senha)
    {
        var dto    = new RegisterRequestDto("João Silva", "joao@email.com", senha);
        var result = _registerValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Senha);
    }

    [Fact]
    public void Register_SenhaForte_SemErro()
    {
        // Exatamente 8 chars, 1 maiúscula, 1 número
        var dto    = new RegisterRequestDto("João Silva", "joao@email.com", "Senha1AB");
        var result = _registerValidator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Senha);
    }

    // ── MatriculaRequestValidator ─────────────────────────────

    private readonly MatriculaRequestValidator _matriculaValidator = new();

    [Fact]
    public void Matricula_DadosValidos_SemErros()
    {
        var dto    = new MatriculaRequestDto("João Silva", "joao@email.com", null, null, null, null, null, true, false);
        var result = _matriculaValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Matricula_EmailInvalido_ComErro()
    {
        var dto    = new MatriculaRequestDto("João Silva", "invalido", null, null, null, null, null, true, false);
        var result = _matriculaValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Matricula_AceitaTermosFalso_ComErro()
    {
        var dto    = new MatriculaRequestDto("João Silva", "joao@email.com", null, null, null, null, null, false, false);
        var result = _matriculaValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.AceitaTermos);
    }

    // ── PedidoVibracaoRequestValidator ────────────────────────

    private readonly PedidoVibracaoRequestValidator _pedidoValidator = new();

    [Fact]
    public void Pedido_DadosValidos_SemErros()
    {
        var dto    = new PedidoVibracaoRequestDto(
            "Ana Lima", "ana@email.com",
            "Peço vibrações de cura para minha família.", null);
        var result = _pedidoValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Pedido_TextoCurto_ComErro()
    {
        var dto    = new PedidoVibracaoRequestDto("Ana Lima", "ana@email.com", "Curto", null);
        var result = _pedidoValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Pedido);
    }

    [Fact]
    public void Pedido_NomeVazio_ComErro()
    {
        var dto    = new PedidoVibracaoRequestDto("", "ana@email.com", "Pedido válido aqui.", null);
        var result = _pedidoValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    // ── AlterarSenhaRequestValidator ──────────────────────────

    private readonly AlterarSenhaRequestValidator _senhaValidator = new();

    [Fact]
    public void AlterarSenha_SenhaIgualAAtual_ComErro()
    {
        var dto    = new AlterarSenhaRequestDto("Senha123", "Senha123");  // igual
        var result = _senhaValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.NovaSenha);
    }

    [Fact]
    public void AlterarSenha_NovaSenhaDiferente_SemErro()
    {
        var dto    = new AlterarSenhaRequestDto("SenhaAntiga1", "SenhaNova2");
        var result = _senhaValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── MarcarAulaConcluidaRequestValidator ───────────────────

    private readonly MarcarAulaConcluidaRequestValidator _marcarValidator = new();

    [Fact]
    public void MarcarConcluida_IdsVazios_ComErros()
    {
        var dto    = new MarcarAulaConcluidaRequestDto("", "");
        var result = _marcarValidator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CursoId);
        result.ShouldHaveValidationErrorFor(x => x.AulaId);
    }

    [Fact]
    public void MarcarConcluida_IdsValidos_SemErros()
    {
        var dto    = new MarcarAulaConcluidaRequestDto("curso-01", "aula-01");
        var result = _marcarValidator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
