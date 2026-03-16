namespace CoracaoEvangelho.API.Models;

public class Usuario
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpira { get; set; }

    // Preferências
    public string Tema { get; set; } = "claro";
    public int TamanhoFonte { get; set; } = 16;

    // Navegação
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
