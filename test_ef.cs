using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ETLVentas.Data.Context;
using ETLVentas.Data.Services;
using ETLVentas.Data.Models.Csv;

class Program {
    static async System.Threading.Tasks.Task Main() {
        var options = new DbContextOptionsBuilder<AnalisisDeVentasContext>()
            .UseSqlServer("Server=.;Database=AnalisisDeVentas;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        var ctx = new AnalisisDeVentasContext(options);
        var procs = new AnalisisDeVentasContextProcedures(ctx);
        try {
            await procs.sp_InsertOrderAsync(1, 1, "Pending", new DateOnly(2021, 1, 1));
            Console.WriteLine("Success order!");
        } catch (Exception ex) {
            Console.WriteLine("ORDER ERROR: " + ex.Message);
            if (ex.InnerException != null) Console.WriteLine("INNER: " + ex.InnerException.Message);
        }
    }
}
