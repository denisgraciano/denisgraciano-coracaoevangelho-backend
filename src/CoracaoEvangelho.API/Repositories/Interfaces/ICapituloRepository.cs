// src/CoracaoEvangelho.API/Repositories/Interfaces/IRepositories.cs
// Adicionar na interface ICapituloRepository

using CoracaoEvangelho.API.Models;

public interface ICapituloRepository
{
    Task<Capitulo?> GetByLivroENumeroAsync(string livroId, int numero, CancellationToken ct = default);
    Task<IEnumerable<Capitulo>> GetByLivroIdAsync(string livroId, CancellationToken ct = default);

    // NOVO — retorna capítulos com contagem de versículos (projeção SQL, sem carregar textos)
    Task<IEnumerable<CapituloSumarioResponseDto>> GetSumarioByLivroIdAsync(string livroId, CancellationToken ct = default);
}
