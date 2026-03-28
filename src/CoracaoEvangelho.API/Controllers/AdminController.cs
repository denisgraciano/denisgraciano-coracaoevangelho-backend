using CoracaoEvangelho.API.Constants;
using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Painel administrativo — todas as rotas exigem role "admin".
///
/// | Método | Rota                                          | Descrição                       |
/// |--------|-----------------------------------------------|---------------------------------|
/// | GET    | /api/admin/usuarios                           | Lista usuários (paginado)       |
/// | PUT    | /api/admin/usuarios/{id}                      | Atualiza dados do usuário       |
/// | PUT    | /api/admin/usuarios/{id}/status               | Habilita / desabilita usuário   |
/// | GET    | /api/admin/pedidos-vibracao                   | Lista pedidos (paginado)        |
/// | PUT    | /api/admin/pedidos-vibracao/{id}/lido         | Marca pedido como lido          |
/// | GET    | /api/admin/cursos                             | Lista todos os cursos (admin)   |
/// | POST   | /api/admin/cursos                             | Cria novo curso                 |
/// | PUT    | /api/admin/cursos/{id}                        | Atualiza curso                  |
/// | DELETE | /api/admin/cursos/{id}                        | Remove curso (soft delete)      |
/// | POST   | /api/admin/cursos/{cursoId}/aulas             | Adiciona aula ao curso          |
/// | PUT    | /api/admin/cursos/{cursoId}/aulas/{aulaId}    | Atualiza aula                   |
/// | DELETE | /api/admin/cursos/{cursoId}/aulas/{aulaId}    | Remove aula (soft delete)       |
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    // ── Usuários ──────────────────────────────────────────────────────────

    [HttpGet("usuarios")]
    [SwaggerOperation(Summary = "Lista todos os usuários (paginado)", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<UsuarioAdminDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarUsuarios(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var resultado = await _adminService.ListarUsuariosAsync(pagina, tamanho, ct);
        return Ok(ApiResponse<PagedResultDto<UsuarioAdminDto>>.Ok(resultado));
    }

    [HttpPut("usuarios/{id}")]
    [SwaggerOperation(Summary = "Atualiza dados de um usuário (admin)", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<UsuarioAdminDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarUsuario(
        string id,
        [FromBody] AtualizarUsuarioAdminRequestDto dto,
        CancellationToken ct = default)
    {
        var usuario = await _adminService.AtualizarUsuarioAdminAsync(id, dto, ct);
        return Ok(ApiResponse<UsuarioAdminDto>.Ok(usuario, "Usuário atualizado com sucesso."));
    }

    [HttpPut("usuarios/{id}/status")]
    [SwaggerOperation(Summary = "Habilita ou desabilita um usuário", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AlterarStatusUsuario(
        string id,
        [FromBody] AlterarStatusUsuarioRequestDto dto,
        CancellationToken ct = default)
    {
        await _adminService.AlterarStatusUsuarioAsync(id, dto.Ativo, ct);
        var mensagem = dto.Ativo ? "Usuário habilitado com sucesso." : "Usuário desabilitado com sucesso.";
        return Ok(ApiResponse<object>.Ok(new { }, mensagem));
    }

    // ── Pedidos de Vibração ───────────────────────────────────────────────

    [HttpGet("pedidos-vibracao")]
    [SwaggerOperation(Summary = "Lista pedidos de vibração (paginado)", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<PedidoVibracaoAdminDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarPedidosVibracao(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        var resultado = await _adminService.ListarPedidosVibracaoAsync(pagina, tamanho, ct);
        return Ok(ApiResponse<PagedResultDto<PedidoVibracaoAdminDto>>.Ok(resultado));
    }

    [HttpPatch("pedidos-vibracao/{id}/lido")]
    [SwaggerOperation(Summary = "Marca pedido de vibração como lido", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarcarPedidoLido(string id, CancellationToken ct = default)
    {
        await _adminService.MarcarPedidoLidoAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { }, "Pedido marcado como lido."));
    }

    // ── Cursos ────────────────────────────────────────────────────────────

    [HttpGet("cursos")]
    [SwaggerOperation(Summary = "Lista todos os cursos incluindo inativos", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CursoAdminResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListarCursos(CancellationToken ct = default)
    {
        var cursos = await _adminService.ListarCursosAsync(ct);
        return Ok(ApiResponse<IEnumerable<CursoAdminResponseDto>>.Ok(cursos));
    }

    [HttpPost("cursos")]
    [SwaggerOperation(Summary = "Cria um novo curso", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<CursoAdminResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CriarCurso(
        [FromBody] CursoRequestDto dto,
        CancellationToken ct = default)
    {
        var curso = await _adminService.CriarCursoAsync(dto, ct);
        return Created($"/api/admin/cursos/{curso.Id}",
            ApiResponse<CursoAdminResponseDto>.Ok(curso, "Curso criado com sucesso."));
    }

    [HttpPut("cursos/{id}")]
    [SwaggerOperation(Summary = "Atualiza dados de um curso", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<CursoAdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarCurso(
        string id,
        [FromBody] CursoRequestDto dto,
        CancellationToken ct = default)
    {
        var curso = await _adminService.AtualizarCursoAsync(id, dto, ct);
        return Ok(ApiResponse<CursoAdminResponseDto>.Ok(curso, "Curso atualizado com sucesso."));
    }

    [HttpDelete("cursos/{id}")]
    [SwaggerOperation(Summary = "Remove um curso (soft delete)", Tags = new[] { "Admin" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoverCurso(string id, CancellationToken ct = default)
    {
        await _adminService.RemoverCursoAsync(id, ct);
        return NoContent();
    }

    // ── Aulas ─────────────────────────────────────────────────────────────

    [HttpPost("cursos/{cursoId}/aulas")]
    [SwaggerOperation(Summary = "Adiciona uma aula ao curso", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<AulaAdminResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AdicionarAula(
        string cursoId,
        [FromBody] AulaRequestDto dto,
        CancellationToken ct = default)
    {
        var aula = await _adminService.AdicionarAulaAsync(cursoId, dto, ct);
        return Created($"/api/admin/cursos/{cursoId}/aulas/{aula.Id}",
            ApiResponse<AulaAdminResponseDto>.Ok(aula, "Aula adicionada com sucesso."));
    }

    [HttpPut("cursos/{cursoId}/aulas/{aulaId}")]
    [SwaggerOperation(Summary = "Atualiza dados de uma aula", Tags = new[] { "Admin" })]
    [ProducesResponseType(typeof(ApiResponse<AulaAdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarAula(
        string cursoId,
        string aulaId,
        [FromBody] AulaRequestDto dto,
        CancellationToken ct = default)
    {
        var aula = await _adminService.AtualizarAulaAsync(cursoId, aulaId, dto, ct);
        return Ok(ApiResponse<AulaAdminResponseDto>.Ok(aula, "Aula atualizada com sucesso."));
    }

    [HttpDelete("cursos/{cursoId}/aulas/{aulaId}")]
    [SwaggerOperation(Summary = "Remove uma aula (soft delete)", Tags = new[] { "Admin" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoverAula(
        string cursoId,
        string aulaId,
        CancellationToken ct = default)
    {
        await _adminService.RemoverAulaAsync(cursoId, aulaId, ct);
        return NoContent();
    }
}
