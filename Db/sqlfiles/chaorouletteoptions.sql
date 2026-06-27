DROP TABLE IF EXISTS `sw_chaoroulette`;

DROP TABLE IF EXISTS `sw_chaorouletteprizelist`

CREATE TABLE
  `sw_chaoroulette` (
    chao_roulette_rank TINYINT NOT NULL,
    chao_rarity INT UNSIGNED NOT NULL,
    chao_rate SMALLINT NOT NULL
  );

INSERT INTO
  `sw_chaoroulette`
VALUES
  ('0', '2', '600'),
  ('0', '1', '1700'),
  ('0', '100', '500'),
  ('0', '1', '1700'),
  ('0', '2', '1600'),
  ('0', '1', '1700'),
  ('0', '100', '500'),
  ('0', '1', '1700'),
  ('1', '2', '1467'),
  ('1', '1', '1200'),
  ('1', '100', '1000'),
  ('1', '2', '1467'),
  ('1', '1', '1200'),
  ('1', '2', '1467'),
  ('1', '100', '1000'),
  ('1', '1', '1200');

CREATE TABLE 
    `sw_chaorouletteprizelist`  (
    chao_id INT MEDIUMINT UNSIGNED NOT NULL
);