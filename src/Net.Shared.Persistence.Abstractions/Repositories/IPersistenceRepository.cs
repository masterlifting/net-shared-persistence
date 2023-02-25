using Net.Shared.Persistence.Abstractions.Entities;
using Net.Shared.Persistence.Abstractions.Repositories.Parts;

namespace Net.Shared.Persistence.Abstractions.Repositories
{
    public interface IPersistenceRepository<T> where T : IPersistent
    {
        IPersistenceReaderRepository<T> Reader { get; }
        IPersistenceWriterRepository<T> Writer { get; }
    }
}
