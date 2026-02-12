using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BodyStack.Server.Tests.Integration;

public class SignalRHubTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;

    public SignalRHubTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Hub_Can_Connect()
    {
        // Arrange
        var connection = new HubConnectionBuilder()
            .WithUrl($"{_factory.Server.BaseAddress}hubs/fitatu-month")
            .Build();

        var connected = false;
        connection.Closed += _ =>
        {
            connected = false;
            return Task.CompletedTask;
        };

        // Act
        await connection.StartAsync();
        connected = connection.State == HubConnectionState.Connected;

        // Assert
        Assert.True(connected);

        // Cleanup
        await connection.StopAsync();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
