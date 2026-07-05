using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    /// <summary>
    /// Representa la estructura de un país extraída del archivo CSV.
    /// </summary>
    public class CountryCsv
    {
        [Name("CountryName")]
        public string CountryName { get; set; } = string.Empty;
    }
}
