DROP TABLE IF EXISTS `sw_leagueoptions`;

DROP TABLE IF EXISTS `sw_endlessleaguestates`;

DROP TABLE IF EXISTS `sw_quickleaguestates`;

CREATE TABLE
    `sw_leagueoptions` (
        id MEDIUMINT UNSIGNED NOT NULL PRIMARY KEY,
        ranking_mode MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        num_up INTEGER UNSIGNED NOT NULL DEFAULT 40,
        num_dowm INTEGER UNSIGNED NOT NULL DEFAULT 0
    );

INSERT INTO
    `sw_leagueoptions` (id, ranking_mode, num_up, num_down)
VALUES
    ("0", "0", "40", "0"),
    ("0", "1", "40", "0"),
    ("1", "0", "35", "0"),
    ("1", "1", "35", "0"),
    ("2", "0", "35", "0"),
    ("2", "1", "35", "0"),
    ("3", "0", "30", "0"),
    ("3", "1", "30", "0"),
    ("4", "0", "30", "0"),
    ("4", "1", "30", "0"),
    ("5", "0", "30", "0"),
    ("5", "1", "30", "0"),
    ("6", "0", "25", "0"),
    ("6", "1", "25", "0"),
    ("7", "0", "25", "0"),
    ("7", "1", "25", "0"),
    ("8", "0", "25", "0"),
    ("8", "1", "25", "0"),
    ("9", "0", "20", "7"),
    ("9", "1", "20", "7"),
    ("10", "0", "20", "7"),
    ("10", "1", "20", "7"),
    ("11", "0", "20", "7"),
    ("11", "1", "20", "7"),
    ("12", "0", "18", "8"),
    ("12", "1", "18", "8"),
    ("13", "0", "18", "8"),
    ("13", "1", "18", "8"),
    ("14", "0", "18", "8"),
    ("14", "1", "18", "8"),
    ("15", "0", "25", "25"),
    ("15", "1", "25", "25"),
    ("16", "0", "25", "25"),
    ("16", "1", "25", "25"),
    ("17", "0", "25", "25"),
    ("17", "1", "25", "25"),
    ("18", "0", "25", "25"),
    ("18", "1", "25", "25"),
    ("19", "0", "25", "25"),
    ("19", "1", "25", "25"),
    ("20", "0", "0", "25"),
    ("20", "1", "0", "25");

CREATE TABLE
    `sw_endlessleaguestates` (
        user_id BIGINT UNSIGNED NOT NULL,
        league_id BIGINT UNSIGNED NOT NULL,
        ranking_mode MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
        group_id BIGINT UNSIGNED NOT NULL,
        group_member TINYINT UNSIGNED NOT NULL DEFAULT 0,
        highscore_rank BIGINT UNSIGNED NOT NULL,
        totalscore_rank BIGINT UNSIGNED NOT NULL,
        high_score BIGINT UNSIGNED NOT NULL DEFAULT 0,
        total_score BIGINT UNSIGNED NOT NULL DEFAULT 0
    );

CREATE TABLE
    `sw_quickleaguestates` (
        user_id BIGINT UNSIGNED NOT NULL,
        league_id BIGINT UNSIGNED NOT NULL,
        ranking_mode MEDIUMINT UNSIGNED NOT NULL DEFAULT 1,
        group_id BIGINT UNSIGNED NOT NULL,
        group_member TINYINT UNSIGNED NOT NULL DEFAULT 0,
        highscore_rank BIGINT UNSIGNED NOT NULL,
        totalscore_rank BIGINT UNSIGNED NOT NULL,
        high_score BIGINT UNSIGNED NOT NULL DEFAULT 0,
        total_score BIGINT UNSIGNED NOT NULL DEFAULT 0
    );