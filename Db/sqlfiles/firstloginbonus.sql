DROP TABLE IF EXISTS `sw_firstloginbonus`;

CREATE TABLE
  `sw_firstloginbonus` (
    day TINYINT NOT NULL,
    item1 MEDIUMINT NOT NULL DEFAULT 900000,
    item1_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
    item2 MEDIUMINT NOT NULL DEFAULT 900000,
    item2_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
    item3 MEDIUMINT NOT NULL DEFAULT 900000,
    item3_count BIGINT UNSIGNED NOT NULL DEFAULT 5,
  );

INSERT INTO
  `sw_firstloginbonus` (
    day,
    item1,
    item1_count,
    item2,
    item2_count,
    item3,
    item3_count
  )
VALUES
  ('1','900000','10','910000','10000','240000','3'),
  ('2','900000','20','910000','20000','240000','3'),
  ('3','900000','20','910000','20000','240000','3'),
  ('4','230000','1','910000','50000','240000','3'),
  ('5','300013','1','910000','50000','240000','3');