using System.Text;
using BodyStack.Server.Security;
using Xunit;

namespace BodyStack.Server.Tests.Security;

public sealed class JwtParserTests
{
    [Fact]
    public void ExtractUserId_WhenTokenContainsIdClaim_ReturnsId()
    {
        var token = CreateUnsignedJwt(payloadJson: "{\"id\":\"user-123\"}");

        var sut = new JwtParser();
        var userId = sut.ExtractUserId(token);

        Assert.Equal("user-123", userId);
    }

    [Fact]
    public void ExtractUserId_WhenTokenDoesNotContainIdClaim_Throws()
    {
        var token = CreateUnsignedJwt(payloadJson: "{\"sub\":\"abc\"}");

        var sut = new JwtParser();

        Assert.Throws<InvalidOperationException>(() => sut.ExtractUserId(token));
    }

    private static string CreateUnsignedJwt(string payloadJson)
    {
        const string headerJson = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        var header = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));
        return $"{header}.{payload}.";
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
