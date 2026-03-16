// src/CoracaoEvangelho.API/DTOs/Response/ResponseDtos.cs
// Adicionar após CapituloResponseDto

// ── Sumário de Capítulo (sem versículos — usado no índice do livro) ────────
// Contrato da issue #1: { id, livroId, numero, titulo, totalVersiculos }
public record CapituloSumarioResponseDto(
    string Id,
    string LivroId,
    int Numero,
    string Titulo,
    int TotalVersiculos
);