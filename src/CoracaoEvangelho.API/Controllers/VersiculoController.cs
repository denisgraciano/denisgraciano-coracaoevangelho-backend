using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Pesquisa full-text de versículos.
///
/// Contrato com Angular:
/// | Método | Rota                            | Service Angular              |
/// |--------|---------------------------------|------------------------------|
/// | GET    | /api/versiculos/pesquisar?termo= | LivroService.pesquisar(termo) |
/// </summary>
[ApiController]
[Route("api/versiculos")]
[Produces("application/json")]
public class VersiculoController : ControllerBase
{
    private readonly IVersiculoService _versiculoService;

    public VersiculoController(IVersiculoService versiculoService)
        => _versiculoService = versiculoService;

    /// <summary>Pesquisa versículos por palavra-chave.</summary>
    /// <remarks>
    /// - Busca case-insensitive (depende do collation MySQL — use utf8mb4_general_ci)
    /// - Paginação obrigatória (padrão: página 1, 20 itens)
    /// - Filtro opcional por livro (?livroId=xxx)
    ///
    /// Exemplo: GET /api/versiculos/pesquisar?termo=amor&pagina=1&tamanhoPagina=20
    ///
    /// Resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "items": [{ "id": "v1", "numero": 3, "texto": "...", "capituloId": "c1", "isFavorito": false }],
    ///     "totalItens": 42,
    ///     "pagina": 1,
    ///     "tamanhoPagina": 20,
    ///     "totalPaginas": 3,
    ///     "temProxima": true,
    ///     "temAnterior": false
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("pesquisar")]
    [SwaggerOperation(Summary = "Pesquisa versículos por palavra-chave", Tags = new[] { "Versículos" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<VersiculoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pesquisar(
        [FromQuery] string termo,
        [FromQuery] string? livroId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return BadRequest(ApiResponse<object>.Fail("O parâmetro 'termo' é obrigatório."));

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 20;

        var usuarioId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var resultado = await _versiculoService.PesquisarAsync(
            termo, livroId, pagina, tamanhoPagina, usuarioId, ct);

        return Ok(ApiResponse<PagedResultDto<VersiculoResponseDto>>.Ok(resultado));
    }
}
