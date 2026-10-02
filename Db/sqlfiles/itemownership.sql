DROP TABLE IF EXISTS `sw_itemownership`;

CREATE TABLE
    `sw_itemownership` (
        user_id BIGINT UNSIGNED NOT NULL,
        item_id BIGINT NOT NULL
    );