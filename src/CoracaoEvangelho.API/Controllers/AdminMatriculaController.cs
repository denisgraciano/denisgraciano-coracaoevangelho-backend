using CoracaoEvangelho.API.Constants;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

/// <summary>
/// Matrículas — visão administrativa.
///
/// Contrato com Angular:
/// | Método | Rota                    | Auth  | Componente Angular          |
/// |--------|-------------------------|-------|------------------------------|
/// | GET    | /api/admin/matriculas   | admin | ListaMatriculasComponent    |
/// </summary>
[ApiController]
[Route("api/admin/matriculas")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public class AdminMatriculaController : ControllerBase
{
    private readonly IMatriculaService _matriculaService;

    public AdminMatriculaController(IMatriculaService matriculaService) =>
        _matriculaService = matriculaService;

    /// <summary>Lista todas as matrículas com dados do aluno e do curso (admin).</summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as matrículas (admin)", Tags = new[] { "Admin — Matrículas" })]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<MatriculaAdminDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = 20,
        CancellationToken ct = default)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho is < 1 or > 100) tamanho = 20;

        var result = await _matriculaService.ListarAsync(pagina, tamanho, ct);
        return Ok(ApiResponse<PagedResultDto<MatriculaAdminDto>>.Ok(result));
    }
}
