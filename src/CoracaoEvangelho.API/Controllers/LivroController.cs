using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Catálogo de livros e leitura de capítulos.
/// Rotas públicas — não requerem autenticação.
/// Se o usuário estiver autenticado, isFavorito é preenchido automaticamente.
///
/// Contrato com Angular:
/// | Método | Rota                           | Service Angular |
/// |--------|--------------------------------|-----------------|
/// | GET    | /api/livros                    | LivroService.getLivros() |
/// | GET    | /api/livros/{id}               | LivroService.getLivroById() |
/// | GET    | /api/livros/{id}/capitulos/{n} | LivroService.getCapitulo() |
/// </summary>
[ApiController]
[Route("api/livros")]
[Produces("application/json")]
public class LivroController : ControllerBase
{
    private readonly ILivroService _livroService;

    public LivroController(ILivroService livroService) => _livroService = livroService;

    /// <summary>Lista todos os livros disponíveis.</summary>
    /// <remarks>
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     { "id": "abc", "titulo": "Evangelho de João", "subtitulo": "NT", "capa": "/capas/joao.jpg" }
    ///   ],
    ///   "message": "",
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os livros", Tags = new[] { "Livros" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LivroResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLivros(CancellationToken ct)
    {
        var livros = await _livroService.GetLivrosAsync(ct);
        return Ok(ApiResponse<IEnumerable<LivroResponseDto>>.Ok(livros));
    }

    /// <summary>Retorna detalhes de um livro (com lista de capítulos).</summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Detalhe de um livro", Tags = new[] { "Livros" })]
    [ProducesResponseType(typeof(ApiResponse<LivroResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLivro(string id, CancellationToken ct)
    {
        var livro = await _livroService.GetLivroByIdAsync(id, ct);
        if (livro is null)
            return NotFound(ApiResponse<object>.Fail($"Livro '{id}' não encontrado."));

        return Ok(ApiResponse<LivroResponseDto>.Ok(livro));
    }

    /// <summary>
    /// Retorna o sumário dos capítulos de um livro (sem versículos).
    /// Ideal para montar o índice/sumário na tela /livros/:id.
    /// </summary>
    /// <remarks>
    /// Por que esse endpoint existe separado de GET /api/livros/{id}/capitulos/{n}?
    /// Carregar todos os versículos de todos os capítulos só para exibir o índice
    /// seria inviável (alto volume de dados, prejudica o PWA offline).
    /// Este endpoint retorna apenas metadados + contagem de versículos por capítulo.
    ///
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     { "id": "cap-1", "livroId": "livro-genesis", "numero": 1, "titulo": "A criação", "totalVersiculos": 31 },
    ///     { "id": "cap-2", "livroId": "livro-genesis", "numero": 2, "titulo": "O jardim do Éden", "totalVersiculos": 25 }
    ///   ],
    ///   "message": "",
    ///   "errors": []
    /// }
    /// ```
    /// </remarks>
    [HttpGet("{id}/capitulos")]
    [SwaggerOperation(
        Summary = "Sumário dos capítulos de um livro",
        Description = "Retorna id, numero, titulo e totalVersiculos de cada capítulo. Não inclui o texto dos versículos.",
        Tags = new[] { "Livros" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CapituloSumarioResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapitulosSumario(string id, CancellationToken ct)
    {
        // Retorna null quando o livro não existe (lógica de negócio no Service)
        var sumario = await _livroService.GetCapitulosSumarioAsync(id, ct);

        if (sumario is null)
            return NotFound(ApiResponse<object>.Fail("Livro não encontrado."));

        return Ok(ApiResponse<IEnumerable<CapituloSumarioResponseDto>>.Ok(sumario));
    }

    /// <summary>Retorna um capítulo específico com seus versículos.</summary>
    /// <remarks>
    /// Quando autenticado, o campo **isFavorito** em cada versículo reflete o estado real do usuário.
    ///
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "id": "cap-1",
    ///     "livroId": "livro-joao",
    ///     "numero": 1,
    ///     "titulo": "Capítulo 1",
    ///     "versiculos": [
    ///       { "id": "v1", "numero": 1, "texto": "No princípio era o Verbo...", "capituloId": "cap-1", "isFavorito": false }
    ///     ]
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("{livroId}/capitulos/{numero:int}")]
    [SwaggerOperation(Summary = "Capítulo com versículos", Tags = new[] { "Livros" })]
    [ProducesResponseType(typeof(ApiResponse<CapituloResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCapitulo(
        string livroId,
        int numero,
        CancellationToken ct)
    {
        // Extrai userId do token se existir (rota pública mas personalizada)
        var usuarioId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var capitulo = await _livroService.GetCapituloAsync(livroId, numero, usuarioId, ct);
        if (capitulo is null)
            return NotFound(ApiResponse<object>.Fail(
                $"Capítulo {numero} do livro '{livroId}' não encontrado."));

        return Ok(ApiResponse<CapituloResponseDto>.Ok(capitulo));
    }
}
