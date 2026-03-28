using CoracaoEvangelho.API.Models;

namespace CoracaoEvangelho.API.Repositories.Interfaces;

// ── IUsuarioRepository ────────────────────────────────────────
public interface IUsuarioRepository
{
    /// <summary>Leitura sem tracking — use para exibição de dados (GET).</summary>
    Task<Usuario?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Leitura COM tracking — use quando for modificar e chamar SaveChanges.
    /// Necessário para AtualizarPerfil e AlterarSenha.
    /// </summary>
    Task<Usuario?> GetTrackedByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Busca por e-mail COM tracking (usado em Login e Register para salvar RefreshToken).
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>Lista paginada de todos os usuários — uso exclusivo admin.</summary>
    Task<IEnumerable<Usuario>> GetAllPagedAsync(int pagina, int tamanho, CancellationToken ct = default);

    Task<int> CountAsync(CancellationToken ct = default);
    Task AddAsync(Usuario usuario, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── ICursoRepository ──────────────────────────────────────────
public interface ICursoRepository
{
    Task<IEnumerable<Curso>> GetAllAsync(CancellationToken ct = default);
    Task<Curso?> GetByIdComAulasAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<Curso>> GetCursosMatriculadosAsync(string usuarioId, CancellationToken ct = default);

    /// <summary>Cursos ativos nos quais o aluno NÃO está matriculado.</summary>
    Task<IEnumerable<Curso>> GetSugestoesAsync(string usuarioId, int quantidade, CancellationToken ct = default);

    /// <summary>
    /// Lista todos os cursos incluindo inativos — uso exclusivo admin.
    /// Ignora o QueryFilter de soft delete.
    /// </summary>
    Task<IEnumerable<Curso>> GetAllAdminAsync(CancellationToken ct = default);

    /// <summary>Leitura COM tracking e sem QueryFilter — para edição admin.</summary>
    Task<Curso?> GetTrackedByIdAsync(string id, CancellationToken ct = default);

    Task AddAsync(Curso curso, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── IMatriculaRepository ──────────────────────────────────────
public interface IMatriculaRepository
{
    Task<Matricula?> GetByUsuarioCursoAsync(string usuarioId, string cursoId, CancellationToken ct = default);
    Task<IEnumerable<Matricula>> GetByUsuarioAsync(string usuarioId, CancellationToken ct = default);
    Task AddAsync(Matricula matricula, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── IProgressoRepository ──────────────────────────────────────
public interface IProgressoRepository
{
    /// <summary>Uma query traz todos os progressos do curso — evita N+1.</summary>
    Task<IEnumerable<Progresso>> GetByCursoAsync(string usuarioId, string cursoId, CancellationToken ct = default);

    Task<Progresso?> GetByAulaAsync(string usuarioId, string aulaId, CancellationToken ct = default);

    /// <summary>Insere se não existe, atualiza se existe (upsert).</summary>
    Task UpsertAsync(Progresso progresso, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── ICertificadoRepository ────────────────────────────────────
public interface ICertificadoRepository
{
    Task<Certificado?> GetByUsuarioCursoAsync(string usuarioId, string cursoId, CancellationToken ct = default);
    Task<IEnumerable<Certificado>> GetByUsuarioAsync(string usuarioId, CancellationToken ct = default);
    Task AddAsync(Certificado certificado, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── IPedidoVibracaoRepository ─────────────────────────────────
public interface IPedidoVibracaoRepository
{
    Task AddAsync(PedidoVibracao pedido, CancellationToken ct = default);
    Task<IEnumerable<PedidoVibracao>> GetAllAsync(int pagina, int tamanho, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);

    /// <summary>Leitura COM tracking — para marcar como lido.</summary>
    Task<PedidoVibracao?> GetTrackedByIdAsync(string id, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── IAulaRepository ───────────────────────────────────────────
public interface IAulaRepository
{
    /// <summary>Leitura sem tracking, ignora QueryFilter (inclui inativas).</summary>
    Task<Aula?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>Leitura COM tracking, ignora QueryFilter — para edição/remoção admin.</summary>
    Task<Aula?> GetTrackedByIdAsync(string id, CancellationToken ct = default);

    Task AddAsync(Aula aula, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

// ── ICategoriaRepository ──────────────────────────────────────
public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> GetAllAsync(CancellationToken ct = default);
    Task<Categoria?> GetByIdAsync(string id, CancellationToken ct = default);
}
