using Microsoft.AspNetCore.DataProtection;

namespace BodyStack.Server.Infrastructure.Security;

public sealed class TokenProtector : ITokenProtector
{
    private readonly IDataProtector _protector;

    public TokenProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("BodyStack.FitatuTokens.v1");
    }

    public string Protect(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            throw new ArgumentException("Value is required.", nameof(plaintext));
        }

        return _protector.Protect(plaintext);
    }

    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrWhiteSpace(protectedText))
        {
            throw new ArgumentException("Value is required.", nameof(protectedText));
        }

        return _protector.Unprotect(protectedText);
    }
}
