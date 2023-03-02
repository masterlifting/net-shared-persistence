using Net.Shared.Exceptions;

namespace Net.Shared.Persistence.Exceptions;

public sealed class SharedPersistenceException : NetSharedException
{
    public SharedPersistenceException(string message) : base(message)
    {
    }

    public SharedPersistenceException(Exception exception) : base(exception)
    {
    }
}