using System.Collections.Generic;
using System.Threading.Tasks;
using ETLVentas.Data.Result;

namespace ETLVentas.Data.Interfaces
{
    public interface ICSVReaderService
    {
        Task<OperationResult<List<T>>> ReadCsvAsync<T>(string filePath);
    }
}
