using CoracaoEvangelho.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CoracaoEvangelho.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Capitulo> Capitulos => Set<Capitulo>();
    public DbSet<Versiculo> Versiculos => Set<Versiculo>();
    public DbSet<Devocional> Devocionais => Set<Devocional>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Livro ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Livro>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Subtitulo).HasMaxLength(300);
            e.Property(x => x.Capa).HasMaxLength(500);
            e.HasQueryFilter(x => !x.Deletado); // soft delete global
        });

        // ── Capitulo ──────────────────────────────────────────────────────
        modelBuilder.Entity<Capitulo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200);
            e.HasOne(x => x.Livro)
             .WithMany(x => x.Capitulos)
             .HasForeignKey(x => x.LivroId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => !x.Deletado);
            e.HasIndex(x => new { x.LivroId, x.Numero }).IsUnique();
        });

        // ── Versiculo ─────────────────────────────────────────────────────
        modelBuilder.Entity<Versiculo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Texto).HasColumnType("TEXT").IsRequired();
            e.HasOne(x => x.Capitulo)
             .WithMany(x => x.Versiculos)
             .HasForeignKey(x => x.CapituloId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => !x.Deletado);
            e.HasIndex(x => new { x.CapituloId, x.Numero }).IsUnique();
        });

        // ── Devocional ────────────────────────────────────────────────────
        modelBuilder.Entity<Devocional>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Passagem).HasMaxLength(200).IsRequired();
            e.Property(x => x.Reflexao).HasColumnType("TEXT").IsRequired();
            e.HasOne(x => x.Versiculo)
             .WithMany()
             .HasForeignKey(x => x.VersiculoId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => !x.Deletado);
            e.HasIndex(x => x.Data).IsUnique(); // 1 devocional por dia
        });

        // ── Usuario ───────────────────────────────────────────────────────
        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.SenhaHash).HasMaxLength(100).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).HasDefaultValue("user");
            e.HasIndex(x => x.Email).IsUnique();
        });

        // ── Favorito ──────────────────────────────────────────────────────
        modelBuilder.Entity<Favorito>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Usuario)
             .WithMany(x => x.Favoritos)
             .HasForeignKey(x => x.UsuarioId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Versiculo)
             .WithMany(x => x.Favoritos)
             .HasForeignKey(x => x.VersiculoId)
             .OnDelete(DeleteBehavior.Cascade);
            // Impede duplicata: mesmo usuário + mesmo versículo
            e.HasIndex(x => new { x.UsuarioId, x.VersiculoId }).IsUnique();
        });
    }
}
