namespace Net.Shared.Persistence.Abstractions.Interfaces.Entities;

public interface IPersistentPayload : IPersistent
{
    byte[] Payload { get; set; }
    byte[] PayloadHash { get; set; }
    string PayloadSource { get; set; }
    string PayloadContentType { get; set; }
    string PayloadHashAlgorithm { get; set; }
}