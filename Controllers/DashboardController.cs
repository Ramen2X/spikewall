using Microsoft.AspNetCore.Mvc;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace spikewall.Controllers
{
    [ApiController]
    [Route("Dashboard")]
    public class DashboardController : ControllerBase
    {
        private void QuickRun(MySqlConnection conn, string query)
        {
            var cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
        }

        [HttpGet]
        [Route("resetDatabase")]
        public IActionResult ResetDatabase(bool chao = false,
                                           bool players = false,
                                           bool characters = false,
                                           bool mileageMapStates = false,
                                           bool config = false,
                                           bool tickers = false,
                                           bool dailyChallenge = false,
                                           bool costs = false,
                                           bool information = false)
        {
            try
            {
                using var conn = Db.Get();
                conn.Open();

                // Drop and recreate chao and chaostates
                if (chao)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_chao`;
                        DROP TABLE IF EXISTS `sw_chaostates`;
                        CREATE TABLE `sw_chao` (
                            id MEDIUMINT UNSIGNED NOT NULL PRIMARY KEY,
                            rarity INTEGER NOT NULL DEFAULT 0,
                            hidden TINYINT NOT NULL DEFAULT 0
                        );
                        INSERT INTO `sw_chao` (id) VALUES ('400000');
                        CREATE TABLE `sw_chaostates` (
                            chao_id MEDIUMINT UNSIGNED NOT NULL,
                            user_id BIGINT UNSIGNED NOT NULL,
                            status TINYINT NOT NULL DEFAULT 0,
                            level INTEGER UNSIGNED NOT NULL DEFAULT 0,
                            set_status TINYINT NOT NULL DEFAULT 0,
                            acquired TINYINT NOT NULL DEFAULT 0
                        );");
                }

                // Drop and recreate players and sessions
                if (players)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_players`;
                        DROP TABLE IF EXISTS `sw_sessions`;
                        DROP TABLE IF EXISTS `sw_itemownership`;
                        CREATE TABLE `sw_players` (
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
                            num_rank INTEGER NOT NULL DEFAULT 0,
                            equip_item_list TINYTEXT NOT NULL DEFAULT ''
                        );
                        ALTER TABLE `sw_players` AUTO_INCREMENT=1000000000;
                        CREATE TABLE `sw_sessions` (
                            sid VARCHAR(48) NOT NULL PRIMARY KEY,
                            uid BIGINT UNSIGNED NOT NULL,
                            expiry BIGINT NOT NULL
                        );
                        CREATE TABLE `sw_itemownership` (
                            user_id BIGINT UNSIGNED NOT NULL,
                            item_id BIGINT UNSIGNED NOT NULL
                        );");
                }

                // Drop and recreate characters, characterstates, and characterupgrades
                if (characters)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_characters`;
                        DROP TABLE IF EXISTS `sw_characterstates`;
                        DROP TABLE IF EXISTS `sw_characterupgrades`;
                        CREATE TABLE `sw_characters` (
                            id MEDIUMINT UNSIGNED NOT NULL PRIMARY KEY,
                            num_rings BIGINT UNSIGNED NOT NULL,
                            num_red_rings BIGINT UNSIGNED NOT NULL,
                            price_num_rings BIGINT UNSIGNED NOT NULL,
                            price_num_red_rings BIGINT UNSIGNED NOT NULL,
                            lock_condition TINYINT NOT NULL,
                            star_max INTEGER NOT NULL DEFAULT 10,
                            visible TINYINT NOT NULL
                        );
                        CREATE TABLE `sw_characterstates` (
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
                        INSERT INTO `sw_characters` (
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
                            '200',
                            '0',
                            '100000',
                            '50',
                            '0',
                            '10',
                            '1'
                        ),
                        (
                            '300001',
                            '200',
                            '0',
                            '100000',
                            '50',
                            '1',
                            '10',
                            '0'
                        ),
                        (
                            '300002',
                            '200',
                            '0',
                            '100000',
                            '50',
                            '1',
                            '10',
                            '0'
                        ),
                        (
                            '300003',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300004',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300005',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300006',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300007',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300008',
                            '40',
                            '0',
                            '1500000',
                            '150',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300009',
                            '40',
                            '0',
                            '1500000',
                            '150',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300010',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300011',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300012',
                            '300',
                            '0',
                            '2000000',
                            '200',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300013',
                            '40',
                            '0',
                            '1500000',
                            '150',
                            '2',
                            '10',
                            '1'
                        ),
                        (
                            '300014',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300015',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300016',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300017',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300018',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300019',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '300020',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '1'
                        ),
                        (
                            '301000',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301001',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301002',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301003',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301004',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301005',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301006',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        ),
                        (
                            '301007',
                            '300',
                            '0',
                            '0',
                            '0',
                            '3',
                            '10',
                            '0'
                        );
                        CREATE TABLE `sw_characterupgrades` (
                            character_id MEDIUMINT UNSIGNED NOT NULL,
                            min_level TINYINT NOT NULL,
                            max_level TINYINT NOT NULL,
                            multiple MEDIUMINT NOT NULL
                        );
                        INSERT INTO `sw_characterupgrades`
                        VALUES (
                            '300000',
                            '0',
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
                        );");
                }

                // Drop and recreate mileagemapstates
                if (mileageMapStates)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_mileagemapstates`;
                        CREATE TABLE `sw_mileagemapstates` (
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
                        );");
                }

                if (config)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_config`;
                        CREATE TABLE `sw_config` (
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
                        INSERT INTO `sw_config` (id) VALUES ('1');");
                }

                if (tickers)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_tickers`;
                        CREATE TABLE `sw_tickers` (
                            id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            start_time BIGINT UNSIGNED NOT NULL,
                            end_time BIGINT UNSIGNED NOT NULL,
                            message VARCHAR(600) NOT NULL,
                            language TINYINT NOT NULL
                        );");
                }

                if (dailyChallenge)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_dailychallenge`;
                        CREATE TABLE `sw_dailychallenge` (
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
                        INSERT INTO `sw_dailychallenge` (id) VALUES ('1');");
                }

                if (costs)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_costlist`;
                        CREATE TABLE `sw_costlist` (
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
                        INSERT INTO `sw_costlist` (id) VALUES ('1');");
                }

                if (information)
                {
                    QuickRun(conn,
                        @"DROP TABLE IF EXISTS `sw_information`;
                        CREATE TABLE `sw_information` (
                            id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                            priority TINYINT NOT NULL,
                            info_type TINYINT NOT NULL,
                            display_type TINYINT NOT NULL,
                            start_time BIGINT UNSIGNED NOT NULL,
                            end_time BIGINT UNSIGNED NOT NULL,
                            message VARCHAR(1000) NOT NULL,
                            image_id TINYTEXT NOT NULL,
                            extra MEDIUMTEXT NOT NULL,
                            language TINYINT NOT NULL
                        );");
                }

                conn.Close();

                return StatusCode(200, "Database reset successfully");
            }
            catch (MySqlException e)
            {
                return StatusCode(500, e.ToString());
            }
        }
    }
}
