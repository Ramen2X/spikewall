DROP TABLE IF EXISTS `sw_mileagemapstates`;

CREATE TABLE
    `sw_mileagemapstates` (
        user_id BIGINT UNSIGNED NOT NULL PRIMARY KEY,
        episode TINYINT NOT NULL,
        chapter TINYINT NOT NULL,
        point BIGINT NOT NULL,
        stage_total_score BIGINT UNSIGNED NOT NULL,
        chapter_start_time BIGINT NOT NULL,
        map_distance BIGINT NOT NULL,
        num_boss_attack BIGINT NOT NULL,
        stage_distance BIGINT NOT NULL,
        stage_max_score BIGINT UNSIGNED NOT NULL
    );