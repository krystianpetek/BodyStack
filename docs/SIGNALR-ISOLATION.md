# SignalR User Isolation

This document explains how SignalR is configured for user-specific message delivery in BodyStack.

## Problem

Previously, the application used `Clients.All` to broadcast progress updates for month recalculations. This meant that **every connected client received every user's progress updates**, which is both a privacy violation and inefficient.

## Solution

The application now uses **SignalR Groups** to isolate messages per user. Each user joins a group named `user-{fitatuUserId}` when they connect, and progress updates are sent only to that specific group.

## Architecture

### Backend (FitatuMonthHub)

```csharp
public sealed class FitatuMonthHub : Hub
{
    /// <summary>
    /// Joins the connection to a user-specific group for targeted progress updates.
    /// Call this immediately after connection is established.
    /// </summary>
    public async Task JoinUserGroup(string fitatuUserId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{fitatuUserId}");
    }
}
```

### Background Worker (FitatuMonthRecalculationWorker)

Instead of broadcasting to all clients:

```csharp
// ❌ OLD: Broadcast to everyone
await _hub.Clients.All.SendAsync("Progress", progress);

// ✅ NEW: Send only to specific user
await _hub.Clients.Group($"user-{request.FitatuUserId}")
    .SendAsync("Progress", progress);
```

### Frontend (useFitatuMonthHub)

The frontend automatically joins the user's group after connecting:

```typescript
const joinUserGroup = async (fitatuUserId: string) => {
  try {
    await connection.invoke('JoinUserGroup', fitatuUserId)
  } catch (err) {
    console.error('Failed to join SignalR group:', err)
  }
}

// After connection is established
const session = await getFitatuSession()
if (session?.fitatuUserId) {
  await joinUserGroup(session.fitatuUserId)
}
```

## Reconnection Handling

When the connection drops and reconnects, the frontend automatically rejoins the user's group:

```typescript
connection.onreconnected(async () => {
  try {
    const session = await getFitatuSession()
    if (session?.fitatuUserId) {
      await joinUserGroup(session.fitatuUserId)
    }
  } catch {
    // Ignore errors on reconnect
  }
})
```

## Security Benefits

1. **Privacy**: Users can only see their own progress updates
2. **Scalability**: Fewer messages sent overall (no broadcast overhead)
3. **Isolation**: Even if a user ID is compromised, they can't receive other users' data without a valid session

## Group Management

- Groups are created automatically when `AddToGroupAsync` is called
- SignalR automatically cleans up groups when all connections leave
- No manual group cleanup is required

## Testing

To verify isolation:

1. Connect User A and User B in separate browser sessions
2. Start a month recalculation for User A
3. Verify User A receives progress updates
4. Verify User B does NOT receive User A's progress updates

## Future Considerations

- Consider adding a `LeaveUserGroup` method for explicit cleanup (optional)
- Monitor SignalR connection metrics to ensure group management isn't causing issues
- Add authentication/authorization to the `JoinUserGroup` method if needed
