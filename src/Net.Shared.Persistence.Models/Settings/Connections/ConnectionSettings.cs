namespace Net.Shared.Persistence.Models.Settings.Connections
{
    public abstract record ConnectionSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string Database { get; set; } = null!;
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;

        public abstract string GetConnectionString();
    }
}
