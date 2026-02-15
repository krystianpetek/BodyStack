using System.Reactive.Linq;
using BodyStack.Server.Integrations.Fitatu;
using BodyStack.Server.Security;

namespace BodyStack.Server.Application.Fitatu;

public sealed class FitatuLoginUseCase
{
    private readonly IFitatuClient _fitatuClient;
    private readonly JwtParser _jwtParser;
    private readonly IFitatuSessionRepository _sessions;

    public FitatuLoginUseCase(IFitatuClient fitatuClient, JwtParser jwtParser, IFitatuSessionRepository sessions)
    {
        _fitatuClient = fitatuClient;
        _jwtParser = jwtParser;
        _sessions = sessions;
    }

    public async Task<string> ExecuteAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var login = await _fitatuClient.LoginAsync(username, password, cancellationToken);
        var fitatuUserId = _jwtParser.ExtractUserId(login.Token);

        await _sessions.UpsertAsync(fitatuUserId, login.Token, login.RefreshToken, cancellationToken);

        return fitatuUserId;
    }
}
