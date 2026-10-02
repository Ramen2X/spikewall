DROP TABLE IF EXISTS `sw_chaowheeloptions`;

CREATE TABLE
  `sw_chaowheeloptions` (
    user_id BIGINT UNSIGNED NOT NULL PRIMARY KEY,
    chao_won INTEGER NOT NULL,
    num_chao_roulette BIGINT NOT NULL DEFAULT 0
  );