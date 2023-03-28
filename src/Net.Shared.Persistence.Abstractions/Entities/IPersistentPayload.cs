namespace Net.Shared.Persistence.Abstractions.Entities;

public interface IPersistentPayload : IPersistent
{
    byte[] Payload { get; set; }
    byte[] PayloadHash { get; set; }
    string PayloadSource { get; set; }
    string PayloadContentType { get; set; }
    string PayloadHashAlgoritm { get; set; }
}