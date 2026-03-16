using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoracaoEvangelho.API.DTOs.Request;
using CoracaoEvangelho.API.DTOs.Response;
using CoracaoEvangelho.API.Models;
using CoracaoEvangelho.API.Repositories.Interfaces;
using CoracaoEvangelho.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CoracaoEvangelho.API.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _config;

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration config)
    {
        _usuarioRepo = usuarioRepo;
        _config = config;
    }

    public async Task<AuthResponseDto> RegistrarAsync(
        RegisterRequestDto dto, CancellationToken ct = default)
    {
        var emailNorm = dto.Email.ToLower().Trim();

        var existente = await _usuarioRepo.GetByEmailAsync(emailNorm, ct);
        if (existente is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var usuario = new Usuario
        {
            Email = emailNorm,
            Nome = dto.Nome.Trim(),
            // BCrypt com work factor 12 — seguro e rápido o suficiente para web
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha, workFactor: 12),
            Role = "user",
            DataCadastro = DateTime.UtcNow
        };

        GerarRefreshToken(usuario);
        await _usuarioRepo.AddAsync(usuario, ct);
        await _usuarioRepo.SaveChangesAsync(ct);

        return GerarAuthResponse(usuario);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto dto, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(dto.Email.ToLower(), ct)
            ?? throw new UnauthorizedAccessException("Credenciais inválidas.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        GerarRefreshToken(usuario);
        await _usuarioRepo.SaveChangesAsync(ct);

        return GerarAuthResponse(usuario);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var usuario = await _usuarioRepo.GetByRefreshTokenAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        GerarRefreshToken(usuario);
        await _usuarioRepo.SaveChangesAsync(ct);

        return GerarAuthResponse(usuario);
    }

    // ── Helpers privados ───────────────────────────────────────────────────

    private AuthResponseDto GerarAuthResponse(Usuario usuario)
    {
        var (token, expira) = GerarJwt(usuario);
        var usuarioDto = new UsuarioResponseDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Role);
        return new AuthResponseDto(token, usuario.RefreshToken!, expira, usuarioDto);
    }

    private (string token, DateTime expira) GerarJwt(Usuario usuario)
    {
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key não configurada.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Role),
            new Claim("userId", usuario.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    private static void GerarRefreshToken(Usuario usuario)
    {
        // Token criptograficamente seguro — 64 bytes → 88 chars base64
        usuario.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        usuario.RefreshTokenExpira = DateTime.UtcNow.AddDays(7);
    }
}

// ── SyncService ───────────────────────────────────────────────────────────
public class SyncService : ISyncService
{
    private readonly ILivroRepository _livroRepo;

    public SyncService(ILivroRepository livroRepo) => _livroRepo = livroRepo;

    public async Task<IEnumerable<SyncLivroDto>> GetLivrosSincronizadosAsync(
        DateTime atualizadoApos, CancellationToken ct = default)
    {
        var livros = await _livroRepo.GetAtualizadosAposAsync(atualizadoApos, ct);
        return livros.Select(l => new SyncLivroDto(
            l.Id, l.Titulo, l.Subtitulo, l.Capa, l.Deletado, l.AtualizadoEm));
    }
}
