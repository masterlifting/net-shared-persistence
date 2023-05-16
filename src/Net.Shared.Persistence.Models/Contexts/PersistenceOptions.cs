namespace Net.Shared.Persistence.Models.Contexts
{
    public sealed class PersistenceOptions
    {
        public int Limit { get; set; }
        public string? OrderSelector { get; set; }
        public bool OrderIsAsc { get; set; }
    }
}
