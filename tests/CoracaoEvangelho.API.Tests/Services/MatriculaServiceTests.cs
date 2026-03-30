using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services;
using FluentAssertions;
using Moq;

namespace CoracaoEvangelho.API.Tests.Services;

public class MatriculaServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────

    private static Curso CursoFake(string id = "curso-01") => new()
    {
        Id        = id,
        Titulo    = "Fundamentos do Espiritismo",
        Descricao = "Desc",
        Aulas     = new List<Aula>
        {
            new() { Id = "aula-01", Titulo = "Aula 1", YoutubeVideoId = "abc", Ordem = 1, Ativa = true }
        }
    };

    private static MatriculaRequestDto DtoFake() =>
        new("João Silva", "joao@email.com", null, null, null, null, null, true, false);

    // ── Inscrever ─────────────────────────────────────────────

    [Fact]
    public async Task InscreverAsync_PrimeiraMatricula_RetornaMatriculaAtiva()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoFake());

        // Sem matrícula existente
        matriculaMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync((Matricula?)null);

        matriculaMock.Setup(r => r.AddAsync(It.IsAny<Matricula>(), default))
                     .Returns(Task.CompletedTask);
        matriculaMock.Setup(r => r.SaveChangesAsync(default)).ReturnsAsync(1);

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);

        var result = await service.InscreverAsync("usr-01", "curso-01", DtoFake());

        result.CursoId.Should().Be("curso-01");
        result.CursoTitulo.Should().Be("Fundamentos do Espiritismo");
        result.Ativa.Should().BeTrue();
        matriculaMock.Verify(r => r.AddAsync(It.IsAny<Matricula>(), default), Times.Once);
        matriculaMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task InscreverAsync_MatriculaDuplicada_LancaInvalidOperation()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-01", default))
                 .ReturnsAsync(CursoFake());

        // Já matriculado
        matriculaMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(new Matricula { Id = "m-01", Ativa = true });

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);

        await service.Invoking(s => s.InscreverAsync("usr-01", "curso-01", DtoFake()))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já está matriculado*");

        matriculaMock.Verify(r => r.AddAsync(It.IsAny<Matricula>(), default), Times.Never);
    }

    [Fact]
    public async Task InscreverAsync_CursoInexistente_LancaKeyNotFound()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        cursoMock.Setup(r => r.GetByIdComAulasAsync("curso-404", default))
                 .ReturnsAsync((Curso?)null);

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);

        await service.Invoking(s => s.InscreverAsync("usr-01", "curso-404", DtoFake()))
            .Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*não encontrado*");
    }

    // ── EstaMatriculado ───────────────────────────────────────

    [Fact]
    public async Task EstaMatriculadoAsync_MatriculaAtiva_RetornaTrue()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        matriculaMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(new Matricula { Ativa = true });

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);
        var result  = await service.EstaMatriculadoAsync("usr-01", "curso-01");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EstaMatriculadoAsync_SemMatricula_RetornaFalse()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        matriculaMock.Setup(r => r.GetByUsuarioCursoAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                     .ReturnsAsync((Matricula?)null);

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);
        var result  = await service.EstaMatriculadoAsync("usr-01", "curso-01");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task EstaMatriculadoAsync_MatriculaInativa_RetornaFalse()
    {
        var cursoMock    = new Mock<ICursoRepository>();
        var matriculaMock = new Mock<IMatriculaRepository>();

        matriculaMock.Setup(r => r.GetByUsuarioCursoAsync("usr-01", "curso-01", default))
                     .ReturnsAsync(new Matricula { Ativa = false }); // cancelada

        var service = new MatriculaService(matriculaMock.Object, cursoMock.Object);
        var result  = await service.EstaMatriculadoAsync("usr-01", "curso-01");

        result.Should().BeFalse();
    }
}
