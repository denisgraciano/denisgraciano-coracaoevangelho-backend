using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Progresso de aulas do aluno — substitui ProgressoService com localStorage.
///
/// Contrato com Angular:
/// | Método | Rota                         | Auth | Componente Angular              |
/// |--------|------------------------------|------|---------------------------------|
/// | GET    | /api/progresso/{cursoId}     | ✅   | DashboardComponent, PlayerAula  |
/// | POST   | /api/progresso/marcar        | ✅   | PlayerAulaComponent             |
///
/// Substitui ProgressoService que persiste em localStorage com chaves:
/// ce_progresso_{userId}_{cursoId}
///
/// Após integrar com a API, o frontend deve:
/// 1. Remover toda lógica de localStorage do ProgressoService
/// 2. Chamar GET /api/progresso/{cursoId} para obter o estado inicial
/// 3. Chamar POST /api/progresso/marcar ao clicar "Marcar como concluída"
/// </summary>
[ApiController]
[Route("api/progresso")]
[Produces("application/json")]
[Authorize]
public class ProgressoController : ControllerBase
{
    private readonly IProgressoService _progressoService;

    public ProgressoController(IProgressoService progressoService) =>
        _progressoService = progressoService;

    private string UsuarioId => User.GetUserId();

    /// <summary>
    /// Retorna o progresso completo do aluno em um curso.
    /// Espelha a interface ProgressoCurso do Angular.
    /// </summary>
    /// <remarks>
    /// Response 200:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": {
    ///     "cursoId": "espiritismo-basico",
    ///     "aulasProgresso": [
    ///       { "aulaId": "aula-01", "concluida": true, "dataConclusao": "2026-03-18T10:00:00Z" },
    ///       { "aulaId": "aula-02", "concluida": false, "dataConclusao": null }
    ///     ],
    ///     "percentualConcluido": 33,
    ///     "dataConclusao": null,
    ///     "certificadoEmitido": false
    ///   }
    /// }
    /// ```
    /// </remarks>
    [HttpGet("{cursoId}")]
    [SwaggerOperation(Summary = "Progresso do aluno no curso", Tags = new[] { "Progresso" })]
    [ProducesResponseType(typeof(ApiResponse<ProgressoCursoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProgresso(string cursoId, CancellationToken ct)
    {
        var progresso = await _progressoService.GetProgressoCursoAsync(UsuarioId, cursoId, ct);
        return Ok(ApiResponse<ProgressoCursoResponseDto>.Ok(progresso));
    }

    /// <summary>
    /// Marca uma aula como concluída e retorna o progresso atualizado do curso.
    ///
    /// Chamado pelo PlayerAulaComponent em dois momentos:
    /// 1. Botão "Marcar como concluída" (marcarConcluida())
    /// 2. Ao avançar para a próxima aula (irParaProximaAula())
    /// </summary>
    /// <remarks>
    /// Request:
    /// ```json
    /// { "cursoId": "espiritismo-basico", "aulaId": "aula-01" }
    /// ```
    /// </remarks>
    [HttpPost("marcar")]
    [SwaggerOperation(Summary = "Marca aula como concluída", Tags = new[] { "Progresso" })]
    [ProducesResponseType(typeof(ApiResponse<ProgressoCursoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarcarConcluida(
        [FromBody] MarcarAulaConcluidaRequestDto dto,
        CancellationToken ct)
    {
        var progresso = await _progressoService.MarcarAulaConcluidaAsync(UsuarioId, dto, ct);
        return Ok(ApiResponse<ProgressoCursoResponseDto>.Ok(progresso));
    }
}
