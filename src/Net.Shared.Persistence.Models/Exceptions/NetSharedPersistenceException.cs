using Net.Shared.Exceptions;

namespace Net.Shared.Persistence.Models.Exceptions;

public sealed class NetSharedPersistenceException : NetSharedException
{
    public NetSharedPersistenceException(string message) : base(message)
    {
    }

    public NetSharedPersistenceException(Exception exception) : base(exception)
    {
    }
}