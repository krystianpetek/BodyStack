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
}
