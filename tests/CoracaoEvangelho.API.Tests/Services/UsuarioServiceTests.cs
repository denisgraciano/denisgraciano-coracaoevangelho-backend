using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class UsuarioServiceTests
{
    private static Usuario UsuarioFake() => new()
    {
        Id        = "usr-01",
        Nome      = "Maria Silva",
        Email     = "maria@email.com",
        AvatarUrl = null,
        SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha123", workFactor: 4)
    };

    // ── GetPerfil ─────────────────────────────────────────────

    [Fact]
    public async Task GetPerfilAsync_UsuarioExistente_RetornaDadosSemSenhaHash()
    {
        var usuario  = UsuarioFake();
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByIdAsync("usr-01", default))
                .ReturnsAsync(usuario);

        var service = new UsuarioService(repoMock.Object);
        var result  = await service.GetPerfilAsync("usr-01");

        result.Id.Should().Be("usr-01");
        result.Nome.Should().Be("Maria Silva");
        result.Email.Should().Be("maria@email.com");
        // Garantia: DTO não tem SenhaHash
        result.GetType().GetProperty("SenhaHash").Should().BeNull();
    }

    [Fact]
    public async Task GetPerfilAsync_UsuarioNaoEncontrado_LancaKeyNotFound()
    {
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Usuario?)null);

        var service = new UsuarioService(repoMock.Object);

        await service.Invoking(s => s.GetPerfilAsync("inexistente"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── AtualizarPerfil ───────────────────────────────────────

    [Fact]
    public async Task AtualizarPerfilAsync_DadosValidos_RetornaPerfilAtualizado()
    {
        var usuario  = UsuarioFake();
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetTrackedByIdAsync("usr-01", default))
                .ReturnsAsync(usuario);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new UsuarioService(repoMock.Object);
        var dto     = new AtualizarPerfilRequestDto("Novo Nome", "https://cdn.example.com/avatar.jpg");

        var result = await service.AtualizarPerfilAsync("usr-01", dto);

        result.Nome.Should().Be("Novo Nome");
        result.AvatarUrl.Should().Be("https://cdn.example.com/avatar.jpg");
        repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AtualizarPerfilAsync_AvatarUrlNull_NaoAlteraAvatarExistente()
    {
        var usuario = UsuarioFake();
        usuario.AvatarUrl = "https://cdn.example.com/foto-antiga.jpg";

        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetTrackedByIdAsync("usr-01", default))
                .ReturnsAsync(usuario);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new UsuarioService(repoMock.Object);
        // AvatarUrl = null → não deve sobrescrever o valor existente
        var dto = new AtualizarPerfilRequestDto("Nome Atualizado", null);

        var result = await service.AtualizarPerfilAsync("usr-01", dto);

        result.AvatarUrl.Should().Be("https://cdn.example.com/foto-antiga.jpg");
    }

    // ── AlterarSenha ──────────────────────────────────────────

    [Fact]
    public async Task AlterarSenhaAsync_SenhaAtualCorreta_SalvaNovaSenhaHasheada()
    {
        var usuario  = UsuarioFake();
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetTrackedByIdAsync("usr-01", default))
                .ReturnsAsync(usuario);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new UsuarioService(repoMock.Object);
        var dto     = new AlterarSenhaRequestDto("Senha123", "NovaSenha456");

        await service.AlterarSenhaAsync("usr-01", dto);

        // Verifica que a nova senha foi salva com hash correto
        BCrypt.Net.BCrypt.Verify("NovaSenha456", usuario.SenhaHash).Should().BeTrue();
        repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_SenhaAtualErrada_LancaUnauthorized()
    {
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetTrackedByIdAsync("usr-01", default))
                .ReturnsAsync(UsuarioFake());

        var service = new UsuarioService(repoMock.Object);
        var dto     = new AlterarSenhaRequestDto("SenhaErrada!", "NovaSenha456");

        await service.Invoking(s => s.AlterarSenhaAsync("usr-01", dto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*incorreta*");
    }

    [Fact]
    public async Task AlterarSenhaAsync_UsuarioNaoEncontrado_LancaKeyNotFound()
    {
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetTrackedByIdAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Usuario?)null);

        var service = new UsuarioService(repoMock.Object);

        await service.Invoking(s =>
                s.AlterarSenhaAsync("inexistente",
                    new AlterarSenhaRequestDto("Senha123", "Nova456")))
            .Should().ThrowAsync<KeyNotFoundException>();
    }
}
