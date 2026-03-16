using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Preferências do usuário: tema e tamanho de fonte.
/// TODAS as rotas requerem autenticação JWT.
///
/// Contrato com Angular:
/// | Método | Rota                         | Service Angular              |
/// |--------|------------------------------|------------------------------|
/// | GET    | /api/configuracoes           | ConfiguracaoService.getTema() + getFonte() |
/// | PUT    | /api/configuracoes/tema      | ConfiguracaoService.setTema() |
/// | PUT    | /api/configuracoes/fonte     | ConfiguracaoService.setFonte() |
/// </summary>
[ApiController]
[Route("api/configuracoes")]
[Authorize]
[Produces("application/json")]
public class ConfiguracaoController : ControllerBase
{
    private readonly IConfiguracaoService _configuracaoService;

    public ConfiguracaoController(IConfiguracaoService configuracaoService)
        => _configuracaoService = configuracaoService;

    /// <summary>Retorna as configurações atuais do usuário.</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Configurações do usuário", Tags = new[] { "Configurações" })]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConfiguracao(CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        var config = await _configuracaoService.GetConfiguracaoAsync(usuarioId, ct);
        return Ok(ApiResponse<ConfiguracaoResponseDto>.Ok(config));
    }

    /// <summary>Atualiza o tema da interface (claro/escuro).</summary>
    /// <remarks>
    /// Body: `{ "tema": "escuro" }`
    /// Valores aceitos: "claro" | "escuro"
    /// </remarks>
    [HttpPut("tema")]
    [SwaggerOperation(Summary = "Atualiza tema", Tags = new[] { "Configurações" })]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetTema([FromBody] SetTemaRequestDto dto, CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        var config = await _configuracaoService.SetTemaAsync(usuarioId, dto.Tema, ct);
        return Ok(ApiResponse<ConfiguracaoResponseDto>.Ok(config, "Tema atualizado."));
    }

    /// <summary>Atualiza o tamanho de fonte (12–28px, incremento de 2px).</summary>
    /// <remarks>
    /// Body: `{ "tamanhoFonte": 18 }`
    /// Faixa aceita: 12 a 28 (inteiros pares recomendados)
    /// </remarks>
    [HttpPut("fonte")]
    [SwaggerOperation(Summary = "Atualiza tamanho de fonte", Tags = new[] { "Configurações" })]
    [ProducesResponseType(typeof(ApiResponse<ConfiguracaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetFonte([FromBody] SetFonteRequestDto dto, CancellationToken ct)
    {
        var usuarioId = User.GetUserIdOrThrow();
        var config = await _configuracaoService.SetFonteAsync(usuarioId, dto.TamanhoFonte, ct);
        return Ok(ApiResponse<ConfiguracaoResponseDto>.Ok(config, "Tamanho de fonte atualizado."));
    }
}
