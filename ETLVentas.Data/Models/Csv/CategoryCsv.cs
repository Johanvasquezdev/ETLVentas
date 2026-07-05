using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    /// <summary>
    /// Representa la estructura de una categoría extraída del archivo CSV.
    /// </summary>
    public class CategoryCsv
    {
        [Name("CategoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }
}
