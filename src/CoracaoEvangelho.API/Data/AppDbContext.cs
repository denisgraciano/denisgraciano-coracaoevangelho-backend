// ============================================================
// Data/AppDbContext.cs
// Contexto EF Core para o domínio real (plataforma espírita)
// ============================================================

using CoracaoEvangelho.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CoracaoEvangelho.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Curso> Cursos => Set<Curso>();
    public DbSet<Aula> Aulas => Set<Aula>();
    public DbSet<Depoimento> Depoimentos => Set<Depoimento>();
    public DbSet<Matricula> Matriculas => Set<Matricula>();
    public DbSet<Progresso> Progressos => Set<Progresso>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<PedidoVibracao> PedidosVibracao => Set<PedidoVibracao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Usuario ────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.SenhaHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("aluno");
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
            e.Property(x => x.Ativo).HasDefaultValue(true);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ── Categoria ──────────────────────────────────────────
        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(500);
            e.Property(x => x.Icone).HasMaxLength(500);
        });

        // ── Curso ──────────────────────────────────────────────
        modelBuilder.Entity<Curso>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descricao).HasColumnType("TEXT").IsRequired();
            e.Property(x => x.ImagemUrl).HasMaxLength(500);
            e.Property(x => x.Instrutor).HasMaxLength(150);
            // Campos da página de detalhes públicos
            e.Property(x => x.Duracao).HasMaxLength(100);
            e.Property(x => x.ObjetivosJson).HasColumnType("TEXT");
            e.Property(x => x.ConteudoProgramaticoJson).HasColumnType("TEXT");
            e.Property(x => x.RequisitosJson).HasColumnType("TEXT");
            e.Property(x => x.Certificacao).HasMaxLength(200);
            e.Property(x => x.Modalidade).HasMaxLength(50);
            e.Property(x => x.DataInicio).HasMaxLength(10);
            e.Property(x => x.DataFim).HasMaxLength(10);
            e.Property(x => x.Horario).HasMaxLength(100);
            e.Property(x => x.Vagas).HasDefaultValue(0);
            e.Property(x => x.Nivel).HasMaxLength(50);
            e.Property(x => x.TagsJson).HasColumnType("TEXT");
            e.HasOne(x => x.Categoria)
             .WithMany(x => x.Cursos)
             .HasForeignKey(x => x.CategoriaId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.Ativo);  // soft delete via flag
        });

        // ── Depoimento ─────────────────────────────────────────
        modelBuilder.Entity<Depoimento>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Comentario).HasColumnType("TEXT").IsRequired();
            e.Property(x => x.Nota).IsRequired();
            e.HasOne(x => x.Curso)
             .WithMany(x => x.Depoimentos)
             .HasForeignKey(x => x.CursoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Aula ───────────────────────────────────────────────
        modelBuilder.Entity<Aula>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(1000);
            e.Property(x => x.YoutubeVideoId).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Curso)
             .WithMany(x => x.Aulas)
             .HasForeignKey(x => x.CursoId)
             .OnDelete(DeleteBehavior.Cascade);
            // Garante que dois aluno não têm ordem igual no mesmo curso
            e.HasIndex(x => new { x.CursoId, x.Ordem }).IsUnique();
            e.HasQueryFilter(x => x.Ativa);
        });

        // ── Matricula ──────────────────────────────────────────
        modelBuilder.Entity<Matricula>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.NomeCompleto).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Telefone).HasMaxLength(20);
            e.Property(x => x.Cpf).HasMaxLength(14);
            e.Property(x => x.DataNascimento).HasMaxLength(10);
            e.Property(x => x.Observacoes).HasColumnType("TEXT");
            e.Property(x => x.Cep).HasMaxLength(10);
            e.Property(x => x.Logradouro).HasMaxLength(300);
            e.Property(x => x.Numero).HasMaxLength(20);
            e.Property(x => x.Complemento).HasMaxLength(100);
            e.Property(x => x.Bairro).HasMaxLength(100);
            e.Property(x => x.Cidade).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(2);
            // UsuarioId nullable: inscrição pública não exige conta
            e.HasOne(x => x.Usuario)
             .WithMany(x => x.Matriculas)
             .HasForeignKey(x => x.UsuarioId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Curso)
             .WithMany(x => x.Matriculas)
             .HasForeignKey(x => x.CursoId)
             .OnDelete(DeleteBehavior.Cascade);
            // Impede dupla inscrição do mesmo e-mail no mesmo curso
            e.HasIndex(x => new { x.Email, x.CursoId }).IsUnique();
        });

        // ── Progresso ──────────────────────────────────────────
        modelBuilder.Entity<Progresso>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Usuario)
             .WithMany(x => x.Progressos)
             .HasForeignKey(x => x.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Aula)
             .WithMany(x => x.Progressos)
             .HasForeignKey(x => x.AulaId)
             .OnDelete(DeleteBehavior.Cascade);
            // Índice único: um progresso por usuário+aula
            e.HasIndex(x => new { x.UsuarioId, x.AulaId }).IsUnique();
            // Índice de cobertura para queries de progresso por curso
            e.HasIndex(x => new { x.UsuarioId, x.CursoId });
        });

        // ── Certificado ────────────────────────────────────────
        modelBuilder.Entity<Certificado>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CursoTitulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.AlunoNome).HasMaxLength(150).IsRequired();
            e.Property(x => x.CargaHoraria).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Usuario)
             .WithMany(x => x.Certificados)
             .HasForeignKey(x => x.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Curso)
             .WithMany()
             .HasForeignKey(x => x.CursoId)
             .OnDelete(DeleteBehavior.Restrict);  // nunca deletar curso com certificados
            // Um certificado por usuário+curso
            e.HasIndex(x => new { x.UsuarioId, x.CursoId }).IsUnique();
        });

        // ── PedidoVibracao ─────────────────────────────────────
        modelBuilder.Entity<PedidoVibracao>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Pedido).HasColumnType("TEXT").IsRequired();
            e.Property(x => x.Cep).HasMaxLength(10);
            e.Property(x => x.Logradouro).HasMaxLength(300);
            e.Property(x => x.Cidade).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(2);
            e.HasOne(x => x.Usuario)
             .WithMany(x => x.PedidosVibracao)
             .HasForeignKey(x => x.UsuarioId)
             .OnDelete(DeleteBehavior.SetNull);  // pedido sobrevive se usuario deletar conta
        });
    }
}
