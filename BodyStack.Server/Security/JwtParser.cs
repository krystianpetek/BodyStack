using System.IdentityModel.Tokens.Jwt;

namespace BodyStack.Server.Security;

public sealed class JwtParser
{
    public string ExtractUserId(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Token is not a valid JWT.", nameof(token), ex);
        }

        var userId = jwt.Claims.FirstOrDefault(c => string.Equals(c.Type, "id", StringComparison.Ordinal))?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("JWT token does not contain required claim 'id'.");
        }

        return userId;
    }
}
