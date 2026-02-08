-- Account
CREATE TABLE account (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    last_login_at TEXT NOT NULL
);

-- Login Bonus
CREATE TABLE login_bonus (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL,
    start_date TEXT NOT NULL, -- YYYY-MM-DD
    end_date TEXT NOT NULL, -- YYYY-MM-DD
    type TEXT NOT NULL -- e.g. "monthly", "event"
);

CREATE TABLE login_bonus_day (
    id INTEGER PRIMARY KEY,
    login_bonus_id INTEGER NOT NULL,
    date TEXT NOT NULL,
    FOREIGN KEY (login_bonus_id) REFERENCES login_bonus(id),
    UNIQUE (login_bonus_id, date)
);
CREATE INDEX idx_login_bonus_day_login_bonus_id ON login_bonus_day(login_bonus_id);

CREATE TABLE login_bonus_day_reward (
    id INTEGER PRIMARY KEY,
    login_bonus_day_id INTEGER NOT NULL,
    reward_id INTEGER NOT NULL,
    FOREIGN KEY (login_bonus_day_id) REFERENCES login_bonus_day(id),
    FOREIGN KEY (reward_id) REFERENCES reward(id),
    UNIQUE (login_bonus_day_id, reward_id)
);
CREATE INDEX idx_login_bonus_day_reward_login_bonus_day_id ON login_bonus_day_reward(login_bonus_day_id);

CREATE TABLE account_login_bonus (
    id INTEGER PRIMARY KEY,
    account_id INTEGER NOT NULL,
    login_bonus_id INTEGER NOT NULL,
    claim_count INTEGER NOT NULL,
    last_claimed_day INTEGER,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (login_bonus_id) REFERENCES login_bonus(id),
    UNIQUE (account_id, login_bonus_id)
);
CREATE INDEX idx_account_login_bonus_account_id ON account_login_bonus(account_id);
CREATE INDEX idx_account_login_bonus_login_bonus_id ON account_login_bonus(login_bonus_id);

CREATE TABLE account_login_bonus_log (
    id INTEGER PRIMARY KEY,
    account_id INTEGER NOT NULL,
    login_bonus_id INTEGER NOT NULL,
    login_bonus_day_id INTEGER NOT NULL,
    claim_count INTEGER NOT NULL,
    claimed_at TEXT NOT NULL,
    FOREIGN KEY (account_id) REFERENCES account(id),
    FOREIGN KEY (login_bonus_id) REFERENCES login_bonus(id),
    FOREIGN KEY (login_bonus_day_id) REFERENCES login_bonus_day(id),
    UNIQUE (account_id, login_bonus_day_id)
);
CREATE INDEX idx_account_login_bonus_log_account_id ON account_login_bonus_log(account_id);

-- Common Reward
CREATE TABLE reward (
    id INTEGER PRIMARY KEY,
    type TEXT NOT NULL,
    item_id INTEGER,
    quantity INTEGER NOT NULL,
    FOREIGN KEY (item_id) REFERENCES item_master(id)
);
CREATE INDEX idx_reward_type ON reward(type);
CREATE INDEX idx_reward_item_id ON reward(item_id);

-- Item Master
CREATE TABLE item_master (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL
);
