using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class CertificadoServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────

    private static Curso CursoFake() => new()
    {
        Id    = "curso-01",
        Titulo = "Fundamentos do Espiritismo",
        Aulas = new List<Aula>
        {
            new() { Id = "aula-01", DuracaoMinutos = 25, Ativa = true },
            new() { Id = "aula-02", DuracaoMinutos = 30, Ativa = true },
            new() { Id = "aula-03", DuracaoMinutos = 28, Ativa = true }
        }
        // Total: 83 min → 1.4h
    };

    private static Usuario UsuarioFake() => new()
    {
        Id   = "usr-01",
        Nome = "João Silva"
    };

    private static List<Progresso> TudasConcluidas() => new()
    {
        new() { AulaId = "aula-01", Concluida = true },
        new() { AulaId = "aula-02", Concluida = true },
        new() { AulaId = "aula-03", Concluida = true }
    };

    private static List<Progresso> ApenasUma() => new()
    {
        new() { AulaId = "aula-01", Concluida = true }
    };

    // ── EmitirAsync ───────────────────────────────────────────

    [Fact]
    public async Task EmitirAsync_CursoConcluidoCertificadoNovo_CriaCertificadoCorreto()
    {
        var certMock     = new Mock<ICertificadoRepository>();
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();
        var usuarioMock  = new Mock<IUsuarioRepository>();

        // Ainda não foi emitido
        certMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                .ReturnsAsync((Certificado?)null);

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoFake());

        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(TudasConcluidas());

        usuarioMock.Setup(r => r.GetByIdAsync("usr-01", default))
                   .ReturnsAsync(UsuarioFake());

        certMock.Setup(r => r.AddAsync(It.IsAny<Certificado>(), default))
                .Returns(Task.CompletedTask);
        certMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new CertificadoService(
            certMock.Object, cursoMock.Object,
            progressoMock.Object, usuarioMock.Object);

        var result = await service.EmitirAsync("usr-01",
            new EmitirCertificadoRequestDto("curso-01"));

        result.CursoId.Should().Be("curso-01");
        result.CursoTitulo.Should().Be("Fundamentos do Espiritismo");
        result.AlunoNome.Should().Be("João Silva");
        result.CargaHoraria.Should().Be(1.4m); // 83 min / 60 = 1.383 → round(1,1) = 1.4
        certMock.Verify(r => r.AddAsync(It.IsAny<Certificado>(), default), Times.Once);
    }

    [Fact]
    public async Task EmitirAsync_CertificadoJaEmitido_RetornaExistenteIdempotente()
    {
        var certExistente = new Certificado
        {
            Id           = "cert-ja-existe",
            UsuarioId    = "usr-01",
            CursoId      = "curso-01",
            CursoTitulo  = "Fundamentos do Espiritismo",
            AlunoNome    = "João Silva",
            DataEmissao  = DateTime.UtcNow.AddDays(-5),
            CargaHoraria = 1.4m
        };

        var certMock     = new Mock<ICertificadoRepository>();
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();
        var usuarioMock  = new Mock<IUsuarioRepository>();

        // Já existe
        certMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                .ReturnsAsync(certExistente);

        var service = new CertificadoService(
            certMock.Object, cursoMock.Object,
            progressoMock.Object, usuarioMock.Object);

        var result = await service.EmitirAsync("usr-01",
            new EmitirCertificadoRequestDto("curso-01"));

        result.Id.Should().Be("cert-ja-existe");
        // Não deve chamar AddAsync nem SaveChanges
        certMock.Verify(r => r.AddAsync(It.IsAny<Certificado>(), default), Times.Never);
        certMock.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task EmitirAsync_CursoNaoConcluido_LancaInvalidOperation()
    {
        var certMock     = new Mock<ICertificadoRepository>();
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();
        var usuarioMock  = new Mock<IUsuarioRepository>();

        certMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                .ReturnsAsync((Certificado?)null);

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoFake());

        // Apenas 1 de 3 aulas concluída
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(ApenasUma());

        var service = new CertificadoService(
            certMock.Object, cursoMock.Object,
            progressoMock.Object, usuarioMock.Object);

        await service.Invoking(s => s.EmitirAsync("usr-01",
                new EmitirCertificadoRequestDto("curso-01")))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*1/3*");
    }

    [Fact]
    public async Task EmitirAsync_CursoNaoEncontrado_LancaKeyNotFound()
    {
        var certMock     = new Mock<ICertificadoRepository>();
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();
        var usuarioMock  = new Mock<IUsuarioRepository>();

        certMock.Setup(r => r.GetByUsuarioCursoAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync((Certificado?)null);

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-404", default))
                 .ReturnsAsync((Curso?)null);

        var service = new CertificadoService(
            certMock.Object, cursoMock.Object,
            progressoMock.Object, usuarioMock.Object);

        await service.Invoking(s => s.EmitirAsync("usr-01",
                new EmitirCertificadoRequestDto("curso-404")))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── GetByUsuarioAsync ─────────────────────────────────────

    [Fact]
    public async Task GetByUsuarioAsync_SemCertificados_RetornaListaVazia()
    {
        var certMock = new Mock<ICertificadoRepository>();
        certMock.Setup(r => r.GetByUsuarioAsync("usr-01", default))
                .ReturnsAsync(new List<Certificado>());

        var service = new CertificadoService(
            certMock.Object,
            new Mock<ICursoRepository>().Object,
            new Mock<IProgressoRepository>().Object,
            new Mock<IUsuarioRepository>().Object);

        var result = await service.GetByUsuarioAsync("usr-01");

        result.Should().BeEmpty();
    }
}
