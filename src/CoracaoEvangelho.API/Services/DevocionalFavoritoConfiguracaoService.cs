using AutoMapper;
using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services.Interfaces;

namespace CoracaoEvangelho.API.Services;

// ── DevocionalService ─────────────────────────────────────────────────────
public class DevocionalService : IDevocionalService
{
    private readonly IDevocionalRepository _repo;
    private readonly IFavoritoRepository _favoritoRepo;

    public DevocionalService(IDevocionalRepository repo, IFavoritoRepository favoritoRepo)
    {
        _repo = repo;
        _favoritoRepo = favoritoRepo;
    }

    public async Task<DevocionalResponseDto?> GetHojeAsync(
        string? usuarioId, CancellationToken ct = default)
    {
        var devocional = await _repo.GetHojeAsync(ct);
        if (devocional is null) return null;

        return await MapDevocionalAsync(devocional, usuarioId, ct);
    }

    public async Task<PagedResultDto<DevocionalResponseDto>> GetHistoricoAsync(
        int pagina, int tamanhoPagina, string? usuarioId, CancellationToken ct = default)
    {
        var devocionais = await _repo.GetHistoricoAsync(pagina, tamanhoPagina, ct);
        var total = await _repo.ContarHistoricoAsync(ct);

        HashSet<string> favIds = usuarioId is not null
            ? await _favoritoRepo.GetVersiculoIdsFavoritosAsync(usuarioId, ct)
            : new HashSet<string>();

        var items = devocionais.Select(d => MapDevocional(d, favIds)).ToList();

        return new PagedResultDto<DevocionalResponseDto>
        {
            Items = items,
            TotalItens = total,
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina
        };
    }

    private async Task<DevocionalResponseDto> MapDevocionalAsync(
        Devocional d, string? usuarioId, CancellationToken ct)
    {
        HashSet<string> favIds = usuarioId is not null
            ? await _favoritoRepo.GetVersiculoIdsFavoritosAsync(usuarioId, ct)
            : new HashSet<string>();

        return MapDevocional(d, favIds);
    }

    private static DevocionalResponseDto MapDevocional(Devocional d, HashSet<string> favIds)
    {
        var v = d.Versiculo;
        var versiculoDto = new VersiculoResponseDto(
            v.Id, v.Numero, v.Texto, v.CapituloId, favIds.Contains(v.Id));

        return new DevocionalResponseDto(
            d.Id, d.Data, d.Passagem, d.Reflexao, versiculoDto);
    }
}

// ── FavoritoService ───────────────────────────────────────────────────────
public class FavoritoService : IFavoritoService
{
    private readonly IFavoritoRepository _favoritoRepo;
    private readonly IVersiculoRepository _versiculoRepo;

    public FavoritoService(IFavoritoRepository favoritoRepo, IVersiculoRepository versiculoRepo)
    {
        _favoritoRepo = favoritoRepo;
        _versiculoRepo = versiculoRepo;
    }

    public async Task<IEnumerable<FavoritoResponseDto>> GetFavoritosAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var favoritos = await _favoritoRepo.GetByUsuarioIdAsync(usuarioId, ct);

        return favoritos.Select(f =>
        {
            var v = f.Versiculo;
            var vDto = new VersiculoResponseDto(v.Id, v.Numero, v.Texto, v.CapituloId, true);
            return new FavoritoResponseDto(f.Id, f.VersiculoId, f.DataSalvo, vDto);
        });
    }

    public async Task<FavoritoResponseDto> AdicionarAsync(
        string usuarioId, AdicionarFavoritoRequestDto dto, CancellationToken ct = default)
    {
        // Verifica se versículo existe
        var versiculo = await _versiculoRepo.GetByIdAsync(dto.VersiculoId, ct)
            ?? throw new KeyNotFoundException($"Versículo '{dto.VersiculoId}' não encontrado.");

        // Idempotente: se já existe, retorna o existente
        var existente = await _favoritoRepo.GetByUsuarioEVersiculoAsync(usuarioId, dto.VersiculoId, ct);
        if (existente is not null)
        {
            var vDto = new VersiculoResponseDto(
                versiculo.Id, versiculo.Numero, versiculo.Texto, versiculo.CapituloId, true);
            return new FavoritoResponseDto(existente.Id, existente.VersiculoId, existente.DataSalvo, vDto);
        }

        var favorito = new Favorito
        {
            UsuarioId = usuarioId,
            VersiculoId = dto.VersiculoId,
            DataSalvo = DateTime.UtcNow
        };

        await _favoritoRepo.AddAsync(favorito, ct);
        await _favoritoRepo.SaveChangesAsync(ct);

        var versiculoDtoNovo = new VersiculoResponseDto(
            versiculo.Id, versiculo.Numero, versiculo.Texto, versiculo.CapituloId, true);
        return new FavoritoResponseDto(favorito.Id, favorito.VersiculoId, favorito.DataSalvo, versiculoDtoNovo);
    }

    public async Task RemoverAsync(string usuarioId, string favoritoId, CancellationToken ct = default)
    {
        var favorito = await _favoritoRepo.GetByIdAsync(favoritoId, ct)
            ?? throw new KeyNotFoundException($"Favorito '{favoritoId}' não encontrado.");

        if (favorito.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para remover este favorito.");

        await _favoritoRepo.RemoveAsync(favorito, ct);
        await _favoritoRepo.SaveChangesAsync(ct);
    }
}

// ── ConfiguracaoService ───────────────────────────────────────────────────
public class ConfiguracaoService : IConfiguracaoService
{
    private readonly IUsuarioRepository _usuarioRepo;

    public ConfiguracaoService(IUsuarioRepository usuarioRepo)
        => _usuarioRepo = usuarioRepo;

    public async Task<ConfiguracaoResponseDto> GetConfiguracaoAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");
        return new ConfiguracaoResponseDto(usuario.Tema, usuario.TamanhoFonte);
    }

    public async Task<ConfiguracaoResponseDto> SetTemaAsync(
        string usuarioId, string tema, CancellationToken ct = default)
    {
        var temaValido = new[] { "claro", "escuro" };
        if (!temaValido.Contains(tema))
            throw new ArgumentException("Tema inválido. Use 'claro' ou 'escuro'.");

        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.Tema = tema;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _usuarioRepo.SaveChangesAsync(ct);

        return new ConfiguracaoResponseDto(usuario.Tema, usuario.TamanhoFonte);
    }

    public async Task<ConfiguracaoResponseDto> SetFonteAsync(
        string usuarioId, int tamanhoFonte, CancellationToken ct = default)
    {
        if (tamanhoFonte < 12 || tamanhoFonte > 28)
            throw new ArgumentException("Tamanho de fonte deve estar entre 12 e 28.");

        var usuario = await _usuarioRepo.GetByIdAsync(usuarioId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.TamanhoFonte = tamanhoFonte;
        usuario.AtualizadoEm = DateTime.UtcNow;
        await _usuarioRepo.SaveChangesAsync(ct);

        return new ConfiguracaoResponseDto(usuario.Tema, usuario.TamanhoFonte);
    }
}
