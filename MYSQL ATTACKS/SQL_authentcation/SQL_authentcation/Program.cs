using System;
using System.Data.SqlClient;

namespace SQL_authentcation
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please indicate your server name");
                Environment.Exit(0);
            }
            String sqlServer = args[0];
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

            /// Request string

            String querylogin = "SELECT SYSTEM_USER;";
            SqlCommand command = new SqlCommand(querylogin, con);
            SqlDataReader reader = command.ExecuteReader();

            reader.Read();
            Console.WriteLine("Console logged in as" + reader[0]);
            reader.Close();

            String queryuser = "SELECT USER_NAME();";
            command = new SqlCommand(queryuser, con);
            reader = command.ExecuteReader();

            reader.Read();
            Console.WriteLine("User mapped to " + reader[0]);
            reader.Close();


            /// Request for roles

            String querypublicrole = "SELECT IS_SRVROLEMEMBER('public');";
            command = new SqlCommand(querypublicrole, con);
            reader = command.ExecuteReader();
            reader.Read();
            Int32 role = Int32.Parse(reader[0].ToString());
            if(role == 1)
            {
                Console.WriteLine("User is a member of public role");
            }
            else
            {
                Console.WriteLine("User is NOT a member of public role");
            }
            reader.Close();

            String querysysadmin = "SELECT IS_SRVROLEMEMBER('sysadmin');";
            command = new SqlCommand(querysysadmin, con);
            reader = command.ExecuteReader();
            reader.Read();
            role = Int32.Parse(reader[0].ToString());
            if (role == 1)
            {
                Console.WriteLine("User is a member of sysadmin role");
            }
            else
            {
                Console.WriteLine("User is NOT a member of sysadmin role");
            }
            reader.Close();
            con.Close();

        }
    }
}
