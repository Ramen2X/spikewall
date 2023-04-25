using MySqlConnector;
using spikewall;
using spikewall.Debug;

namespace spikewall
{
    public class Config
    {
        public static object Get(string key)
        {
            return m_configCache[key];
        }

        public static void Set(string key, object newValue)
        {
            // Set in cache
            m_configCache[key] = newValue;

            // Save in database
            using (var conn = Db.Get())
            {
                conn.Open();

                var sql = Db.GetCommand("UPDATE `sw_config` SET {0} = '{1}' WHERE id = '{2}';",
                                        key, newValue.ToString(), m_currentConfig);

                var command = new MySqlCommand(sql, conn);

                command.ExecuteNonQuery();

                conn.Close();
            }
        }

        public static void RefreshConfig()
        {
            SwitchConfig(m_currentConfig);
        }

        public static void SwitchConfig(int newConf)
        {
            m_currentConfig = newConf;

            m_configCache.Clear();
            
            using (var conn = Db.Get())
            {
                conn.Open();

                try
                {
                    var sql = Db.GetCommand("SELECT * FROM `sw_config` WHERE id = '{0}';", m_currentConfig);

                    var command = new MySqlCommand(sql, conn);

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            m_configCache[reader.GetName(i)] = reader[i];
                        }
                    }
                }
                catch (MySqlException)
                {
                    // most likely the config table doesn't exist - attempt to initialize it
                    DebugHelper.ColorfulWrite(new ColorfulString(ConsoleColor.Yellow, Console.BackgroundColor, "Failed to fetch config! Creating config database table if it doesn't exist yet...\n"));
                    Db.ResetDatabase(false,false,false,false,true);
                    DebugHelper.ColorfulWrite(new ColorfulString(ConsoleColor.Yellow, Console.BackgroundColor, "Okay, created. Trying again...\n\n"));
                    var sql = Db.GetCommand("SELECT * FROM `sw_config` WHERE id = '{0}';", m_currentConfig);

                    var command = new MySqlCommand(sql, conn);

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            m_configCache[reader.GetName(i)] = reader[i];
                        }
                    }
                }

                conn.Close();
            }

        }

        private static int m_currentConfig = 1;

        private static Dictionary<string, object> m_configCache = new();
    }
}
