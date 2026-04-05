namespace CoracaoEvangelho.API.DTOs.Response;

// ── Wrapper global ────────────────────────────────────────────
public record ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public IEnumerable<string> Errors { get; init; } = [];

    public static ApiResponse<T> Ok(T data, string message = "") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}

// ── Paginação ─────────────────────────────────────────────────
public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int TotalItens,
    int Pagina,
    int TamanhoPagina,
    int TotalPaginas,
    bool TemProxima,
    bool TemAnterior
);

// ── Auth ──────────────────────────────────────────────────────
public record AuthResponseDto(
    string AccessToken,
    DateTime Expira,
    string RefreshToken,
    UsuarioResponseDto Usuario
);

// ── Usuario ───────────────────────────────────────────────────
// Espelha: interface Usuario { id, nome, email, avatarUrl? }
// SenhaHash: NUNCA exposta em nenhum DTO de resposta
public record UsuarioResponseDto(
    string Id,
    string Nome,
    string Email,
    string? AvatarUrl
);

// ── Categoria ─────────────────────────────────────────────────
// home.component.html: {{ categoria.totalCursos }} cursos disponíveis
public record CategoriaResponseDto(
    string Id,
    string Nome,
    string? Descricao,
    string? Icone,
    int TotalCursos
);

// ── Depoimento ────────────────────────────────────────────────
// Espelha: depoimentos: { nome, comentario, nota }[] em Curso (curso.model.ts)
public record DepoimentoResponseDto(
    string Nome,
    string Comentario,
    int Nota
);

// ── Curso completo (com aulas e depoimentos) ──────────────────
// Espelha: interface Curso (detalhes-curso/curso.model.ts) — todos os campos
// usados por DetalhesCursoComponent na rota /curso/:id.
// imagemUrl: mesmo nome que no admin (não "imagem") — atualizado no modelo Angular
// vagasDisponiveis: computado como Vagas - matriculas ativas
public record CursoResponseDto(
    string Id,
    string Titulo,
    string Categoria,
    string? CategoriaId,
    string Instrutor,
    string? Duracao,
    string Descricao,
    IEnumerable<string> Objetivos,
    IEnumerable<string> ConteudoProgramatico,
    IEnumerable<string> Requisitos,
    string? Certificacao,
    string? Modalidade,
    string? DataInicio,
    string? DataFim,
    string? Horario,
    int Vagas,
    int VagasDisponiveis,
    string ImagemUrl,
    string? Nivel,
    IEnumerable<string> Tags,
    IEnumerable<DepoimentoResponseDto> Depoimentos,
    int TotalAulas,
    bool CertificadoDisponivel,
    IEnumerable<AulaResponseDto> Aulas
);

// ── Curso resumo (sem aulas) ──────────────────────────────────
// Usado em listagens para manter payload leve (HomeComponent)
// categoriaId: opcional — necessário para o admin saber qual categoria editar
public record CursoResumoResponseDto(
    string Id,
    string Titulo,
    string Descricao,
    string Categoria,
    string? CategoriaId,
    string ImagemUrl,
    string Instrutor,
    int TotalAulas,
    bool CertificadoDisponivel
);

// ── Aula ──────────────────────────────────────────────────────
// Espelha: interface Aula { id, titulo, descricao, youtubeVideoId, duracaoMinutos, ordem }
// youtubeVideoId: APENAS o ID (ex: "dQw4w9WgXcQ"), nunca a URL completa
// O PlayerAulaComponent constrói o embedUrl via DomSanitizer:
//   `https://www.youtube.com/embed/${aula.youtubeVideoId}?rel=0&modestbranding=1`
public record AulaResponseDto(
    string Id,
    string Titulo,
    string? Descricao,
    string YoutubeVideoId,
    int DuracaoMinutos,
    int Ordem
);

// ── Matrícula — check de inscrição ───────────────────────────
// Espelha: { matriculado: boolean } — usado pelo DetalhesCursoComponent
public record MatriculaCheckResponseDto(bool Matriculado);

// ── Matrícula ─────────────────────────────────────────────────
public record MatriculaResponseDto(
    string Id,
    string CursoId,
    string CursoTitulo,
    DateTime DataMatricula,
    bool Ativa
);

// ── Progresso por aula ────────────────────────────────────────
// Espelha: interface ProgressoAula { aulaId, concluida, dataConlusao? }
// Nota: o Angular usa "dataConlusao" (com typo) — mantemos o contrato no frontend,
// mas no backend usamos o nome correto "DataConclusao"
public record ProgressoAulaResponseDto(
    string AulaId,
    bool Concluida,
    string? DataConclusao
);

// ── Progresso do curso completo ───────────────────────────────
// Espelha: interface ProgressoCurso { cursoId, aulasProgresso,
//           percentualConcluido, dataConclusao?, certificadoEmitido }
public record ProgressoCursoResponseDto(
    string CursoId,
    IEnumerable<ProgressoAulaResponseDto> AulasProgresso,
    int PercentualConcluido,
    string? DataConclusao,
    bool CertificadoEmitido
);

// ── Certificado ───────────────────────────────────────────────
// Espelha: interface Certificado { id, cursoId, cursoTitulo, alunoNome,
//           dataEmissao, cargaHoraria }
// dataEmissao: ISO string → {{ cert.dataEmissao | date:'dd/MM/yyyy' }} funciona direto
public record CertificadoResponseDto(
    string Id,
    string CursoId,
    string CursoTitulo,
    string AlunoNome,
    string DataEmissao,
    decimal CargaHoraria
);

// ── Admin — Usuario completo ───────────────────────────────────
// Inclui Role e Ativo — nunca exposto fora de rotas [admin]
public record UsuarioAdminDto(
    string Id,
    string Nome,
    string Email,
    string? AvatarUrl,
    string Role,
    DateTime DataCadastro,
    bool Ativo
);

// ── Admin — Curso completo (inclui inativos) ───────────────────
// Espelha: interface CursoDetalhe extends CursoResumo + campos admin extras
// categoria: campo nomeado igual ao do CursoResumo do Angular (não CategoriaNome)
public record CursoAdminResponseDto(
    string Id,
    string Titulo,
    string Descricao,
    string? CategoriaId,
    string? Categoria,
    string ImagemUrl,
    string Instrutor,
    bool CertificadoDisponivel,
    bool Ativo,
    int TotalAulas,
    DateTime CriadoEm,
    DateTime AtualizadoEm,
    IEnumerable<AulaAdminResponseDto> Aulas
);

// ── Admin — Aula completa (inclui inativas) ────────────────────
public record AulaAdminResponseDto(
    string Id,
    string CursoId,
    string Titulo,
    string? Descricao,
    string YoutubeVideoId,
    int DuracaoMinutos,
    int Ordem,
    bool Ativa
);

// ── Matrícula — listagem admin ─────────────────────────────────
// DTO separado de MatriculaResponseDto para não quebrar o contrato
// existente com o frontend do aluno.
public record MatriculaAdminDto(
    string Id,
    string UsuarioId,
    string UsuarioNome,
    string UsuarioEmail,
    string CursoId,
    string CursoTitulo,
    DateTime DataMatricula,
    bool Ativa
);

// ── Pedido de Vibrações — resposta ao aluno ───────────────────
public record PedidoVibracaoResponseDto(
    string Id,
    string Mensagem
);

// ── Pedido de Vibrações — listagem admin ──────────────────────
// DTO separado para evitar conflito de nome com Models.PedidoVibracao
public record PedidoVibracaoAdminDto(
    string Id,
    string Nome,
    string? Email,
    string Pedido,
    string? Cidade,
    string? Estado,
    DateTime CriadoEm,
    bool Lido
);
