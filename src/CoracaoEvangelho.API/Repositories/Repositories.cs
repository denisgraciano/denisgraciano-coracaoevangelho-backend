using CoracaoEvangelho.API.Data;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoracaoEvangelho.API.Repositories;

// ── UsuarioRepository ─────────────────────────────────────────
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;
    public UsuarioRepository(AppDbContext db) => _db = db;

    // AsNoTracking — só leitura, não vai salvar
    public Task<Usuario?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    // COM tracking — vai modificar e salvar (AtualizarPerfil, AlterarSenha)
    public Task<Usuario?> GetTrackedByIdAsync(string id, CancellationToken ct = default) =>
        _db.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    // COM tracking — Login e Register salvam RefreshToken na mesma instância
    public Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> GetByRefreshTokenAsync(string token, CancellationToken ct = default) =>
        _db.Usuarios
            .FirstOrDefaultAsync(
                u => u.RefreshToken == token && u.RefreshTokenExpira > DateTime.UtcNow, ct);

    public async Task<IEnumerable<Usuario>> GetAllPagedAsync(
        int pagina, int tamanho, CancellationToken ct = default) =>
        await _db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(ct);

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.Usuarios.CountAsync(ct);

    public Task AddAsync(Usuario usuario, CancellationToken ct = default) =>
        _db.Usuarios.AddAsync(usuario, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── CursoRepository ───────────────────────────────────────────
public class CursoRepository : ICursoRepository
{
    private readonly AppDbContext _db;
    public CursoRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Curso>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            // Filtered include sem OrderBy — ordenação feita no Service ao projetar
            // EF Core suporta Where dentro de Include, mas não .OrderBy encadeado
            .Include(c => c.Aulas.Where(a => a.Ativa))
            .ToListAsync(ct);

    // Inclui aulas, depoimentos e contagem de matrículas para DetalhesCursoComponent
    public Task<Curso?> GetByIdComAulasAsync(string id, CancellationToken ct = default) =>
        _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Aulas.Where(a => a.Ativa))
            .Include(c => c.Depoimentos)
            .Include(c => c.Matriculas.Where(m => m.Ativa))
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    // Cursos matriculados com aulas (Dashboard + Player)
    public async Task<IEnumerable<Curso>> GetCursosMatriculadosAsync(
        string usuarioId, CancellationToken ct = default) =>
        await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Aulas.Where(a => a.Ativa))
            .Where(c => c.Matriculas.Any(m => m.UsuarioId == usuarioId && m.Ativa))
            .ToListAsync(ct);

    // Sugestões: cursos que o aluno ainda não está matriculado
    public async Task<IEnumerable<Curso>> GetSugestoesAsync(
        string usuarioId, int quantidade, CancellationToken ct = default) =>
        await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Aulas.Where(a => a.Ativa))
            .Where(c => !c.Matriculas.Any(m => m.UsuarioId == usuarioId))
            .Take(quantidade)
            .ToListAsync(ct);

    // Admin: inclui cursos inativos (IgnoreQueryFilters)
    public async Task<IEnumerable<Curso>> GetAllAdminAsync(CancellationToken ct = default) =>
        await _db.Cursos
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Aulas)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(ct);

    // Admin: tracked, sem QueryFilter — permite editar cursos inativos
    public Task<Curso?> GetTrackedByIdAsync(string id, CancellationToken ct = default) =>
        _db.Cursos
            .IgnoreQueryFilters()
            .Include(c => c.Aulas)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task AddAsync(Curso curso, CancellationToken ct = default) =>
        _db.Cursos.AddAsync(curso, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── MatriculaRepository ───────────────────────────────────────
public class MatriculaRepository : IMatriculaRepository
{
    private readonly AppDbContext _db;
    public MatriculaRepository(AppDbContext db) => _db = db;

    public Task<Matricula?> GetByEmailCursoAsync(
        string email, string cursoId, CancellationToken ct = default) =>
        _db.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.Email == email && m.CursoId == cursoId, ct);

    public Task<Matricula?> GetByUsuarioCursoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default) =>
        _db.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                m => m.UsuarioId == usuarioId && m.CursoId == cursoId, ct);

    public async Task<IEnumerable<Matricula>> GetByUsuarioAsync(
        string usuarioId, CancellationToken ct = default) =>
        await _db.Matriculas
            .AsNoTracking()
            .Include(m => m.Curso)
            .Where(m => m.UsuarioId == usuarioId && m.Ativa)
            .ToListAsync(ct);

    public Task<int> CountAllAsync(CancellationToken ct = default) =>
        _db.Matriculas.CountAsync(ct);

    public async Task<IEnumerable<Matricula>> GetAllAsync(
        int pagina, int tamanho, CancellationToken ct = default) =>
        await _db.Matriculas
            .Include(m => m.Usuario)
            .Include(m => m.Curso)
            .OrderByDescending(m => m.DataMatricula)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task AddAsync(Matricula matricula, CancellationToken ct = default) =>
        _db.Matriculas.AddAsync(matricula, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── ProgressoRepository ───────────────────────────────────────
public class ProgressoRepository : IProgressoRepository
{
    private readonly AppDbContext _db;
    public ProgressoRepository(AppDbContext db) => _db = db;

    // Uma query para todo o curso — sem N+1
    public async Task<IEnumerable<Progresso>> GetByCursoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default) =>
        await _db.Progressos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId && p.CursoId == cursoId)
            .ToListAsync(ct);

    // COM tracking para upsert
    public Task<Progresso?> GetByAulaAsync(
        string usuarioId, string aulaId, CancellationToken ct = default) =>
        _db.Progressos
            .FirstOrDefaultAsync(
                p => p.UsuarioId == usuarioId && p.AulaId == aulaId, ct);

    // Upsert: insere se não existe, atualiza se existe
    public async Task UpsertAsync(Progresso progresso, CancellationToken ct = default)
    {
        var existente = await _db.Progressos
            .FirstOrDefaultAsync(
                p => p.UsuarioId == progresso.UsuarioId
                  && p.AulaId    == progresso.AulaId, ct);

        if (existente is null)
        {
            await _db.Progressos.AddAsync(progresso, ct);
        }
        else
        {
            existente.Concluida      = progresso.Concluida;
            existente.DataConclusao  = progresso.DataConclusao;
            // _db já rastreia existente — não precisa de Update() explícito
        }
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── CertificadoRepository ─────────────────────────────────────
public class CertificadoRepository : ICertificadoRepository
{
    private readonly AppDbContext _db;
    public CertificadoRepository(AppDbContext db) => _db = db;

    public Task<Certificado?> GetByUsuarioCursoAsync(
        string usuarioId, string cursoId, CancellationToken ct = default) =>
        _db.Certificados
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.UsuarioId == usuarioId && c.CursoId == cursoId, ct);

    public async Task<IEnumerable<Certificado>> GetByUsuarioAsync(
        string usuarioId, CancellationToken ct = default) =>
        await _db.Certificados
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.DataEmissao)
            .ToListAsync(ct);

    public Task AddAsync(Certificado certificado, CancellationToken ct = default) =>
        _db.Certificados.AddAsync(certificado, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── PedidoVibracaoRepository ──────────────────────────────────
public class PedidoVibracaoRepository : IPedidoVibracaoRepository
{
    private readonly AppDbContext _db;
    public PedidoVibracaoRepository(AppDbContext db) => _db = db;

    public Task AddAsync(PedidoVibracao pedido, CancellationToken ct = default) =>
        _db.PedidosVibracao.AddAsync(pedido, ct).AsTask();

    public async Task<IEnumerable<PedidoVibracao>> GetAllAsync(
        int pagina, int tamanho, CancellationToken ct = default) =>
        await _db.PedidosVibracao
            .AsNoTracking()
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync(ct);

    public Task<int> CountAsync(CancellationToken ct = default) =>
        _db.PedidosVibracao.CountAsync(ct);

    // COM tracking para marcar como lido
    public Task<PedidoVibracao?> GetTrackedByIdAsync(string id, CancellationToken ct = default) =>
        _db.PedidosVibracao
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── AulaRepository ────────────────────────────────────────────
public class AulaRepository : IAulaRepository
{
    private readonly AppDbContext _db;
    public AulaRepository(AppDbContext db) => _db = db;

    // IgnoreQueryFilters: admin precisa ver aulas inativas também
    public Task<Aula?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Aulas
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Aula?> GetTrackedByIdAsync(string id, CancellationToken ct = default) =>
        _db.Aulas
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task AddAsync(Aula aula, CancellationToken ct = default) =>
        _db.Aulas.AddAsync(aula, ct).AsTask();

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

// ── CategoriaRepository ───────────────────────────────────────
public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _db;
    public CategoriaRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Categorias
            .AsNoTracking()
            .Include(c => c.Cursos)
            .ToListAsync(ct);

    public Task<Categoria?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _db.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
}
