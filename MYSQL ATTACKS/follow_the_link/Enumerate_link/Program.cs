using System;
using System.Data.SqlClient;

namespace Enumerate_link
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please enter the target name");
                Environment.Exit(0);
            }

            String server = args[0];
            String database = "master";
            String conString = "Server = " + server + "; Database = " + database + "; Integrated Security =True;";
            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
                Console.WriteLine("Auth success");
            }
            catch
            {
                Console.WriteLine("Auth fail");
                Environment.Exit(0);
            }

            String query = "EXEC sp_linkedservers;";

            SqlCommand command = new SqlCommand(query, con);
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine("Linked SQL Server: " + reader[0]);
            }
            reader.Close();

            query = "EXEC ('SELECT name from sys.servers;') AT DC01.CORP2.COM";
            command = new SqlCommand(query, con);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("The linked server = > " + reader[0]);
            }

            reader.Close();

            //String login = "SELECT SYSTEM_USER;";
            //command = new SqlCommand(login, con);
            //reader = command.ExecuteReader();
            //reader.Read();
            //Console.WriteLine("Login as => " + reader[0]);
            //reader.Close();

            //login = "EXEC ('LOGIN AS =''SA'';select system_user;') AT DC01";
            //command = new SqlCommand(login, con);
            //reader = command.ExecuteReader();
            //reader.Read();
            //Console.WriteLine("Login as => " + reader[0]);

            //reader.Close();
            con.Close();
        }
    }
}
