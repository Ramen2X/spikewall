DROP TABLE IF EXISTS `sw_itemroulette`;

DROP TABLE IF EXISTS `sw_chaoitemrouletteprizelist`;

CREATE TABLE
    `sw_itemroulette` (
        roulette_rank TINYINT NOT NULL,
        item_id BIGINT NOT NULL,
        item_num BIGINT NOT NULL,
        item_rate SMALLINT NOT NULL
    );

INSERT INTO
    `sw_itemroulette`
VALUES
    ('0', '200000', '1', '1250'),
    ('0', '120000', '2', '1250'),
    ('0', '120001', '2', '1250'),
    ('0', '120002', '2', '1250'),
    ('0', '200000', '1', '1250'),
    ('0', '900000', '3', '1250'),
    ('0', '120003', '2', '1250'),
    ('0', '120004', '2', '1250'),
    ('1', '200000', '1', '1250'),
    ('1', '120007', '2', '1250'),
    ('1', '0', '1', '1250'),
    ('1', '120005', '2', '1250'),
    ('1', '200000', '1', '1250'),
    ('1', '120007', '2', '1250'),
    ('1', '900000', '5', '1250'),
    ('1', '120006', '1', '1250'),
    ('2', '200000', '1', '1250'),
    ('2', '0', '1', '1250'),
    ('2', '900000', '5', '1250'),
    ('2', '0', '1', '1250'),
    ('2', '900000', '10', '1250'),
    ('2', '0', '1', '1250'),
    ('2', '900000', '5', '1250'),
    ('2', '0', '1', '1250');

CREATE TABLE 
    `sw_chaoitemrouletteprizelist` (
        chao_id INT MEDIUMINT UNSIGNED NOT NULL
    );