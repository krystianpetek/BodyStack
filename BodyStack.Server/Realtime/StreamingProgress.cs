namespace BodyStack.Server.Realtime;

/// <summary>
/// Represents the progress of a streaming operation
/// </summary>
/// <param name="ItemsProcessed">Number of items processed so far</param>
/// <param name="TotalItems">Total number of items (null if unknown)</param>
/// <param name="IsComplete">Whether the operation is complete</param>
public record StreamingProgress(
    int ItemsProcessed,
    int? TotalItems,
    bool IsComplete);
