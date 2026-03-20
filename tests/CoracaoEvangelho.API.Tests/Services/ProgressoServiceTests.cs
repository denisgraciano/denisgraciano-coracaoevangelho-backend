using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class ProgressoServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────

    private static Curso CursoComTresAulas() => new()
    {
        Id    = "curso-01",
        Aulas = new List<Aula>
        {
            new() { Id = "aula-01", Ordem = 1, Ativa = true },
            new() { Id = "aula-02", Ordem = 2, Ativa = true },
            new() { Id = "aula-03", Ordem = 3, Ativa = true }
        }
    };

    private static List<Progresso> SemProgresso() => new();

    private static List<Progresso> UmaAulaConcluida() => new()
    {
        new() { AulaId = "aula-01", CursoId = "curso-01", Concluida = true,
                DataConclusao = DateTime.UtcNow.AddHours(-1) }
    };

    private static List<Progresso> TudasConcluidas() => new()
    {
        new() { AulaId = "aula-01", CursoId = "curso-01", Concluida = true, DataConclusao = DateTime.UtcNow.AddHours(-3) },
        new() { AulaId = "aula-02", CursoId = "curso-01", Concluida = true, DataConclusao = DateTime.UtcNow.AddHours(-2) },
        new() { AulaId = "aula-03", CursoId = "curso-01", Concluida = true, DataConclusao = DateTime.UtcNow.AddHours(-1) }
    };

    // ── GetProgressoCursoAsync ────────────────────────────────

    [Fact]
    public async Task GetProgressoCursoAsync_SemAulasConcluidas_RetornaZeroPorcento()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoComTresAulas());
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(SemProgresso());

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);
        var result  = await service.GetProgressoCursoAsync("usr-01", "curso-01");

        result.PercentualConcluido.Should().Be(0);
        result.AulasProgresso.Should().BeEmpty();
        result.DataConclusao.Should().BeNull();
        result.CertificadoEmitido.Should().BeFalse();
    }

    [Fact]
    public async Task GetProgressoCursoAsync_UmaAulaDeTres_Retorna33Porcento()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoComTresAulas());
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(UmaAulaConcluida());

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);
        var result  = await service.GetProgressoCursoAsync("usr-01", "curso-01");

        result.PercentualConcluido.Should().Be(33);
        result.DataConclusao.Should().BeNull();
    }

    [Fact]
    public async Task GetProgressoCursoAsync_TudasConcluidas_Retorna100EDataConclusao()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoComTresAulas());
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(TudasConcluidas());

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);
        var result  = await service.GetProgressoCursoAsync("usr-01", "curso-01");

        result.PercentualConcluido.Should().Be(100);
        result.DataConclusao.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProgressoCursoAsync_CursoNaoEncontrado_LancaKeyNotFound()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync(It.IsAny<string>(), default))
                 .ReturnsAsync((Curso?)null);

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);

        await service.Invoking(s => s.GetProgressoCursoAsync("usr-01", "curso-404"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── MarcarAulaConcluidaAsync ──────────────────────────────

    [Fact]
    public async Task MarcarAulaConcluidaAsync_PrimeiraAula_UpsertERetornaProgressoAtualizado()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoComTresAulas());

        // UpsertAsync não falha
        progressoMock.Setup(r => r.UpsertAsync(It.IsAny<Progresso>(), default))
                     .Returns(Task.CompletedTask);
        progressoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        // Depois do upsert, retorna 1 aula concluída
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(UmaAulaConcluida());

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);
        var dto     = new MarcarAulaConcluidaRequestDto("curso-01", "aula-01");

        var result = await service.MarcarAulaConcluidaAsync("usr-01", dto);

        result.PercentualConcluido.Should().Be(33);
        progressoMock.Verify(r => r.UpsertAsync(
            It.Is<Progresso>(p => p.AulaId == "aula-01" && p.Concluida),
            default), Times.Once);
        progressoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task MarcarAulaConcluidaAsync_UltimaAula_Retorna100Porcento()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var progressoMock = new Mock<IProgressoRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoComTresAulas());

        progressoMock.Setup(r => r.UpsertAsync(It.IsAny<Progresso>(), default))
                     .Returns(Task.CompletedTask);
        progressoMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        // Todas concluídas após marcar a última
        progressoMock.Setup(r => r.GetByCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(TudasConcluidas());

        var service = new ProgressoService(progressoMock.Object, cursoMock.Object);
        var dto     = new MarcarAulaConcluidaRequestDto("curso-01", "aula-03");

        var result = await service.MarcarAulaConcluidaAsync("usr-01", dto);

        result.PercentualConcluido.Should().Be(100);
        result.DataConclusao.Should().NotBeNullOrEmpty();
    }
}
