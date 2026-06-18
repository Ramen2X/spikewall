DROP TABLE IF EXISTS `sw_tickers`;

CREATE TABLE
    `sw_tickers` (
        id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
        start_time BIGINT UNSIGNED NOT NULL,
        end_time BIGINT UNSIGNED NOT NULL,
        message VARCHAR(600) NOT NULL,
        language TINYINT NOT NULL
    );