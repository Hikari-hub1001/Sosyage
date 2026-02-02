"use strict";

const form = document.getElementById("login-bonus-form");
const calendarGrid = document.getElementById("calendar");
const result = document.getElementById("result");
const monthInput = document.getElementById("month");
const startDateInput = document.getElementById("startDate");
const endDateInput = document.getElementById("endDate");
const loginBonusStatus = document.getElementById("login-bonus-status");
const loginBonusDelete = document.getElementById("login-bonus-delete");

const navItems = Array.from(document.querySelectorAll(".nav-item"));
const views = Array.from(document.querySelectorAll(".view"));
const homeLink = document.getElementById("home-link");

const accountTableBody = document.getElementById("account-table-body");
const accountStatus = document.getElementById("account-status");
const accountReload = document.getElementById("account-reload");
const accountPrev = document.getElementById("account-prev");
const accountNext = document.getElementById("account-next");
const accountPage = document.getElementById("account-page");
const accountCreateForm = document.getElementById("account-create-form");
const accountNameInput = document.getElementById("account-name");
const accountCreateStatus = document.getElementById("account-create-status");

const accountLimit = 50;
let accountOffset = 0;
let accountTotal = 0;
let accountLoading = false;

const assetsItemEndpoint = "/assets/item/list";
let assetsItems = [];
let assetsItemsLoading = false;
let assetsItemsLoaded = false;
let loginBonusLoading = false;
let loginBonusLoaded = false;

function pad2(value) {
    return String(value).padStart(2, "0");
}

function formatDate(date) {
    return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}`;
}

function parseDateInput(value) {
    const [year, month, day] = value.split("-").map(Number);
    return new Date(year, month - 1, day);
}

function daysInMonth(year, monthIndex) {
    return new Date(year, monthIndex + 1, 0).getDate();
}

function getAssetsItemId(item) {
    if (item == null || typeof item !== "object") {
        return null;
    }
    return item.id ?? item.Id ?? null;
}

function getAssetsItemName(item) {
    if (item == null || typeof item !== "object") {
        return "";
    }
    return item.name ?? item.Name ?? "";
}

function fillRewardSelect(select, selectedValue = "") {
    const storedValue = selectedValue || select.dataset.selectedValue || select.value;
    select.innerHTML = "";

    const hasItems = assetsItems.length > 0;
    const placeholder = document.createElement("option");
    placeholder.value = "";
    placeholder.textContent = hasItems ? "アイテムを選択" : "アイテムがありません";
    placeholder.disabled = hasItems;
    placeholder.selected = true;
    select.appendChild(placeholder);

    for (const item of assetsItems) {
        const id = getAssetsItemId(item);
        if (id == null) {
            continue;
        }

        const option = document.createElement("option");
        option.value = String(id);
        option.textContent = getAssetsItemName(item);
        select.appendChild(option);
    }

    if (storedValue) {
        const matched = Array.from(select.options).find((option) => option.value === storedValue);
        if (matched) {
            matched.selected = true;
            delete select.dataset.selectedValue;
        } else {
            select.dataset.selectedValue = storedValue;
        }
    } else {
        delete select.dataset.selectedValue;
    }
}

function updateRewardSelects() {
    const selects = Array.from(document.querySelectorAll(".reward-id"));
    for (const select of selects) {
        const selectedValue = select.dataset.selectedValue || select.value;
        fillRewardSelect(select, selectedValue);
    }
}

async function loadAssetsItems() {
    if (assetsItemsLoading || assetsItemsLoaded) {
        return;
    }

    assetsItemsLoading = true;
    try {
        const response = await fetch(assetsItemEndpoint);
        if (!response.ok) {
            console.error(`アイテム一覧の取得に失敗しました: ${response.status}`);
            return;
        }

        const data = await response.json();
        assetsItems = Array.isArray(data) ? data : [];
        assetsItemsLoaded = true;
        updateRewardSelects();
    } catch (error) {
        console.error("アイテム一覧の取得に失敗しました:", error);
    } finally {
        assetsItemsLoading = false;
    }
}

function createRewardRow(selectedRewardId = null, quantityValue = null) {
    const row = document.createElement("div");
    row.className = "reward-row";

    const itemSelect = document.createElement("select");
    itemSelect.className = "reward-id";
    itemSelect.required = true;
    const selectedValue = selectedRewardId == null ? "" : String(selectedRewardId);
    if (selectedValue) {
        itemSelect.dataset.selectedValue = selectedValue;
    }
    fillRewardSelect(itemSelect, selectedValue);

    const quantityInput = document.createElement("input");
    quantityInput.type = "number";
    quantityInput.min = "1";
    quantityInput.placeholder = "数量";
    quantityInput.className = "reward-qty";
    quantityInput.required = true;
    if (quantityValue != null) {
        quantityInput.value = String(quantityValue);
    }

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "ghost remove-reward";
    removeButton.textContent = "削除";

    row.append(itemSelect, quantityInput, removeButton);

    removeButton.addEventListener("click", () => {
        row.remove();
    });

    return row;
}

function createDayCell(dayNumber) {
    const cell = document.createElement("div");
    cell.className = "day-cell";
    cell.dataset.day = String(dayNumber);
    cell.innerHTML = `
    <div class="day-header">
      <span class="day-number">${dayNumber}日</span>
    </div>
    <div class="reward-list"></div>
    <button type="button" class="ghost add-reward">報酬を追加</button>
  `;

    const rewardList = cell.querySelector(".reward-list");
    cell.querySelector(".add-reward").addEventListener("click", () => {
        rewardList.appendChild(createRewardRow());
    });

    return cell;
}

function renderCalendar(date) {
    calendarGrid.innerHTML = "";

    const year = date.getFullYear();
    const monthIndex = date.getMonth();
    const totalDays = daysInMonth(year, monthIndex);
    const firstWeekday = new Date(year, monthIndex, 1).getDay();

    for (let i = 0; i < firstWeekday; i += 1) {
        const empty = document.createElement("div");
        empty.className = "calendar-empty";
        calendarGrid.appendChild(empty);
    }

    for (let day = 1; day <= totalDays; day += 1) {
        calendarGrid.appendChild(createDayCell(day));
    }
}

function setLoginBonusLoaded(value) {
    loginBonusLoaded = value;
    if (loginBonusDelete) {
        loginBonusDelete.disabled = !value;
    }
}

function applyLoginBonusData(data) {
    const monthValue = data.month ?? data.Month ?? monthInput.value;
    const startValue = data.startDate ?? data.StartDate ?? "";
    const endValue = data.endDate ?? data.EndDate ?? "";
    const days = Array.isArray(data.days ?? data.Days) ? (data.days ?? data.Days) : [];

    if (monthValue) {
        monthInput.value = monthValue;
        renderCalendar(parseDateInput(monthValue));
    }

    if (startValue) {
        startDateInput.value = startValue;
    }

    if (endValue) {
        endDateInput.value = endValue;
    }

    for (const day of days) {
        const dayNumber = day.dayNumber ?? day.DayNumber;
        if (!Number.isInteger(dayNumber)) {
            continue;
        }

        const cell = calendarGrid.querySelector(`.day-cell[data-day="${dayNumber}"]`);
        if (!cell) {
            continue;
        }

        const rewardList = cell.querySelector(".reward-list");
        const rewards = Array.isArray(day.rewards ?? day.Rewards) ? (day.rewards ?? day.Rewards) : [];
        for (const reward of rewards) {
            const rewardId = reward.rewardId ?? reward.RewardId;
            const quantity = reward.quantity ?? reward.Quantity;
            rewardList.appendChild(createRewardRow(rewardId, quantity));
        }
    }
}

async function loadLoginBonusForMonth(month) {
    if (!month || loginBonusLoading) {
        return;
    }

    loginBonusLoading = true;
    if (loginBonusStatus) {
        loginBonusStatus.textContent = "読み込み中...";
    }
    setLoginBonusLoaded(false);

    try {
        const response = await fetch(`/admin/login-bonus?month=${encodeURIComponent(month)}`);
        if (response.status === 404) {
            if (loginBonusStatus) {
                loginBonusStatus.textContent = "未登録です。";
            }
            return;
        }

        if (!response.ok) {
            if (loginBonusStatus) {
                loginBonusStatus.textContent = `取得に失敗しました: ${response.status}`;
            }
            return;
        }

        const data = await response.json();
        applyLoginBonusData(data);
        setLoginBonusLoaded(true);
        if (loginBonusStatus) {
            loginBonusStatus.textContent = "読み込みました。";
        }
    } catch (error) {
        if (loginBonusStatus) {
            loginBonusStatus.textContent = `取得に失敗しました: ${error}`;
        }
    } finally {
        loginBonusLoading = false;
    }
}

function syncDateRangeFromMonth() {
    if (!monthInput.value) {
        return;
    }

    const selected = parseDateInput(monthInput.value);
    selected.setDate(1);
    monthInput.value = formatDate(selected);

    const end = new Date(selected.getFullYear(), selected.getMonth() + 1, 0);
    startDateInput.value = formatDate(selected);
    endDateInput.value = formatDate(end);

    renderCalendar(selected);
}

async function loadAccounts() {
    if (accountLoading) {
        return;
    }

    accountLoading = true;
    accountStatus.textContent = "読み込み中...";
    accountTableBody.innerHTML = "";

    try {
        const response = await fetch(`/admin/account?offset=${accountOffset}&limit=${accountLimit}`);
        if (!response.ok) {
            accountStatus.textContent = `取得に失敗しました: ${response.status}`;
            return;
        }

        const data = await response.json();
        accountTotal = Number(data.total ?? 0);
        const accounts = Array.isArray(data.accounts) ? data.accounts : [];

        if (accounts.length === 0) {
            accountStatus.textContent = "データがありません。";
        } else {
            accountStatus.textContent = "";
        }

        for (const account of accounts) {
            const row = document.createElement("tr");
            const accountId = account.id;
            const idCell = document.createElement("td");
            idCell.textContent = accountId == null ? "" : String(accountId);
            row.appendChild(idCell);

            const nameCell = document.createElement("td");
            nameCell.textContent = account.name ?? "";
            row.appendChild(nameCell);

            const lastLoginCell = document.createElement("td");
            lastLoginCell.textContent = account.lastLoginAt ?? "";
            row.appendChild(lastLoginCell);

            const actionCell = document.createElement("td");
            const deleteButton = document.createElement("button");
            deleteButton.type = "button";
            deleteButton.className = "ghost danger";
            deleteButton.dataset.accountId = accountId == null ? "" : String(accountId);
            deleteButton.textContent = "削除";
            actionCell.appendChild(deleteButton);
            row.appendChild(actionCell);

            deleteButton.addEventListener("click", async () => {
                const confirmed = window.confirm(`アカウント ${accountId} を削除しますか？`);
                if (!confirmed) {
                    return;
                }

                const deleteResponse = await fetch(`/admin/account/${accountId}`, {
                    method: "DELETE"
                });

                if (!deleteResponse.ok) {
                    alert(`削除に失敗しました: ${deleteResponse.status}`);
                    return;
                }

                await loadAccounts();
            });

            accountTableBody.appendChild(row);
        }

        const currentPage = Math.floor(accountOffset / accountLimit) + 1;
        const totalPages = Math.max(1, Math.ceil(accountTotal / accountLimit));
        accountPage.textContent = `${currentPage} / ${totalPages}`;
        accountPrev.disabled = accountOffset <= 0;
        accountNext.disabled = accountOffset + accountLimit >= accountTotal;
    } catch (error) {
        accountStatus.textContent = `取得に失敗しました: ${error}`;
    } finally {
        accountLoading = false;
    }
}

function setView(viewName) {
    navItems.forEach((item) => {
        item.classList.toggle("is-active", item.dataset.view === viewName);
    });

    views.forEach((view) => {
        view.classList.toggle("is-active", view.dataset.view === viewName);
    });

    if (viewName === "accounts") {
        loadAccounts();
    }

    if (viewName === "login-bonus") {
        loadAssetsItems();
        loadLoginBonusForMonth(monthInput.value);
    }
}

navItems.forEach((item) => {
    item.addEventListener("click", () => {
        const viewName = item.dataset.view;
        if (!viewName) {
            return;
        }
        setView(viewName);
    });
});

if (homeLink) {
    homeLink.addEventListener("click", () => {
        setView("home");
    });
}

accountPrev.addEventListener("click", () => {
    accountOffset = Math.max(0, accountOffset - accountLimit);
    loadAccounts();
});

accountNext.addEventListener("click", () => {
    accountOffset += accountLimit;
    loadAccounts();
});

accountReload.addEventListener("click", () => {
    loadAccounts();
});

accountCreateForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const name = accountNameInput.value.trim();
    if (!name) {
        accountCreateStatus.textContent = "名前を入力してください。";
        return;
    }

    accountCreateStatus.textContent = "作成中...";

    try {
        const response = await fetch("/account/registration", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name })
        });

        if (!response.ok) {
            accountCreateStatus.textContent = `作成に失敗しました: ${response.status}`;
            return;
        }

        accountNameInput.value = "";
        accountCreateStatus.textContent = "作成しました。";
        accountOffset = 0;
        loadAccounts();
    } catch (error) {
        accountCreateStatus.textContent = `作成に失敗しました: ${error}`;
    }
});

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    const payload = {
        month: monthInput.value,
        startDate: startDateInput.value,
        endDate: endDateInput.value,
        days: []
    };

    if (!payload.month || !payload.startDate || !payload.endDate) {
        result.textContent = "日付と期間を設定してください。";
        return;
    }

    const dayCells = Array.from(document.querySelectorAll(".day-cell"));
    for (const cell of dayCells) {
        const dayNumber = Number(cell.dataset.day);
        const rewardRows = Array.from(cell.querySelectorAll(".reward-row"));

        if (rewardRows.length === 0) {
            continue;
        }

        const rewards = rewardRows.map((row) => {
            const rewardId = Number(row.querySelector(".reward-id").value);
            const quantity = Number(row.querySelector(".reward-qty").value);
            return { rewardId, quantity };
        });

        if (rewards.some((reward) => !Number.isInteger(reward.rewardId) || reward.rewardId < 1 || !Number.isInteger(reward.quantity) || reward.quantity < 1)) {
            result.textContent = `${dayNumber}日の報酬IDと数量は1以上の整数で入力してください。`;
            return;
        }

        payload.days.push({ dayNumber, rewards });
    }

    if (payload.days.length === 0) {
        result.textContent = "報酬が設定された日がありません。";
        return;
    }

    try {
        result.textContent = "送信中...";
        const response = await fetch("/admin/login-bonus", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        const text = await response.text();
        let data = text;
        try {
            data = JSON.parse(text);
        } catch {
            // text is fine
        }

        if (!response.ok) {
            result.textContent = JSON.stringify({ status: response.status, data }, null, 2);
            return;
        }

        result.textContent = JSON.stringify({ status: response.status, data }, null, 2);
        await loadLoginBonusForMonth(monthInput.value);
    } catch (error) {
        result.textContent = `送信に失敗しました: ${error}`;
    }
});

monthInput.addEventListener("change", () => {
    syncDateRangeFromMonth();
    loadLoginBonusForMonth(monthInput.value);
});

if (loginBonusDelete) {
    loginBonusDelete.addEventListener("click", async () => {
        const monthValue = monthInput.value;
        if (!monthValue || !loginBonusLoaded) {
            return;
        }

        const confirmed = window.confirm(`${monthValue} のログインボーナスを削除しますか？`);
        if (!confirmed) {
            return;
        }

        if (loginBonusStatus) {
            loginBonusStatus.textContent = "削除中...";
        }

        try {
            const response = await fetch(`/admin/login-bonus?month=${encodeURIComponent(monthValue)}`, {
                method: "DELETE"
            });

            if (!response.ok) {
                if (loginBonusStatus) {
                    loginBonusStatus.textContent = `削除に失敗しました: ${response.status}`;
                }
                return;
            }

            if (loginBonusStatus) {
                loginBonusStatus.textContent = "削除しました。";
            }
            result.textContent = "未登録";
            setLoginBonusLoaded(false);
            syncDateRangeFromMonth();
        } catch (error) {
            if (loginBonusStatus) {
                loginBonusStatus.textContent = `削除に失敗しました: ${error}`;
            }
        }
    });
}

if (monthInput.value) {
    syncDateRangeFromMonth();
} else {
    const now = new Date();
    monthInput.value = formatDate(new Date(now.getFullYear(), now.getMonth(), 1));
    syncDateRangeFromMonth();
}

setLoginBonusLoaded(false);
setView("home");
