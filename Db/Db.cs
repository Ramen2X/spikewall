using MySql.Data;
using MySql.Data.MySqlClient;

namespace spikewall
{
    public class Db
    {
        public static void Initialize(ref WebApplicationBuilder builder)
        {
            // MySQL connection details are stored as .NET secrets. To set these on the server (or
            // locally), issue the following commands (replacing the second value as necessary):
            //
            //   dotnet user-secrets set "Db:Host" "localhost"
            //   dotnet user-secrets set "Db:Port" "3306"
            //   dotnet user-secrets set "Db:Username" "dbuser"
            //   dotnet user-secrets set "Db:Password" "dbpass"
            //   dotnet user-secrets set "Db:Database" "spikewall"

            dbHost = builder.Configuration["Db:Host"];
            dbUser = builder.Configuration["Db:Username"];
            dbPass = builder.Configuration["Db:Password"];
            dbPort = Int16.Parse(builder.Configuration["Db:Port"]);
            dbName = builder.Configuration["Db:Database"];
        }

        public static MySqlConnection Get()
        {
            // Build MySQL connection string
            string connectionString = String.Format("server={0};user={1};database={2};port={3};password={4}",
                                                    dbHost, dbUser, dbName, dbPort, dbPass);

            // Return MySqlConnection based on connection string. Note that if the details are wrong
            // or missing, this has a chance of throwing a MySqlException.
            return new MySqlConnection(connectionString);
        }

        private static string dbHost = "";
        private static string dbUser = "";
        private static string dbPass = "";
        private static Int16 dbPort = 0;
        private static string dbName = "";
    }
}
