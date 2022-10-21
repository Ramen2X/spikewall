using MySql.Data;
using MySql.Data.MySqlClient;

namespace spikewall
{
    public class Db
    {
        /// <summary>
        /// Initialize database details from locally-stored secrets
        /// </summary>
        /// MySQL connection details are stored as .NET secrets. To set these on the server (or
        /// locally), issue the following commands (replacing the second value as necessary):
        ///
        ///   dotnet user-secrets set "Db:Host" "localhost"
        ///   dotnet user-secrets set "Db:Port" "3306"
        ///   dotnet user-secrets set "Db:Username" "dbuser"
        ///   dotnet user-secrets set "Db:Password" "dbpass"
        ///   dotnet user-secrets set "Db:Database" "spikewall"
        ///
        public static void Initialize(ref WebApplicationBuilder builder)
        {
            dbHost = builder.Configuration["Db:Host"];
            dbUser = builder.Configuration["Db:Username"];
            dbPass = builder.Configuration["Db:Password"];
            dbPort = Int16.Parse(builder.Configuration["Db:Port"]);
            dbName = builder.Configuration["Db:Database"];
        }

        /// <summary>
        /// Retrieve valid MySQL connection to make queries with.
        /// </summary>
        ///
        /// This function will return a valid MySqlConnection, or pass a MySqlException if an error
        /// occurs. Using try/catch is recommended, at least in sections where the validity of
        /// database details is checked.
        ///
        /// This can also be used in a `using` statement, e.g. `using (var conn = Db.Get())`
        ///
        /// After calling this function, call `Open()` on the resulting object. Then queries can be
        /// made with MySqlCommand. Call `Close()` when you're finished (this might happen
        /// automatically with `using`, but it's probably good practice either way),
        public static MySqlConnection Get()
        {
            // Build MySQL connection string out of loaded parameters
            string connectionString = String.Format("server={0};user={1};database={2};port={3};password={4}",
                                                    dbHost, dbUser, dbName, dbPort, dbPass);

            // Return connection
            return new MySqlConnection(connectionString);
        }

        private static string dbHost = "";
        private static string dbUser = "";
        private static string dbPass = "";
        private static Int16 dbPort = 0;
        private static string dbName = "";
    }
}
