using System.Threading.Channels;

namespace BodyStack.Server.Infrastructure.Background;

public interface IBackgroundTaskQueue<T>
{
    ValueTask QueueAsync(T item, CancellationToken cancellationToken = default);
    ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default);
}

public sealed class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ValueTask QueueAsync(T item, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(item, cancellationToken);

    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAsync(cancellationToken);
}
