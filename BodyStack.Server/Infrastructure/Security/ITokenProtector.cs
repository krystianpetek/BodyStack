namespace BodyStack.Server.Infrastructure.Security;

public interface ITokenProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedText);
}
