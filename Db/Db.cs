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
            m_dbHost = builder.Configuration["Db:Host"];
            m_dbUser = builder.Configuration["Db:Username"];
            m_dbPass = builder.Configuration["Db:Password"];
            m_dbName = builder.Configuration["Db:Database"];

            try
            {
                m_dbPort = Int16.Parse(builder.Configuration["Db:Port"]);
            }
            catch (ArgumentNullException)
            {
                m_dbPort = 0;
            }
        }

        /// <summary>
        /// Set up necessary database tables
        /// </summary>
        public static void SetupTables()
        {
            try
            {
                using (var conn = Get())
                {
                    conn.Open();

                    var cmd = new MySqlCommand(
                        @"CREATE TABLE IF NOT EXISTS `sw_players` (
                            id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            password VARCHAR(20) NOT NULL,
                            server_key VARCHAR(20) NOT NULL,

                            username VARCHAR(12) NOT NULL DEFAULT '',
                            migrate_password VARCHAR(12),
                            language INTEGER,
                            suspended_until BIGINT,
                            suspend_reason INTEGER,

                            last_login BIGINT,
                            last_login_device TEXT,
                            last_login_platform INTEGER,
                            last_login_version TEXT,

                            main_chara_id MEDIUMINT NOT NULL DEFAULT 300000,
                            sub_chara_id MEDIUMINT NOT NULL DEFAULT -1,
                            main_chao_id MEDIUMINT NOT NULL DEFAULT -1,
                            sub_chao_id MEDIUMINT NOT NULL DEFAULT -1,

                            num_rings BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            num_buy_rings BIGINT NOT NULL DEFAULT 0,
                            num_red_rings BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            num_buy_red_rings BIGINT NOT NULL DEFAULT 0,
                            energy BIGINT NOT NULL DEFAULT 0,
                            energy_buy BIGINT NOT NULL DEFAULT 0,
                            energy_renews_at BIGINT NOT NULL DEFAULT 0,
                            num_messages BIGINT NOT NULL DEFAULT 0,
                            ranking_league BIGINT NOT NULL DEFAULT 0,
                            quick_ranking_league BIGINT NOT NULL DEFAULT 0,
                            num_roulette_ticket BIGINT NOT NULL DEFAULT 0,
                            num_chao_roulette_ticket BIGINT NOT NULL DEFAULT 0,
                            chao_eggs BIGINT NOT NULL DEFAULT 0,
                            story_high_score BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            quick_high_score BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            total_distance BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            maximum_distance BIGINT NOT NULL DEFAULT 0,
                            daily_mission_id INTEGER NOT NULL DEFAULT 0,
                            daily_mission_end_time BIGINT NOT NULL DEFAULT 0,
                            daily_challenge_value INTEGER NOT NULL DEFAULT 0,
                            daily_challenge_complete BIGINT NOT NULL DEFAULT 0,
                            num_daily_challenge_cont BIGINT NOT NULL DEFAULT 0,
                            num_playing BIGINT NOT NULL DEFAULT 0,
                            num_animals BIGINT UNSIGNED NOT NULL DEFAULT 0,
                            num_rank INTEGER NOT NULL DEFAULT 0

                        );
                        ALTER TABLE `sw_players` AUTO_INCREMENT=1000000000;
                        CREATE TABLE IF NOT EXISTS `sw_sessions` (
                            sid VARCHAR(48) NOT NULL PRIMARY KEY,
                            uid BIGINT UNSIGNED NOT NULL,
                            expiry BIGINT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS `sw_config` (
                            id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            is_maintenance TINYINT NOT NULL DEFAULT 0,
                            support_legacy_versions TINYINT NOT NULL DEFAULT 1,
                            debug_log TINYINT NOT NULL DEFAULT 0,
                            enable_debug_endpoints TINYINT NOT NULL DEFAULT 0,
                            encryption_iv VARCHAR(16) NOT NULL DEFAULT 'burgersMetKortin',
                            session_time INT NOT NULL DEFAULT 3600,
                            assets_version VARCHAR(3) NOT NULL DEFAULT '049',
                            client_version VARCHAR(8) NOT NULL DEFAULT '2.0.3',
                            data_version VARCHAR(2) NOT NULL DEFAULT '15',
                            info_version VARCHAR(3) NOT NULL DEFAULT '017',
                            revive_rsr_cost BIGINT UNSIGNED NOT NULL DEFAULT 5,
                            enable_limited_time_incentives TINYINT NOT NULL DEFAULT 1
                        );
                        INSERT IGNORE INTO `sw_config` (id) VALUES ('1');
                        CREATE TABLE IF NOT EXISTS `sw_tickers` (
                            id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            start_time BIGINT UNSIGNED NOT NULL,
                            end_time BIGINT UNSIGNED NOT NULL,
                            message VARCHAR(600) NOT NULL,
                            language TINYINT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS `sw_dailychallenge` (
                            id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            item1 MEDIUMINT NOT NULL DEFAULT 910000,
                            item1_count BIGINT UNSIGNED NOT NULL DEFAULT 1000,
                            item2 MEDIUMINT NOT NULL DEFAULT 900000,
                            item2_count BIGINT UNSIGNED NOT NULL DEFAULT 10,
                            item3 MEDIUMINT NOT NULL DEFAULT 910000,
                            item3_count BIGINT UNSIGNED NOT NULL DEFAULT 5000,
                            item4 MEDIUMINT NOT NULL DEFAULT 900000,
                            item4_count BIGINT UNSIGNED NOT NULL DEFAULT 20,
                            item5 MEDIUMINT NOT NULL DEFAULT 910000,
                            item5_count BIGINT UNSIGNED NOT NULL DEFAULT 10000,
                            item6 MEDIUMINT NOT NULL DEFAULT 900000,
                            item6_count BIGINT UNSIGNED NOT NULL DEFAULT 30,
                            item7 MEDIUMINT NOT NULL DEFAULT 900000,
                            item7_count BIGINT UNSIGNED NOT NULL DEFAULT 60
                        );
                        INSERT IGNORE INTO `sw_dailychallenge` (id) VALUES ('1');
                        CREATE TABLE IF NOT EXISTS `sw_costlist` (
                            id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            item1 MEDIUMINT NOT NULL DEFAULT 910000,
                            item1_cost BIGINT UNSIGNED NOT NULL DEFAULT 6000,
                            item1_id MEDIUMINT NOT NULL DEFAULT 110000,
                            item2 MEDIUMINT NOT NULL DEFAULT 910000,
                            item2_cost BIGINT UNSIGNED NOT NULL DEFAULT 1000,
                            item2_id MEDIUMINT NOT NULL DEFAULT 110001,
                            item3 MEDIUMINT NOT NULL DEFAULT 910000,
                            item3_cost BIGINT UNSIGNED NOT NULL DEFAULT 4000,
                            item3_id MEDIUMINT NOT NULL DEFAULT 110002,

                            item4 MEDIUMINT NOT NULL DEFAULT 910000,
                            item4_cost BIGINT UNSIGNED NOT NULL DEFAULT 3000,
                            item4_id MEDIUMINT NOT NULL DEFAULT 120000,
                            item5 MEDIUMINT NOT NULL DEFAULT 910000,
                            item5_cost BIGINT UNSIGNED NOT NULL DEFAULT 1000,
                            item5_id MEDIUMINT NOT NULL DEFAULT 120001,
                            item6 MEDIUMINT NOT NULL DEFAULT 910000,
                            item6_cost BIGINT UNSIGNED NOT NULL DEFAULT 3000,
                            item6_id MEDIUMINT NOT NULL DEFAULT 120002,
                            item7 MEDIUMINT NOT NULL DEFAULT 910000,
                            item7_cost BIGINT UNSIGNED NOT NULL DEFAULT 2000,
                            item7_id MEDIUMINT NOT NULL DEFAULT 120003,
                            item8 MEDIUMINT NOT NULL DEFAULT 910000,
                            item8_cost BIGINT UNSIGNED NOT NULL DEFAULT 3000,
                            item8_id MEDIUMINT NOT NULL DEFAULT 120004,
                            item9 MEDIUMINT NOT NULL DEFAULT 910000,
                            item9_cost BIGINT UNSIGNED NOT NULL DEFAULT 5000,
                            item9_id MEDIUMINT NOT NULL DEFAULT 120005,
                            item10 MEDIUMINT NOT NULL DEFAULT 910000,
                            item10_cost BIGINT UNSIGNED NOT NULL DEFAULT 4000,
                            item10_id MEDIUMINT NOT NULL DEFAULT 120006,
                            item11 MEDIUMINT NOT NULL DEFAULT 910000,
                            item11_cost BIGINT UNSIGNED NOT NULL DEFAULT 5000,
                            item11_id MEDIUMINT NOT NULL DEFAULT 120007,

                            item12 MEDIUMINT NOT NULL DEFAULT 900000,
                            item12_cost BIGINT UNSIGNED NOT NULL DEFAULT 5,
                            item12_id MEDIUMINT NOT NULL DEFAULT 950000,
                            item13 MEDIUMINT NOT NULL DEFAULT 900000,
                            item13_cost BIGINT UNSIGNED NOT NULL DEFAULT 2,
                            item13_id MEDIUMINT NOT NULL DEFAULT 980000,
                            item14 MEDIUMINT NOT NULL DEFAULT 900000,
                            item14_cost BIGINT UNSIGNED NOT NULL DEFAULT 5,
                            item14_id MEDIUMINT NOT NULL DEFAULT 980001,
                            item15 MEDIUMINT NOT NULL DEFAULT 900000,
                            item15_cost BIGINT UNSIGNED NOT NULL DEFAULT 10,
                            item15_id MEDIUMINT NOT NULL DEFAULT 980002
                        );
                        INSERT IGNORE INTO `sw_costlist` (id) VALUES ('1');
                        CREATE TABLE IF NOT EXISTS `sw_itemownership` (
                            user_id BIGINT UNSIGNED NOT NULL,
                            item_id BIGINT UNSIGNED NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS `sw_characters` (
                            id MEDIUMINT UNSIGNED NOT NULL PRIMARY KEY,
                            num_rings BIGINT UNSIGNED NOT NULL,
                            num_red_rings BIGINT UNSIGNED NOT NULL,
                            price_num_rings BIGINT UNSIGNED NOT NULL,
                            price_num_red_rings BIGINT UNSIGNED NOT NULL,
                            lock_condition TINYINT NOT NULL,
                            star_max INTEGER NOT NULL DEFAULT 10,
                            visible TINYINT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS `sw_characterstates` (
                            user_id BIGINT UNSIGNED NOT NULL,
                            character_id BIGINT UNSIGNED NOT NULL,
                            status TINYINT NOT NULL,
                            level TINYINT NOT NULL,
                            exp BIGINT UNSIGNED NOT NULL,
                            star TINYINT NOT NULL,
                            ability_level TINYTEXT NOT NULL,
                            ability_num_rings TINYTEXT NOT NULL,
                            ability_levelup TINYTEXT NOT NULL,
                            ability_levelup_exp TINYTEXT NOT NULL
                        );
                        INSERT IGNORE INTO `sw_characters` (
                            id,
                            num_rings,
                            num_red_rings,
                            price_num_rings,
                            price_num_red_rings,
                            lock_condition,
                            star_max,
                            visible
                        ) VALUES (
                            '300000',
                            '0',
                            '1337',
                            '100000',
                            '50',
                            '0',
                            '10',
                            '1'
                        );
                        CREATE TABLE IF NOT EXISTS `sw_characterupgrades` (
                            character_id MEDIUMINT UNSIGNED NOT NULL,
                            min_level TINYINT NOT NULL,
                            max_level TINYINT NOT NULL,
                            multiple MEDIUMINT NOT NULL
                        );
                        INSERT IGNORE INTO `sw_characterupgrades` 
                        VALUES (
                            '300000',
                            '0',
                            '0',
                            '0'
                        ),
                        (
                            '300000',
                            '1',
                            '9',
                            '200'
                        ),
                        (
                            '300000',
                            '10',
                            '19',
                            '400'
                        ),
                        (
                            '300000',
                            '20',
                            '29',
                            '600'
                        ),
                        (
                            '300000',
                            '30',
                            '39',
                            '800'
                        ),
                        (
                            '300000',
                            '40',
                            '49',
                            '1000'
                        ),
                        (
                            '300000',
                            '50',
                            '59',
                            '1200'
                        ),
                        (
                            '300000',
                            '60',
                            '69',
                            '1400'
                        ),
                        (
                            '300000',
                            '70',
                            '79',
                            '1600'
                        ),
                        (
                            '300000',
                            '80',
                            '89',
                            '1800'
                        ),
                        (
                            '300000',
                            '90',
                            '100',
                            '2000'
                        );
                        CREATE TABLE IF NOT EXISTS `sw_chao` (
                            id MEDIUMINT UNSIGNED NOT NULL PRIMARY KEY,
                            rarity INTEGER NOT NULL DEFAULT 0,
                            hidden TINYINT NOT NULL DEFAULT 0
                        );
                        INSERT IGNORE INTO `sw_chao` (id) VALUES ('400000');
                        CREATE TABLE IF NOT EXISTS `sw_chaostates` (
                            chao_id MEDIUMINT UNSIGNED NOT NULL,
                            user_id BIGINT UNSIGNED NOT NULL,
                            status TINYINT NOT NULL DEFAULT 0,
                            level INTEGER UNSIGNED NOT NULL DEFAULT 0,
                            set_status TINYINT NOT NULL DEFAULT 0,
                            acquired TINYINT NOT NULL DEFAULT 0
                        );
                        CREATE TABLE IF NOT EXISTS `sw_mileagemapstates` (
                            user_id BIGINT UNSIGNED NOT NULL,
                            episode TINYINT NOT NULL,
                            chapter TINYINT NOT NULL,
                            point BIGINT NOT NULL,
                            stage_total_score BIGINT UNSIGNED NOT NULL,
                            chapter_start_time BIGINT NOT NULL,
                            
                            map_distance BIGINT NOT NULL,
                            num_boss_attack BIGINT NOT NULL,
                            stage_distance BIGINT NOT NULL,
                            stage_max_score BIGINT UNSIGNED NOT NULL
                        );", conn);

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Failed to set up tables: " + e.ToString());
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
            string connectionString = String.Format("server={0};user={1};database={2};port={3};password={4}",
                                                    m_dbHost, m_dbUser, m_dbName, m_dbPort, m_dbPass);

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
            for (int i = 0; i < arg.Length; i++) {
                if (arg[i] is string) {
                    arg[i] = EscapeString((string) arg[i]);
                }
            }
            return string.Format(format, arg);
        }

        private static string m_dbHost = "";
        private static string m_dbUser = "";
        private static string m_dbPass = "";
        private static Int16 m_dbPort = 0;
        private static string m_dbName = "";
    }
}
