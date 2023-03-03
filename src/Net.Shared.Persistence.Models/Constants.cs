namespace Net.Shared.Persistence.Models;

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
            Processed
        }
        public enum ContextCommands
        {
            Set,
            Inc
        }
    }
}
