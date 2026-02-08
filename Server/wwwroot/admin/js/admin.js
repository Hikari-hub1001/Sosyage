"use strict";

const form = document.getElementById("login-bonus-form");
const calendarGrid = document.getElementById("calendar");
const result = document.getElementById("result");
const loginBonusSelect = document.getElementById("login-bonus-select");
const loginBonusNameInput = document.getElementById("login-bonus-name");
const loginBonusType = document.getElementById("login-bonus-type");
const monthInput = document.getElementById("month");
const startDateInput = document.getElementById("startDate");
const endDateInput = document.getElementById("endDate");
const loginBonusStatus = document.getElementById("login-bonus-status");
const loginBonusDelete = document.getElementById("login-bonus-delete");
const loginBonusCalendar = document.getElementById("login-bonus-calendar");
const eventList = document.getElementById("event-list");
const eventDays = document.getElementById("event-days");
const eventAdd = document.getElementById("event-add");
const monthlyModeLabels = Array.from(document.querySelectorAll("[data-mode=\"monthly\"]"));

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

function unwrapResponse(data) {
    if (data && typeof data === "object") {
        if ("response" in data) {
            return data.response;
        }
        if ("Response" in data) {
            return data.Response;
        }
    }
    return data;
}

function pad2(value) {
    return String(value).padStart(2, "0");
}

function formatDate(date) {
    return `${date.getFullYear()}-${pad2(date.getMonth() + 1)}-${pad2(date.getDate())}`;
}

function parseDateInput(value) {
    if (!value) {
        return null;
    }
    const [year, month, day] = value.split("-").map(Number);
    if (!year || !month || !day) {
        return null;
    }
    const date = new Date(year, month - 1, day);
    if (Number.isNaN(date.getTime())) {
        return null;
    }
    return date;
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

        const data = unwrapResponse(await response.json());
        const items = Array.isArray(data)
            ? data
            : Array.isArray(data.items ?? data.Items)
                ? (data.items ?? data.Items)
                : [];
        assetsItems = items;
        assetsItemsLoaded = true;
        updateRewardSelects();
    } catch (error) {
        console.error("アイテム一覧の取得に失敗しました:", error);
    } finally {
        assetsItemsLoading = false;
    }
}

async function loadLoginBonusOptions(selectedId = null) {
    if (!loginBonusSelect) {
        return;
    }

    try {
        const response = await fetch("/admin/login-bonus/list");
        if (!response.ok) {
            console.error(`ログインボーナス一覧の取得に失敗しました: ${response.status}`);
            return;
        }

        const data = unwrapResponse(await response.json());
        const items = Array.isArray(data.items ?? data.Items) ? (data.items ?? data.Items) : [];
        const currentId = selectedId ?? getLoginBonusId();

        loginBonusSelect.innerHTML = "";
        const emptyOption = document.createElement("option");
        emptyOption.value = "";
        emptyOption.textContent = "-";
        loginBonusSelect.appendChild(emptyOption);

        for (const item of items) {
            const id = item.id ?? item.Id;
            const name = item.name ?? item.Name ?? "";
            if (!Number.isInteger(id) || id <= 0) {
                continue;
            }

            const option = document.createElement("option");
            option.value = String(id);
            option.textContent = name ? `${id}: ${name}` : String(id);
            loginBonusSelect.appendChild(option);
        }

        if (currentId && Array.from(loginBonusSelect.options).some((option) => option.value === String(currentId))) {
            loginBonusSelect.value = String(currentId);
            if (selectedId) {
                await loadLoginBonusById();
            }
        } else {
            loginBonusSelect.value = "";
        }
    } catch (error) {
        console.error("ログインボーナス一覧の取得に失敗しました:", error);
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

    if (!date) {
        return;
    }

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

function setLoginBonusMode(type) {
    const isMonthly = type === "monthly";
    loginBonusCalendar.classList.toggle("is-hidden", !isMonthly);
    eventList.classList.toggle("is-hidden", isMonthly);
    monthlyModeLabels.forEach((label) => {
        label.classList.toggle("is-hidden", !isMonthly);
    });
    if (monthInput) {
        monthInput.required = isMonthly;
    }
}

function clearEventDays() {
    eventDays.innerHTML = "";
}

function updateEventDayIndices() {
    const rows = Array.from(eventDays.querySelectorAll(".event-day"));
    const entries = rows.map((row, index) => {
        const dateInput = row.querySelector(".event-date");
        const dateValue = dateInput ? dateInput.value : "";
        return { row, index, dateValue };
    });

    const ordered = entries
        .filter((entry) => entry.dateValue)
        .sort((a, b) => a.dateValue.localeCompare(b.dateValue) || a.index - b.index);

    const orderMap = new Map();
    ordered.forEach((entry, index) => {
        orderMap.set(entry.row, index + 1);
    });

    for (const entry of entries) {
        const label = entry.row.querySelector(".event-day-index");
        if (!label) {
            continue;
        }

        if (!entry.dateValue) {
            label.textContent = "日付未設定";
            continue;
        }

        const order = orderMap.get(entry.row);
        label.textContent = `${order}日目 (${entry.dateValue})`;
    }
}

function createEventDayRow(dateValue = "", rewards = []) {
    const row = document.createElement("div");
    row.className = "event-day";

    const header = document.createElement("div");
    header.className = "event-day-header";

    const indexLabel = document.createElement("span");
    indexLabel.className = "event-day-index";
    indexLabel.textContent = "日付未設定";

    const dateLabel = document.createElement("label");
    dateLabel.textContent = "日付";

    const dateInput = document.createElement("input");
    dateInput.type = "date";
    dateInput.className = "event-date";
    dateInput.required = true;
    if (dateValue) {
        dateInput.value = dateValue;
    }
    dateLabel.appendChild(dateInput);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "ghost danger";
    removeButton.textContent = "削除";

    header.append(indexLabel, dateLabel, removeButton);

    const rewardList = document.createElement("div");
    rewardList.className = "reward-list";

    const addButton = document.createElement("button");
    addButton.type = "button";
    addButton.className = "ghost add-reward";
    addButton.textContent = "報酬を追加";

    row.append(header, rewardList, addButton);

    addButton.addEventListener("click", () => {
        rewardList.appendChild(createRewardRow());
    });

    removeButton.addEventListener("click", () => {
        row.remove();
        updateEventDayIndices();
    });

    dateInput.addEventListener("change", () => {
        updateEventDayIndices();
    });

    for (const reward of rewards) {
        const itemId = reward.itemId ?? reward.ItemId;
        const quantity = reward.quantity ?? reward.Quantity;
        rewardList.appendChild(createRewardRow(itemId, quantity));
    }

    return row;
}

function appendEventDay(dateValue = "", rewards = []) {
    const row = createEventDayRow(dateValue, rewards);
    eventDays.appendChild(row);
    updateEventDayIndices();
}

function setLoginBonusLoaded(value) {
    loginBonusLoaded = value;
    if (loginBonusDelete) {
        loginBonusDelete.disabled = !value;
    }
}

function applyLoginBonusData(data) {
    const idValue = data.id ?? data.Id ?? null;
    const nameValue = data.name ?? data.Name ?? "";
    const typeValue = data.type ?? data.Type ?? loginBonusType.value;
    const startValue = data.startDate ?? data.StartDate ?? "";
    const endValue = data.endDate ?? data.EndDate ?? "";
    const days = Array.isArray(data.days ?? data.Days) ? (data.days ?? data.Days) : [];

    if (idValue != null && idValue !== "" && loginBonusSelect) {
        const idText = String(idValue);
        if (!Array.from(loginBonusSelect.options).some((option) => option.value === idText)) {
            const option = document.createElement("option");
            option.value = idText;
            option.textContent = nameValue ? `${idText}: ${nameValue}` : idText;
            loginBonusSelect.appendChild(option);
        }
        loginBonusSelect.value = idText;
    }

    if (nameValue) {
        loginBonusNameInput.value = nameValue;
    }

    if (typeValue) {
        loginBonusType.value = typeValue;
    }

    setLoginBonusMode(loginBonusType.value);

    if (startValue) {
        startDateInput.value = startValue;
    }

    if (endValue) {
        endDateInput.value = endValue;
    }

    if (loginBonusType.value === "monthly") {
        const baseDate = parseDateInput(startValue) ?? parseDateInput(monthInput.value);
        if (baseDate) {
            const monthStart = new Date(baseDate.getFullYear(), baseDate.getMonth(), 1);
            monthInput.value = formatDate(monthStart);
            renderCalendar(monthStart);
        }

        for (const day of days) {
            const dateValue = day.date ?? day.Date;
            const date = parseDateInput(dateValue);
            if (!date) {
                continue;
            }

            const dayNumber = date.getDate();
            const cell = calendarGrid.querySelector(`.day-cell[data-day="${dayNumber}"]`);
            if (!cell) {
                continue;
            }

            const rewardList = cell.querySelector(".reward-list");
            const rewards = Array.isArray(day.rewards ?? day.Rewards) ? (day.rewards ?? day.Rewards) : [];
            for (const reward of rewards) {
                const itemId = reward.itemId ?? reward.ItemId;
                const quantity = reward.quantity ?? reward.Quantity;
                rewardList.appendChild(createRewardRow(itemId, quantity));
            }
        }

        clearEventDays();
    } else {
        clearEventDays();
        for (const day of days) {
            const dateValue = day.date ?? day.Date ?? "";
            const rewards = Array.isArray(day.rewards ?? day.Rewards) ? (day.rewards ?? day.Rewards) : [];
            appendEventDay(dateValue, rewards);
        }
        updateEventDayIndices();
    }
}

function getLoginBonusId() {
    if (!loginBonusSelect) {
        return null;
    }

    const value = Number(loginBonusSelect.value);
    if (!Number.isInteger(value) || value <= 0) {
        return null;
    }

    return value;
}

function resetLoginBonusEditor() {
    if (loginBonusType.value === "monthly") {
        renderCalendar(parseDateInput(monthInput.value));
    } else {
        clearEventDays();
    }
}

async function loadLoginBonusById() {
    const id = getLoginBonusId();
    if (!id || loginBonusLoading) {
        return;
    }

    loginBonusLoading = true;
    if (loginBonusStatus) {
        loginBonusStatus.textContent = "読み込み中...";
    }
    setLoginBonusLoaded(false);

    try {
        const response = await fetch(`/admin/login-bonus?id=${encodeURIComponent(id)}`);
        if (response.status === 404) {
            if (loginBonusStatus) {
                loginBonusStatus.textContent = "未登録です。";
            }
            result.textContent = "未登録";
            resetLoginBonusEditor();
            return;
        }

        if (!response.ok) {
            if (loginBonusStatus) {
                loginBonusStatus.textContent = `取得に失敗しました: ${response.status}`;
            }
            return;
        }

        const data = unwrapResponse(await response.json());
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
    if (!selected) {
        return;
    }
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

        const data = unwrapResponse(await response.json());
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
        setLoginBonusMode(loginBonusType.value);
        if (loginBonusType.value === "monthly") {
            syncDateRangeFromMonth();
        }
        loadLoginBonusOptions(getLoginBonusId());
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

    const nameValue = loginBonusNameInput.value.trim();
    const payload = {
        id: getLoginBonusId() ?? 0,
        name: nameValue,
        type: loginBonusType.value,
        startDate: startDateInput.value,
        endDate: endDateInput.value,
        days: []
    };

    if (!payload.name) {
        result.textContent = "名称を入力してください。";
        return;
    }

    if (!payload.type || !payload.startDate || !payload.endDate) {
        result.textContent = "日付と期間を設定してください。";
        return;
    }

    if (payload.type === "monthly") {
        if (!monthInput.value) {
            result.textContent = "月を設定してください。";
            return;
        }

        const monthDate = parseDateInput(monthInput.value);
        if (!monthDate) {
            result.textContent = "月の形式が正しくありません。";
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
                const itemId = Number(row.querySelector(".reward-id").value);
                const quantity = Number(row.querySelector(".reward-qty").value);
                return { itemId, quantity };
            });

            if (rewards.some((reward) => !Number.isInteger(reward.itemId) || reward.itemId < 1 || !Number.isInteger(reward.quantity) || reward.quantity < 1)) {
                result.textContent = `${dayNumber}日のアイテムと数量は1以上の整数で入力してください。`;
                return;
            }

            const date = new Date(monthDate.getFullYear(), monthDate.getMonth(), dayNumber);
            payload.days.push({ date: formatDate(date), rewards });
        }
    } else {
        const dayRows = Array.from(eventDays.querySelectorAll(".event-day"));
        for (const row of dayRows) {
            const dateValue = row.querySelector(".event-date").value;
            const rewardRows = Array.from(row.querySelectorAll(".reward-row"));

            if (!dateValue) {
                result.textContent = "日付を入力してください。";
                return;
            }

            if (rewardRows.length === 0) {
                continue;
            }

            const rewards = rewardRows.map((rewardRow) => {
                const itemId = Number(rewardRow.querySelector(".reward-id").value);
                const quantity = Number(rewardRow.querySelector(".reward-qty").value);
                return { itemId, quantity };
            });

            if (rewards.some((reward) => !Number.isInteger(reward.itemId) || reward.itemId < 1 || !Number.isInteger(reward.quantity) || reward.quantity < 1)) {
                result.textContent = "報酬のアイテムと数量は1以上の整数で入力してください。";
                return;
            }

            payload.days.push({ date: dateValue, rewards });
        }
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

        const responsePayload = unwrapResponse(data);
        if (!response.ok) {
            result.textContent = JSON.stringify({ status: response.status, data }, null, 2);
            return;
        }

        result.textContent = JSON.stringify({ status: response.status, data }, null, 2);
        const loginBonusId = responsePayload?.loginBonusId ?? responsePayload?.LoginBonusId ?? null;
        if (loginBonusId != null) {
            await loadLoginBonusOptions(loginBonusId);
        } else {
            await loadLoginBonusById();
        }
    } catch (error) {
        result.textContent = `送信に失敗しました: ${error}`;
    }
});

monthInput.addEventListener("change", () => {
    syncDateRangeFromMonth();
});

if (loginBonusDelete) {
    loginBonusDelete.addEventListener("click", async () => {
        const id = getLoginBonusId();
        if (!id || !loginBonusLoaded) {
            return;
        }

        const nameText = loginBonusNameInput.value.trim();
        const label = nameText ? `${nameText} (#${id})` : `#${id}`;
        const confirmed = window.confirm(`${label} のログインボーナスを削除しますか？`);
        if (!confirmed) {
            return;
        }

        if (loginBonusStatus) {
            loginBonusStatus.textContent = "削除中...";
        }

        try {
            const response = await fetch(`/admin/login-bonus?id=${encodeURIComponent(id)}`, { method: "DELETE" });

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
            resetLoginBonusEditor();
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

loginBonusType.addEventListener("change", () => {
    setLoginBonusMode(loginBonusType.value);
    if (loginBonusType.value === "monthly") {
        syncDateRangeFromMonth();
    } else {
        updateEventDayIndices();
    }
});

startDateInput.addEventListener("change", () => {
    if (loginBonusType.value === "event") {
        updateEventDayIndices();
    }
});

endDateInput.addEventListener("change", () => {
    if (loginBonusType.value === "event") {
        updateEventDayIndices();
    }
});

if (eventAdd) {
    eventAdd.addEventListener("click", () => {
        appendEventDay();
    });
}

if (loginBonusSelect) {
    loginBonusSelect.addEventListener("change", () => {
        const id = getLoginBonusId();
        setLoginBonusLoaded(false);
        if (!id) {
            loginBonusNameInput.value = "";
            result.textContent = "未登録";
            resetLoginBonusEditor();
            return;
        }
        loadLoginBonusById();
    });
}

setLoginBonusLoaded(false);
setLoginBonusMode(loginBonusType.value);
setView("home");
