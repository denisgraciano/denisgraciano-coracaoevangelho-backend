using CoracaoEvangelho.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CoracaoEvangelho.API.Data;

/// <summary>
/// Seed de dados para desenvolvimento.
/// Popula o banco com os mesmos dados do CURSOS_MOCK do frontend Angular,
/// garantindo que o frontend funcione imediatamente após trocar os mocks pela API.
///
/// USO: chamado pelo Program.cs apenas em Development.
/// NUNCA executar automaticamente em produção.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger)
    {
        // Verifica se já tem dados — idempotente
        if (await db.Cursos.AnyAsync())
        {
            logger.LogInformation("Seed ignorado — banco já possui dados.");
            return;
        }

        logger.LogInformation("Executando seed de desenvolvimento...");

        // ── Categorias ────────────────────────────────────────
        var catDoutrina = new Categoria
        {
            Id        = "cat-doutrina",
            Nome      = "Doutrina",
            Descricao = "Fundamentos da Doutrina Espírita",
            Icone     = "assets/icons/doutrina.svg"
        };

        var catPratica = new Categoria
        {
            Id        = "cat-pratica",
            Nome      = "Prática Espírita",
            Descricao = "Aplicação prática dos ensinamentos",
            Icone     = "assets/icons/pratica.svg"
        };

        var catMediunidade = new Categoria
        {
            Id        = "cat-mediunidade",
            Nome      = "Mediunidade",
            Descricao = "Desenvolvimento mediúnico",
            Icone     = "assets/icons/mediunidade.svg"
        };

        var catEvangelho = new Categoria
        {
            Id        = "cat-evangelho",
            Nome      = "Evangelho no Lar",
            Descricao = "Estudo do Evangelho em família",
            Icone     = "assets/icons/evangelho.svg"
        };

        await db.Categorias.AddRangeAsync(
            catDoutrina, catPratica, catMediunidade, catEvangelho);

        // ── Cursos (espelham CURSOS_MOCK do Angular) ──────────
        var cursoEspiritismo = new Curso
        {
            // ID igual ao mock para não quebrar links existentes
            Id                    = "espiritismo-basico",
            Titulo                = "Fundamentos do Espiritismo",
            Descricao             = "Entenda as bases da doutrina espírita com didática e profundidade.",
            CategoriaId           = catDoutrina.Id,
            ImagemUrl             = "assets/images/curso-espiritismo.jpg",
            Instrutor             = "Prof. Allan Kardec Jr.",
            CertificadoDisponivel = true,
            Ativo                 = true,
            CriadoEm            = DateTime.UtcNow,
            AtualizadoEm         = DateTime.UtcNow
        };

        var cursoEvangelho = new Curso
        {
            Id                    = "evangelho-no-lar",
            Titulo                = "O Evangelho no Lar",
            Descricao             = "A prática do Evangelho como fonte de paz e harmonia familiar.",
            CategoriaId           = catEvangelho.Id,
            ImagemUrl             = "assets/images/curso-evangelho.jpg",
            Instrutor             = "Coord. Maria da Graça",
            CertificadoDisponivel = true,
            Ativo                 = true,
            CriadoEm            = DateTime.UtcNow,
            AtualizadoEm         = DateTime.UtcNow
        };

        await db.Cursos.AddRangeAsync(cursoEspiritismo, cursoEvangelho);

        // ── Aulas (espelham aulas dos mocks) ──────────────────
        var aulasEspiritismo = new List<Aula>
        {
            new() { Id = "aula-01", CursoId = cursoEspiritismo.Id, Titulo = "O que é o Espiritismo?",
                    Descricao = "Introdução à doutrina.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 25, Ordem = 1, Ativa = true },
            new() { Id = "aula-02", CursoId = cursoEspiritismo.Id, Titulo = "Os Três Aspectos",
                    Descricao = "Ciência, filosofia e religião.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 30, Ordem = 2, Ativa = true },
            new() { Id = "aula-03", CursoId = cursoEspiritismo.Id, Titulo = "Moral Espírita",
                    Descricao = "Amor, caridade e evolução.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 28, Ordem = 3, Ativa = true },
        };

        var aulasEvangelho = new List<Aula>
        {
            new() { Id = "aula-ev-01", CursoId = cursoEvangelho.Id, Titulo = "A Família no Plano Espiritual",
                    Descricao = "Vínculos além do plano físico.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 22, Ordem = 1, Ativa = true },
            new() { Id = "aula-ev-02", CursoId = cursoEvangelho.Id, Titulo = "Leitura e Comentário",
                    Descricao = "Como conduzir uma sessão.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 20, Ordem = 2, Ativa = true },
            new() { Id = "aula-ev-03", CursoId = cursoEvangelho.Id, Titulo = "Prece e Vibração",
                    Descricao = "O poder da oração.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 18, Ordem = 3, Ativa = true },
            new() { Id = "aula-ev-04", CursoId = cursoEvangelho.Id, Titulo = "Desdobramentos Práticos",
                    Descricao = "Aplicando no cotidiano.", YoutubeVideoId = "dQw4w9WgXcQ",
                    DuracaoMinutos = 26, Ordem = 4, Ativa = true },
        };

        await db.Aulas.AddRangeAsync(aulasEspiritismo);
        await db.Aulas.AddRangeAsync(aulasEvangelho);

        await db.SaveChangesAsync();
        logger.LogInformation("Seed concluído: {Cursos} cursos, {Aulas} aulas.",
            2, aulasEspiritismo.Count + aulasEvangelho.Count);
    }
}
