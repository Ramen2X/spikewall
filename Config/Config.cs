using MySql.Data;
using MySql.Data.MySqlClient;
using spikewall;

namespace spikewall
{
    public class Config
    {
        public static object Get(string key)
        {
            using var conn = Db.Get();

            conn.Open();

            var sql = Db.GetCommand("SELECT {0} FROM `sw_config` WHERE id = '{1}';",
                                    key, m_currentConfig);

            var command = new MySqlCommand(sql, conn);

            object r = command.ExecuteScalar().ToString();

            conn.Close();

            return r;
        }

        public static void Set(string key, object newValue)
        {
            using var conn = Db.Get();

            conn.Open();

            var sql = Db.GetCommand("UPDATE `sw_config` SET {0} = '{1}' WHERE id = '{2}';",
                                    key, newValue.ToString(), m_currentConfig);

            var command = new MySqlCommand(sql, conn);

            command.ExecuteNonQuery();

            conn.Close();
        }

        public static void SwitchConfig(int newConf)
        {
            m_currentConfig = newConf;
        }

        private static int m_currentConfig = 1;
    }
}
