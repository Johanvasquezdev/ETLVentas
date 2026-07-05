using System;
using System.IO;
using System.Data;
using Microsoft.Data.SqlClient;

class Program {
    static void Main() {
        var cs = "Server=.;Database=AnalisisDeVentas;Trusted_Connection=True;TrustServerCertificate=True;";
        using (var con = new SqlConnection(cs)) {
            con.Open();
            try {
                using (var cmd = new SqlCommand("EXEC sp_InsertOrder @OrderID=10248, @CustomerID=123, @Status='Shipped', @OrderDate='2021-01-01'", con)) {
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("Success");
            } catch (Exception ex) {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }
    }
}
