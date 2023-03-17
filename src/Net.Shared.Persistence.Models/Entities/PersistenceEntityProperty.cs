using static Net.Shared.Persistence.Models.Constants.Enums;

namespace Net.Shared.Persistence.Models.Entities;

public sealed record PersistenceEntityProperty(string Name, object Value, ContextCommands Command);
