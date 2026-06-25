using Microsoft.AspNetCore.DataProtection;
using MySqlConnector;
using spikewall.Object;
using System.Security.Cryptography;
using System.Text;

namespace spikewall
{
    public class Db
    {
        private static IDataProtector m_protector;

        private static string GetDbConfigFilename()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "spikewall.db");
        }

        /// <summary>
        /// Initialize database details from locally-stored secrets
        /// </summary>
        public static void Initialize()
        {
            // Create
            var provider = DataProtectionProvider.Create("spikewall");
            m_protector = provider.CreateProtector("spikewall.db");

            // Decrypt string
            string protectedPayload = null;
            try
            {
                // Read encrypted string from disk
                protectedPayload = File.ReadAllText(GetDbConfigFilename());

                // Attempt decryption
                m_connectionString = m_protector.Unprotect(protectedPayload);

                // Attempt connection with MySQL
                spikewall.Config.RefreshConfig();
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FileNotFoundException) || string.IsNullOrEmpty(protectedPayload))
                {
                    Console.WriteLine("No database details were found. Please use /Dashboard/setDatabaseDetails to update them.");
                }
                else if (e.GetType() == typeof(CryptographicException))
                {
                    Console.WriteLine("Failed to decrypt database details. Please use /Dashboard/setDatabaseDetails to update them.");
                    Console.WriteLine(e);
                }
                else if (e.GetType() == typeof(MySqlException))
                {
                    Console.WriteLine("Failed to connect to MySQL with stored details. Please use /Dashboard/setDatabaseDetails to update them.");
                    Console.WriteLine(e);
                }
            }
        }

        public static void SetDetails(string host, string port, string username, string password, string database)
        {
            m_connectionString =
                $"server={host};user={username};database={database};port={port};password={password}";
            File.WriteAllText(GetDbConfigFilename(), m_protector.Protect(m_connectionString));
            spikewall.Config.RefreshConfig();
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
            // Return connection
            return new MySqlConnection(m_connectionString);
        }

        private static string EscapeString(string s)
        {
            return s.Replace("'", "\\'");
        }

        /// <summary>
        /// Generate an SQL string where all parameters are escaped (assumes single quotes are used for values)
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

        private static void QuickRun(MySqlConnection conn, string filePath)
        {
            var command = new MySqlCommand(File.ReadAllText(filePath), conn);
            command.ExecuteNonQuery();
        }

        public static void ResetDatabase(bool chao = false,
                                         bool players = false,
                                         bool characters = false,
                                         bool mileageMapStates = false,
                                         bool config = false,
                                         bool tickers = false,
                                         bool dailyChallenge = false,
                                         bool costs = false,
                                         bool itemOwnership = false,
                                         bool information = false,
                                         bool incentives = false,
                                         bool wheelOptions = false,
                                         bool itemRouletteOptions = false,
                                         bool chaoWheelOptions = false,
                                         bool chaoRouletteOptions = false)
        {
            using var conn = Db.Get();
            conn.Open();

            // Drop and recreate chao and chaostates
            if (chao)
            {
                QuickRun(conn, @"..\sqlfiles\chao.sql");
            }

            // Drop and recreate players and sessions
            if (players)
            {
                QuickRun(conn, @"..\sqlfiles\players.sql");
            }

            // Drop and recreate characters, characterstates, and characterupgrades
            if (characters)
            {
                QuickRun(conn, @"..\sqlfiles\characters.sql");
            }

            // Drop and recreate mileagemapstates
            if (mileageMapStates)
            {
                QuickRun(conn, @"..\sqlfiles\mileagemapstates.sql");
            }

            if (config)
            {
                QuickRun(conn, @"..\sqlfiles\config.sql");
            }

            if (tickers)
            {
                QuickRun(conn, @"..\sqlfiles\tickers.sql");
            }

            if (dailyChallenge)
            {
                QuickRun(conn, @"..\sqlfiles\dailychallenge.sql");
            }

            if (costs)
            {
                QuickRun(conn, @"..\sqlfiles\costs.sql");
            }

            // Drop and recreate itemownership
            if (itemOwnership)
            {
                QuickRun(conn, @"..\sqlfiles\itemownership.sql;");
            }

            if (information)
            {
                QuickRun(conn, @"..\sqlfiles\information.sql");
            }

            if (incentives)
            {
                QuickRun(conn, @"..\sqlfiles\incentives.sql");
            }

            if (wheelOptions)
            {
                QuickRun(conn, @"..\sqlfiles\wheeloptions.sql");
            }

            if (itemRouletteOptions)
            {
                QuickRun(conn, @"..\sqlfiles\itemrouletteoptions.sql");
            }

            if (chaoWheelOptions)
            {
                QuickRun(conn, @"..\sqlfiles\chaoWheelOptions.sql");
            }

            if (chaoRouletteOptions)
            {
                QuickRun(conn, @"..\sqlfiles\chaoRouletteOptions.sql");
            }

            conn.Close();
        }

        private static string m_connectionString;
    }
}
