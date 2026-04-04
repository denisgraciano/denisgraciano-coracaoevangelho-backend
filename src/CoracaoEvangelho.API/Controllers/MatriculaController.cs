using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Inscrição de alunos em cursos.
///
/// Contrato com Angular:
/// | Método | Rota                           | Auth    | Componente Angular         |
/// |--------|--------------------------------|---------|----------------------------|
/// | POST   | /api/matriculas/{cursoId}      | pública | InscricaoCursoComponent    |
/// | GET    | /api/matriculas/{cursoId}/check| ✅      | DetalhesCursoComponent     |
///
/// POST é público: qualquer visitante pode se inscrever sem conta.
/// O userId é vinculado automaticamente quando o usuário estiver logado.
/// </summary>
[ApiController]
[Route("api/matriculas")]
[Produces("application/json")]
public class MatriculaController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;

    public MatriculaController(IMatriculaService matriculaService) =>
        _matriculaService = matriculaService;

    // Retorna null para requisições anônimas (sem JWT válido)
    private string? UsuarioIdOuNulo =>
        User.Identity?.IsAuthenticated == true
            ? User.FindFirst("userId")?.Value
            : null;

    // Usado apenas em rotas com [Authorize], onde o claim é garantido
    private string UsuarioId => User.GetUserId();

    /// <summary>
    /// Inscreve o visitante no curso — rota pública, sem necessidade de login.
    /// Retorna 409 se o mesmo e-mail já estiver inscrito neste curso.
    /// </summary>
    [HttpPost("{cursoId}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Inscreve aluno no curso (público)", Tags = new[] { "Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<MatriculaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inscrever(
        string cursoId,
        [FromBody] MatriculaRequestDto dto,
        CancellationToken ct)
    {
        var matricula = await _matriculaService.InscreverAsync(UsuarioIdOuNulo, cursoId, dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<MatriculaResponseDto>.Ok(matricula, "Inscrição realizada com sucesso!"));
    }

    /// <summary>
    /// Verifica se o aluno autenticado já está matriculado no curso.
    /// Usado pelo DetalhesCursoComponent para mostrar/ocultar o botão "Inscrever-se".
    /// </summary>
    [HttpGet("{cursoId}/check")]
    [Authorize]
    [SwaggerOperation(Summary = "Verifica se aluno está matriculado", Tags = new[] { "Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<MatriculaCheckResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Check(string cursoId, CancellationToken ct)
    {
        var estaMatriculado = await _matriculaService.EstaMatriculadoAsync(UsuarioId, cursoId, ct);
        return Ok(ApiResponse<MatriculaCheckResponseDto>.Ok(new MatriculaCheckResponseDto(estaMatriculado)));
    }
}
