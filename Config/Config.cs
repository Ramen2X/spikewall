namespace spikewall.Config
{
    // Config Object
    public class Current
    {
            int port = 9001;
            string dbName = "spikewall";
            string dbPassword = "default";
            string dbUser = "root";
            int dbPort = 3306;
            bool debugLog;
            bool isMaintenance;
            bool supportLegacyVersions;

            public int Port {
                get { return port; }
                set { port = value; }
            }

            public string DbName {
                get { return dbName; }
                set { dbName = value; }
            }

            public string DbPassword {
                get { return dbPassword; }
                set { dbPassword = value; }
            }

            public string DbUser {
                get { return dbUser; }
                set { dbUser = value; }
            }

            public int DbPort {
                get { return dbPort; }
                set { dbPort = value; }
            }

            public bool IsMaintenance {
                get { return isMaintenance; }
                set { isMaintenance = value; }
            }

            public bool SupportLegacyVersions {
                get { return supportLegacyVersions; }
                set { supportLegacyVersions = value; }
            }

            public bool DebugLog
            {
                get { return debugLog; }
                set { debugLog = value; }
            }
    }
    
    public static class Set
    {
        // Create a new config object and return it, TODO: take values from a config file
        public static Current Config()
        {
            var config = new Config.Current();
            return config;
        }
    }
}