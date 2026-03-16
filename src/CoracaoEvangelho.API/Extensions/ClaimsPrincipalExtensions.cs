using System.Security.Claims;

namespace CoracaoEvangelho.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Retorna o userId da claim "userId" ou null se não autenticado.</summary>
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue("userId");

    /// <summary>Lança UnauthorizedAccessException se não houver userId.</summary>
    public static string GetUserIdOrThrow(this ClaimsPrincipal user)
        => user.GetUserId()
            ?? throw new UnauthorizedAccessException("Token inválido ou ausente.");
}
