DROP TABLE IF EXISTS `sw_chaowheeloptions`;

CREATE TABLE
  `sw_chaowheeloptions` (
    user_id BIGINT UNSIGNED NOT NULL PRIMARY KEY,
    campaign INT NOT NULL DEFAULT 0,
    num_special_egg  BIGINT NOT NULL DEFAULT 0,
    chao_roulette_ticket BIGINT DEFAULT 0,
    chao_roulette_cost BIGINT NOT NULL DEFAULT 50,
    chao_won INTEGER NOT NULL,
    chao_roulette_rank TINYINT NOT NULL DEFAULT 0
  );