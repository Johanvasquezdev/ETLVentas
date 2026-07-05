using CsvHelper.Configuration.Attributes;

namespace ETLVentas.Data.Models.Csv
{
    public class CustomerCsv
    {
        [Name("CustomerID")] public string CustomerID { get; set; } = string.Empty;
        [Name("FirstName")] public string FirstName { get; set; } = string.Empty;
        [Name("LastName")] public string LastName { get; set; } = string.Empty;
        [Name("Email")] public string Email { get; set; } = string.Empty;
        [Name("Phone")] public string Phone { get; set; } = string.Empty;
        [Name("City")] public string City { get; set; } = string.Empty;
        [Name("Country")] public string Country { get; set; } = string.Empty;
    }
}
