namespace CoracaoEvangelho.API.Models;

public class Devocional
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateOnly Data { get; set; }
    public string Passagem { get; set; } = string.Empty;
    public string Reflexao { get; set; } = string.Empty;
    public string VersiculoId { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public bool Deletado { get; set; } = false;

    // Navegação
    public Versiculo Versiculo { get; set; } = null!;
}
