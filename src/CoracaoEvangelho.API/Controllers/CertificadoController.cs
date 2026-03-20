using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Extensions;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Certificados de conclusão de cursos.
///
/// Contrato com Angular:
/// | Método | Rota                           | Auth | Componente Angular                     |
/// |--------|--------------------------------|------|----------------------------------------|
/// | GET    | /api/certificados              | ✅   | DashboardComponent (aba Certificados)  |
/// | GET    | /api/certificados/{cursoId}    | ✅   | CertificadoComponent                   |
/// | POST   | /api/certificados/emitir       | ✅   | CertificadoComponent (emitir)          |
///
/// Substitui ProgressoService.emitirCertificado() e listarCertificados()
/// que hoje usam localStorage com chave: ce_certificados
///
/// A emissão valida que 100% das aulas foram concluídas antes de prosseguir.
/// Se o certificado já existe, retorna o existente (idempotente).
///
/// Campos do response espelham interface Certificado do Angular:
/// { id, cursoId, cursoTitulo, alunoNome, dataEmissao, cargaHoraria }
/// O frontend usa: {{ cert.dataEmissao | date:'dd/MM/yyyy' }}
/// A API retorna dataEmissao como ISO string — o pipe Angular formata corretamente.
/// </summary>
[ApiController]
[Route("api/certificados")]
[Produces("application/json")]
[Authorize]
public class CertificadoController : ControllerBase
{
    private readonly ICertificadoService _certService;

    public CertificadoController(ICertificadoService certService) =>
        _certService = certService;

    private string UsuarioId => User.GetUserId();

    /// <summary>
    /// Lista todos os certificados do aluno — aba "Certificados" do Dashboard.
    /// </summary>
    /// <remarks>
    /// Response 200:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": [
    ///     {
    ///       "id": "cert-01",
    ///       "cursoId": "espiritismo-basico",
    ///       "cursoTitulo": "Fundamentos do Espiritismo",
    ///       "alunoNome": "João Silva",
    ///       "dataEmissao": "2026-03-18T15:00:00.0000000Z",
    ///       "cargaHoraria": 1.4
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista certificados do aluno", Tags = new[] { "Certificados" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CertificadoResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodos(CancellationToken ct)
    {
        var certs = await _certService.GetByUsuarioAsync(UsuarioId, ct);
        return Ok(ApiResponse<IEnumerable<CertificadoResponseDto>>.Ok(certs));
    }

    /// <summary>
    /// Retorna o certificado de um curso específico — CertificadoComponent.
    /// Rota: /area-aluno/certificado/:cursoId
    /// </summary>
    [HttpGet("{cursoId}")]
    [SwaggerOperation(Summary = "Certificado de um curso específico", Tags = new[] { "Certificados" })]
    [ProducesResponseType(typeof(ApiResponse<CertificadoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCurso(string cursoId, CancellationToken ct)
    {
        var cert = await _certService.GetByUsuarioCursoAsync(UsuarioId, cursoId, ct);
        if (cert is null)
            return NotFound(ApiResponse<object>.Fail("Certificado não encontrado. Conclua o curso para emiti-lo."));

        return Ok(ApiResponse<CertificadoResponseDto>.Ok(cert));
    }

    /// <summary>
    /// Emite o certificado de conclusão do curso.
    /// Valida que 100% das aulas foram concluídas — retorna 400 caso contrário.
    /// Operação idempotente: se já emitido, retorna o existente sem duplicar.
    /// </summary>
    /// <remarks>
    /// Request:
    /// ```json
    /// { "cursoId": "espiritismo-basico" }
    /// ```
    /// Response 201:
    /// ```json
    /// {
    ///   "success": true,
    ///   "data": { "id": "...", "cursoTitulo": "...", "cargaHoraria": 1.4, ... },
    ///   "message": "Certificado emitido com sucesso! Parabéns pela conclusão! 🎓"
    /// }
    /// ```
    /// </remarks>
    [HttpPost("emitir")]
    [SwaggerOperation(Summary = "Emite certificado de conclusão", Tags = new[] { "Certificados" })]
    [ProducesResponseType(typeof(ApiResponse<CertificadoResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Emitir(
        [FromBody] EmitirCertificadoRequestDto dto,
        CancellationToken ct)
    {
        var cert = await _certService.EmitirAsync(UsuarioId, dto, ct);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CertificadoResponseDto>.Ok(cert,
                "Certificado emitido com sucesso! Parabéns pela conclusão! 🎓"));
    }
}
