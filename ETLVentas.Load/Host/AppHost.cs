using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ETLVentas.Data.Context;

namespace ETLVentas.Load.Host
{
    public static class AppHost
    {
        public static IHost CreateHost(string[] args)
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

            builder.ConfigureServices((hostContext, services) =>
            {
                var connString = hostContext.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<AnalisisDeVentasContext>(options =>
                    options.UseSqlServer(connString));

                services.AddEtlServices();
            });

            return builder.Build();
        }
    }
}
