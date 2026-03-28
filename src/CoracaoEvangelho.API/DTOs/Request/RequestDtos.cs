// ============================================================
// DTOs/Request/RequestDtos.cs
// Todos os DTOs de entrada — validados pelo FluentValidation
// ============================================================

namespace CoracaoEvangelho.API.DTOs.Request;

// ── Auth ──────────────────────────────────────────────────────
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

// ── Curso (admin) ─────────────────────────────────────────────
public record CursoRequestDto(
    string Titulo,
    string Descricao,
    string? CategoriaId,
    string ImagemUrl,
    string Instrutor,
    bool CertificadoDisponivel
);

// ── Aula (admin) ──────────────────────────────────────────────
public record AulaRequestDto(
    string Titulo,
    string? Descricao,
    string YoutubeVideoId,
    int DuracaoMinutos,
    int Ordem
);

// ── Matricula (inscrição do aluno) ────────────────────────────
// Espelha o formulário InscricaoCursoComponent
public record MatriculaRequestDto(
    string NomeCompleto,
    string Email,
    string? Telefone,
    EnderecoDto? Endereco
);

public record EnderecoDto(
    string? Cep,
    string? Logradouro,
    string? Cidade,
    string? Estado
);

// ── Progresso ─────────────────────────────────────────────────
// Marcação de aula concluída — POST /api/progresso
public record MarcarAulaConcluidaRequestDto(
    string CursoId,
    string AulaId
);

// ── Certificado ───────────────────────────────────────────────
// Emissão de certificado — POST /api/certificados
public record EmitirCertificadoRequestDto(
    string CursoId
);

// ── Pedido de Vibrações ───────────────────────────────────────
// Espelha formulário PedidoVibracoesComponent
public record PedidoVibracaoRequestDto(
    string Nome,
    string? Email,
    string Pedido,
    EnderecoDto? Endereco
);

// ── Admin — alteração de status de usuário ────────────────────
public record AlterarStatusUsuarioRequestDto(bool Ativo);

// ── Admin — atualização completa de usuário ───────────────────
public record AtualizarUsuarioAdminRequestDto(
    string Nome,
    string Email,
    string? AvatarUrl,
    string Role
);

// ── Usuario (perfil) ──────────────────────────────────────────
public record AtualizarPerfilRequestDto(
    string Nome,
    string? AvatarUrl
);

public record AlterarSenhaRequestDto(
    string SenhaAtual,
    string NovaSenha
);
