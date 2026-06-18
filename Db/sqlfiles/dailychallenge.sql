DROP TABLE IF EXISTS `sw_dailychallenge`;

CREATE TABLE
    `sw_dailychallenge` (
        id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
        item1 MEDIUMINT NOT NULL DEFAULT 900000,
        item1_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item2 MEDIUMINT NOT NULL DEFAULT 900000,
        item2_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item3 MEDIUMINT NOT NULL DEFAULT 900000,
        item3_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item4 MEDIUMINT NOT NULL DEFAULT 900000,
        item4_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item5 MEDIUMINT NOT NULL DEFAULT 900000,
        item5_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item6 MEDIUMINT NOT NULL DEFAULT 900000,
        item6_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
        item7 MEDIUMINT NOT NULL DEFAULT 900000,
        item7_count BIGINT UNSIGNED NOT NULL DEFAULT 10
    );

INSERT INTO
    `sw_dailychallenge` (
        id,
        item1,
        item1_count,
        item2,
        item2_count,
        item3,
        item3_count,
        item4,
        item4_count,
        item5,
        item5_count,
        item6,
        item6_count,
        item7,
        item7_count
    )
VALUES
    (
        '1',
        "910000",
        "5000",
        "120000",
        "3",
        "120001",
        "3",
        "120002",
        "3",
        "120003",
        "3",
        "120004",
        "3",
        "900000",
        "10"
    ),
    (
        '2',
        "910000",
        "4000",
        "120005",
        "3",
        "910000",
        "4000",
        "120006",
        "3",
        "910000",
        "4000",
        "120007",
        "3",
        "900000",
        "10"
    ),
    (
        '3',
        "910000",
        "2500",
        "910000",
        "2500",
        "910000",
        "2500",
        "910000",
        "5000",
        "910000",
        "5000",
        "910000",
        "10000",
        "900000",
        "10"
    ),
    (
        '4',
        "900000",
        "5",
        "900000",
        "5",
        "900000",
        "5",
        "900000",
        "5",
        "900000",
        "5",
        "900000",
        "5",
        "900000",
        "10"
    );