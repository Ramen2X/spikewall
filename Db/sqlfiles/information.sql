DROP TABLE IF EXISTS `sw_information`;

CREATE TABLE
    `sw_information` (
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
    );