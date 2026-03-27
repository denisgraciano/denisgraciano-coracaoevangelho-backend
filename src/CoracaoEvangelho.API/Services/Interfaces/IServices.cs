using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;

namespace CoracaoEvangelho.API.Services.Interfaces;

// ── IAuthService ──────────────────────────────────────────────
public interface IAuthService
{
    Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

// ── IUsuarioService ───────────────────────────────────────────
public interface IUsuarioService
{
    Task<UsuarioResponseDto> GetPerfilAsync(string usuarioId, CancellationToken ct = default);
    Task<UsuarioResponseDto> AtualizarPerfilAsync(string usuarioId, AtualizarPerfilRequestDto dto, CancellationToken ct = default);
    Task AlterarSenhaAsync(string usuarioId, AlterarSenhaRequestDto dto, CancellationToken ct = default);
}

// ── ICursoService ─────────────────────────────────────────────
public interface ICursoService
{
    Task<IEnumerable<CursoResumoResponseDto>> GetTodosAsync(CancellationToken ct = default);
    Task<CursoResponseDto?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<CursoResponseDto>> GetCursosMatriculadosAsync(string usuarioId, CancellationToken ct = default);
    Task<IEnumerable<CursoResumoResponseDto>> GetSugestoesAsync(string usuarioId, CancellationToken ct = default);
    Task<CursoResponseDto> CriarAsync(CursoRequestDto dto, CancellationToken ct = default);
}

// ── IMatriculaService ─────────────────────────────────────────
public interface IMatriculaService
{
    Task<MatriculaResponseDto> InscreverAsync(string usuarioId, string cursoId, MatriculaRequestDto dto, CancellationToken ct = default);
    Task<bool> EstaMatriculadoAsync(string usuarioId, string cursoId, CancellationToken ct = default);
}

// ── IProgressoService ─────────────────────────────────────────
public interface IProgressoService
{
    Task<ProgressoCursoResponseDto> GetProgressoCursoAsync(string usuarioId, string cursoId, CancellationToken ct = default);
    Task<ProgressoCursoResponseDto> MarcarAulaConcluidaAsync(string usuarioId, MarcarAulaConcluidaRequestDto dto, CancellationToken ct = default);
}

// ── ICertificadoService ───────────────────────────────────────
public interface ICertificadoService
{
    Task<IEnumerable<CertificadoResponseDto>> GetByUsuarioAsync(string usuarioId, CancellationToken ct = default);
    Task<CertificadoResponseDto?> GetByUsuarioCursoAsync(string usuarioId, string cursoId, CancellationToken ct = default);
    Task<CertificadoResponseDto> EmitirAsync(string usuarioId, EmitirCertificadoRequestDto dto, CancellationToken ct = default);
}

// ── IAdminService ─────────────────────────────────────────────
public interface IAdminService
{
    // Usuários
    Task<PagedResultDto<UsuarioAdminDto>> ListarUsuariosAsync(int pagina, int tamanho, CancellationToken ct = default);
    Task AlterarStatusUsuarioAsync(string usuarioId, bool ativo, CancellationToken ct = default);

    // Pedidos de Vibração
    Task<PagedResultDto<PedidoVibracaoAdminDto>> ListarPedidosVibracaoAsync(int pagina, int tamanho, CancellationToken ct = default);
    Task MarcarPedidoLidoAsync(string pedidoId, CancellationToken ct = default);

    // Cursos
    Task<IEnumerable<CursoAdminResponseDto>> ListarCursosAsync(CancellationToken ct = default);
    Task<CursoAdminResponseDto> CriarCursoAsync(CursoRequestDto dto, CancellationToken ct = default);
    Task<CursoAdminResponseDto> AtualizarCursoAsync(string cursoId, CursoRequestDto dto, CancellationToken ct = default);
    Task RemoverCursoAsync(string cursoId, CancellationToken ct = default);

    // Aulas
    Task<AulaAdminResponseDto> AdicionarAulaAsync(string cursoId, AulaRequestDto dto, CancellationToken ct = default);
    Task<AulaAdminResponseDto> AtualizarAulaAsync(string cursoId, string aulaId, AulaRequestDto dto, CancellationToken ct = default);
    Task RemoverAulaAsync(string cursoId, string aulaId, CancellationToken ct = default);
}

// ── IPedidoVibracaoService ────────────────────────────────────
public interface IPedidoVibracaoService
{
    Task<PedidoVibracaoResponseDto> EnviarAsync(string? usuarioId, PedidoVibracaoRequestDto dto, CancellationToken ct = default);

    // Retorna PagedResultDto usando o DTO de admin — sem conflito com Model
    Task<PagedResultDto<PedidoVibracaoAdminDto>> ListarAsync(int pagina, int tamanho, CancellationToken ct = default);
}
