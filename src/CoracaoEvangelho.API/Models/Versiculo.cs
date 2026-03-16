namespace CoracaoEvangelho.API.Models;

public class Versiculo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CapituloId { get; set; } = string.Empty;
    public int Numero { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;

    // Navegação
    public Capitulo Capitulo { get; set; } = null!;
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
