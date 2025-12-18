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

  const getRowValue = (row, fieldName) => {
    if (!row || fieldName == null) return "";
    if (row[fieldName] !== undefined) return row[fieldName];
    const want = String(fieldName).toLowerCase();
    const hit = Object.keys(row).find(k => String(k).toLowerCase() === want);
    return hit ? row[hit] : "";
  };

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
  const keyFields = Array.isArray(tbody._keyFields) ? tbody._keyFields : [];
  const pkNames = [
    ...new Set([
      ...dict.filter(f => DICT.isKey(f)).map(f => f.FieldName),
      ...keyFields
    ])
  ];

  rows.forEach(row => {
    const tr = document.createElement("tr");
    tr.style.cursor = "pointer";

    // ★ 插入 PK hidden（包含 cfg/推斷出來的 KeyFields，即使該欄位不顯示也要帶給 SaveTableChanges）
    pkNames.forEach(name => {
      const pk = document.createElement("input");
      pk.type = "hidden";
      pk.name = name;
      const v = getRowValue(row, name);
      pk.value = (v ?? "").toString();
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
      if (DICT.readonly(f) || f.KeySelfName || (DICT.isKey(f) && tr.dataset.state !== "added")) {
        inp.readOnly = true;
        td.classList.add("mmd-readonly-cell");
      }

      td.append(span);
      td.append(inp);
      tr.appendChild(td);
    });

    if (onRowClick) {
      tr.addEventListener("click", () => onRowClick(row, tr));
    }
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

    const uniq = (arr) => [...new Set((arr || []).filter(Boolean).map(s => String(s)))];

    // === Master 資料 ===
    const masterUrl = cfg.MasterApi
      ? cfg.MasterApi
      : `/api/CommonTable/TopRows?table=${encodeURIComponent(
          cfg.MasterTable
        )}&top=${cfg.MasterTop || 200}`;

    const masterRows = await fetch(masterUrl).then(r => r.json());

    // Master key fields: prefer cfg.MasterPkFields, fallback to dict IsKey
    const masterKeyFields = uniq([
      ...(masterDict.filter(f => DICT.isKey(f)).map(f => f.FieldName)),
      ...((cfg.MasterPkFields || []))
    ]);
    // Let buildBody inject hidden keys even if not visible
    mBody._keyFields = masterKeyFields;

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

    const activeTarget = { type: "master", index: -1 };
    let lastMasterRow = null;

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
    const ctx = {};

    // ⭐ 正確作法：查詢明細只用 Master → Detail 的 KeyMap.Master 來查！
    (d.KeyMap || []).forEach(k => {
      names.push(k.Detail);               // 用明細的欄位當查詢欄位
      values.push(row[k.Master]);         // 值取 Master 的欄位值
      ctx[k.Detail] = row[k.Master];
    });
    tbody._lastQueryCtx = ctx;

    const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(d.DetailTable)}`
      + names.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
      + values.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("");

    const rows = await fetch(url).then(r => r.json());

    // ★ 為 Detail 表格添加 row click 事件處理，實現 Focus 功能
    await buildBody(tbody, detailDicts[i], rows, (row, tr) => {
      // 移除所有行的 selected class
      tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
      // 添加 selected class 到被點擊的行
      tr.classList.add("selected");
      activeTarget.type = "detail";
      activeTarget.index = i;

      // ★★★ Detail Focus 聯動功能 ★★★
      if (cfg.EnableDetailFocusCascade) {
        loadNextDetailFromFocus(i, row);
      }
    });

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
    //   Detail Focus 聯動：點擊某層 Detail 時，載入下一層 Detail 的關聯資料
    // ==============================================================================
    const loadNextDetailFromFocus = async (currentDetailIndex, focusedRow) => {
      const nextIndex = currentDetailIndex + 1;

      // 檢查是否有下一層 Detail
      if (nextIndex >= (cfg.Details || []).length) {
        return; // 已經是最後一層，不需要聯動
      }

      const nextDetail = cfg.Details[nextIndex];
      const tbody = document.getElementById(`${cfg.DomId}-detail-${nextIndex}-body`);
      if (!tbody) return;

      const names = [];
      const values = [];
      const ctx = {};

      // 根據下一層 Detail 的 KeyMap 從當前 focusedRow 中提取對應的欄位值
      (nextDetail.KeyMap || []).forEach(k => {
        names.push(k.Detail);           // 用明細的欄位當查詢欄位
        values.push(focusedRow[k.Master]); // 值取 focusedRow 的欄位值
        ctx[k.Detail] = focusedRow[k.Master];
      });
      tbody._lastQueryCtx = ctx;

      const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(nextDetail.DetailTable)}`
        + names.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
        + values.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("");

      const rows = await fetch(url).then(r => r.json());

      // 載入下一層 Detail 的資料，同時保留 Focus 聯動功能
      await buildBody(tbody, detailDicts[nextIndex], rows, (row, tr) => {
        tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
        tr.classList.add("selected");
        activeTarget.type = "detail";
        activeTarget.index = nextIndex;

        // 遞迴：如果還有下一層，繼續聯動
        if (cfg.EnableDetailFocusCascade) {
          loadNextDetailFromFocus(nextIndex, row);
        }
      });

      // 編輯模式處理
      if (window._mmdEditing && tbody._editorInstance) {
        if (tbody.offsetParent !== null) {
          tbody._editorInstance.toggleEdit(true);
          tbody._pendingRebind = false;
        } else {
          tbody._pendingRebind = true;
        }
      }
    };


    // ==============================================================================
    //   畫 Master 表格，掛上 row click event
    // ==============================================================================
    await buildBody(mBody, masterDict, masterRows, async (row, tr) => {
      Array.from(mBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");
      activeTarget.type = "master";
      activeTarget.index = -1;
      lastMasterRow = row;
      await loadAllDetails(row);
    });

    // ==============================================================================
    //   EditableGrid 初始化（含自動偵測 PK）
    // ==============================================================================
    const btnEdit = document.getElementById(`${cfg.DomId}-btnEdit`);
    const btnAdd = document.getElementById(`${cfg.DomId}-btnAdd`);
    const btnSave = document.getElementById(`${cfg.DomId}-btnSave`);
    const btnDelete = document.getElementById(`${cfg.DomId}-btnDelete`);
    const btnCancel = document.getElementById(`${cfg.DomId}-btnCancel`);

    // === 1. Master PK ===
    const masterPK = masterKeyFields;

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
const detailKeyFields = [];

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

  detailEditors[i] = window.makeEditableGrid({
    wrapper: root.querySelector(`#${cfg.DomId}-detail-${i}-wrapper`),
    table: root.querySelector(`#${cfg.DomId}-detail-${i}-grid`),
    tableName: d.DetailTable,
    keyFields
  });
  detailKeyFields[i] = keyFields;

  // 讓 buildBody 知道該塞哪些 FK hidden
  const tbody = root.querySelector(`#${cfg.DomId}-detail-${i}-body`);
  tbody._editorInstance = detailEditors[i];
  tbody._keyMap = d.KeyMap;
  tbody._keyFields = keyFields;
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

            const m = (targetId || "").match(/tabpane-(\\d+)$/);
            if (m) {
                activeTarget.type = "detail";
                activeTarget.index = Number(m[1]);
            }
        });
    });

    const setEditMode = (toEdit) => {
      const on = !!toEdit;
      window._mmdEditing = on;
      masterEditor.toggleEdit(on);
      detailEditors.forEach(ed => ed.toggleEdit(on));
      if (btnEdit) btnEdit.textContent = on ? "檢視" : "編輯";
      root.classList.toggle("mmd-editing", on);
    };

    const ensureEditMode = () => {
      if (!masterEditor.isEdit()) setEditMode(true);
    };

    const getActive = () => {
      if (activeTarget.type === "detail") {
        const idx = Number(activeTarget.index);
        const tbody = document.getElementById(`${cfg.DomId}-detail-${idx}-body`);
        return { type: "detail", index: idx, tbody, editor: detailEditors[idx], keyFields: detailKeyFields[idx], dict: detailDicts[idx] };
      }
      return { type: "master", index: -1, tbody: mBody, editor: masterEditor, keyFields: masterPK, dict: masterDict };
    };

    const clearSelected = (tbody) => {
      tbody?.querySelectorAll?.('tr')?.forEach?.(x => x.classList.remove("selected"));
    };

    const addRowTo = (target) => {
      const tbody = target.tbody;
      const dict = target.dict || [];
      const keyMap = tbody?._keyMap || [];
      const keyFields = Array.isArray(tbody?._keyFields) ? tbody._keyFields : (target.keyFields || []);
      if (!tbody) return null;

      const defaults = {};
      if (target.type === "detail") {
        const ctx = tbody._lastQueryCtx;
        if (!ctx || Object.keys(ctx).length === 0) {
          Swal?.fire({ icon: "info", title: "請先選取上一層資料", timer: 900, showConfirmButton: false });
          return null;
        }
        Object.assign(defaults, ctx);
      }

      // 系統慣例：UseId 缺值時預設帶 A001（或由全域覆蓋）
      const hasUseIdKey = (keyFields || []).some(k => String(k).toLowerCase() === "useid");
      if (hasUseIdKey && !defaults.UseId) {
        defaults.UseId = window.DEFAULT_USEID || window._useId || "A001";
      }

      // 系統慣例：Item 若是鍵欄位且未帶值，預設使用目前明細最大 Item + 1（依當前查詢條件群組）
      const hasItemKey = (keyFields || []).some(k => String(k).toLowerCase() === "item");
      if (hasItemKey && (defaults.Item == null || String(defaults.Item).trim() === "")) {
        const getElValue = (tr, name) => {
          if (!tr || !name) return "";
          const want = String(name).toLowerCase();
          const inp = Array.from(tr.querySelectorAll("input")).find(x => (x.name || "").toLowerCase() === want);
          if (inp) return inp.value ?? "";
          const edit = Array.from(tr.querySelectorAll("input.cell-edit")).find(x => (x.name || "").toLowerCase() === want);
          return edit?.value ?? "";
        };

        const ctxKeys = Object.keys(tbody._lastQueryCtx || {}).filter(k => String(k).toLowerCase() !== "item");
        const matchesCtx = (tr) => {
          for (const k of ctxKeys) {
            const want = (defaults[k] ?? "").toString();
            const got = (getElValue(tr, k) ?? "").toString();
            if (want !== got) return false;
          }
          return true;
        };

        let maxItem = 0;
        tbody.querySelectorAll("tr").forEach(tr => {
          if (tr.dataset?.state === "deleted") return;
          if (!matchesCtx(tr)) return;
          const raw = getElValue(tr, "Item");
          const n = parseInt(String(raw ?? "").trim(), 10);
          if (!isNaN(n) && n > maxItem) maxItem = n;
        });

        defaults.Item = String(maxItem + 1);
      }

      const pkNames = [
        ...new Set([
          ...dict.filter(f => DICT.isKey(f)).map(f => f.FieldName),
          ...keyFields
        ])
      ];

      const fields = dict
        .filter(f => DICT.visible(f) || DICT.isKey(f))
        .sort((a, b) => DICT.order(a) - DICT.order(b));

      const tr = document.createElement("tr");
      tr.dataset.state = "added";
      tr.classList.add("table-warning");
      tr.style.cursor = "pointer";

      pkNames.forEach(name => {
        const pk = document.createElement("input");
        pk.type = "hidden";
        pk.name = name;
        pk.value = (defaults[name] ?? "").toString();
        pk.className = "mmd-pk-hidden";
        tr.append(pk);
      });

      keyMap.forEach(k => {
        const fk = document.createElement("input");
        fk.type = "hidden";
        fk.name = k.Detail;
        fk.value = (defaults[k.Detail] ?? "").toString();
        fk.className = "mmd-fk-hidden";
        tr.append(fk);
      });

      fields.forEach(f => {
        const td = document.createElement("td");
        const raw = defaults[f.FieldName] ?? "";

        const span = document.createElement("span");
        span.className = "cell-view d-none";
        span.textContent = "";

        const inp = document.createElement("input");
        inp.className = "form-control form-control-sm cell-edit";
        inp.value = (raw ?? "").toString();
        inp.dataset.raw = (raw ?? "").toString();
        inp.name = f.FieldName;

        if (DICT.readonly(f) || f.KeySelfName || (DICT.isKey(f) && tr.dataset.state !== "added")) {
          inp.readOnly = true;
          td.classList.add("mmd-readonly-cell");
        }

        td.append(span);
        td.append(inp);
        tr.appendChild(td);
      });

      tr.addEventListener("click", () => {
        clearSelected(tbody);
        tr.classList.add("selected");
        activeTarget.type = target.type;
        activeTarget.index = target.index;
      });

      clearSelected(tbody);
      tr.classList.add("selected");
      if (tbody.firstChild) tbody.insertBefore(tr, tbody.firstChild);
      else tbody.appendChild(tr);

      try { tr.scrollIntoView({ block: "nearest" }); } catch { }
      const firstEditable = Array.from(tr.querySelectorAll("input.cell-edit"))
        .find(i => !i.readOnly && i.getAttribute("readonly") !== "readonly");
      if (firstEditable) {
        try { firstEditable.focus(); } catch { }
        try { firstEditable.select?.(); } catch { }
      }
      return tr;
    };

    const saveAll = async () => {
      const rMaster = await masterEditor.saveChanges();
      const rDetails = await Promise.all(detailEditors.map(ed => ed.saveChanges()));

      if (rMaster.skipped && rDetails.every(r => r.skipped)) {
        Swal?.fire({ icon: "info", title: "沒有變更", timer: 1000, showConfirmButton: false });
        return { ok: true, skipped: true };
      }

      const ok = rMaster.ok && rDetails.every(r => r.ok);
      if (!ok) {
        Swal?.fire({ icon: "error", title: "儲存失敗", text: "部分明細未成功" });
        return { ok: false, skipped: false };
      }

      Swal?.fire({ icon: "success", title: "儲存完成", timer: 900, showConfirmButton: false });
      setEditMode(false);
      return { ok: true, skipped: false };
    };

    const deleteSelected = async () => {
      const notify = (kind, title, text) => {
        if (window.Swal?.fire) {
          const opts = { icon: kind, title };
          if (text) opts.text = text;
          if (kind === "success" || kind === "info") {
            opts.timer = 900;
            opts.showConfirmButton = false;
          }
          return Swal.fire(opts);
        }
        if (text) alert(`${title}\n${text}`);
        else alert(title);
      };

      const confirmDelete = async () => {
        if (window.Swal?.fire) {
          const r = await Swal.fire({
            icon: "warning",
            title: "確定要刪除？",
            showCancelButton: true,
            confirmButtonText: "刪除",
            cancelButtonText: "取消"
          });
          return !!r.isConfirmed;
        }
        return confirm("確定要刪除這筆資料嗎？");
      };

      const t = getActive();
      const tbody = t.tbody;
      if (!tbody) return;

      const pickSelectedRow = () => {
        const activeTr = tbody.querySelector("tr.selected");
        if (activeTr) return { tr: activeTr, target: t };

        const any = root.querySelector("tbody tr.selected");
        if (!any) return null;

        const anyTbody = any.closest("tbody");
        const anyId = anyTbody?.id || "";
        const m = anyId.match(new RegExp(`^${cfg.DomId}-detail-(\\d+)-body$`));
        if (m) {
          const idx = Number(m[1]);
          const tb = document.getElementById(`${cfg.DomId}-detail-${idx}-body`);
          return { tr: any, target: { type: "detail", index: idx, tbody: tb, editor: detailEditors[idx] } };
        }
        if (anyId === `${cfg.DomId}-m-body`) {
          return { tr: any, target: { type: "master", index: -1, tbody: mBody, editor: masterEditor } };
        }
        return { tr: any, target: t };
      };

      const picked = pickSelectedRow();
      const tr = picked?.tr;
      const target = picked?.target || t;
      if (!tr) {
        await notify("info", "請先選擇要刪除的資料列");
        return;
      }

      if (tr.dataset.state === "added") {
        tr.remove();
        return;
      }

      const confirmed = await confirmDelete();
      if (!confirmed) return;

      ensureEditMode();
      tr.dataset.state = "deleted";
      tr.classList.add("table-danger");

      const resp = await target.editor.saveChanges();
      if (!resp.ok) {
        await notify("error", "刪除失敗", resp.text || "刪除失敗");
        delete tr.dataset.state;
        tr.classList.remove("table-danger");
        return;
      }

      await notify("success", "刪除完成");
      if (target.type === "master") location.reload();
    };

    // 統一按鈕行為：使用通用 toolbar controller（與 MultiGrid 共用）
    if (window.createGridController) {
      window.createGridController({
        name: `mmd-${cfg.DomId}`,
        btnToggle: btnEdit,
        btnAdd,
        btnSave,
        btnDelete,
        btnCancel,
        customToggle: () => setEditMode(!masterEditor.isEdit()),
        customAdd: () => {
          ensureEditMode();
          const row = addRowTo(getActive());
          return row || false;
        },
        customSave: async () => { await saveAll(); },
        onDelete: async () => { await deleteSelected(); },
        onCancelPending: (row) => { row?.remove?.(); },
        onCancelEdit: async () => { location.reload(); }
      });
    } else {
      btnEdit?.addEventListener("click", () => setEditMode(!masterEditor.isEdit()));
      btnSave?.addEventListener("click", async () => { await saveAll(); });
    }
  };
  // ==============================================================================
  // INIT：初始化所有 MMD 配置
  // ==============================================================================
  document.addEventListener("DOMContentLoaded", () => {
    const cfgs = window._mmdConfigs || {};
    Object.values(cfgs).forEach(cfg => initOne(cfg));
  });

})();   // End of IIFE
