namespace Net.Shared.Persistence.Models;

public static class Constants
{
    public static class Enums
    {
        public enum ProcessStatuses
        {
            Error = -1,
            None = 1,
            Draft,
            Ready,
            Processing,
            Processed,
            Completed
        }
    }
}
