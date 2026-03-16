using CoracaoEvangelho.API.Data;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoracaoEvangelho.API.Repositories;

// ── LivroRepository ───────────────────────────────────────────────────────
public class LivroRepository : ILivroRepository
{
    private readonly AppDbContext _db;
    public LivroRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Livro>> GetAllAsync(CancellationToken ct = default)
        => await _db.Livros
            .Where(l => l.Ativo)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Livro?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Livros
            .Include(l => l.Capitulos.OrderBy(c => c.Numero))
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    // Para sincronização offline — ignora o QueryFilter de Deletado
    public async Task<IEnumerable<Livro>> GetAtualizadosAposAsync(DateTime data, CancellationToken ct = default)
        => await _db.Livros
            .IgnoreQueryFilters() // precisa ver registros deletados para sync
            .Where(l => l.AtualizadoEm > data)
            .AsNoTracking()
            .ToListAsync(ct);
}

// ── CapituloRepository ────────────────────────────────────────────────────
public class CapituloRepository : ICapituloRepository
{
    private readonly AppDbContext _db;
    public CapituloRepository(AppDbContext db) => _db = db;

    public async Task<Capitulo?> GetByLivroENumeroAsync(string livroId, int numero, CancellationToken ct = default)
        => await _db.Capitulos
            .Include(c => c.Versiculos.OrderBy(v => v.Numero))
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.LivroId == livroId && c.Numero == numero, ct);

    public async Task<IEnumerable<Capitulo>> GetByLivroIdAsync(string livroId, CancellationToken ct = default)
        => await _db.Capitulos
            .Where(c => c.LivroId == livroId)
            .OrderBy(c => c.Numero)
            .AsNoTracking()
            .ToListAsync(ct);

    // src/CoracaoEvangelho.API/Repositories/Repositories.cs
    // Adicionar dentro de CapituloRepository

    public async Task<IEnumerable<CapituloSumarioResponseDto>> GetSumarioByLivroIdAsync(
        string livroId, CancellationToken ct = default)
        => await _db.Capitulos
            .Where(c => c.LivroId == livroId)
            .OrderBy(c => c.Numero)
            // Projeção direta no banco — evita carregar texto dos versículos em memória
            .Select(c => new CapituloSumarioResponseDto(
                c.Id,
                c.LivroId,
                c.Numero,
                c.Titulo,
                c.Versiculos.Count // traduzido para COUNT(*) pelo EF Core
            ))
            .AsNoTracking()
            .ToListAsync(ct);
}

// ── VersiculoRepository ───────────────────────────────────────────────────
public class VersiculoRepository : IVersiculoRepository
{
    private readonly AppDbContext _db;
    public VersiculoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Versiculo>> PesquisarAsync(
        string termo, string? livroId, int pagina, int tamanhoPagina,
        CancellationToken ct = default)
    {
        var query = _db.Versiculos
            .Include(v => v.Capitulo)
            .Where(v => EF.Functions.Like(v.Texto, $"%{termo}%"));

        // Filtro opcional por livro
        if (!string.IsNullOrWhiteSpace(livroId))
            query = query.Where(v => v.Capitulo.LivroId == livroId);

        return await query
            .OrderBy(v => v.Capitulo.Numero)
            .ThenBy(v => v.Numero)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> ContarPesquisaAsync(string termo, string? livroId, CancellationToken ct = default)
    {
        var query = _db.Versiculos
            .Include(v => v.Capitulo)
            .Where(v => EF.Functions.Like(v.Texto, $"%{termo}%"));

        if (!string.IsNullOrWhiteSpace(livroId))
            query = query.Where(v => v.Capitulo.LivroId == livroId);

        return await query.CountAsync(ct);
    }

    public async Task<Versiculo?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Versiculos
            .Include(v => v.Capitulo)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id, ct);
}

// ── DevocionalRepository ──────────────────────────────────────────────────
public class DevocionalRepository : IDevocionalRepository
{
    private readonly AppDbContext _db;
    public DevocionalRepository(AppDbContext db) => _db = db;

    public async Task<Devocional?> GetHojeAsync(CancellationToken ct = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _db.Devocionais
            .Include(d => d.Versiculo)
                .ThenInclude(v => v.Capitulo)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Data == hoje, ct);
    }

    public async Task<IEnumerable<Devocional>> GetHistoricoAsync(
        int pagina, int tamanhoPagina, CancellationToken ct = default)
        => await _db.Devocionais
            .Include(d => d.Versiculo)
            .OrderByDescending(d => d.Data)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<int> ContarHistoricoAsync(CancellationToken ct = default)
        => await _db.Devocionais.CountAsync(ct);

    public async Task<Devocional?> GetMaisRecenteAsync(CancellationToken ct = default)
    => await _db.Devocionais
        .Include(d => d.Versiculo)
            .ThenInclude(v => v.Capitulo)
        .OrderByDescending(d => d.Data)
        .AsNoTracking()
        .FirstOrDefaultAsync(ct);
}

// ── FavoritoRepository ────────────────────────────────────────────────────
public class FavoritoRepository : IFavoritoRepository
{
    private readonly AppDbContext _db;
    public FavoritoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Favorito>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        => await _db.Favoritos
            .Include(f => f.Versiculo)
                .ThenInclude(v => v.Capitulo)
            .Where(f => f.UsuarioId == usuarioId)
            .OrderByDescending(f => f.DataSalvo)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<Favorito?> GetByUsuarioEVersiculoAsync(
        string usuarioId, string versiculoId, CancellationToken ct = default)
        => await _db.Favoritos
            .FirstOrDefaultAsync(f => f.UsuarioId == usuarioId && f.VersiculoId == versiculoId, ct);

    public async Task<Favorito?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Favoritos.FirstOrDefaultAsync(f => f.Id == id, ct);

    // HashSet para verificação O(1) de isFavorito em listagens
    public async Task<HashSet<string>> GetVersiculoIdsFavoritosAsync(
        string usuarioId, CancellationToken ct = default)
    {
        var ids = await _db.Favoritos
            .Where(f => f.UsuarioId == usuarioId)
            .Select(f => f.VersiculoId)
            .ToListAsync(ct);
        return ids.ToHashSet();
    }

    public async Task AddAsync(Favorito favorito, CancellationToken ct = default)
        => await _db.Favoritos.AddAsync(favorito, ct);

    public Task RemoveAsync(Favorito favorito, CancellationToken ct = default)
    {
        _db.Favoritos.Remove(favorito);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}

// ── UsuarioRepository ─────────────────────────────────────────────────────
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;
    public UsuarioRepository(AppDbContext db) => _db = db;

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<Usuario?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await _db.Usuarios.FirstOrDefaultAsync(
            u => u.RefreshToken == refreshToken && u.RefreshTokenExpira > DateTime.UtcNow, ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
        => await _db.Usuarios.AddAsync(usuario, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
