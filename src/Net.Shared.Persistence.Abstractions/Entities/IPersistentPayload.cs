namespace Shared.Persistence.Abstractions.Entities;

public interface IPersistentPayload : IPersistent
{
    byte[] Payload { get; init; }
    byte[] PayloadHash { get; init; }
    string PayloadSource { get; init; }
    string PayloadContentType { get; init; }
    string PayloadHashAlgoritm { get; init; }
}