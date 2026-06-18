DROP TABLE IF EXISTS `sw_config`;

CREATE TABLE
    `sw_config` (
        id TINYINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
        is_maintenance TINYINT NOT NULL DEFAULT 0,
        support_legacy_versions TINYINT NOT NULL DEFAULT 1,
        debug_log TINYINT NOT NULL DEFAULT 0,
        enable_debug_endpoints TINYINT NOT NULL DEFAULT 0,
        encryption_iv VARCHAR(16) NOT NULL DEFAULT 'burgersMetKortin',
        session_time INT NOT NULL DEFAULT 3600,
        assets_version VARCHAR(3) NOT NULL DEFAULT '049',
        client_version VARCHAR(8) NOT NULL DEFAULT '2.0.3',
        data_version VARCHAR(2) NOT NULL DEFAULT '15',
        info_version VARCHAR(3) NOT NULL DEFAULT '017',
        revive_rsr_cost BIGINT UNSIGNED NOT NULL DEFAULT 5,
        enable_limited_time_incentives TINYINT NOT NULL DEFAULT 1
    );

INSERT INTO
    `sw_config` (id)
VALUES
    ('1');