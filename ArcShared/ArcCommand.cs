namespace ArcShared;

public record ArcCommand(
    string Action,
    string ArcIdentifier,
    string Requester = "UNKNOWN"
    )
{
    public Dictionary<string, string> Parameters { get; init; } = new Dictionary<string, string>();
    public Guid Id { get; init; } = Guid.NewGuid();
}