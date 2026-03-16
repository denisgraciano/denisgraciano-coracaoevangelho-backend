using AutoMapper;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services.Interfaces;

namespace CoracaoEvangelho.API.Services;

// ── LivroService ──────────────────────────────────────────────────────────
public class LivroService : ILivroService
{
    private readonly ILivroRepository _livroRepo;
    private readonly ICapituloRepository _capituloRepo;
    private readonly IFavoritoRepository _favoritoRepo;
    private readonly IMapper _mapper;

    public LivroService(
        ILivroRepository livroRepo,
        ICapituloRepository capituloRepo,
        IFavoritoRepository favoritoRepo,
        IMapper mapper)
    {
        _livroRepo = livroRepo;
        _capituloRepo = capituloRepo;
        _favoritoRepo = favoritoRepo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LivroResponseDto>> GetLivrosAsync(CancellationToken ct = default)
    {
        var livros = await _livroRepo.GetAllAsync(ct);
        return _mapper.Map<IEnumerable<LivroResponseDto>>(livros);
    }

    public async Task<LivroResponseDto?> GetLivroByIdAsync(string id, CancellationToken ct = default)
    {
        var livro = await _livroRepo.GetByIdAsync(id, ct);
        return livro is null ? null : _mapper.Map<LivroResponseDto>(livro);
    }

    public async Task<CapituloResponseDto?> GetCapituloAsync(
        string livroId, int numero, string? usuarioId, CancellationToken ct = default)
    {
        var capitulo = await _capituloRepo.GetByLivroENumeroAsync(livroId, numero, ct);
        if (capitulo is null) return null;

        // Busca favoritos do usuário de uma vez — evita N+1
        HashSet<string> favoritosIds = usuarioId is not null
            ? await _favoritoRepo.GetVersiculoIdsFavoritosAsync(usuarioId, ct)
            : new HashSet<string>();

        var versiculos = capitulo.Versiculos
            .OrderBy(v => v.Numero)
            .Select(v => new VersiculoResponseDto(
                v.Id,
                v.Numero,
                v.Texto,
                v.CapituloId,
                favoritosIds.Contains(v.Id)
            ))
            .ToList();

        return new CapituloResponseDto(
            capitulo.Id,
            capitulo.LivroId,
            capitulo.Numero,
            capitulo.Titulo,
            versiculos
        );
    }

    public async Task<IEnumerable<CapituloSumarioResponseDto>?> GetCapitulosSumarioAsync(
    string livroId, CancellationToken ct = default)
    {
        // Verifica se o livro existe antes de buscar capítulos
        // Usa GetByIdAsync que já aplica o filtro Ativo=true
        var livro = await _livroRepo.GetByIdAsync(livroId, ct);
        if (livro is null) return null; // sinaliza 404 para o controller

        return await _capituloRepo.GetSumarioByLivroIdAsync(livroId, ct);
    }
}

// ── VersiculoService ──────────────────────────────────────────────────────
public class VersiculoService : IVersiculoService
{
    private readonly IVersiculoRepository _versiculoRepo;
    private readonly IFavoritoRepository _favoritoRepo;

    public VersiculoService(IVersiculoRepository versiculoRepo, IFavoritoRepository favoritoRepo)
    {
        _versiculoRepo = versiculoRepo;
        _favoritoRepo = favoritoRepo;
    }

    public async Task<PagedResultDto<VersiculoResponseDto>> PesquisarAsync(
        string termo, string? livroId, int pagina, int tamanhoPagina,
        string? usuarioId, CancellationToken ct = default)
    {
        var versiculos = await _versiculoRepo.PesquisarAsync(termo, livroId, pagina, tamanhoPagina, ct);
        var total = await _versiculoRepo.ContarPesquisaAsync(termo, livroId, ct);

        HashSet<string> favIds = usuarioId is not null
            ? await _favoritoRepo.GetVersiculoIdsFavoritosAsync(usuarioId, ct)
            : new HashSet<string>();

        var items = versiculos.Select(v => new VersiculoResponseDto(
            v.Id,
            v.Numero,
            v.Texto,
            v.CapituloId,
            favIds.Contains(v.Id)
        )).ToList();

        return new PagedResultDto<VersiculoResponseDto>
        {
            Items = items,
            TotalItens = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };
    }
}
