using System.Threading.Tasks;

namespace ETLVentas.Data.Interfaces
{
    public interface IEtlOrchestrator
    {
        Task ExecuteEtlAsync();
    }
}
