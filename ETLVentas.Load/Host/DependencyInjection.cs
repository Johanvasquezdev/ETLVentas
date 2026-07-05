using Microsoft.Extensions.DependencyInjection;
using ETLVentas.Data.Interfaces;
using ETLVentas.Data.Services;
using ETLVentas.Data.Validators;

namespace ETLVentas.Load.Host
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEtlServices(this IServiceCollection services)
        {
            services.AddScoped<ICSVReaderService, CSVReaderService>();
            services.AddScoped<CsvValidator>();
            services.AddScoped<DataCleaner>();
            services.AddScoped<DuplicateValidator>();
            services.AddScoped<ReferentialIntegrityValidator>();
            services.AddScoped<TypeValidator>();

            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            
            services.AddScoped<IEtlOrchestrator, EtlOrchestrator>();
            
            return services;
        }
    }
}
