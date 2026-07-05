using System.Threading.Tasks;
using ETLVentas.Data.Result;

namespace ETLVentas.Data.Interfaces
{
    public interface ICountryService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface ICityService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface ICategoryService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface ICustomerService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface IProductService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface IOrderService { Task<EntityProcessResult> ProcessAsync(string filePath); }
    public interface IOrderDetailService { Task<EntityProcessResult> ProcessAsync(string filePath); }
}
