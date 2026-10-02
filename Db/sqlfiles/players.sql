DROP TABLE IF EXISTS `sw_players`;

DROP TABLE IF EXISTS `sw_sessions`;

DROP TABLE IF EXISTS `sw_itemownership`;

CREATE TABLE
    `sw_players` (
        id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
        password VARCHAR(32) NOT NULL,
        server_key VARCHAR(32) NOT NULL,
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
        daily_mission_id INTEGER NOT NULL DEFAULT 1,
        daily_mission_end_time BIGINT NOT NULL DEFAULT 0,
        daily_challenge_value INTEGER NOT NULL DEFAULT 0,
        daily_challenge_complete BIGINT NOT NULL DEFAULT 0,
        num_daily_challenge_cont BIGINT NOT NULL DEFAULT 0,
        num_playing BIGINT NOT NULL DEFAULT 0,
        num_animals BIGINT UNSIGNED NOT NULL DEFAULT 0,
        num_rank INTEGER NOT NULL DEFAULT 0,
        equip_item_list TINYTEXT NOT NULL DEFAULT ''
    );

ALTER TABLE `sw_players` AUTO_INCREMENT = 1000000000;

CREATE TABLE
    `sw_sessions` (
        sid VARCHAR(67) NOT NULL PRIMARY KEY,
        uid BIGINT UNSIGNED NOT NULL,
        expiry BIGINT NOT NULL
    );

CREATE TABLE
    `sw_itemownership` (
        user_id BIGINT UNSIGNED NOT NULL,
        item_id BIGINT UNSIGNED NOT NULL
    );