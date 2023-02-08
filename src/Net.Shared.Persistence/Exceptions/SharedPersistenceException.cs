using Shared.Exceptions.Abstractions;
using Shared.Exceptions.Models;

namespace Net.Shared.Persistence.Exceptions;

public sealed class SharedPersistenceException : SharedException
{
    public SharedPersistenceException(string initiator, string action, ExceptionDescription description) : base(initiator, action, description) { }
}