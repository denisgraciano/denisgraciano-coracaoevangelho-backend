using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;

namespace CoracaoEvangelho.API.Services.Interfaces;

public interface ILivroService
{
    Task<IEnumerable<LivroResponseDto>> GetLivrosAsync(CancellationToken ct = default);
    Task<LivroResponseDto?> GetLivroByIdAsync(string id, CancellationToken ct = default);
    Task<CapituloResponseDto?> GetCapituloAsync(string livroId, int numero, string? usuarioId, CancellationToken ct = default);
    Task<IEnumerable<CapituloSumarioResponseDto>?> GetCapitulosSumarioAsync(string livroId, CancellationToken ct = default);
}

public interface IVersiculoService
{
    Task<PagedResultDto<VersiculoResponseDto>> PesquisarAsync(
        string termo, string? livroId, int pagina, int tamanhoPagina,
        string? usuarioId, CancellationToken ct = default);
}

public interface IDevocionalService
{
    Task<DevocionalResponseDto?> GetHojeAsync(string? usuarioId, CancellationToken ct = default);
    Task<PagedResultDto<DevocionalResponseDto>> GetHistoricoAsync(int pagina, int tamanhoPagina, string? usuarioId, CancellationToken ct = default);
}

public interface IFavoritoService
{
    Task<IEnumerable<FavoritoResponseDto>> GetFavoritosAsync(string usuarioId, CancellationToken ct = default);
    Task<FavoritoResponseDto> AdicionarAsync(string usuarioId, AdicionarFavoritoRequestDto dto, CancellationToken ct = default);
    Task RemoverAsync(string usuarioId, string favoritoId, CancellationToken ct = default);
}

public interface IConfiguracaoService
{
    Task<ConfiguracaoResponseDto> GetConfiguracaoAsync(string usuarioId, CancellationToken ct = default);
    Task<ConfiguracaoResponseDto> SetTemaAsync(string usuarioId, string tema, CancellationToken ct = default);
    Task<ConfiguracaoResponseDto> SetFonteAsync(string usuarioId, int tamanhoFonte, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<AuthResponseDto> RegistrarAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

public interface ISyncService
{
    Task<IEnumerable<SyncLivroDto>> GetLivrosSincronizadosAsync(DateTime atualizadoApos, CancellationToken ct = default);
}
