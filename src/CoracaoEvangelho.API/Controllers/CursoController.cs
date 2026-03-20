using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Catálogo de cursos — rotas públicas e área do aluno.
///
/// Contrato com Angular:
/// | Método | Rota                    | Auth | Componente Angular               |
/// |--------|-------------------------|------|----------------------------------|
/// | GET    | /api/cursos             | ❌   | HomeComponent (cursos destaque)  |
/// | GET    | /api/cursos/{id}        | ❌   | DetalhesCursoComponent           |
/// | GET    | /api/cursos/meus        | ✅   | DashboardComponent (meus cursos) |
/// | GET    | /api/cursos/sugestoes   | ✅   | DashboardComponent (sugestões)   |
///
/// IMPORTANTE: GET /api/cursos/meus e /sugestoes devem ser registrados
/// ANTES de /api/cursos/{id} no roteamento — caso contrário "meus" seria
/// interpretado como um {id}. No ASP.NET Core isso é resolvido
/// automaticamente pela especificidade da rota literal vs. parâmetro.
/// </summary>
[ApiController]
[Route("api/cursos")]
[Produces("application/json")]
public class CursoController : ControllerBase
{
    private readonly ICursoService _cursoService;

    public CursoController(ICursoService cursoService) => _cursoService = cursoService;

    /// <summary>
    /// Lista todos os cursos ativos (sem array de aulas — payload leve para listagem).
    /// Usado pela HomeComponent para exibir os cards de cursos destaque.
    /// </summary>
    /// <remarks>
    /// Response 200:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "id": "espiritismo-basico",
    ///       "titulo": "Fundamentos do Espiritismo",
    ///       "descricao": "...",
    ///       "categoria": "Doutrina",
    ///       "imagemUrl": "assets/images/curso-espiritismo.jpg",
    ///       "instrutor": "Prof. Allan Kardec Jr.",
    ///       "totalAulas": 3,
    ///       "certificadoDisponivel": true
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os cursos ativos", Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CursoResumoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodos(CancellationToken ct)
    {
        var cursos = await _cursoService.GetTodosAsync(ct);
        return Ok(ApiResponse<IEnumerable<CursoResumoResponseDto>>.Ok(cursos));
    }

    /// <summary>
    /// Cursos matriculados do aluno — substitui CURSOS_MOCK no DashboardComponent.
    /// Retorna cursos COM array de aulas (necessário para PlayerAulaComponent).
    /// </summary>
    [HttpGet("meus")]
    [Authorize]
    [SwaggerOperation(Summary = "Cursos em que o aluno está matriculado", Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CursoResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMeus(CancellationToken ct)
    {
        var cursos = await _cursoService.GetCursosMatriculadosAsync(User.GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<CursoResponseDto>>.Ok(cursos));
    }

    /// <summary>
    /// Sugestões de cursos para o aluno — aba "Sugestões para você" do Dashboard.
    /// Retorna até 3 cursos nos quais o aluno ainda não está matriculado.
    /// </summary>
    [HttpGet("sugestoes")]
    [Authorize]
    [SwaggerOperation(Summary = "Sugestões de cursos para o aluno", Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CursoResumoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSugestoes(CancellationToken ct)
    {
        var sugestoes = await _cursoService.GetSugestoesAsync(User.GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<CursoResumoResponseDto>>.Ok(sugestoes));
    }

    /// <summary>
    /// Detalhe completo do curso com array de aulas ordenadas.
    /// Usado pelo DetalhesCursoComponent e pelo PlayerAulaComponent.
    /// </summary>
    /// <remarks>
    /// O campo `youtubeVideoId` retorna APENAS o ID do vídeo (ex: "dQw4w9WgXcQ"),
    /// nunca a URL completa — o frontend constrói o embedUrl via DomSanitizer.
    /// </remarks>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Detalhe do curso com lista de aulas", Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(ApiResponse<CursoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var curso = await _cursoService.GetByIdAsync(id, ct);
        if (curso is null)
            return NotFound(ApiResponse<object>.Fail($"Curso '{id}' não encontrado."));

        return Ok(ApiResponse<CursoResponseDto>.Ok(curso));
    }
}
