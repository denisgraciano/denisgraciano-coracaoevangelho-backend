using CoracaoEvangelho.API.Models;

namespace CoracaoEvangelho.API.Repositories.Interfaces;

public interface ILivroRepository
{
    Task<IEnumerable<Livro>> GetAllAsync(CancellationToken ct = default);
    Task<Livro?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IEnumerable<Livro>> GetAtualizadosAposAsync(DateTime data, CancellationToken ct = default);
}

public interface ICapituloRepository
{
    Task<Capitulo?> GetByLivroENumeroAsync(string livroId, int numero, CancellationToken ct = default);
    Task<IEnumerable<Capitulo>> GetByLivroIdAsync(string livroId, CancellationToken ct = default);
}

public interface IVersiculoRepository
{
    Task<IEnumerable<Versiculo>> PesquisarAsync(string termo, string? livroId, int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<int> ContarPesquisaAsync(string termo, string? livroId, CancellationToken ct = default);
    Task<Versiculo?> GetByIdAsync(string id, CancellationToken ct = default);
}

public interface IDevocionalRepository
{
    Task<Devocional?> GetHojeAsync(CancellationToken ct = default);
    Task<IEnumerable<Devocional>> GetHistoricoAsync(int pagina, int tamanhoPagina, CancellationToken ct = default);
    Task<int> ContarHistoricoAsync(CancellationToken ct = default);
    Task<Devocional?> GetMaisRecenteAsync(CancellationToken ct = default);
}

public interface IFavoritoRepository
{
    Task<IEnumerable<Favorito>> GetByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
    Task<Favorito?> GetByUsuarioEVersiculoAsync(string usuarioId, string versiculoId, CancellationToken ct = default);
    Task<Favorito?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<HashSet<string>> GetVersiculoIdsFavoritosAsync(string usuarioId, CancellationToken ct = default);
    Task AddAsync(Favorito favorito, CancellationToken ct = default);
    Task RemoveAsync(Favorito favorito, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Usuario?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task AddAsync(Usuario usuario, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}


