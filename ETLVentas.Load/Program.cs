using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ETLVentas.Data.Interfaces;
using ETLVentas.Load.Host;

namespace ETLVentas.Load
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = AppHost.CreateHost(args);

            using (var scope = host.Services.CreateScope())
            {
                var orchestrator = scope.ServiceProvider.GetRequiredService<IEtlOrchestrator>();
                try
                {
                    await orchestrator.ExecuteEtlAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error critico en la orquestacion del ETL: " + ex.ToString());
                }
            }
        }
    }
}
