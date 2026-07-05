using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ETLVentas.Data.Context;
using ETLVentas.Data.Interfaces;
using ETLVentas.Load.Host;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    var connString = hostContext.Configuration.GetConnectionString("DefaultConnection");
    services.AddDbContext<AnalisisDeVentasContext>(options =>
        options.UseSqlServer(connString));

    services.AddEtlServices();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
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
