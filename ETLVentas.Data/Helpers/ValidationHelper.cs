namespace ETLVentas.Data.Helpers
{
    public static class ValidationHelper
    {
        public static string FormatError(string recordId, string errorType, string details)
        {
            return "Record: " + recordId + " | Error: " + errorType + " | Details: " + details;
        }
    }
}
