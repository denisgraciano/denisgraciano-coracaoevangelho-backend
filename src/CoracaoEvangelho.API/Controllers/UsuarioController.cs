using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Perfil e gerenciamento de conta do aluno autenticado.
///
/// Contrato com Angular:
/// | Método | Rota                  | Auth | Service Angular                      |
/// |--------|-----------------------|------|--------------------------------------|
/// | GET    | /api/usuario/perfil   | ✅   | AuthService.usuario$ (hidrata dados) |
/// | PUT    | /api/usuario/perfil   | ✅   | Futura tela de edição de perfil      |
/// | PUT    | /api/usuario/senha    | ✅   | Futura tela de troca de senha        |
///
/// Campos expostos espelham interface Usuario do Angular:
/// { id, nome, email, avatarUrl? }
/// SenhaHash: NUNCA exposta.
/// </summary>
[ApiController]
[Route("api/usuario")]
[Produces("application/json")]
[Authorize]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService) =>
        _usuarioService = usuarioService;

    // Helper tipado — lança 401 se claim ausente
    private string UsuarioId => User.GetUserId();

    /// <summary>Retorna o perfil do aluno logado.</summary>
    /// <remarks>
    /// Usado pelo DashboardComponent para exibir nome e avatar:
    /// ```html
    /// {{ usuario?.nome }} — [src]="usuario?.avatarUrl"
    /// ```
    /// Response 200:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": { "id": "abc", "nome": "João Silva", "email": "joao@email.com", "avatarUrl": null }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("perfil")]
    [SwaggerOperation(Summary = "Perfil do aluno logado", Tags = new[] { "Usuário" })]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPerfil(CancellationToken ct)
    {
        var perfil = await _usuarioService.GetPerfilAsync(UsuarioId, ct);
        return Ok(ApiResponse<UsuarioResponseDto>.Ok(perfil));
    }

    /// <summary>Atualiza nome e URL do avatar do aluno.</summary>
    /// <remarks>
    /// Request:
    /// ```json
    /// { "nome": "João da Silva", "avatarUrl": "https://cdn.example.com/foto.jpg" }
    /// ```
    /// </remarks>
    [HttpPut("perfil")]
    [SwaggerOperation(Summary = "Atualiza perfil do aluno", Tags = new[] { "Usuário" })]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AtualizarPerfil(
        [FromBody] AtualizarPerfilRequestDto dto, CancellationToken ct)
    {
        var perfil = await _usuarioService.AtualizarPerfilAsync(UsuarioId, dto, ct);
        return Ok(ApiResponse<UsuarioResponseDto>.Ok(perfil, "Perfil atualizado com sucesso."));
    }

    /// <summary>Altera a senha do aluno autenticado.</summary>
    /// <remarks>
    /// Request:
    /// ```json
    /// { "senhaAtual": "SenhaVelha1", "novaSenha": "SenhaNova2" }
    /// ```
    /// </remarks>
    [HttpPut("senha")]
    [SwaggerOperation(Summary = "Altera senha do aluno", Tags = new[] { "Usuário" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha(
        [FromBody] AlterarSenhaRequestDto dto, CancellationToken ct)
    {
        await _usuarioService.AlterarSenhaAsync(UsuarioId, dto, ct);
        return NoContent();
    }
}
