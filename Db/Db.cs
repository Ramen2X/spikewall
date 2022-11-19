using MySql.Data;
using MySql.Data.MySqlClient;
using System.Text;

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
            m_dbHost = builder.Configuration["Db:Host"] ?? throw new InvalidOperationException("The \"Db:Host\" secret cannot be null. Did you make sure you set up the secrets properly?");
            m_dbUser = builder.Configuration["Db:Username"] ?? throw new InvalidOperationException("The \"Db:Username\" secret cannot be null. Did you make sure you set up the secrets properly?");
            m_dbPass = builder.Configuration["Db:Password"] ?? throw new InvalidOperationException("The \"Db:Password\" secret cannot be null. Did you make sure you set up the secrets properly?");
            m_dbName = builder.Configuration["Db:Database"] ?? throw new InvalidOperationException("The \"Db:Database\" secret cannot be null. Did you make sure you set up the secrets properly?");

            try
            {
                m_dbPort = short.Parse(builder.Configuration["Db:Port"] ?? throw new InvalidOperationException("The \"Db:Port\" secret cannot be null. Did you make sure you set up the secrets properly?"));
            }
            catch (ArgumentNullException)
            {
                m_dbPort = 0;
            }
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
            var connectionString =
                $"server={m_dbHost};user={m_dbUser};database={m_dbName};port={m_dbPort};password={m_dbPass}";

            // Return connection
            return new MySqlConnection(connectionString);
        }

        public static string EscapeString(string s)
        {
            return s.Replace("'", "\\'");
        }

        /// <summary>
        /// Generate an SQL string where all paramters are escaped (assumes single quotes are used for values)
        /// </summary>
        public static string GetCommand(string format, params object[] arg)
        {
            for (var i = 0; i < arg.Length; i++) {
                if (arg[i] is string) {
                    arg[i] = EscapeString((string) arg[i]);
                }
            }
            return string.Format(format, arg);
        }

        public static long[] ConvertDBListToIntArray(string s)
        {
            var tokens = s.Split(' ');
            var values = new long[tokens.Length];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = long.Parse(tokens[i]);
            }
            return values;
        }

        public static string ConvertIntArrayToDBList(IEnumerable<long> a)
        {
            StringBuilder dbList = new();
            dbList.AppendJoin(' ', a);

            return dbList.ToString();
        }

        private static string m_dbHost = "";
        private static string m_dbUser = "";
        private static string m_dbPass = "";
        private static short m_dbPort = 0;
        private static string m_dbName = "";
    }
}
