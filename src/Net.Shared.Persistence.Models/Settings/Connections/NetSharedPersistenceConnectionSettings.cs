using Net.Shared.Models.Settings;

namespace Net.Shared.Persistence.Models.Settings.Connections
{
    public abstract record NetSharedPersistenceConnectionSettings : ConnectionSettings
    {
        public string Database { get; set; } = null!;
        public abstract string GetConnectionString();
    }
}
