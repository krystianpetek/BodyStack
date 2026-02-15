using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace BodyStack.Server.Infrastructure.Http.Resilience;

/// <summary>
/// Factory for creating resilience policies using Polly
/// </summary>
public static class ResiliencePolicyFactory
{
    /// <summary>
    /// Creates a retry policy with exponential backoff for HTTP requests
    /// </summary>
    public static AsyncRetryPolicy CreateRetryPolicy(
        int maxRetries = 3,
        TimeSpan? baseDelay = null)
    {
        var delay = baseDelay ?? TimeSpan.FromSeconds(1);

        return Policy
            .Handle<HttpRequestException>(ex => IsRetryableException(ex))
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(delay.TotalSeconds * Math.Pow(2, retryAttempt - 1)));
    }

    /// <summary>
    /// Creates a timeout policy for HTTP requests
    /// </summary>
    public static AsyncTimeoutPolicy CreateTimeoutPolicy(TimeSpan? timeout = null)
    {
        var timeoutDuration = timeout ?? TimeSpan.FromSeconds(30);
        return Policy.TimeoutAsync(timeoutDuration, TimeoutStrategy.Pessimistic);
    }

    /// <summary>
    /// Creates a combined policy for HTTP operations
    /// </summary>
    public static IAsyncPolicy CreateCombinedPolicy(
        int maxRetries = 3,
        TimeSpan? timeout = null)
    {
        var retryPolicy = CreateRetryPolicy(maxRetries);
        var timeoutPolicy = CreateTimeoutPolicy(timeout ?? TimeSpan.FromSeconds(30));
        
        // Wrap: timeout (outer) -> retry (inner)
        return Policy.WrapAsync(timeoutPolicy, retryPolicy);
    }

    /// <summary>
    /// Creates a policy for streaming operations with longer timeout
    /// </summary>
    public static IAsyncPolicy CreateStreamingPolicy()
    {
        return CreateCombinedPolicy(maxRetries: 3, timeout: TimeSpan.FromMinutes(5));
    }

    private static bool IsRetryableException(HttpRequestException ex)
    {
        if (ex.StatusCode.HasValue)
        {
            var code = (int)ex.StatusCode.Value;
            return code >= 500 || code == 408 || code == 429;
        }
        return true;
    }
}
