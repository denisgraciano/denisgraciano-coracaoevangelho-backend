// ============================================================
// Models/Entities.cs
// Todas as entidades EF Core do domínio real (plataforma espírita)
// Gerado a partir dos contratos TypeScript do frontend Angular
// ============================================================

namespace CoracaoEvangelho.API.Models;

// ── Usuario ───────────────────────────────────────────────────
// Espelha: interface Usuario { id, nome, email, avatarUrl? }
public class Usuario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "aluno";   // "aluno" | "admin"
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpira { get; set; }

    // Navegação
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Progresso> Progressos { get; set; } = new List<Progresso>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<PedidoVibracao> PedidosVibracao { get; set; } = new List<PedidoVibracao>();
}

// ── Categoria ─────────────────────────────────────────────────
// Espelha: categoria dos cursos (Doutrina, Prática Espírita…)
public class Categoria
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }

    // Navegação
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}

// ── Curso ─────────────────────────────────────────────────────
// Espelha: interface Curso (detalhes-curso/curso.model.ts) — modelo completo
// da página pública de detalhes do curso.
public class Curso
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? CategoriaId { get; set; }
    public string ImagemUrl { get; set; } = string.Empty;
    public string Instrutor { get; set; } = string.Empty;
    public bool CertificadoDisponivel { get; set; } = true;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Campos da página de detalhes públicos (DetalhesCursoComponent)
    public string? Duracao { get; set; }
    public string? ObjetivosJson { get; set; }           // JSON: string[]
    public string? ConteudoProgramaticoJson { get; set; } // JSON: string[]
    public string? RequisitosJson { get; set; }          // JSON: string[]
    public string? Certificacao { get; set; }
    public string? Modalidade { get; set; }
    public string? DataInicio { get; set; }
    public string? DataFim { get; set; }
    public string? Horario { get; set; }
    public int Vagas { get; set; } = 0;
    public string? Nivel { get; set; }
    public string? TagsJson { get; set; }                // JSON: string[]

    // Calculado — não persiste: int TotalAulas → via Aulas.Count()
    // VagasDisponiveis → Vagas - Matriculas.Count(m => m.Ativa)
    public Categoria? Categoria { get; set; }
    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Depoimento> Depoimentos { get; set; } = new List<Depoimento>();
}

// ── Depoimento ────────────────────────────────────────────────
// Espelha: depoimentos: { nome, comentario, nota }[] em Curso (curso.model.ts)
public class Depoimento
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CursoId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public int Nota { get; set; }  // 1–5

    // Navegação
    public Curso Curso { get; set; } = null!;
}

// ── Aula ──────────────────────────────────────────────────────
// Espelha: interface Aula { id, titulo, descricao, youtubeVideoId,
//           duracaoMinutos, ordem }
public class Aula
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CursoId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string YoutubeVideoId { get; set; } = string.Empty;  // apenas o ID, nunca a URL
    public int DuracaoMinutos { get; set; }
    public int Ordem { get; set; }
    public bool Ativa { get; set; } = true;

    // Navegação
    public Curso Curso { get; set; } = null!;
    public ICollection<Progresso> Progressos { get; set; } = new List<Progresso>();
}

// ── Matricula ─────────────────────────────────────────────────
// Representa a inscrição de um aluno em um curso
// Espelha: InscricaoService / formulário de inscrição do frontend
public class Matricula
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UsuarioId { get; set; }   // nullable: inscrição pública sem login
    public string CursoId { get; set; } = string.Empty;
    public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    public bool Ativa { get; set; } = true;

    // Dados do formulário de inscrição (InscricaoCursoComponent)
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public string? DataNascimento { get; set; }
    public string? Observacoes { get; set; }
    public bool AceitaTermos { get; set; }
    public bool ReceberEmails { get; set; }

    // Endereço (ViaCEP)
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    // Navegação
    public Usuario? Usuario { get; set; }   // nullable: inscrição pública sem conta
    public Curso Curso { get; set; } = null!;
}

// ── Progresso ─────────────────────────────────────────────────
// Espelha: interface ProgressoAula { aulaId, concluida, dataConlusao? }
// e ProgressoCurso (calculado no Service)
public class Progresso
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; } = string.Empty;
    public string AulaId { get; set; } = string.Empty;
    public string CursoId { get; set; } = string.Empty;  // desnormalizado p/ queries rápidas
    public bool Concluida { get; set; } = false;
    public DateTime? DataConclusao { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public Aula Aula { get; set; } = null!;
}

// ── Certificado ───────────────────────────────────────────────
// Espelha: interface Certificado { id, cursoId, cursoTitulo,
//           alunoNome, dataEmissao, cargaHoraria }
public class Certificado
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; } = string.Empty;
    public string CursoId { get; set; } = string.Empty;
    public string CursoTitulo { get; set; } = string.Empty;   // desnormalizado: nome pode mudar
    public string AlunoNome { get; set; } = string.Empty;     // idem
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public decimal CargaHoraria { get; set; }                  // em horas

    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public Curso Curso { get; set; } = null!;
}

// ── PedidoVibracao ────────────────────────────────────────────
// Espelha: formulário pedido-vibracoes do frontend
public class PedidoVibracao
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UsuarioId { get; set; }   // nullable: envio anônimo permitido
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string Pedido { get; set; } = string.Empty;
    // Endereço (ViaCEP)
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool Lido { get; set; } = false;

    // Navegação
    public Usuario? Usuario { get; set; }
}
