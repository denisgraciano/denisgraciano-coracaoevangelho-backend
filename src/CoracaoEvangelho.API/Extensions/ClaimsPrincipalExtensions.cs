using System.Security.Claims;

namespace CoracaoEvangelho.API.Extensions;

/// <summary>
/// Extensões tipadas para ClaimsPrincipal — elimina User.FindFirstValue() espalhado nos controllers.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Retorna o userId do claim — lança UnauthorizedAccessException se ausente.
    /// Use em rotas com [Authorize] onde a claim é garantida pelo middleware JWT.
    /// </summary>
    public static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue("userId")
            ?? throw new UnauthorizedAccessException("userId não encontrado no token.");

    public static string GetEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("email não encontrado no token.");

    public static string GetRole(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Role) ?? "aluno";
}
