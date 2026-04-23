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
/// | Método | Rota                           | Auth | Componente Angular     |
/// |--------|--------------------------------|------|------------------------|
/// | POST   | /api/matriculas/{cursoId}      | ✅   | DetalhesCursoComponent |
/// | GET    | /api/matriculas/{cursoId}/check| ✅   | DetalhesCursoComponent |
///
/// O usuário precisa estar autenticado para se inscrever.
/// Os dados do aluno (nome, e-mail) são obtidos do banco via JWT — sem formulário.
/// </summary>
[ApiController]
[Route("api/matriculas")]
[Produces("application/json")]
public class MatriculaController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;
    private readonly IUsuarioService   _usuarioService;

    public MatriculaController(
        IMatriculaService matriculaService,
        IUsuarioService usuarioService)
    {
        _matriculaService = matriculaService;
        _usuarioService   = usuarioService;
    }

    private string UsuarioId => User.GetUserId();

    /// <summary>
    /// Inscreve o aluno autenticado no curso — sem necessidade de formulário.
    /// Os dados do aluno são recuperados automaticamente do banco pelo JWT.
    /// Retorna 409 se o aluno já estiver inscrito neste curso.
    /// </summary>
    [HttpPost("{cursoId}")]
    [Authorize]
    [SwaggerOperation(Summary = "Inscreve aluno autenticado no curso", Tags = new[] { "Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<InscricaoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Inscrever(string cursoId, CancellationToken ct)
    {
        var usuario = await _usuarioService.GetPerfilAsync(UsuarioId, ct);

        var dto = new MatriculaRequestDto(
            NomeCompleto:   usuario.Nome,
            Email:          usuario.Email,
            Telefone:       null,
            Cpf:            null,
            DataNascimento: null,
            Endereco:       null,
            Observacoes:    null,
            AceitaTermos:   true,
            ReceberEmails:  false,
            Senha:          null
        );

        var inscricao = await _matriculaService.InscreverAsync(UsuarioId, cursoId, dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<InscricaoResponseDto>.Ok(inscricao, "Inscrição realizada com sucesso!"));
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
