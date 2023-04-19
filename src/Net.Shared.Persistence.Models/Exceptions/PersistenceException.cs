namespace Net.Shared.Persistence.Models.Exceptions;

public sealed class PersistenceException : Net.Shared.Exception
{
    public PersistenceException(string message) : base(message)
    {
    }

    public PersistenceException(System.Exception exception) : base(exception)
    {
    }
}