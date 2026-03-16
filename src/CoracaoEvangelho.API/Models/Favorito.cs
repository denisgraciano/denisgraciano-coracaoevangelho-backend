namespace CoracaoEvangelho.API.Models;

public class Favorito
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UsuarioId { get; set; } = string.Empty;
    public string VersiculoId { get; set; } = string.Empty;
    public DateTime DataSalvo { get; set; } = DateTime.UtcNow;

    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public Versiculo Versiculo { get; set; } = null!;
}
