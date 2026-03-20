using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class PedidoVibracaoServiceTests
{
    private static PedidoVibracaoRequestDto DtoFake() => new(
        Nome:     "Ana Lima",
        Email:    "ana@email.com",
        Pedido:   "Peço vibrações de cura para minha família.",
        Endereco: new EnderecoDto("01310-100", "Av. Paulista", "São Paulo", "SP")
    );

    // ── EnviarAsync ───────────────────────────────────────────

    [Fact]
    public async Task EnviarAsync_UsuarioAnonimo_SalvaSemUsuarioId()
    {
        var repoMock = new Mock<IPedidoVibracaoRepository>();

        PedidoVibracao? pedidoCapturado = null;
        repoMock.Setup(r => r.AddAsync(It.IsAny<PedidoVibracao>(), default))
                .Callback<PedidoVibracao, CancellationToken>((p, _) => pedidoCapturado = p)
                .Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new PedidoVibracaoService(repoMock.Object);
        var result  = await service.EnviarAsync(null, DtoFake());

        result.Mensagem.Should().Contain("carinho");
        result.Id.Should().NotBeNullOrEmpty();

        pedidoCapturado.Should().NotBeNull();
        pedidoCapturado!.UsuarioId.Should().BeNull();
        pedidoCapturado.Nome.Should().Be("Ana Lima");
        pedidoCapturado.Email.Should().Be("ana@email.com");
        pedidoCapturado.Cidade.Should().Be("São Paulo");
        pedidoCapturado.Estado.Should().Be("SP");
        pedidoCapturado.Lido.Should().BeFalse();

        repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task EnviarAsync_UsuarioAutenticado_AssociaUsuarioId()
    {
        var repoMock = new Mock<IPedidoVibracaoRepository>();

        PedidoVibracao? pedidoCapturado = null;
        repoMock.Setup(r => r.AddAsync(It.IsAny<PedidoVibracao>(), default))
                .Callback<PedidoVibracao, CancellationToken>((p, _) => pedidoCapturado = p)
                .Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new PedidoVibracaoService(repoMock.Object);
        await service.EnviarAsync("usr-42", DtoFake());

        pedidoCapturado!.UsuarioId.Should().Be("usr-42");
    }

    [Fact]
    public async Task EnviarAsync_EmailNormalizado_SalvaEmMinusculo()
    {
        var repoMock = new Mock<IPedidoVibracaoRepository>();

        PedidoVibracao? pedidoCapturado = null;
        repoMock.Setup(r => r.AddAsync(It.IsAny<PedidoVibracao>(), default))
                .Callback<PedidoVibracao, CancellationToken>((p, _) => pedidoCapturado = p)
                .Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new PedidoVibracaoService(repoMock.Object);
        var dto = DtoFake() with { Email = "ANA@EMAIL.COM" };
        await service.EnviarAsync(null, dto);

        pedidoCapturado!.Email.Should().Be("ana@email.com");
    }

    // ── ListarAsync ───────────────────────────────────────────

    [Fact]
    public async Task ListarAsync_PaginaUm_RetornaMetadadosPaginacaoCorretos()
    {
        var repoMock = new Mock<IPedidoVibracaoRepository>();

        repoMock.Setup(r => r.CountAsync(default)).ReturnsAsync(45);
        repoMock.Setup(r => r.GetAllAsync(1, 20, default))
                .ReturnsAsync(Enumerable.Range(1, 20).Select(i => new PedidoVibracao
                {
                    Id       = $"ped-{i:D2}",
                    Nome     = $"Pessoa {i}",
                    Email    = $"p{i}@email.com",
                    Pedido   = "Pedido de teste",
                    CriadoEm = DateTime.UtcNow
                }));

        var service = new PedidoVibracaoService(repoMock.Object);
        var result  = await service.ListarAsync(1, 20);

        result.TotalItens.Should().Be(45);
        result.TotalPaginas.Should().Be(3);       // ceil(45/20)
        result.TemProxima.Should().BeTrue();       // 1*20 < 45
        result.TemAnterior.Should().BeFalse();     // pagina 1
        result.Items.Should().HaveCount(20);
    }

    [Fact]
    public async Task ListarAsync_UltimaPagina_TemAnteriorTrueTemProximaFalse()
    {
        var repoMock = new Mock<IPedidoVibracaoRepository>();

        repoMock.Setup(r => r.CountAsync(default)).ReturnsAsync(45);
        repoMock.Setup(r => r.GetAllAsync(3, 20, default))
                .ReturnsAsync(Enumerable.Range(1, 5).Select(i => new PedidoVibracao
                {
                    Id     = $"ped-{i}",
                    Nome   = $"P{i}",
                    Email  = $"p{i}@e.com",
                    Pedido = "pedido"
                }));

        var service = new PedidoVibracaoService(repoMock.Object);
        var result  = await service.ListarAsync(3, 20);

        result.TemProxima.Should().BeFalse();   // 3*20=60 >= 45
        result.TemAnterior.Should().BeTrue();    // pagina 3 > 1
        result.Items.Should().HaveCount(5);
    }
}
