namespace ETLVentas.Data.Validators
{
    public class DataCleaner
    {
        public string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return input.Trim();
        }
    }
}
