using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ETLVentas.Data.Interfaces;
using ETLVentas.Data.Result;

namespace ETLVentas.Data.Services
{
    public class CSVReaderService : ICSVReaderService
    {
        public async Task<OperationResult<List<T>>> ReadCsvAsync<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return OperationResult<List<T>>.Fail($"File not found: {filePath}");
            }

            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecords<T>().ToList();
                return await Task.FromResult(OperationResult<List<T>>.Ok(records));
            }
            catch (System.Exception ex)
            {
                return OperationResult<List<T>>.Fail($"Error reading CSV: {ex.Message}");
            }
        }
    }
}
