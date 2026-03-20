using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class AuthServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────

    private static IConfiguration BuildConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"]      = "test-jwt-key-coracao-evangelho-32chars",
                ["Jwt:Issuer"]   = "CoracaoEvangelho.API",
                ["Jwt:Audience"] = "CoracaoEvangelho.Frontend"
            })
            .Build();

    private static Usuario UsuarioFake(string email = "joao@email.com") => new()
    {
        Id           = "user-01",
        Email        = email,
        Nome         = "João Silva",
        SenhaHash    = BCrypt.Net.BCrypt.HashPassword("Senha123", workFactor: 4), // workFactor baixo p/ testes
        Role         = "aluno",
        RefreshToken = "refresh-tok-valido",
        RefreshTokenExpira = DateTime.UtcNow.AddDays(7)
    };

    // ── Registrar ─────────────────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_EmailNovo_RetornaTokenEUsuario()
    {
        // Arrange
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("joao@email.com", default))
                .ReturnsAsync((Usuario?)null);            // e-mail livre
        repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>(), default))
                .Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync(default))
                .ReturnsAsync(1);

        var service = new AuthService(repoMock.Object, BuildConfig());
        var dto     = new RegisterRequestDto("João Silva", "joao@email.com", "Senha123");

        // Act
        var result = await service.RegistrarAsync(dto);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Usuario.Email.Should().Be("joao@email.com");
        result.Usuario.Nome.Should().Be("João Silva");
        // SenhaHash nunca deve aparecer no response
        result.Should().NotBeOfType<Usuario>();
    }

    [Fact]
    public async Task RegistrarAsync_EmailDuplicado_LancaInvalidOperation()
    {
        // Arrange
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("joao@email.com", default))
                .ReturnsAsync(UsuarioFake());             // e-mail já existe

        var service = new AuthService(repoMock.Object, BuildConfig());
        var dto     = new RegisterRequestDto("Outro", "joao@email.com", "Senha123");

        // Act & Assert
        await service.Invoking(s => s.RegistrarAsync(dto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já cadastrado*");
    }

    // ── Login ─────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_CredenciaisValidas_RetornaTokenEUsuario()
    {
        // Arrange
        var usuario  = UsuarioFake();
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("joao@email.com", default))
                .ReturnsAsync(usuario);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new AuthService(repoMock.Object, BuildConfig());
        var dto     = new LoginRequestDto("joao@email.com", "Senha123");

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Usuario.Id.Should().Be("user-01");
    }

    [Fact]
    public async Task LoginAsync_SenhaErrada_LancaUnauthorized()
    {
        // Arrange
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByEmailAsync("joao@email.com", default))
                .ReturnsAsync(UsuarioFake());

        var service = new AuthService(repoMock.Object, BuildConfig());
        var dto     = new LoginRequestDto("joao@email.com", "SenhaErrada99");

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(dto))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Credenciais inválidas*");
    }

    [Fact]
    public async Task LoginAsync_EmailNaoExiste_LancaUnauthorized()
    {
        // Arrange
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Usuario?)null);

        var service = new AuthService(repoMock.Object, BuildConfig());

        // Act & Assert
        await service.Invoking(s => s.LoginAsync(new LoginRequestDto("nao@existe.com", "Senha123")))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Refresh ───────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_TokenValido_RetornaNovoToken()
    {
        // Arrange
        var usuario  = UsuarioFake();
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByRefreshTokenAsync("refresh-tok-valido", default))
                .ReturnsAsync(usuario);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new AuthService(repoMock.Object, BuildConfig());

        // Act
        var result = await service.RefreshTokenAsync("refresh-tok-valido");

        // Assert
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe("refresh-tok-valido"); // novo token gerado
    }

    [Fact]
    public async Task RefreshTokenAsync_TokenInvalido_LancaUnauthorized()
    {
        // Arrange
        var repoMock = new Mock<IUsuarioRepository>();
        repoMock.Setup(r => r.GetByRefreshTokenAsync(It.IsAny<string>(), default))
                .ReturnsAsync((Usuario?)null);

        var service = new AuthService(repoMock.Object, BuildConfig());

        // Act & Assert
        await service.Invoking(s => s.RefreshTokenAsync("token-invalido"))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inválido ou expirado*");
    }
}
