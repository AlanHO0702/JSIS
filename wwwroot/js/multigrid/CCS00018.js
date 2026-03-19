(function () {
  const ITEM_ID = "CCS00018";
  const itemId = (window._multiGridItemId || "").toString().toUpperCase();
  if (itemId !== ITEM_ID) return;

  const KEYWORD_RE = /付款日|試算|pay\s*date|payway/i;
  const modalState = {
    payWayCode: 0,
    customers: [],
    allCustomers: [],
    bound: false
  };

  function getUseId() {
    return (
      window._useId ||
      window.DEFAULT_USEID ||
      localStorage.getItem("useId") ||
      localStorage.getItem("erpUseId") ||
      "A001"
    );
  }

  function withJwtHeaders(init) {
    const base = init || {};
    const headers = Object.assign({}, base.headers || {});
    const jwt = localStorage.getItem("jwtId");
    if (jwt) headers["X-JWTID"] = jwt;
    return Object.assign({}, base, { headers });
  }

  function pickPayWayCode(row) {
    if (!row || typeof row !== "object") return 0;
    const keys = Object.keys(row);
    const exact = ["PayWayCode", "payWayCode", "PAYWAYCODE", "付款序號", "付款方式序號"];
    for (const k of exact) {
      if (Object.prototype.hasOwnProperty.call(row, k)) {
        const n = parseInt(String(row[k] ?? "").trim(), 10);
        if (Number.isFinite(n) && n > 0) return n;
      }
    }
    const fuzzyKey = keys.find((k) => /pay.*way.*code|pay.*code|付款.*序號/i.test(k));
    if (fuzzyKey) {
      const n = parseInt(String(row[fuzzyKey] ?? "").trim(), 10);
      if (Number.isFinite(n) && n > 0) return n;
    }
    return 0;
  }

  function today() {
    const d = new Date();
    const mm = String(d.getMonth() + 1).padStart(2, "0");
    const dd = String(d.getDate()).padStart(2, "0");
    return `${d.getFullYear()}-${mm}-${dd}`;
  }

  function ensureModalDom() {
    let modal = document.getElementById("ccs00018PayWayModal");
    if (modal) return modal;

    modal = document.createElement("div");
    modal.id = "ccs00018PayWayModal";
    modal.className = "modal fade";
    modal.tabIndex = -1;
    modal.setAttribute("aria-hidden", "true");
    modal.innerHTML = `
      <div class="modal-dialog" style="--bs-modal-width:560px">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">付款日試算</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div style="display:grid;grid-template-columns:100px minmax(0,1fr);gap:8px 10px;align-items:center">
              <label class="form-label mb-1">進/銷項</label>
              <select id="pwcIsIn" class="form-select form-select-sm">
                <option value="1" selected>銷項</option>
                <option value="0">進項</option>
              </select>
              <label class="form-label mb-1">發票日期</label>
              <input id="pwcPaperDate" type="date" class="form-control form-control-sm" />
              <label class="form-label mb-1">客戶/廠商</label>
              <select id="pwcCompanyId" class="form-select form-select-sm"></select>
              <label class="form-label mb-1">結帳日</label>
              <input id="pwcPrDay" type="text" class="form-control form-control-sm" readonly />
              <label class="form-label mb-1">付款日期</label>
              <input id="pwcExpectDate" type="text" class="form-control form-control-sm" readonly />
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" id="pwcCalcBtn" class="btn btn-primary btn-sm">試算</button>
            <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">關閉</button>
          </div>
        </div>
      </div>
    `;
    document.body.appendChild(modal);
    return modal;
  }

  function fillCompanyOptions() {
    const modal = ensureModalDom();
    const isInEl = modal.querySelector("#pwcIsIn");
    const companyEl = modal.querySelector("#pwcCompanyId");
    if (!isInEl || !companyEl) return;

    const list = isInEl.value === "0" ? modalState.allCustomers : modalState.customers;
    companyEl.innerHTML = "";
    const empty = document.createElement("option");
    empty.value = "";
    empty.textContent = "";
    companyEl.appendChild(empty);
    list.forEach((x) => {
      const opt = document.createElement("option");
      opt.value = x.companyId || "";
      opt.textContent = `${x.companyId || ""} ${x.shortName || ""}`.trim();
      companyEl.appendChild(opt);
    });
  }

  async function calcPRDay() {
    const modal = ensureModalDom();
    const isInEl = modal.querySelector("#pwcIsIn");
    const companyEl = modal.querySelector("#pwcCompanyId");
    const paperDateEl = modal.querySelector("#pwcPaperDate");
    const prDayEl = modal.querySelector("#pwcPrDay");
    if (!isInEl || !companyEl || !paperDateEl || !prDayEl) return;
    if (!companyEl.value || !paperDateEl.value) {
      prDayEl.value = "";
      return;
    }
    const qs = new URLSearchParams({
      paperDate: paperDateEl.value,
      useId: getUseId(),
      companyId: companyEl.value,
      isIn: isInEl.value || "1"
    });
    const resp = await fetch(`/api/ClassPayWayCalc/PRDay?${qs.toString()}`, withJwtHeaders());
    const data = await resp.json().catch(() => null);
    prDayEl.value = data && data.ok ? (data.prDay || "") : "";
  }

  async function calcExpectDate() {
    const modal = ensureModalDom();
    const isInEl = modal.querySelector("#pwcIsIn");
    const companyEl = modal.querySelector("#pwcCompanyId");
    const paperDateEl = modal.querySelector("#pwcPaperDate");
    const expectDateEl = modal.querySelector("#pwcExpectDate");
    if (!isInEl || !companyEl || !paperDateEl || !expectDateEl) return;
    if (!companyEl.value || !paperDateEl.value || !modalState.payWayCode) {
      expectDateEl.value = "";
      return;
    }
    const qs = new URLSearchParams({
      paperDate: paperDateEl.value,
      useId: getUseId(),
      companyId: companyEl.value,
      payWayCode: String(modalState.payWayCode),
      isIn: isInEl.value || "1"
    });
    const resp = await fetch(`/api/ClassPayWayCalc/ExpectDate?${qs.toString()}`, withJwtHeaders());
    const data = await resp.json().catch(() => null);
    if (data && data.ok) {
      expectDateEl.value = data.expectDate || "";
      return;
    }
    alert((data && data.error) || "試算失敗");
  }

  function bindModalEvents() {
    if (modalState.bound) return;
    const modal = ensureModalDom();
    const isInEl = modal.querySelector("#pwcIsIn");
    const companyEl = modal.querySelector("#pwcCompanyId");
    const paperDateEl = modal.querySelector("#pwcPaperDate");
    const calcBtn = modal.querySelector("#pwcCalcBtn");
    if (isInEl) {
      isInEl.addEventListener("change", async function () {
        fillCompanyOptions();
        await calcPRDay();
      });
    }
    if (companyEl) companyEl.addEventListener("change", calcPRDay);
    if (paperDateEl) paperDateEl.addEventListener("change", calcPRDay);
    if (calcBtn) calcBtn.addEventListener("click", calcExpectDate);
    modalState.bound = true;
  }

  async function openPayWayCalc(ctx) {
    const modal = ensureModalDom();
    bindModalEvents();

    const row = (ctx && ctx.selectedRow) || {};
    modalState.payWayCode = pickPayWayCode(row);
    if (!modalState.payWayCode) {
      alert("目前資料列找不到付款序號，無法試算");
      return;
    }

    const resp = await fetch(
      `/api/ClassPayWayCalc/Init?payWayCode=${encodeURIComponent(String(modalState.payWayCode))}`,
      withJwtHeaders()
    );
    const data = await resp.json().catch(() => null);
    if (!resp.ok || !data || data.ok === false) {
      alert((data && data.error) || "初始化失敗");
      return;
    }

    modalState.customers = Array.isArray(data.customers) ? data.customers : [];
    modalState.allCustomers = Array.isArray(data.allCustomers) ? data.allCustomers : [];

    const titleEl = modal.querySelector(".modal-title");
    if (titleEl) {
      const suffix = data.payWayName ? ` - ${data.payWayName}` : "";
      titleEl.textContent = `付款日試算${suffix}`;
    }
    const paperDateEl = modal.querySelector("#pwcPaperDate");
    const prDayEl = modal.querySelector("#pwcPrDay");
    const expectDateEl = modal.querySelector("#pwcExpectDate");
    if (paperDateEl && !paperDateEl.value) paperDateEl.value = today();
    if (prDayEl) prDayEl.value = "";
    if (expectDateEl) expectDateEl.value = "";
    fillCompanyOptions();

    const bs = window.bootstrap && window.bootstrap.Modal;
    if (!bs) {
      alert("Bootstrap modal 無法使用");
      return;
    }
    bs.getOrCreateInstance(modal).show();
  }

  function wireHandlers() {
    const bar = document.getElementById("mgCustomButtonBar");
    if (!bar) return;

    const toolbarMainRow = document.querySelector(".multigrid-toolbar .toolbar-main .d-flex");
    if (toolbarMainRow && bar.parentElement !== toolbarMainRow) {
      bar.classList.remove("justify-content-end");
      bar.classList.add("justify-content-start", "align-items-center");
      toolbarMainRow.appendChild(bar);
    }

    const map = (window.MultiGridCustomHandlers = window.MultiGridCustomHandlers || {});
    const itemMap = (map[ITEM_ID] = map[ITEM_ID] || {});
    bar.querySelectorAll("[data-custom-btn]").forEach((btn) => {
      const name = (btn.dataset.buttonName || "").toString().trim();
      const text = (btn.textContent || "").toString().trim();
      const caption = (btn.dataset.dialogCaption || "").toString().trim();
      if (!name) return;
      if (KEYWORD_RE.test(name) || KEYWORD_RE.test(text) || KEYWORD_RE.test(caption)) {
        itemMap[name] = openPayWayCalc;
      }
      btn.classList.remove("fab");
      btn.classList.add("btn", "toolbar-btn");
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", wireHandlers);
  } else {
    wireHandlers();
  }
})();
