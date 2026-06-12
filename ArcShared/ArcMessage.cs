namespace ArcShared;

public record ArcMessage(string Type, Guid CommandId, string? Payload, bool IsSuccess);