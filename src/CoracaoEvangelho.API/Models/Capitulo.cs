namespace CoracaoEvangelho.API.Models;

public class Capitulo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LivroId { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;

    // Navegação
    public Livro Livro { get; set; } = null!;
    public ICollection<Versiculo> Versiculos { get; set; } = new List<Versiculo>();
}
