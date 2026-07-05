using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    /// <summary>
    /// Representa la estructura de una ciudad extraída del archivo CSV.
    /// </summary>
    public class CityCsv
    {
        [Name("CityName")]
        public string CityName { get; set; } = string.Empty;

        [Name("CountryName")]
        public string CountryName { get; set; } = string.Empty;
    }
}
