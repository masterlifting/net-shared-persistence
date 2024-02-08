namespace Net.Shared.Persistence.Abstractions;

public static class Constants
{
    public static class Enums
    {
        public enum ProcessStatuses
        {
            Error = -1,
            Draft = 1,
            Ready,
            Processing,
            Processed,
            Completed
        }

        public static readonly Dictionary<int, string> ProcessStatusesMap = new()
        {
            { (int)ProcessStatuses.Error, "Error" },
            { (int)ProcessStatuses.Draft, "Draft" },
            { (int)ProcessStatuses.Ready, "Ready" },
            { (int)ProcessStatuses.Processing, "Processing" },
            { (int)ProcessStatuses.Processed, "Processed" },
            { (int)ProcessStatuses.Completed, "Completed" }
        };
    }
}
