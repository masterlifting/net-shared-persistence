using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Contexts
{
    public interface IPersistenceMongoContext : IPersistenceContext<IPersistentNoSql>
    {
    }
}
