namespace Net.Shared.Persistence;

public static class Constants
{
    public static class Actions
    {
        internal const string Success = "Success";

        public const string Created = "Created";
        public const string Updated = "Updated";
        public const string Deleted = "Deleted";
        public const string NoData = "Data was not found";
    }
    public static class Enums
    {
        public enum Comparisons
        {
            Equal,
            More,
            Less
        }
    }
}
