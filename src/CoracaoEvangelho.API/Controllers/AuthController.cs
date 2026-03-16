using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using CoracaoEvangelho.API.Extensions;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Autenticação e gerenciamento de sessão.
/// Rotas públicas — não requerem [Authorize].
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Registra um novo usuário.</summary>
    /// <remarks>
    /// Contrato: POST /api/auth/register
    /// | Campo  | Tipo   | Obrigatório |
    /// |--------|--------|-------------|
    /// | nome   | string | sim         |
    /// | email  | string | sim         |
    /// | senha  | string | sim (≥8 chars, 1 maiúscula, 1 número) |
    /// </remarks>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Registra novo usuário", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto dto,
        CancellationToken ct)
    {
        var result = await _authService.RegistrarAsync(dto, ct);
        return CreatedAtAction(nameof(Register), ApiResponse<AuthResponseDto>.Ok(result, "Usuário registrado com sucesso."));
    }

    /// <summary>Autentica o usuário e retorna JWT + RefreshToken.</summary>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login do usuário", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login realizado com sucesso."));
    }

    /// <summary>Renova o AccessToken usando o RefreshToken.</summary>
    [HttpPost("refresh")]
    [SwaggerOperation(Summary = "Renova AccessToken", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto dto,
        CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token renovado com sucesso."));
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado.
    /// Usado pelo Angular após refresh de token para re-hidratar nome e e-mail
    /// no header/menu sem forçar novo login.
    /// </summary>
    /// <remarks>
    /// Requer header: Authorization: Bearer {accessToken}
    ///
    /// Exemplo de resposta:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "id": "u1",
    ///     "nome": "João Silva",
    ///     "email": "joao@email.com",
    ///     "role": "user"
    ///   },
    ///   "message": "",
    ///   "errors": []
    /// }
    /// ```
    ///
    /// Contrato com Angular:
    /// | Método | Rota          | Auth | Service Angular |
    /// |--------|---------------|------|-----------------|
    /// | GET    | /api/auth/me  | ✅   | AuthService     |
    /// </remarks>
    [HttpGet("me")]
    [Authorize]  // ← JWT obrigatório: token ausente/inválido → 401 automaticamente
    [SwaggerOperation(
        Summary = "Dados do usuário autenticado",
        Description = "Re-hidrata nome e e-mail após refresh de token. Sempre retorna dados atuais do banco.",
        Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        // GetUserIdOrThrow() garante que o userId existe na claim — nunca null aqui
        // porque [Authorize] já bloqueou o request antes de chegar nesta linha
        var usuarioId = User.GetUserIdOrThrow();

        var usuario = await _authService.GetMeAsync(usuarioId, ct);
        return Ok(ApiResponse<UsuarioResponseDto>.Ok(usuario));
    }
}
