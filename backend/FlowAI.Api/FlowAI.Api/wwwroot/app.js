const apiKeyInput = document.getElementById("apiKeyInput");
const messageBox = document.getElementById("messageBox");
const dashboardStatus = document.getElementById("dashboardStatus");
const dashboardMetrics = document.getElementById("dashboardMetrics");
const agentQuestionInput = document.getElementById("agentQuestionInput");
const agentResult = document.getElementById("agentResult");
const lookupStatus = document.getElementById("lookupStatus");
const lookupResult = document.getElementById("lookupResult");

document.getElementById("demoResetButton").addEventListener("click", resetDemo);
document.getElementById("refreshDashboardButton").addEventListener("click", loadDashboard);
document.getElementById("agentQueryButton").addEventListener("click", queryAgent);
document.getElementById("pendingContractsButton").addEventListener("click", () => loadLookup("/api/agent/contracts/pending-approval", "승인 대기 계약"));
document.getElementById("todayWorkOrdersButton").addEventListener("click", () => loadLookup("/api/agent/workorders/today", "오늘 진행 중 작업"));
document.getElementById("onHoldSettlementsButton").addEventListener("click", () => loadLookup("/api/agent/settlements/on-hold", "보류 정산"));
document.querySelectorAll("[data-question]").forEach((button) => {
    button.addEventListener("click", () => {
        agentQuestionInput.value = button.dataset.question;
        queryAgent();
    });
});

async function apiFetch(path, options = {}) {
    const headers = new Headers(options.headers || {});
    headers.set("X-FlowAI-Api-Key", apiKeyInput.value.trim());

    if (options.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
    }

    const response = await fetch(path, {
        ...options,
        headers
    });

    const text = await response.text();
    const data = text ? parseJson(text) : null;

    if (!response.ok) {
        const detail = typeof data === "string" ? data : JSON.stringify(data ?? text);
        throw new Error(`${response.status} ${response.statusText}${detail ? ` - ${detail}` : ""}`);
    }

    return data;
}

function parseJson(text) {
    try {
        return JSON.parse(text);
    } catch {
        return text;
    }
}

async function resetDemo() {
    setMessage("샘플 데이터를 초기화하는 중입니다.", "info");

    try {
        await apiFetch("/api/demo/reset", { method: "POST" });
        setMessage("샘플 데이터가 생성되었습니다.", "success");
        await Promise.all([loadDashboard(false), queryAgent(false)]);
    } catch (error) {
        showError(error);
    }
}

async function loadDashboard(showLoading = true) {
    if (showLoading) {
        setMessage("업무 현황을 불러오는 중입니다.", "info");
    }

    dashboardStatus.textContent = "조회 중";

    try {
        const summary = await apiFetch("/api/dashboard/summary");
        dashboardStatus.textContent = "최신";
        dashboardMetrics.innerHTML = "";
        dashboardMetrics.append(
            metric("Total Contracts", summary.totalContracts),
            metric("Approved Contracts", summary.approvedContracts),
            metric("Total Work Orders", summary.totalWorkOrders),
            metric("Work In Progress", summary.workInProgress),
            metric("Work Completed", summary.workCompleted),
            metric("Total Settlements", summary.totalSettlements),
            metric("Settlement Requested", summary.settlementRequested),
            metric("Settlement Reviewing", summary.settlementReviewing),
            metric("Settlement Approved", summary.settlementApproved),
            metric("Settlement Held", summary.settlementHeld),
            metric("Settlement Rejected", summary.settlementRejected)
        );

        if (showLoading) {
            setMessage("업무 현황 조회가 완료되었습니다.", "success");
        }
    } catch (error) {
        dashboardStatus.textContent = "오류";
        showError(error);
    }
}

async function queryAgent(showLoading = true) {
    if (showLoading) {
        setMessage("Agent에게 질문하는 중입니다.", "info");
    }

    try {
        const response = await apiFetch("/api/agent/query", {
            method: "POST",
            body: JSON.stringify({ question: agentQuestionInput.value })
        });

        agentResult.innerHTML = "";
        agentResult.append(
            agentModeBadge(response.source),
            paragraph(response.answer || "응답 내용이 없습니다.", "answer"),
            metaRow("source", response.source),
            metaRow("generatedAt", response.generatedAt),
            factsTable(response.facts)
        );

        if (showLoading) {
            setMessage("Agent 응답이 도착했습니다.", "success");
        }
    } catch (error) {
        showError(error);
    }
}

async function loadLookup(path, label) {
    setMessage(`${label} 데이터를 조회하는 중입니다.`, "info");
    lookupStatus.textContent = "조회 중";

    try {
        const data = await apiFetch(path);
        lookupStatus.textContent = `${Array.isArray(data) ? data.length : 1}건`;
        lookupResult.innerHTML = "";
        const pre = document.createElement("pre");
        pre.textContent = JSON.stringify(data, null, 2);
        lookupResult.append(pre);
        setMessage(`${label} 조회가 완료되었습니다.`, "success");
    } catch (error) {
        lookupStatus.textContent = "오류";
        showError(error);
    }
}

function metric(label, value) {
    const item = document.createElement("div");
    item.className = "metric";

    const valueElement = document.createElement("strong");
    valueElement.textContent = value ?? 0;

    const labelElement = document.createElement("span");
    labelElement.textContent = label;

    item.append(valueElement, labelElement);
    return item;
}

function paragraph(text, className) {
    const element = document.createElement("p");
    element.className = className;
    element.textContent = text;
    return element;
}

function agentModeBadge(source) {
    const badge = document.createElement("div");
    const isOllama = source === "ollama";
    const isSystem = source === "system" || source === "flowai-agent";
    badge.className = `agent-mode ${isOllama ? "ollama" : isSystem ? "system" : "rule-based"}`;
    badge.textContent = isOllama
        ? "AI Mode: Ollama Local LLM"
        : isSystem
            ? "FlowAI Agent Guardrail"
            : "Mode: RuleBased Fallback";
    return badge;
}

function metaRow(label, value) {
    const row = document.createElement("div");
    row.className = "meta-row";
    row.innerHTML = `<span>${label}</span><strong>${value ?? "-"}</strong>`;
    return row;
}

function factsTable(facts) {
    const wrapper = document.createElement("div");

    if (!facts) {
        wrapper.innerHTML = `<p class="empty">facts 데이터가 없습니다.</p>`;
        return wrapper;
    }

    const labels = {
        pendingApprovalContracts: "승인 대기 계약",
        todayWorkOrders: "오늘 진행 중 작업",
        delayedWorkOrders: "지연 작업",
        onHoldSettlements: "정산 보류",
        totalSettlements: "전체 정산",
        requestedSettlements: "정산 요청",
        reviewingSettlements: "검토 중",
        approvedSettlements: "정산 승인 완료",
        rejectedSettlements: "정산 반려"
    };

    const table = document.createElement("table");
    table.innerHTML = "<thead><tr><th>항목</th><th>건수</th></tr></thead>";
    const body = document.createElement("tbody");

    for (const [key, label] of Object.entries(labels)) {
        const row = document.createElement("tr");
        const name = document.createElement("td");
        const count = document.createElement("td");
        name.textContent = label;
        count.textContent = facts[key] ?? 0;
        row.append(name, count);
        body.append(row);
    }

    table.append(body);
    wrapper.append(table);
    return wrapper;
}

function setMessage(message, type) {
    messageBox.hidden = false;
    messageBox.className = `message ${type}`;
    messageBox.textContent = message;
}

function showError(error) {
    setMessage(error.message || "알 수 없는 오류가 발생했습니다.", "error");
}
