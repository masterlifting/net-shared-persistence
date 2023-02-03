using Shared.Persistence.Abstractions.Entities;
using Shared.Persistence.Abstractions.Repositories.Parts;

namespace Shared.Persistence.Abstractions.Repositories
{
    public interface IPersistenceRepository<T> where T : class, IPersistent
    {
        IPersistenceReaderRepository<T> Reader { get; }
        IPersistenceWriterRepository<T> Writer { get; }
    }
}
