using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ETLVentas.Data.Interfaces;
using ETLVentas.Data.Result;

namespace ETLVentas.Data.Services
{
    public class EtlOrchestrator : IEtlOrchestrator
    {
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public EtlOrchestrator(
            ICountryService countryService,
            ICityService cityService,
            ICategoryService categoryService,
            ICustomerService customerService,
            IProductService productService,
            IOrderService orderService,
            IOrderDetailService orderDetailService)
        {
            _countryService = countryService;
            _cityService = cityService;
            _categoryService = categoryService;
            _customerService = customerService;
            _productService = productService;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        public async Task ExecuteEtlAsync()
        {
            try { Console.Clear(); } catch {}
            Console.WriteLine("=======================================================");
            Console.WriteLine("PROCESO ETL - Sistema de Análisis de Ventas");
            Console.WriteLine("=======================================================");
            Console.WriteLine();

            // Cargar dimensiones silenciosamente
            await _countryService.ProcessAsync("Countries.csv");
            await _cityService.ProcessAsync("Cities.csv");
            await _categoryService.ProcessAsync("Categories.csv");

            var results = new List<EntityProcessResult>();

            // Mostrar salida exactamente como el profesor lo requiere
            results.Add(await ProcessAndPrint("Clientes", "Customers.csv", _customerService.ProcessAsync));
            results.Add(await ProcessAndPrint("Productos", "Products.csv", _productService.ProcessAsync));
            results.Add(await ProcessAndPrint("Órdenes", "Orders.csv", _orderService.ProcessAsync));
            results.Add(await ProcessAndPrint("Detalles de Orden", "OrderDetails.csv", _orderDetailService.ProcessAsync));

            Console.WriteLine("=======================================================");
            Console.WriteLine("RESUMEN FINAL DEL PROCESO");
            Console.WriteLine("=======================================================");

            int totalLeidos = 0, totalInsertados = 0, totalDuplicados = 0, totalRechazados = 0;
            string[] names = { "Customers", "Products", "Orders", "OrderDetails" };

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                Console.WriteLine($"[{names[i]}] Leídos: {r.TotalRecords} | Insertados: {r.InsertedRecords} | Duplicados: {r.DuplicatedRecords} | Rechazados: {r.RejectedRecords}");
                totalLeidos += r.TotalRecords;
                totalInsertados += r.InsertedRecords;
                totalDuplicados += r.DuplicatedRecords;
                totalRechazados += r.RejectedRecords;
            }

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine($"TOTAL LEÍDOS:      {totalLeidos}");
            Console.WriteLine($"TOTAL INSERTADOS: {totalInsertados}");
            Console.WriteLine($"TOTAL DUPLICADOS: {totalDuplicados}");
            Console.WriteLine($"TOTAL RECHAZADOS: {totalRechazados}");
            Console.WriteLine("=======================================================");
            Console.WriteLine();
            Console.WriteLine("Proceso finalizado. Presione una tecla para salir...");
            
            // Pausa (opcional, en algunos entornos la terminal se cierra)
            try { Console.ReadKey(); } catch { }
        }

        private async Task<EntityProcessResult> ProcessAndPrint(string displayName, string fileName, Func<string, Task<EntityProcessResult>> processFunc)
        {
            Console.WriteLine($"--- Cargando: {displayName} ---");
            var result = await processFunc(fileName);
            
            Console.WriteLine($"Leídos:     {result.TotalRecords}");
            Console.WriteLine($"Insertados: {result.InsertedRecords}");
            Console.WriteLine($"Duplicados: {result.DuplicatedRecords}");
            Console.WriteLine($"Rechazados: {result.RejectedRecords}");
            Console.WriteLine();
            
            return result;
        }
    }
}

