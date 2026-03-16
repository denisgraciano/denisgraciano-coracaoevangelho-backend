using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Gerenciamento de versículos favoritos do usuário autenticado.
/// TODAS as rotas requerem autenticação JWT.
///
/// Contrato com Angular:
/// | Método | Rota                  | Service Angular              |
/// |--------|-----------------------|------------------------------|
/// | GET    | /api/favoritos        | FavoritosService.getFavoritos() |
/// | POST   | /api/favoritos        | FavoritosService.adicionar()    |
/// | DELETE | /api/favoritos/{id}   | FavoritosService.remover()      |
/// </summary>
[ApiController]
[Route("api/favoritos")]
[Authorize]                      // ← todas as rotas protegidas
[Produces("application/json")]
public class FavoritosController : ControllerBase
{
    private readonly IFavoritoService _favoritoService;

    public FavoritosController(IFavoritoService favoritoService)
        => _favoritoService = favoritoService;

    /// <summary>Lista todos os favoritos do usuário autenticado.</summary>
    /// <remarks>
    /// Requer header: Authorization: Bearer {token}
    ///
    /// Resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "id": "fav-1",
    ///       "versiculoId": "v1",
    ///       "dataSalvo": "2026-03-15T10:00:00Z",
    ///       "versiculo": { "id": "v1", "numero": 16, "texto": "...", "capituloId": "c1", "isFavorito": true }
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista favoritos do usuário", Tags = new[] { "Favoritos" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FavoritoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFavoritos(CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        var favoritos = await _favoritoService.GetFavoritosAsync(usuarioId, ct);
        return Ok(ApiResponse<IEnumerable<FavoritoResponseDto>>.Ok(favoritos));
    }

    /// <summary>Adiciona um versículo aos favoritos (idempotente).</summary>
    /// <remarks>
    /// Body:
    /// ```json
    /// { "versiculoId": "abc-123" }
    /// ```
    ///
    /// Se o versículo já for favorito, retorna 201 com o favorito existente (sem duplicar).
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(Summary = "Adiciona versículo aos favoritos", Tags = new[] { "Favoritos" })]
    [ProducesResponseType(typeof(ApiResponse<FavoritoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Adicionar(
        [FromBody] AdicionarFavoritoRequestDto dto,
        CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        var favorito = await _favoritoService.AdicionarAsync(usuarioId, dto, ct);
        return CreatedAtAction(nameof(GetFavoritos),
            ApiResponse<FavoritoResponseDto>.Ok(favorito, "Adicionado aos favoritos."));
    }

    /// <summary>Remove um favorito pelo ID.</summary>
    /// <remarks>
    /// Retorna 204 NoContent em caso de sucesso.
    /// Retorna 403 se o favorito pertencer a outro usuário.
    /// </remarks>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove versículo dos favoritos", Tags = new[] { "Favoritos" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remover(string id, CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        await _favoritoService.RemoverAsync(usuarioId, id, ct);
        return NoContent();
    }
}
