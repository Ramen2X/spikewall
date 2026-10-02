DROP TABLE IF EXISTS `sw_wheeloptions`;

CREATE TABLE
    `sw_wheeloptions` (
        user_id BIGINT UNSIGNED NOT NULL PRIMARY KEY,
        next_free_spin BIGINT NOT NULL DEFAULT 0,
        num_free_spins TINYINT NOT NULL DEFAULT 3,
        item_won INTEGER NOT NULL,
        roulette_rank TINYINT NOT NULL DEFAULT 0
    );