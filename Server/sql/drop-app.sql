-- SQLite
PRAGMA foreign_keys = OFF;

DROP TABLE IF EXISTS account_login_bonus_log;
DROP TABLE IF EXISTS account_login_bonus;
DROP TABLE IF EXISTS login_bonus_day_reward;
DROP TABLE IF EXISTS login_bonus_day;
DROP TABLE IF EXISTS login_bonus_month;
DROP TABLE IF EXISTS item_master;
DROP TABLE IF EXISTS account;

DELETE FROM sqlite_sequence
WHERE name IN (
    'account_login_bonus_log',
    'account_login_bonus',
    'login_bonus_day_reward',
    'login_bonus_day',
    'login_bonus_month',
    'item_master',
    'account'
);

PRAGMA foreign_keys = ON;
