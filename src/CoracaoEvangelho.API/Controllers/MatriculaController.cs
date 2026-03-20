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
/// | Método | Rota                           | Auth | Componente Angular         |
/// |--------|--------------------------------|------|----------------------------|
/// | POST   | /api/matriculas/{cursoId}      | ✅   | InscricaoCursoComponent    |
/// | GET    | /api/matriculas/{cursoId}/check| ✅   | DetalhesCursoComponent     |
///
/// Substitui InscricaoService.inscrever() que hoje chama /inscricoes.
/// O frontend usa: POST /inscricoes → deve ser atualizado para POST /api/matriculas/{cursoId}
/// </summary>
[ApiController]
[Route("api/matriculas")]
[Produces("application/json")]
[Authorize]
public class MatriculaController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;

    public MatriculaController(IMatriculaService matriculaService) =>
        _matriculaService = matriculaService;

    private string UsuarioId => User.GetUserId();

    /// <summary>
    /// Inscreve o aluno autenticado no curso.
    /// Retorna 409 se o aluno já estiver matriculado (sem duplicata).
    /// </summary>
    /// <remarks>
    /// Request body (campos do formulário InscricaoCursoComponent):
    /// ```json
    /// {
    ///   "nomeCompleto": "João Silva",
    ///   "email": "joao@email.com",
    ///   "telefone": null,
    ///   "endereco": { "cep": "01310-100", "logradouro": "Av. Paulista", "cidade": "São Paulo", "estado": "SP" }
    /// }
    /// ```
    /// Response 201:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": { "id": "...", "cursoId": "espiritismo-basico", "cursoTitulo": "Fundamentos...", "dataMatricula": "2026-03-18T...", "ativa": true },
    ///   "message": "Inscrição realizada com sucesso!"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("{cursoId}")]
    [SwaggerOperation(Summary = "Inscreve aluno no curso", Tags = new[] { "Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<MatriculaResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inscrever(
        string cursoId,
        [FromBody] MatriculaRequestDto dto,
        CancellationToken ct)
    {
        var matricula = await _matriculaService.InscreverAsync(UsuarioId, cursoId, dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<MatriculaResponseDto>.Ok(matricula, "Inscrição realizada com sucesso!"));
    }

    /// <summary>
    /// Verifica se o aluno já está matriculado no curso.
    /// Usado pelo DetalhesCursoComponent para mostrar/ocultar botão de inscrição.
    /// </summary>
    [HttpGet("{cursoId}/check")]
    [SwaggerOperation(Summary = "Verifica se aluno está matriculado", Tags = new[] { "Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Check(string cursoId, CancellationToken ct)
    {
        var estaMatriculado = await _matriculaService.EstaMatriculadoAsync(UsuarioId, cursoId, ct);
        return Ok(ApiResponse<bool>.Ok(estaMatriculado));
    }
}
