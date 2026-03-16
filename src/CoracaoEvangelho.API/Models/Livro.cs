namespace CoracaoEvangelho.API.Models;

public class Livro
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Titulo { get; set; } = string.Empty;
    public string Subtitulo { get; set; } = string.Empty;
    public string Capa { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;

    // Navegação
    public ICollection<Capitulo> Capitulos { get; set; } = new List<Capitulo>();
}
