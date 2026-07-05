using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    public class ProductCsv
    {
        [Name("ProductID")] public string ProductID { get; set; } = string.Empty;
        [Name("ProductName")] public string ProductName { get; set; } = string.Empty;
        [Name("Category")] public string Category { get; set; } = string.Empty;
        [Name("Price")] public string Price { get; set; } = string.Empty;
        [Name("Stock")] public string Stock { get; set; } = string.Empty;
    }
}
