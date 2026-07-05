using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    public class OrderCsv
    {
        [Name("OrderID")] public string OrderID { get; set; } = string.Empty;
        [Name("CustomerID")] public string CustomerID { get; set; } = string.Empty;
        [Name("OrderDate")] public string OrderDate { get; set; } = string.Empty;
        [Name("Status")] public string Status { get; set; } = string.Empty;
    }
}
