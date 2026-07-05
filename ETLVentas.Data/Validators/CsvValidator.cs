using System.IO;

namespace ETLVentas.Data.Validators
{
    public class CsvValidator
    {
        public bool ValidateFile(string filePath, out string errorMessage)
        {
            if (!File.Exists(filePath))
            {
                errorMessage = "File does not exist: " + filePath;
                return false;
            }
            if (Path.GetExtension(filePath).ToLower() != ".csv")
            {
                errorMessage = "File is not a CSV: " + filePath;
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }
    }
}
