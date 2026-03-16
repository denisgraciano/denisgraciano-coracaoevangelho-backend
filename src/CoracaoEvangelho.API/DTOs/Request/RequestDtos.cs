namespace CoracaoEvangelho.API.DTOs.Request;

// ── Auth ──────────────────────────────────────────────────────────────────
public record RegisterRequestDto(
    string Nome,
    string Email,
    string Senha
);

public record LoginRequestDto(
    string Email,
    string Senha
);

public record RefreshTokenRequestDto(
    string RefreshToken
);

// ── Favoritos ─────────────────────────────────────────────────────────────
public record AdicionarFavoritoRequestDto(
    string VersiculoId
);

// ── Configurações ─────────────────────────────────────────────────────────
public record SetTemaRequestDto(string Tema);

public record SetFonteRequestDto(int TamanhoFonte);
