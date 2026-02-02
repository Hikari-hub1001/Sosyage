-- Account
CREATE TABLE account (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    last_login_at TEXT NOT NULL
);

-- Login Bonus
CREATE TABLE login_bonus_month (
    id INTEGER PRIMARY KEY,
    month TEXT NOT NULL, -- YYYY-MM-DD (first day of month)
    start_date TEXT NOT NULL, -- YYYY-MM-DD
    end_date TEXT NOT NULL, -- YYYY-MM-DD
    UNIQUE (month)
);

CREATE TABLE login_bonus_day (
    id INTEGER PRIMARY KEY,
    month_id INTEGER NOT NULL,
    day_number INTEGER NOT NULL, -- 1..31
    FOREIGN KEY (month_id) REFERENCES login_bonus_month(id)
);
CREATE INDEX idx_login_bonus_day_month_id ON login_bonus_day(month_id);

CREATE TABLE login_bonus_day_reward (
    id INTEGER PRIMARY KEY,
    day_id INTEGER NOT NULL,
    reward_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (day_id) REFERENCES login_bonus_day(id)
);
CREATE INDEX idx_login_bonus_day_reward_day_id ON login_bonus_day_reward(day_id);

CREATE TABLE account_login_bonus (
    id INTEGER PRIMARY KEY,
    account_id INTEGER NOT NULL,
    month_id INTEGER NOT NULL,
    current_day INTEGER NOT NULL,
    last_claimed_day INTEGER,
    FOREIGN KEY (account_id) REFERENCES Account(id),
    FOREIGN KEY (month_id) REFERENCES login_bonus_month(id),
    UNIQUE (account_id, month_id)
);
CREATE INDEX idx_account_login_bonus_account_id ON account_login_bonus(account_id);

CREATE TABLE account_login_bonus_log (
    id INTEGER PRIMARY KEY,
    account_id INTEGER NOT NULL,
    month_id INTEGER NOT NULL,
    day_number INTEGER NOT NULL,
    claimed_at TEXT NOT NULL,
    FOREIGN KEY (account_id) REFERENCES Account(id),
    FOREIGN KEY (month_id) REFERENCES login_bonus_month(id),
    UNIQUE (account_id, month_id, day_number)
);
CREATE INDEX idx_account_login_bonus_log_account_id ON account_login_bonus_log(account_id);

-- Item Master
CREATE TABLE item_master (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
);
