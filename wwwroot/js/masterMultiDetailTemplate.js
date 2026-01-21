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
  //  Data Fetching Helpers (通用資料抓取函數)
  // ==============================================================================

  /**
   * 抓取資料表字典（Table Field Layout）
   * @param {string} tableName - 資料表名稱
   * @returns {Promise<Array>} 字典資料
   */
  const fetchDict = async (tableName) => {
    if (!tableName) return [];
    const url = `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(tableName)}`;
    return await fetch(url).then(r => r.json());
  };

  /**
   * 抓取資料表的前 N 筆資料
   * @param {string} tableName - 資料表名稱
   * @param {number} top - 抓取筆數，預設 200
   * @returns {Promise<Array>} 資料列陣列
   */
  const fetchTopRows = async (tableName, top = 200) => {
    if (!tableName) return [];
    const url = `/api/CommonTable/TopRows?table=${encodeURIComponent(tableName)}&top=${top}`;
    return await fetch(url).then(r => r.json());
  };

  /**
   * 根據多組 Key 抓取資料（用於 Master → Detail 關聯查詢）
   * @param {string} tableName - 資料表名稱
   * @param {Array<string>} keyNames - Key 欄位名稱陣列
   * @param {Array<any>} keyValues - Key 值陣列
   * @returns {Promise<Array>} 資料列陣列
   */
  const fetchByKeys = async (tableName, keyNames, keyValues) => {
    if (!tableName || !keyNames || !keyValues) return [];
    const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(tableName)}`
      + keyNames.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
      + keyValues.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("");
    return await fetch(url).then(r => r.json());
  };

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

  const WIDTH_SAVE_API = "/api/DictSetupApi/FieldWidth/Save";
  const normalizeTableName = (name) => (name || "").replace(/^dbo\./i, "").trim().toLowerCase();

  const persistWidthField = async (table, field, width) => {
    if (!table || !field || !Number.isFinite(width)) return;
    try {
      await fetch(WIDTH_SAVE_API, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          tableName: normalizeTableName(table),
          fieldName: field,
          widthPx: Math.round(width)
        })
      });
    } catch { /* ignore */ }
  };

  const enableColumnResize = (tableEl, tableName) => {
    if (!tableEl || !tableName) return;
    const ths = Array.from(tableEl.querySelectorAll("thead th"));
    if (!ths.length) return;

    ths.forEach(th => {
      if (!th.querySelector(".col-resizer")) {
        const handle = document.createElement("span");
        handle.className = "col-resizer";
        th.appendChild(handle);
      }
    });

    let isDown = false, startX = 0, startW = 0, th = null, activeField = "";

    ths.forEach(h => {
      const handle = h.querySelector(".col-resizer");
      if (!handle) return;
      handle.addEventListener("mousedown", (e) => {
        e.preventDefault();
        isDown = true;
        th = h;
        startX = e.pageX;
        startW = th.getBoundingClientRect().width;
        activeField = th.dataset.field || "";
        document.body.style.cursor = "col-resize";
      });
    });

    document.addEventListener("mousemove", (e) => {
      if (!isDown || !th) return;
      const dx = e.pageX - startX;
      const newW = Math.max(48, startW + dx);
      th.style.width = `${newW}px`;
    });

    document.addEventListener("mouseup", () => {
      if (!isDown) return;
      isDown = false;
      document.body.style.cursor = "";
      if (th && activeField) {
        const w = th.getBoundingClientRect().width;
        persistWidthField(tableName, activeField, w);
      }
      th = null;
      activeField = "";
    });
  };

  // ==============================================================================
  //  UI Helpers - 將佈局模式轉換為字串
  // ==============================================================================
  function getLayoutModeString(layout) {
    const modes = {
      0: 'Tabs',
      1: 'ThreeColumn',
      2: 'VerticalStack',
      3: 'BalanceSheet'
    };
    return modes[layout] || 'Tabs';
  }

  // ==============================================================================
  //  UI Helpers - 初始化表格計數（通用版，支援所有佈局）
  // ==============================================================================
  function initGridCounts(domId) {
    // 選擇所有表格：支援 Tabs、ThreeColumn、VerticalStack、BalanceSheet 佈局
    const tables = document.querySelectorAll(
      `#${domId} .mmd-grid, ` +
      `#${domId} .mmd-master-table, ` +
      `#${domId} .mmd-detail-table`
    );

    tables.forEach(table => {
      // 支援大小寫的 wrapper：-wrapper（小寫）和 Wrapper（大寫）
      const wrapper = table.closest('[id$="-wrapper"], [id$="Wrapper"]') || table.closest('.mmd-section-body');
      if (!wrapper) return;

      // 解析 wrapper ID 以確定計數器 ID
      const wrapperId = wrapper.id.replace('-wrapper', '').replace('Wrapper', '').replace(`${domId}-`, '');
      let countId = '';

      // Master 表格計數器
      if (wrapperId === 'master') {
        countId = `${domId}-count-master`;
      }
      // Detail 表格計數器
      else if (wrapperId.startsWith('detail-')) {
        const detailIndex = wrapperId.replace('detail-', '');
        countId = `${domId}-count-detail-${detailIndex}`;
      }

      if (countId) {
        // 初始更新計數
        updateGridCount(table, countId);

        // 監聽表格變化，自動更新計數
        const tbody = table.querySelector('tbody');
        if (tbody) {
          const observer = new MutationObserver(() => {
            updateGridCount(table, countId);
          });
          observer.observe(tbody, {
            childList: true,
            subtree: true
          });
        }
      }
    });
  }

  // ==============================================================================
  //  UI Helpers - 更新表格計數
  // ==============================================================================
  function updateGridCount(table, countId) {
    const tbody = table.querySelector('tbody');
    if (!tbody) return;

    const rows = tbody.querySelectorAll('tr');
    let count = 0;
    let selectedIndex = 0;

    rows.forEach((row) => {
      // 排除提示訊息行
      if (!row.textContent.includes('請點選') && !row.textContent.includes('載入中')) {
        count++;
        if (row.classList.contains('selected')) {
          selectedIndex = count;
        }
      }
    });

    const countElem = document.getElementById(countId);
    if (countElem) {
      // ★ 統一設定計數器格式（只需在此處修改）
      countElem.className = 'badge bg-secondary';
      countElem.textContent = `${selectedIndex > 0 ? selectedIndex : count} / ${count}`;
    }
  }

  // ==============================================================================
  //  UI Helpers - 綁定分隔器拖曳
  // ==============================================================================
  function bindSplitters() {
    const splitters = document.querySelectorAll('.mmd-splitter-v, .mmd-splitter-h');
    splitters.forEach(splitter => {
      splitter.addEventListener('mousedown', function(e) {
        e.preventDefault();
        const isVertical = splitter.classList.contains('mmd-splitter-v');
        const startPos = isVertical ? e.clientX : e.clientY;
        const prevPanel = splitter.previousElementSibling;
        const nextPanel = splitter.nextElementSibling;
        const startSize = isVertical ? prevPanel.offsetWidth : prevPanel.offsetHeight;
        const startNextSize = nextPanel ? (isVertical ? nextPanel.offsetWidth : nextPanel.offsetHeight) : 0;

        // 檢查是否為最後一個水平分隔器（垂直堆疊佈局的底部拖曳器）
        const isLastHSplitter = !isVertical &&
                                prevPanel &&
                                prevPanel.classList.contains('mmd-panel-stack-subdetail');

        // 從 data 屬性讀取最小高度/寬度限制
        const minHeightLimit = parseInt(prevPanel.getAttribute('data-min-height')) || 100;
        const minWidthLimit = parseInt(prevPanel.getAttribute('data-min-width')) || 200;

        if (isLastHSplitter) {
          prevPanel.style.flex = 'none';
          prevPanel.style.height = startSize + 'px';
          if (nextPanel) {
            nextPanel.style.flex = '1';
          }
        }

        function onMouseMove(e) {
          const delta = (isVertical ? e.clientX : e.clientY) - startPos;
          const newSize = startSize + delta;
          const newNextSize = startNextSize - delta;

          // 使用對應的最小限制
          const minLimit = isVertical ? minWidthLimit : minHeightLimit;

          if (newSize >= minLimit) {
            if (isVertical) {
              prevPanel.style.width = newSize + 'px';
              if (nextPanel && !isLastHSplitter && newNextSize > minWidthLimit) {
                nextPanel.style.width = newNextSize + 'px';
              }
            } else {
              prevPanel.style.height = newSize + 'px';
              // 最後一個水平分隔器允許無限增長，不限制 nextPanel 大小
              if (!isLastHSplitter && nextPanel && newNextSize > minHeightLimit) {
                nextPanel.style.height = newNextSize + 'px';
              }
              // 對於最後一個分隔器，讓 filler 自動調整（可以為 0）
              if (isLastHSplitter && nextPanel) {
                nextPanel.style.minHeight = '0';
              }
            }
          }
        }

        function onMouseUp() {
          document.removeEventListener('mousemove', onMouseMove);
          document.removeEventListener('mouseup', onMouseUp);
        }

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
      });
    });
  }

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

  const isDateType = (dataType) => {
    return !!(dataType && String(dataType).toLowerCase().includes("date"));
  };

  const toDateInputValue = (val) => {
    if (!val) return "";
    const s = String(val).trim();
    if (!s) return "";
    const d = new Date(s.replace(/\//g, "-"));
    if (!isNaN(d)) return d.toISOString().slice(0, 10);
    return "";
  };

  const toDisplayDate = (val) => {
    if (!val) return "";
    const s = String(val).trim();
    if (!s) return "";
    const d = new Date(s.replace(/\//g, "-"));
    if (!isNaN(d)) return d.toISOString().slice(0, 10).replace(/-/g, "/");
    return s;
  };

  const attachDatePicker = (td, inp) => {
    if (!td || !inp || inp.readOnly) return;
    td.style.position = "relative";
    inp.style.paddingRight = "22px";

    const btn = document.createElement("button");
    btn.type = "button";
    btn.className = "btn btn-outline-secondary btn-sm date-picker-btn cell-edit d-none";
    btn.innerHTML = "&#9662;";
    btn.style.position = "absolute";
    btn.style.right = "4px";
    btn.style.top = "50%";
    btn.style.transform = "translateY(-50%)";
    btn.tabIndex = -1;

    const picker = document.createElement("input");
    picker.type = "date";
    picker.className = "date-picker-input";
    picker.style.position = "absolute";
    picker.style.opacity = "0";
    picker.style.pointerEvents = "none";
    picker.style.width = "1px";
    picker.style.height = "1px";

    btn.addEventListener("click", () => {
      if (inp.readOnly) return;
      picker.value = toDateInputValue(inp.value);
      if (picker.showPicker) picker.showPicker();
      else picker.focus();
    });

    picker.addEventListener("change", () => {
      const v = toDisplayDate(picker.value);
      inp.value = v;
      inp.dataset.raw = v;
    });

    td.appendChild(btn);
    td.appendChild(picker);
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
        th.dataset.field = f.FieldName;
        const w = DICT.width(f);
        if (w) th.style.width = w + "px";
        th.style.whiteSpace = "nowrap";
        const handle = document.createElement("span");
        handle.className = "col-resizer";
        th.appendChild(handle);
        tr.appendChild(th);
      });
  };

  // ==============================================================================
  //  建表身（含 PK hidden 欄位）
  // ==============================================================================
    // ======================================================================
//  建表身（正確版 PK / FK 儲存格式）
// ======================================================================
const buildBody = async (tbody, dict, rows, onRowClick, isDetail = false) => {
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
      if (isDetail && isDateType(DICT.type(f))) {
        attachDatePicker(td, inp);
      }
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

    // ★ UI 初始化：計數器、拖曳器等
    const layoutMode = cfg.Layout;
    const layoutModeStr = getLayoutModeString(layoutMode);

    // 根據配置啟用計數器
    if (cfg.EnableGridCounts) {
      initGridCounts(cfg.DomId);
    }

    // VerticalStack 佈局：計算並記錄 SubDetail 的動態最小高度
    if (layoutModeStr === 'VerticalStack') {
      const container = root.querySelector('.mmd-vertical-stack-container');
      const subdetailPanel = root.querySelector('.mmd-panel-stack-subdetail');

      if (container && subdetailPanel) {
        // 計算 SubDetail 的初始高度
        // SubDetail高度 = 容器總高度 - Master(120px) - Detail(150px) - 3個分隔器(3x5px=15px)
        const containerHeight = container.offsetHeight;
        const calculatedSubDetailHeight = containerHeight - 120 - 150 - 15;

        // 保存計算出的初始高度作為最小高度限制，用於拖曳限制
        subdetailPanel.setAttribute('data-min-height', calculatedSubDetailHeight);
      }
    }

    // 根據配置啟用拖曳器
    if (cfg.EnableSplitters) {
      bindSplitters();
    }

    // === Master 辭典 ===
    const masterDict = await fetchDict(cfg.MasterDict || cfg.MasterTable);

    buildHead(mHead, masterDict);
    const masterGrid = document.getElementById(`${cfg.DomId}-masterGrid`);
    enableColumnResize(masterGrid, cfg.MasterTable || cfg.MasterDict);

    const uniq = (arr) => [...new Set((arr || []).filter(Boolean).map(s => String(s)))];

    // === Master 資料 ===
    const masterRows = cfg.MasterApi
      ? await fetch(cfg.MasterApi).then(r => r.json())
      : await fetchTopRows(cfg.MasterTable, cfg.MasterTop || 200);

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

      const dict = await fetchDict(d.DetailDict || d.DetailTable);

      const headTr = document.getElementById(hid);
      if (headTr) buildHead(headTr, dict);
      const detailGrid = document.getElementById(`${cfg.DomId}-detail-${i}-grid`);
      enableColumnResize(detailGrid, d.DetailTable || d.DetailDict);
      detailDicts[i] = dict;
    }

    const activeTarget = { type: "master", index: -1 };
    let lastMasterRow = null;

// ======================================================================
//   切換 Master → 重新載入全部明細
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

    const rows = await fetchByKeys(d.DetailTable, names, values);

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
        // BalanceSheet 佈局（Layout = 3）使用平行式聯動，其他佈局使用串聯式聯動
        if (layoutMode === 3) {
          // 平行式：點擊 Detail[0] → 同時載入 Detail[1], Detail[2], ...
          loadAllSubDetailsFromFocus(i, row);
        } else {
          // 串聯式：點擊 Detail[i] → 載入 Detail[i+1] → 載入 Detail[i+2] → ...
          loadNextDetailFromFocus(i, row);
        }
      }
    }, true);

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
    //   Detail Focus 聯動：點擊某層 Detail 時，載入下一層 Detail 的關聯資料（串聯式）
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

      const rows = await fetchByKeys(nextDetail.DetailTable, names, values);

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
      }, true);

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
    //   Detail Focus 聯動：點擊某層 Detail 時，載入所有後續的 SubDetail 關聯資料（平行式）
    //   適用於資產負債表等需要同時更新多個頁籤的佈局
    // ==============================================================================
    const loadAllSubDetailsFromFocus = async (currentDetailIndex, focusedRow) => {
      // ★ 從當前 Detail 的下一個開始，平行載入所有後續的 Detail
      // 例如：點擊 Detail[0] → 同時載入 Detail[1], Detail[2], Detail[3]...
      for (let nextIndex = currentDetailIndex + 1; nextIndex < (cfg.Details || []).length; nextIndex++) {
        const nextDetail = cfg.Details[nextIndex];
        const tbody = document.getElementById(`${cfg.DomId}-detail-${nextIndex}-body`);
        if (!tbody) continue;

        const names = [];
        const values = [];
        const ctx = {};

        // 根據 SubDetail 的 KeyMap 從當前 focusedRow 中提取對應的欄位值
        (nextDetail.KeyMap || []).forEach(k => {
          names.push(k.Detail);           // 用明細的欄位當查詢欄位
          values.push(focusedRow[k.Master]); // 值取 focusedRow 的欄位值
          ctx[k.Detail] = focusedRow[k.Master];
        });
        tbody._lastQueryCtx = ctx;

        const rows = await fetchByKeys(nextDetail.DetailTable, names, values);

        // 載入 SubDetail 的資料（不需要遞迴，因為迴圈已經處理所有層級）
        await buildBody(tbody, detailDicts[nextIndex], rows, (row, tr) => {
          tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
          tr.classList.add("selected");
          activeTarget.type = "detail";
          activeTarget.index = nextIndex;
          // ★ 不再遞迴聯動，避免串聯式連動
        }, true);

        // 編輯模式處理
        if (window._mmdEditing && tbody._editorInstance) {
          if (tbody.offsetParent !== null) {
            tbody._editorInstance.toggleEdit(true);
            tbody._pendingRebind = false;
          } else {
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
      activeTarget.type = "master";
      activeTarget.index = -1;
      lastMasterRow = row;
      await loadAllDetails(row);
    }, false);

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
        if (isDetail && isDateType(DICT.type(f))) {
          attachDatePicker(td, inp);
        }
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

    const addSubDetailRow = () => {
      const t = getActive();
      if (t.type !== "detail" || t.index < 1) {
        if (window.Swal?.fire) {
          Swal.fire({ icon: "info", title: "請先選取第二層或第三層明細" });
        } else {
          alert("請先選取第二層或第三層明細");
        }
        return false;
      }
      ensureEditMode();
      const row = addRowTo(t);
      return !!row;
    };

    const deleteSubDetailRow = async () => {
      const t = getActive();
      if (t.type !== "detail" || t.index < 1) {
        if (window.Swal?.fire) {
          await Swal.fire({ icon: "info", title: "請先選取第二層或第三層明細" });
        } else {
          alert("請先選取第二層或第三層明細");
        }
        return false;
      }
      await deleteSelected();
      return true;
    };

    window._mmdApi = window._mmdApi || {};
    window._mmdApi[cfg.DomId] = {
      addSubDetailRow,
      deleteSubDetailRow
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
    Object.entries(cfgs).forEach(([domId, cfg]) => {
      // 統一使用 initOne 處理所有佈局模式
      initOne(cfg);
    });
  });

  // ==============================================================================
  //  全局監聽：監聽選中行變化，更新計數顯示（通用版，支援所有佈局）
  // ==============================================================================
  document.addEventListener('click', function(e) {
    const tr = e.target.closest('tr');
    if (tr && tr.parentElement.tagName === 'TBODY') {
      const table = tr.closest('table');
      // 支援所有表格類型
      const isValidTable = table && (
        table.classList.contains('mmd-grid') ||
        table.classList.contains('mmd-master-table') ||
        table.classList.contains('mmd-detail-table')
      );

      if (isValidTable) {
        // 支援大小寫的 wrapper：-wrapper（小寫）和 Wrapper（大寫）
        const wrapper = table.closest('[id$="-wrapper"], [id$="Wrapper"]') || table.closest('.mmd-section-body');
        if (!wrapper) return;

        const domId = table.id.split('-')[0];
        const wrapperId = wrapper.id.replace('-wrapper', '').replace('Wrapper', '').replace(`${domId}-`, '');
        let countId = '';

        // Master 表格計數器
        if (wrapperId === 'master') {
          countId = `${domId}-count-master`;
        }
        // Detail 表格計數器
        else if (wrapperId.startsWith('detail-')) {
          const detailIndex = wrapperId.replace('detail-', '');
          countId = `${domId}-count-detail-${detailIndex}`;
        }

        if (countId) {
          setTimeout(() => updateGridCount(table, countId), 100);
        }
      }
    }
  });

})();   // End of IIFE
