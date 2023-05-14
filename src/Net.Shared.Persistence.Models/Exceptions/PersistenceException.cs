namespace Net.Shared.Persistence.Models.Exceptions;

public sealed class PersistenceException : NetSharedException
{
    public PersistenceException(string message) : base(message)
    {
    }

    public PersistenceException(Exception exception) : base(exception)
    {
    }
}