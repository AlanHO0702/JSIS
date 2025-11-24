(() => {

  // ==============================================================================
  //  Lookup & OCX Cache
  // ==============================================================================
  const LOOKUP_CACHE = {};
  const OCX_CACHE = {};

  async function loadLookup(f) {
    const key = `${f.LookupTable}|${f.LookupKeyField}|${f.LookupResultField}`;
    if (LOOKUP_CACHE[key]) return LOOKUP_CACHE[key];

    if (!f.LookupTable || !f.LookupKeyField || !f.LookupResultField)
      return LOOKUP_CACHE[key] = null;

    const url = `/api/TableFieldLayout/LookupData`
      + `?table=${encodeURIComponent(f.LookupTable)}`
      + `&key=${encodeURIComponent(f.LookupKeyField)}`
      + `&result=${encodeURIComponent(f.LookupResultField)}`;

    const rows = await fetch(url).then(r => r.json());
    const map = {};
    rows.forEach(r => map[r.key] = r.result0);
    return LOOKUP_CACHE[key] = map;
  }

  async function loadOCXLookup(f) {
    const key = `${f.OCXLKTableName}|${f.KeyFieldName}|${f.OCXLKResultName}`;
    if (OCX_CACHE[key]) return OCX_CACHE[key];

    if (!f.OCXLKTableName || !f.KeyFieldName || !f.OCXLKResultName)
      return OCX_CACHE[key] = null;

    const url = `/api/TableFieldLayout/LookupData`
      + `?table=${encodeURIComponent(f.OCXLKTableName)}`
      + `&key=${encodeURIComponent(f.KeyFieldName)}`
      + `&result=${encodeURIComponent(f.OCXLKResultName)}`;

    const rows = await fetch(url).then(r => r.json());
    const map = {};
    rows.forEach(r => map[r.key] = r.result0);
    return OCX_CACHE[key] = map;
  }

  // ==============================================================================
  //  Dict Helpers
  // ==============================================================================
  const DICT = {
    visible: f => (f.Visible ?? 1) === 1,
    order:   f => f.SerialNum ?? 99999,
    header:  f => f.DisplayLabel || f.FieldName,
    width:   f => {
      const n = Number(f.DisplaySize || f.iFieldWidth || 0);
      return n > 0 ? n * 10 : null;
    },
    fmt:     f => f.FormatStr,
    type:    f => f.DataType,
    readonly:f => (f.ReadOnly ?? 0) === 1,
    isKey:   f => (f.IsKey ?? 0) === 1
  };

  const fmtCell = (val, fmt, dataType) => {
    if (val == null || val === "") return "";

    if (dataType?.toLowerCase().includes("date")) {
      const d = new Date(val);
      if (!isNaN(d)) return d.toISOString().slice(0, 10).replace(/-/g, "/");
    }

    if (typeof val === "number") {
      if (fmt?.includes(".000")) return val.toFixed(3);
      if (fmt?.includes(".00"))  return val.toFixed(2);
      return val.toLocaleString();
    }

    return String(val);
  };

  // ==============================================================================
  //  建表頭
  // ==============================================================================
  const buildHead = (tr, dict) => {
    tr.innerHTML = "";
    dict
      .filter(DICT.visible)
      .sort((a, b) => DICT.order(a) - DICT.order(b))
      .forEach(f => {
        const th = document.createElement("th");
        th.textContent = DICT.header(f);
        const w = DICT.width(f);
        if (w) th.style.width = w + "px";
        th.style.whiteSpace = "nowrap";
        tr.appendChild(th);
      });
  };

  // ==============================================================================
  //  建表身（含 PK hidden 欄位）
  // ==============================================================================
    // ======================================================================
//  建表身（正確版 PK / FK 儲存格式）
// ======================================================================
const buildBody = async (tbody, dict, rows, onRowClick) => {
  tbody.innerHTML = "";

  const fields = dict
    .filter(f => DICT.visible(f) || DICT.isKey(f))
    .sort((a, b) => DICT.order(a) - DICT.order(b));

  const lk = {};
  const oc = {};

  for (const f of fields) {
    lk[f.FieldName] = await loadLookup(f);
    oc[f.FieldName] = await loadOCXLookup(f);
  }

  const keyMap = tbody._keyMap || [];

  rows.forEach(row => {
    const tr = document.createElement("tr");
    tr.style.cursor = "pointer";

    // ★ 插入 PK hidden
    dict.filter(f => DICT.isKey(f)).forEach(f => {
      const pk = document.createElement("input");
      pk.type = "hidden";
      pk.name = f.FieldName;
      pk.value = row[f.FieldName];
      pk.className = "mmd-pk-hidden";
      tr.append(pk);
    });

    // ★ 插入 FK hidden（只有 KeyMap.Detail）
    keyMap.forEach(k => {
      const fk = document.createElement("input");
      fk.type = "hidden";
      fk.name = k.Detail;     // Detail 的欄位名稱
      fk.value = row[k.Detail];
      fk.className = "mmd-fk-hidden";
      tr.append(fk);
    });

    // ===== 表格 UI 欄位產生 =====
    fields.forEach(f => {
      const td = document.createElement("td");

      let raw = row[f.FieldName];
      if (!raw && f.KeySelfName) raw = row[f.KeySelfName];

      let display = raw;
      if (oc[f.FieldName]?.[raw] != null) display = oc[f.FieldName][raw];
      else if (lk[f.FieldName]?.[raw] != null) display = lk[f.FieldName][raw];

      const span = document.createElement("span");
      span.className = "cell-view";
      span.textContent = fmtCell(display, DICT.fmt(f), DICT.type(f));

      const inp = document.createElement("input");
      inp.className = "form-control form-control-sm cell-edit d-none";
      inp.value = display;
      inp.dataset.raw = raw;
      inp.name = f.FieldName;

  // ★★★ 修正：如果該欄位是 PK (IsKey)，或者是唯讀，或者是關聯鍵，都必須鎖定不可編輯 ★★★
      if (DICT.readonly(f) || f.KeySelfName || DICT.isKey(f)) { 
        inp.readOnly = true;
        td.classList.add("mmd-readonly-cell");
      }

      td.append(span);
      td.append(inp);
      tr.appendChild(td);
    });

    if (onRowClick) tr.addEventListener("click", () => onRowClick(row, tr));
    tbody.appendChild(tr);
  });
};



  // ==============================================================================
  //  初始化主從頁（initOne）
  // ==============================================================================
  const initOne = async (cfg) => {

    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const mHead = root.querySelector(`#${cfg.DomId}-m-head`);
    const mBody = root.querySelector(`#${cfg.DomId}-m-body`);

    // === Master 辭典 ===
    const masterDict = await fetch(
      `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(
        cfg.MasterDict || cfg.MasterTable
      )}`
    ).then(r => r.json());

    buildHead(mHead, masterDict);

    // === Master 資料 ===
    const masterUrl = cfg.MasterApi
      ? cfg.MasterApi
      : `/api/CommonTable/TopRows?table=${encodeURIComponent(
          cfg.MasterTable
        )}&top=${cfg.MasterTop || 200}`;

    const masterRows = await fetch(masterUrl).then(r => r.json());

    // === Detail 辭典 ===
    const detailDicts = [];
    for (let i = 0; i < (cfg.Details || []).length; i++) {
      const d = cfg.Details[i];
      const hid = `${cfg.DomId}-detail-${i}-head`;

      const dict = await fetch(
        `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(
          d.DetailDict || d.DetailTable
        )}`
      ).then(r => r.json());

      const headTr = document.getElementById(hid);
      if (headTr) buildHead(headTr, dict);
      detailDicts[i] = dict;
    }

    // ==============================================================================
    //   切換 Master → 重新載入全部明細
    // ==============================================================================
// ======================================================================
//   切換 Master → 重新載入全部明細（正確版）
// ======================================================================
const loadAllDetails = async (row) => {

  for (let i = 0; i < (cfg.Details || []).length; i++) {
    const d = cfg.Details[i];
    const tbody = document.getElementById(`${cfg.DomId}-detail-${i}-body`);
    if (!tbody) continue;

    const names = [];
    const values = [];

    // ⭐ 正確作法：查詢明細只用 Master → Detail 的 KeyMap.Master 來查！
    (d.KeyMap || []).forEach(k => {
      names.push(k.Detail);               // 用明細的欄位當查詢欄位
      values.push(row[k.Master]);         // 值取 Master 的欄位值
    });

    const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(d.DetailTable)}`
      + names.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
      + values.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("");

    console.log("Detail Query:", url);

    const rows = await fetch(url).then(r => r.json());

    await buildBody(tbody, detailDicts[i], rows);

    // if (window._mmdEditing && tbody._editorInstance) {
    //   tbody._editorInstance.rebind();
    // }
    if (window._mmdEditing && tbody._editorInstance) {
      // 檢查 tbody 是否可見 (offsetParent 在 display:none 時為 null)
      if (tbody.offsetParent !== null) {
  // ★★★ 修正點：原本的 rebind() 不存在，改成直接呼叫 toggleEdit(true) ★★★
          // 這會做兩件事：1.重新掃描 DOM 綁定事件 2.移除 d-none 顯示輸入框
          tbody._editorInstance.toggleEdit(true);
          tbody._pendingRebind = false; // 清除標記
      } else {
          // 如果是隱藏的，標記為待處理，等切換頁籤時再 rebind
          tbody._pendingRebind = true;
      }
    }
  }
};


    // ==============================================================================
    //   畫 Master 表格，掛上 row click event
    // ==============================================================================
    await buildBody(mBody, masterDict, masterRows, async (row, tr) => {
      Array.from(mBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");
      await loadAllDetails(row);
    });

    // ==============================================================================
    //   EditableGrid 初始化（含自動偵測 PK）
    // ==============================================================================
    const editBtn = document.getElementById(`${cfg.DomId}-btnEdit`);
    const saveBtn = document.getElementById(`${cfg.DomId}-btnSave`);

    // === 1. Master PK ===
    const masterPK = masterDict.filter(f => DICT.isKey(f)).map(f => f.FieldName);

    const masterEditor = window.makeEditableGrid({
      wrapper: root.querySelector(`#${cfg.DomId}-masterWrapper`),
      table:   root.querySelector(`#${cfg.DomId}-masterGrid`),
      tableName: cfg.MasterTable,
      keyFields: masterPK
    });

    // ======================================================================
//  Detail editors 初始化（正確版）
// ======================================================================
const detailEditors = [];

for (let i = 0; i < (cfg.Details || []).length; i++) {

  const d = cfg.Details[i];
  const dict = detailDicts[i];

// ★ 取 PK（IsKey=1）
let keyFields = dict.filter(f => DICT.isKey(f)).map(f => f.FieldName);

// ★ 加上 KeyMap.Detail
const keyMapKeys = (d.KeyMap || []).map(k => k.Detail);
keyFields = [...new Set([...keyFields, ...keyMapKeys])];

// ★ 如果 Razor 有指定 PkFields → 通通加入（最高優先權）
if (d.PkFields && Array.isArray(d.PkFields)) {
    keyFields = [...new Set([...keyFields, ...d.PkFields])];
}

console.log(`Detail ${i} final keyFields =`, keyFields);

  detailEditors[i] = window.makeEditableGrid({
    wrapper: root.querySelector(`#${cfg.DomId}-detail-${i}-wrapper`),
    table: root.querySelector(`#${cfg.DomId}-detail-${i}-grid`),
    tableName: d.DetailTable,
    keyFields
  });

  // 讓 buildBody 知道該塞哪些 FK hidden
  const tbody = root.querySelector(`#${cfg.DomId}-detail-${i}-body`);
  tbody._editorInstance = detailEditors[i];
  tbody._keyMap = d.KeyMap;
}

// ==============================================================================
    //  ★ 修正：監聽 Tab 切換，處理隱藏頁籤的 Lazy Rebind
    // ==============================================================================
    const tabButtons = root.querySelectorAll('button[data-bs-toggle="tab"]');
    tabButtons.forEach(btn => {
        btn.addEventListener('shown.bs.tab', (e) => {
            // 取得目標 TabPane 的 ID
            const targetId = btn.getAttribute('data-bs-target'); 
            const pane = root.querySelector(targetId);
            const tbody = pane ? pane.querySelector('tbody') : null;

            // 如果該 tbody 之前因為隱藏而跳過 rebind，現在補做
            if (tbody && tbody._pendingRebind && tbody._editorInstance && window._mmdEditing) {

                // ★★★ 新增這行：補做的同時，也要記得切換成編輯顯示狀態 ★★★
                tbody._editorInstance.toggleEdit(true);
                tbody._pendingRebind = false;
            }
        });
    });


    // ==============================================================================
    //  修改 / 保留
    // ==============================================================================
    editBtn?.addEventListener("click", () => {

      const toEdit = !masterEditor.isEdit();
      window._mmdEditing = toEdit;

      masterEditor.toggleEdit(toEdit);
      detailEditors.forEach(ed => ed.toggleEdit(toEdit));

      editBtn.textContent = toEdit ? "保留" : "修改";
      saveBtn?.classList.toggle("d-none", !toEdit);

      root.classList.toggle("mmd-editing", toEdit);
    });

    // ==============================================================================
    //  儲存
    // ==============================================================================
    saveBtn?.addEventListener("click", async () => {

      const rMaster  = await masterEditor.saveChanges();
      const rDetails = await Promise.all(detailEditors.map(ed => ed.saveChanges()));

      if (rMaster.skipped && rDetails.every(r => r.skipped)) {
        Swal?.fire({
          icon: "info",
          title: "沒有變更",
          timer: 1000,
          showConfirmButton: false
        });
        return;
      }

      const ok = rMaster.ok && rDetails.every(r => r.ok);
      if (!ok) {
        Swal?.fire({
          icon: "error",
          title: "儲存失敗",
          text: "部分明細未成功"
        });
        return;
      }

      Swal?.fire({
        icon: "success",
        title: "儲存完成",
        timer: 900,
        showConfirmButton: false
      });

      masterEditor.toggleEdit(false);
      detailEditors.forEach(ed => ed.toggleEdit(false));

      editBtn.textContent = "修改";
      saveBtn.classList.add("d-none");
      root.classList.remove("mmd-editing");
    });
  };
  // ==============================================================================
  // INIT：初始化所有 MMD 配置
  // ==============================================================================
  document.addEventListener("DOMContentLoaded", () => {
    const cfgs = window._mmdConfigs || {};
    Object.values(cfgs).forEach(cfg => initOne(cfg));
  });

})();   // End of IIFE
