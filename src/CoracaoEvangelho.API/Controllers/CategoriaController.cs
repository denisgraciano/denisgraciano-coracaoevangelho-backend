using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CoracaoEvangelho.API.Controllers;

[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
public class CategoriaController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriaController(ICategoriaService categoriaService) =>
        _categoriaService = categoriaService;

    [HttpGet]
    [SwaggerOperation(Summary = "Lista todas as categorias com total de cursos", Tags = new[] { "Categorias" })]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoriaResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var categorias = await _categoriaService.GetAllAsync(ct);
        return Ok(ApiResponse<IEnumerable<CategoriaResponseDto>>.Ok(categorias));
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Detalhe de uma categoria", Tags = new[] { "Categorias" })]
    [ProducesResponseType(typeof(ApiResponse<CategoriaResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var categoria = await _categoriaService.GetByIdAsync(id, ct);
        if (categoria is null)
            return NotFound(ApiResponse<object>.Fail($"Categoria '{id}' não encontrada."));

        return Ok(ApiResponse<CategoriaResponseDto>.Ok(categoria));
    }
}
