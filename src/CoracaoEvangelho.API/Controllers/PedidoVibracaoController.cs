using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
// Nota: PedidoVibracaoAdminDto está em DTOs.Response — sem conflito com Models.PedidoVibracao

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Pedidos de vibrações / orações.
///
/// Contrato com Angular:
/// | Método | Rota                    | Auth          | Componente Angular         |
/// |--------|-------------------------|---------------|----------------------------|
/// | POST   | /api/pedidos-vibracao   | ❌ (opcional) | PedidoVibracoesComponent   |
/// | GET    | /api/pedidos-vibracao   | ✅ admin      | Painel administrativo      |
///
/// O envio é público — aluno logado ou anônimo pode enviar.
/// Se autenticado, o usuarioId é associado ao pedido automaticamente.
/// </summary>
[ApiController]
[Route("api/pedidos-vibracao")]
[Produces("application/json")]
public class PedidoVibracaoController : ControllerBase
{
    private readonly IPedidoVibracaoService _pedidoService;

    public PedidoVibracaoController(IPedidoVibracaoService pedidoService) =>
        _pedidoService = pedidoService;

    /// <summary>
    /// Envia pedido de vibrações — rota pública, auth opcional.
    /// Se o usuário estiver logado, o pedido é associado à conta dele.
    /// </summary>
    /// <remarks>
    /// Request (espelha formulário PedidoVibracoesComponent):
    /// ```json
    /// {
    ///   "nome": "Maria Oliveira",
    ///   "email": "maria@email.com",
    ///   "pedido": "Peço vibrações de cura para minha mãe...",
    ///   "endereco": {
    ///     "cep": "01310-100",
    ///     "logradouro": "Av. Paulista, 1000",
    ///     "cidade": "São Paulo",
    ///     "estado": "SP"
    ///   }
    /// }
    /// ```
    /// Response 201:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "id": "abc-123",
    ///     "mensagem": "Seu pedido foi recebido com muito carinho e será incluído em nossas orações."
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpPost]
    [SwaggerOperation(Summary = "Envia pedido de vibrações (público)", Tags = new[] { "Pedidos" })]
    [ProducesResponseType(typeof(ApiResponse<PedidoVibracaoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enviar(
        [FromBody] PedidoVibracaoRequestDto dto,
        CancellationToken ct)
    {
        // Se autenticado, associa ao userId — se anônimo, null
        var usuarioId = User.Identity?.IsAuthenticated == true
            ? User.GetUserId()
            : null;

        var result = await _pedidoService.EnviarAsync(usuarioId, dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<PedidoVibracaoResponseDto>.Ok(result));
    }

    /// <summary>Lista todos os pedidos — exclusivo para admin (paginado).</summary>
    [HttpGet]
    [Authorize(Roles = "admin")]
    [SwaggerOperation(Summary = "Lista pedidos de vibrações (admin)", Tags = new[] { "Pedidos" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<PedidoVibracaoAdminDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho is < 1 or > 50) tamanho = 20;

        var result = await _pedidoService.ListarAsync(pagina, tamanho, ct);
        return Ok(ApiResponse<PagedResultDto<PedidoVibracaoAdminDto>>.Ok(result));
    }
}
