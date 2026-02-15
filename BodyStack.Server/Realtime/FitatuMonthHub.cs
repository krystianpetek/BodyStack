using Microsoft.AspNetCore.SignalR;

namespace BodyStack.Server.Realtime;

public sealed class FitatuMonthHub : Hub
{
    /// <summary>
    /// Joins the connection to a user-specific group for targeted progress updates.
    /// Call this immediately after connection is established.
    /// </summary>
    /// <param name="fitatuUserId">The Fitatu user ID to group by</param>
    public async Task JoinUserGroup(string fitatuUserId)
    {
        if (string.IsNullOrWhiteSpace(fitatuUserId))
        {
            throw new ArgumentException("FitatuUserId is required", nameof(fitatuUserId));
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{fitatuUserId}");
    }

    /// <summary>
    /// Leaves the user-specific group. Called automatically on disconnect.
    /// </summary>
    /// <param name="fitatuUserId">The Fitatu user ID</param>
    public async Task LeaveUserGroup(string fitatuUserId)
    {
        if (!string.IsNullOrWhiteSpace(fitatuUserId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{fitatuUserId}");
        }
    }

    /// <summary>
    /// Sends streaming progress update to the client
    /// </summary>
    /// <param name="progress">Progress information</param>
    public async Task SendProgressUpdate(StreamingProgress progress)
    {
        await Clients.Caller.SendAsync("StreamingProgress", progress);
    }

    /// <summary>
    /// Sends streaming progress update to a specific user group
    /// </summary>
    /// <param name="fitatuUserId">The Fitatu user ID</param>
    /// <param name="progress">Progress information</param>
    public async Task SendProgressToUserGroup(string fitatuUserId, StreamingProgress progress)
    {
        await Clients.Group($"user-{fitatuUserId}").SendAsync("StreamingProgress", progress);
    }

    /// <summary>
    /// Notifies clients that streaming operation has started
    /// </summary>
    /// <param name="operationId">Unique operation identifier</param>
    /// <param name="totalItems">Total number of items to process (if known)</param>
    public async Task NotifyStreamingStarted(string operationId, int? totalItems = null)
    {
        await Clients.Caller.SendAsync("StreamingStarted", new { OperationId = operationId, TotalItems = totalItems });
    }

    /// <summary>
    /// Notifies clients that streaming operation has completed
    /// </summary>
    /// <param name="operationId">Unique operation identifier</param>
    /// <param name="itemsProcessed">Total items processed</param>
    public async Task NotifyStreamingCompleted(string operationId, int itemsProcessed)
    {
        await Clients.Caller.SendAsync("StreamingCompleted", new { OperationId = operationId, ItemsProcessed = itemsProcessed });
    }

    /// <summary>
    /// Notifies clients that streaming operation was cancelled
    /// </summary>
    /// <param name="operationId">Unique operation identifier</param>
    /// <param name="itemsProcessed">Items processed before cancellation</param>
    public async Task NotifyStreamingCancelled(string operationId, int itemsProcessed)
    {
        await Clients.Caller.SendAsync("StreamingCancelled", new { OperationId = operationId, ItemsProcessed = itemsProcessed });
    }

    /// <summary>
    /// Notifies clients about streaming error
    /// </summary>
    /// <param name="operationId">Unique operation identifier</param>
    /// <param name="errorMessage">Error message</param>
    public async Task NotifyStreamingError(string operationId, string errorMessage)
    {
        await Clients.Caller.SendAsync("StreamingError", new { OperationId = operationId, Error = errorMessage });
    }
}
