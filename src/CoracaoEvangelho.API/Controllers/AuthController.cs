using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Autenticação JWT — registro, login e refresh de token.
///
/// Contrato com Angular:
/// | Método | Rota                | Auth | Service Angular              |
/// |--------|---------------------|------|------------------------------|
/// | POST   | /api/auth/register  | ❌   | AuthService.register()       |
/// | POST   | /api/auth/login     | ❌   | AuthService.login()          |
/// | POST   | /api/auth/refresh   | ❌   | AuthService.refresh()        |
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Registra um novo aluno na plataforma.</summary>
    /// <remarks>
    /// Request:
    /// ```json
    /// { "nome": "João Silva", "email": "joao@email.com", "senha": "Senha123" }
    /// ```
    /// Response 201:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "accessToken": "eyJ...",
    ///     "expira": "2026-03-18T02:00:00Z",
    ///     "refreshToken": "abc123...",
    ///     "usuario": { "id": "...", "nome": "João Silva", "email": "joao@email.com", "avatarUrl": null }
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpPost("register")]
    [SwaggerOperation(Summary = "Registra novo aluno", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.RegistrarAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponseDto>.Ok(result, "Usuário registrado com sucesso."));
    }

    /// <summary>Autentica o aluno e retorna JWT + RefreshToken.</summary>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login → JWT + RefreshToken", Tags = new[] { "Auth" })]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto dto, CancellationToken ct)
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
        [FromBody] RefreshTokenRequestDto dto, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ct);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token renovado."));
    }
}
