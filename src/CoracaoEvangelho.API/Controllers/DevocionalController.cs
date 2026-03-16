using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Devocional diário e histórico.
/// Rota pública — autenticação opcional (enriquece isFavorito).
///
/// Contrato com Angular:
/// | Método | Rota                      | Service Angular      |
/// |--------|---------------------------|----------------------|
/// | GET    | /api/devocional/hoje      | DevocionalService    |
/// | GET    | /api/devocional/historico | DevocionalService    |
/// </summary>
[ApiController]
[Route("api/devocional")]
[Produces("application/json")]
public class DevocionalController : ControllerBase
{
    private readonly IDevocionalService _devocionalService;

    public DevocionalController(IDevocionalService devocionalService)
        => _devocionalService = devocionalService;

    /// <summary>Retorna o devocional do dia atual.</summary>
    /// <remarks>
    /// Muda automaticamente à meia-noite (UTC).
    ///
    /// Resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "id": "dev-01",
    ///     "data": "2026-03-15",
    ///     "passagem": "João 3:16",
    ///     "reflexao": "Deus amou o mundo de tal maneira...",
    ///     "versiculo": { "id": "v1", "numero": 16, "texto": "...", "capituloId": "c1", "isFavorito": false }
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("hoje")]
    [SwaggerOperation(Summary = "Devocional do dia", Tags = new[] { "Devocional" })]
    [ProducesResponseType(typeof(ApiResponse<DevocionalResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHoje(CancellationToken ct)
    {
        var usuarioId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var devocional = await _devocionalService.GetHojeAsync(usuarioId, ct);

        if (devocional is null)
            return NotFound(ApiResponse<object>.Fail("Nenhum devocional cadastrado para hoje."));

        return Ok(ApiResponse<DevocionalResponseDto>.Ok(devocional));
    }

    /// <summary>Retorna o histórico paginado de devocionais (últimos 30 dias por padrão).</summary>
    [HttpGet("historico")]
    [SwaggerOperation(Summary = "Histórico de devocionais", Tags = new[] { "Devocional" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<DevocionalResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorico(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1 || tamanhoPagina > 30) tamanhoPagina = 10;

        var usuarioId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var resultado = await _devocionalService.GetHistoricoAsync(pagina, tamanhoPagina, usuarioId, ct);

        return Ok(ApiResponse<PagedResultDto<DevocionalResponseDto>>.Ok(resultado));
    }
}
