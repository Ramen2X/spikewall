using MySql.Data;
using MySql.Data.MySqlClient;
using spikewall;

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

        public static void SwitchConfig(int newConf)
        {
            m_currentConfig = newConf;

            m_configCache.Clear();

            using (var conn = Db.Get())
            {
                conn.Open();

                var sql = Db.GetCommand("SELECT * FROM `sw_config` WHERE id = '{0}';", m_currentConfig);

                var command = new MySqlCommand(sql, conn);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        m_configCache[reader.GetName(i)] = reader[i];
                    }
                }

                conn.Close();
            }
        }

        private static int m_currentConfig = 1;

        private static Dictionary<string, object> m_configCache = new Dictionary<string, object>();
    }
}
