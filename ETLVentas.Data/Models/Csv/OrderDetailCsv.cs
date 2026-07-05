using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    public class OrderDetailCsv
    {
        [Name("OrderID")] public string OrderID { get; set; } = string.Empty;
        [Name("ProductID")] public string ProductID { get; set; } = string.Empty;
        [Name("Quantity")] public string Quantity { get; set; } = string.Empty;
        [Name("TotalPrice")] public string TotalPrice { get; set; } = string.Empty;
    }
}
