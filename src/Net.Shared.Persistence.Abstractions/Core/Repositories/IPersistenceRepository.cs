using Net.Shared.Persistence.Abstractions.Core.Repositories.Parts;
using Net.Shared.Persistence.Abstractions.Entities;

namespace Net.Shared.Persistence.Abstractions.Core.Repositories
{
    public interface IPersistenceRepository<T> where T : class, IPersistent
    {
        IPersistenceReaderRepository<T> Reader { get; }
        IPersistenceWriterRepository<T> Writer { get; }
    }
}
