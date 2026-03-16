namespace CoracaoEvangelho.API.DTOs.Response;

// ── Wrapper padrão de resposta ─────────────────────────────────────────────
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string message = "")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        => new() { Success = false, Message = message, Errors = errors ?? new() };
}

// ── Paginação ─────────────────────────────────────────────────────────────
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalItens { get; set; }
    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;
}

// ── Livro ─────────────────────────────────────────────────────────────────
// Espelha exatamente a interface Angular: { id, titulo, subtitulo, capa }
public record LivroResponseDto(
    string Id,
    string Titulo,
    string Subtitulo,
    string Capa
);

// ── Capítulo ──────────────────────────────────────────────────────────────
public record CapituloResponseDto(
    string Id,
    string LivroId,
    int Numero,
    string Titulo,
    List<VersiculoResponseDto> Versiculos
);

// ── Versículo ─────────────────────────────────────────────────────────────
// Espelha: { id, numero, texto, capituloId, isFavorito? }
public record VersiculoResponseDto(
    string Id,
    int Numero,
    string Texto,
    string CapituloId,
    bool IsFavorito
);

// ── Devocional ────────────────────────────────────────────────────────────
// Espelha: { id, data, passagem, reflexao, versiculo }
public record DevocionalResponseDto(
    string Id,
    DateOnly Data,
    string Passagem,
    string Reflexao,
    VersiculoResponseDto Versiculo
);

// ── Auth ──────────────────────────────────────────────────────────────────
public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    UsuarioResponseDto Usuario
);

// ── Usuário (sem SenhaHash!) ──────────────────────────────────────────────
public record UsuarioResponseDto(
    string Id,
    string Nome,
    string Email,
    string Role
);

// ── Favorito ──────────────────────────────────────────────────────────────
public record FavoritoResponseDto(
    string Id,
    string VersiculoId,
    DateTime DataSalvo,
    VersiculoResponseDto Versiculo
);

// ── Configurações do usuário ──────────────────────────────────────────────
public record ConfiguracaoResponseDto(
    string Tema,
    int TamanhoFonte
);

// ── Sync ──────────────────────────────────────────────────────────────────
public record SyncLivroDto(
    string Id,
    string Titulo,
    string Subtitulo,
    string Capa,
    bool Deletado,
    DateTime AtualizadoEm
);
