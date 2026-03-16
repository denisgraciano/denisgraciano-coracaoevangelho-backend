using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Sincronização offline para suporte PWA.
/// Retorna apenas registros alterados após uma data — economiza banda.
///
/// Contrato:
/// | Método | Rota                                   | Descrição                 |
/// |--------|----------------------------------------|---------------------------|
/// | GET    | /api/sync/livros?atualizadoApos=...    | Livros alterados/deletados |
/// </summary>
[ApiController]
[Route("api/sync")]
[Produces("application/json")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService) => _syncService = syncService;

    /// <summary>Retorna livros atualizados/deletados após a data informada.</summary>
    /// <remarks>
    /// - Inclui campo `deletado: true` para remoções (soft delete)
    /// - Suporta ETag e Last-Modified para cache HTTP
    /// - Se omitir `atualizadoApos`, retorna todos os registros
    ///
    /// Exemplo: GET /api/sync/livros?atualizadoApos=2026-01-01T00:00:00Z
    ///
    /// Resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     { "id": "l1", "titulo": "...", "subtitulo": "...", "capa": "...", "deletado": false, "atualizadoEm": "2026-03-10T..." }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    [HttpGet("livros")]
    [SwaggerOperation(Summary = "Sincronização de livros (PWA offline)", Tags = new[] { "Sync" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncLivroDto>>), StatusCodes.Status200OK)]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "atualizadoApos" })]
    public async Task<IActionResult> SincronizarLivros(
        [FromQuery] DateTime? atualizadoApos,
        CancellationToken ct)
    {
        // Se não informar data, retorna todo o catálogo (primeira sincronização)
        var data = atualizadoApos ?? DateTime.MinValue;

        var livros = await _syncService.GetLivrosSincronizadosAsync(data, ct);

        // ETag baseado no timestamp mais recente dos resultados
        var etag = $"\"{data:yyyyMMddHHmmss}\"";
        Response.Headers.ETag = etag;
        Response.Headers["Last-Modified"] = DateTime.UtcNow.ToString("R");

        return Ok(ApiResponse<IEnumerable<SyncLivroDto>>.Ok(livros));
    }
}
