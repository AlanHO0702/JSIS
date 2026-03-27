(() => {

  // ==============================================================================
  //  Lookup & OCX Cache
  // ==============================================================================
  const LOOKUP_CACHE = {};
  const OCX_CACHE = {};
  const COMPOSITE_KEY_SEP = '\x1f'; // 複合鍵分隔符（與後端一致）

  // 同步讀取已快取的 Lookup 資料（供 addRowTo 等非 async 場景使用）
  function getCachedLookup(f) {
    const key = `${f.LookupTable}|${f.LookupKeyField}|${f.LookupResultField}`;
    return LOOKUP_CACHE[key] || null;
  }
  function getCachedOCXLookup(f) {
    const composite = parseKeyMaps(f);
    const compositeKeyFields = composite ? composite.keyFieldNames.join(',') : null;
    const effectiveKeyField = compositeKeyFields || f.KeyFieldName;
    const key = `${f.OCXLKTableName}|${effectiveKeyField}|${f.OCXLKResultName}`;
    return OCX_CACHE[key] || null;
  }

  /// 解析 KeyMapsJson，取得所有 key 欄位名稱陣列
  function parseKeyMaps(f) {
    if (!f.KeyMapsJson) return null;
    try {
      const maps = JSON.parse(f.KeyMapsJson);
      if (!Array.isArray(maps) || maps.length <= 1) return null;
      return {
        keyFieldNames: maps.map(m => m.KeyFieldName).filter(Boolean),
        keySelfNames:  maps.map(m => m.KeySelfName).filter(Boolean)
      };
    } catch { return null; }
  }

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
    const cell = {}, dropdown = {};
    rows.forEach(r => {
      const k = String(r.key ?? "");
      cell[k]     = (r.result0 ?? "").toString().trim();
      dropdown[k] = combineResults(r);
    });
    return LOOKUP_CACHE[key] = { cell, dropdown };
  }

  async function loadOCXLookup(f) {
    // ★ 檢查是否有複合鍵設定
    const composite = parseKeyMaps(f);
    const compositeKeyFields = composite ? composite.keyFieldNames.join(',') : null;
    const effectiveKeyField = compositeKeyFields || f.KeyFieldName;

    const key = `${f.OCXLKTableName}|${effectiveKeyField}|${f.OCXLKResultName}`;
    if (OCX_CACHE[key]) return OCX_CACHE[key];

    if (!f.OCXLKTableName || !effectiveKeyField || !f.OCXLKResultName)
      return OCX_CACHE[key] = null;

    const url = `/api/TableFieldLayout/LookupData`
      + `?table=${encodeURIComponent(f.OCXLKTableName)}`
      + `&key=${encodeURIComponent(effectiveKeyField)}`
      + `&result=${encodeURIComponent(f.OCXLKResultName)}`;

    const rows = await fetch(url).then(r => r.json());
    const cell = {}, dropdown = {};
    rows.forEach(r => {
      const k = String(r.key ?? "");
      cell[k]     = (r.result0 ?? "").toString().trim();
      dropdown[k] = combineResults(r);
    });

    const result = { cell, dropdown };
    // ★ 記錄複合鍵資訊，供 cell rendering 時使用
    if (composite) {
      result._compositeKeySelfNames = composite.keySelfNames;
    }
    return OCX_CACHE[key] = result;
  }

  // 將 result0, result1, ... 合併成顯示字串
  function combineResults(r) {
    const parts = [];
    let i = 0;
    while (r[`result${i}`] !== undefined) {
      const v = (r[`result${i}`] ?? "").toString().trim();
      if (v !== "") parts.push(v);
      i++;
    }
    return parts.join('　');
  }

  // ==============================================================================
  //  Lookup Dropdown（雙擊下拉選單）
  // ==============================================================================
  function ensureLookupDropdownCss() {
    if (document.getElementById('mmd-lookup-dd-css')) return;
    const style = document.createElement('style');
    style.id = 'mmd-lookup-dd-css';
    style.textContent = `
.mmd-lookup-dd{background:#fff;border:1px solid #b0c4de;border-radius:6px;box-shadow:0 4px 18px rgba(0,0,0,.18);max-height:300px;display:flex;flex-direction:column;overflow:hidden;font-size:.85rem}
.mmd-lookup-search{border:none;border-bottom:1px solid #dde6f0;padding:5px 9px;outline:none;font-size:.85rem;min-width:120px}
.mmd-lookup-list{overflow-y:auto;max-height:252px}
.mmd-lookup-item{padding:4px 10px;cursor:pointer;white-space:nowrap}
.mmd-lookup-item:hover,.mmd-lookup-item:focus{background:#e8f0ff;outline:none}
.mmd-lookup-item.selected{background:#d0e4ff;font-weight:500}
.mmd-lookup-dd[data-readonly="1"] .mmd-lookup-item{cursor:default;color:#555}
.mmd-lookup-dd[data-readonly="1"] .mmd-lookup-item:hover,.mmd-lookup-dd[data-readonly="1"] .mmd-lookup-item:focus{background:#f5f5f5}
`;
    document.head.appendChild(style);
  }

  function showLookupDropdown(inp, td, lookupMap, readOnly, onSelect) {
    ensureLookupDropdownCss();
    const existing = document.getElementById('mmd-lookup-dd');
    if (existing) existing.remove();

    const entries = Object.entries(lookupMap);
    if (!entries.length) return;

    // 取得目前的 raw key（瀏覽模式從 span data-raw 取，編輯模式從 inp）
    const currentRaw = readOnly
      ? (td.querySelector('.cell-view')?.dataset?.raw ?? inp.dataset.raw ?? "")
      : (inp.dataset.raw ?? "");

    const dd = document.createElement('div');
    dd.id = 'mmd-lookup-dd';
    dd.className = 'mmd-lookup-dd';
    if (readOnly) dd.dataset.readonly = '1';

    const search = document.createElement('input');
    search.type = 'text';
    search.placeholder = readOnly ? '搜尋（唯讀）…' : '搜尋…';
    search.className = 'mmd-lookup-search';
    dd.appendChild(search);

    const list = document.createElement('div');
    list.className = 'mmd-lookup-list';
    dd.appendChild(list);

    // ── 模糊比對：判斷 query 的每個字元是否依序出現在 text 中 ──
    const fuzzyMatch = (text, query) => {
      let ti = 0;
      for (let qi = 0; qi < query.length; qi++) {
        const idx = text.indexOf(query[qi], ti);
        if (idx < 0) return false;
        ti = idx + 1;
      }
      return true;
    };

    // ── 匹配等級：0=前綴, 1=包含, 2=模糊, -1=不符 ──
    const matchRank = (text, key, q) => {
      const tL = text.toLowerCase(), kL = key.toLowerCase();
      if (tL.startsWith(q) || kL.startsWith(q)) return 0;
      if (tL.includes(q) || kL.includes(q)) return 1;
      if (fuzzyMatch(tL, q) || fuzzyMatch(kL, q)) return 2;
      return -1;
    };

    // ── 渲染下拉項目（含搜尋過濾＋依匹配程度排序） ──
    const renderItems = (filter) => {
      list.innerHTML = '';
      const q = (filter || '').toLowerCase();
      let matched = [];
      entries.forEach(([key, label]) => {
        const text = `${label}`;
        if (!q) {
          matched.push({ key, label, text, rank: 0 });
        } else {
          const rank = matchRank(text, String(key), q);
          if (rank >= 0) matched.push({ key, label, text, rank });
        }
      });
      if (q) matched.sort((a, b) => a.rank - b.rank);
      matched.forEach(({ key, label, text }) => {
        const item = document.createElement('div');
        item.className = 'mmd-lookup-item';
        item.tabIndex = -1;
        item.textContent = text;
        item.dataset.key = key;
        if (String(currentRaw) === String(key)) item.classList.add('selected');
        if (!readOnly) {
          item.addEventListener('mousedown', (e) => {
            e.preventDefault();
            if (onSelect) {
              onSelect(key, label);
            } else {
              inp.value = key;
              inp.dataset.raw = key;
              const span = td.querySelector('.cell-view');
              if (span) span.textContent = label;
              inp.dispatchEvent(new Event('input', { bubbles: true }));
              inp.dispatchEvent(new Event('change', { bubbles: true }));
            }
            dd.remove();
          });
        }
        list.appendChild(item);
      });
    };

    renderItems('');
    search.addEventListener('input', () => renderItems(search.value));

    // 定位（避免超出視窗底部）
    const rect = td.getBoundingClientRect();
    dd.style.position = 'fixed';
    dd.style.left = `${rect.left}px`;
    dd.style.minWidth = `${Math.max(rect.width, 180)}px`;
    dd.style.zIndex = '99999';
    document.body.appendChild(dd);

    const ddH = dd.offsetHeight || 300;
    const below = window.innerHeight - rect.bottom;
    dd.style.top = below >= ddH || below >= rect.top
      ? `${rect.bottom + 1}px`
      : `${rect.top - ddH - 1}px`;

    requestAnimationFrame(() => search.focus());

    const close = (e) => {
      if (!dd.contains(e.target)) {
        dd.remove();
        document.removeEventListener('mousedown', close);
      }
    };
    setTimeout(() => document.addEventListener('mousedown', close), 0);

    search.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') { dd.remove(); inp.focus(); }
      if (e.key === 'ArrowDown') {
        const first = list.querySelector('.mmd-lookup-item');
        if (first) first.focus();
        e.preventDefault();
      }
    });

    list.addEventListener('keydown', (e) => {
      const focused = document.activeElement;
      if (e.key === 'ArrowDown') {
        const next = focused?.nextElementSibling;
        if (next) next.focus();
        e.preventDefault();
      } else if (e.key === 'ArrowUp') {
        const prev = focused?.previousElementSibling;
        if (prev) prev.focus(); else search.focus();
        e.preventDefault();
      } else if (e.key === 'Enter') {
        focused?.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
        e.preventDefault();
      } else if (e.key === 'Escape') {
        dd.remove(); inp.focus();
      }
    });
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
  const fetchTopRows = async (tableName, top = 200, filter = '') => {
    if (!tableName) return [];
    let url = `/api/CommonTable/TopRows?table=${encodeURIComponent(tableName)}&top=${top}`;
    if (filter) url += `&filter=${encodeURIComponent(filter)}`;
    return await fetch(url).then(r => r.json());
  };

  /**
   * 根據多組 Key 抓取資料（用於 Master → Detail 關聯查詢）
   * @param {string} tableName - 資料表名稱
   * @param {Array<string>} keyNames - Key 欄位名稱陣列
   * @param {Array<any>} keyValues - Key 值陣列
   * @returns {Promise<Array>} 資料列陣列
   */
  const fetchByKeys = async (tableName, keyNames, keyValues, orderBy = null, orderDir = "ASC") => {
    if (!tableName || !keyNames || !keyValues) return [];
    const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(tableName)}`
      + keyNames.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
      + keyValues.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("")
      + (orderBy ? `&orderBy=${encodeURIComponent(orderBy)}` : "")
      + (orderBy ? `&orderDir=${encodeURIComponent(orderDir || "ASC")}` : "");
    try {
      const res = await fetch(url);
      if (!res.ok) {
        const errText = await res.text().catch(() => res.statusText);
        console.error(`[MMD] fetchByKeys 失敗: ${tableName}`, { keyNames, keyValues, status: res.status, error: errText });
        return [];
      }
      return await res.json();
    } catch (err) {
      console.error(`[MMD] fetchByKeys 例外: ${tableName}`, { keyNames, keyValues, error: err.message });
      return [];
    }
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
  //  表格排序狀態管理
  // ==============================================================================
  const SORT_STATE = {}; // { tableId: { fieldName: 'asc' | 'desc' | null } }

  const getSortIndicator = (direction) => {
    if (direction === 'desc') return ' ▼';
    if (direction === 'asc') return ' ▲';
    return '';
  };

  const sortTableByField = (tbody, dict, fieldName, direction) => {
    if (!tbody || !fieldName || !direction) return;

    const getRowValue = (row, field) => {
      if (!row) return "";
      const want = String(field).toLowerCase();
      const hit = Object.keys(row).find(k => String(k).toLowerCase() === want);
      return hit ? row[hit] : "";
    };

    const getFieldDef = (dict, fieldName) => {
      return dict.find(f => f.FieldName === fieldName);
    };

    const parseValue = (value, dataType) => {
      if (value == null || value === "") return null;

      // 日期類型
      if (dataType && String(dataType).toLowerCase().includes("date")) {
        const d = new Date(value);
        return isNaN(d) ? null : d.getTime();
      }

      // 數字類型
      if (typeof value === "number") return value;
      const strValue = String(value).replace(/,/g, "").trim();
      const num = Number(strValue);
      if (!isNaN(num) && strValue.match(/^-?\d+(\.\d+)?$/)) return num;

      // 字串類型
      return String(value).toLowerCase();
    };

    // 從 tbody 中取得所有資料列及其原始資料
    const rows = Array.from(tbody.querySelectorAll('tr')).map(tr => {
      // 排除佔位列和已刪除的列
      const isPlaceholder = tr.dataset.placeholder === "1";
      const isDeleted = tr.dataset.state === "deleted";
      if (isPlaceholder || isDeleted) {
        return { tr, data: {}, skip: true };
      }

      const rowData = {};
      const inputs = tr.querySelectorAll('input.cell-edit[name]');
      inputs.forEach(inp => {
        if (inp.name) {
          // 優先使用 dataset.raw (原始值)，如果不存在則使用 value
          rowData[inp.name] = inp.dataset.raw !== undefined ? inp.dataset.raw : inp.value;
        }
      });
      return { tr, data: rowData, skip: false };
    });

    const fieldDef = getFieldDef(dict, fieldName);
    const dataType = fieldDef?.DataType;

    // 分離需要排序的列和不需要排序的列
    const sortableRows = rows.filter(r => !r.skip);
    const skipRows = rows.filter(r => r.skip);

    // 排序
    sortableRows.sort((a, b) => {
      const aVal = parseValue(getRowValue(a.data, fieldName), dataType);
      const bVal = parseValue(getRowValue(b.data, fieldName), dataType);

      // 處理 null 值 (null 排在最後)
      if (aVal === null && bVal === null) return 0;
      if (aVal === null) return 1;
      if (bVal === null) return -1;

      // 比較值
      let result = 0;
      if (typeof aVal === 'string' && typeof bVal === 'string') {
        result = aVal.localeCompare(bVal, 'zh-TW');
      } else {
        result = aVal > bVal ? 1 : aVal < bVal ? -1 : 0;
      }

      // 套用排序方向
      return direction === 'desc' ? -result : result;
    });

    // 重新排列 DOM：先放置已排序的列，再放置佔位列/已刪除的列
    sortableRows.forEach(({ tr }) => tbody.appendChild(tr));
    skipRows.forEach(({ tr }) => tbody.appendChild(tr));
  };

  // ==============================================================================
  //  建表頭
  // ==============================================================================
  const buildHead = (tr, dict) => {
    tr.innerHTML = "";
    const tableId = tr.closest('table')?.id || '';

    if (!SORT_STATE[tableId]) {
      SORT_STATE[tableId] = {};
    }

    dict
      .filter(DICT.visible)
      .sort((a, b) => DICT.order(a) - DICT.order(b))
      .forEach(f => {
        const th = document.createElement("th");
        const textSpan = document.createElement("span");
        textSpan.className = "th-text";
        textSpan.textContent = DICT.header(f);

        th.appendChild(textSpan);
        th.dataset.field = f.FieldName;
        const w = DICT.width(f);
        if (w) th.style.width = w + "px";
        th.style.whiteSpace = "nowrap";
        th.style.cursor = "pointer";
        th.style.userSelect = "none";
        th.title = "雙擊可排序";

        // 雙擊事件：排序
        th.addEventListener('dblclick', (e) => {
          e.preventDefault();
          e.stopPropagation();

          const table = th.closest('table');
          const tbody = table?.querySelector('tbody');
          if (!tbody) return;

          const currentSort = SORT_STATE[tableId][f.FieldName];
          let newSort = null;

          // 切換排序狀態：null → desc → asc → desc → ...
          if (!currentSort || currentSort === 'asc') {
            newSort = 'desc';
          } else {
            newSort = 'asc';
          }

          // 清除其他欄位的排序狀態
          SORT_STATE[tableId] = { [f.FieldName]: newSort };

          // 更新所有表頭的視覺指示器
          table.querySelectorAll('thead th').forEach(header => {
            const field = header.dataset.field;
            const span = header.querySelector('.th-text');
            if (span) {
              const baseText = DICT.header(dict.find(d => d.FieldName === field));
              const indicator = field === f.FieldName ? getSortIndicator(newSort) : '';
              span.textContent = baseText + indicator;
            }
          });

          // 執行排序
          sortTableByField(tbody, dict, f.FieldName, newSort);
        });

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
      fk.value = getRowValue(row, k.Detail);
      fk.className = "mmd-fk-hidden";
      tr.append(fk);
    });

    // ===== 表格 UI 欄位產生 =====
    fields.forEach(f => {
      const td = document.createElement("td");
      if (DICT.visible(f)) td.dataset.field = f.FieldName;

      // ★ 修正：使用 getRowValue 處理大小寫，且只有 undefined 時才取 KeySelfName（避免 0 被當成 falsy）
      let raw = getRowValue(row, f.FieldName);
      if (raw === "" && f.KeySelfName) raw = getRowValue(row, f.KeySelfName);

      let display = raw;
      // ★ 複合鍵支援：若 OCX lookup 有 _compositeKeySelfNames，用多欄位組成複合鍵查找
      const ocxData = oc[f.FieldName];
      if (ocxData?._compositeKeySelfNames) {
        const parts = ocxData._compositeKeySelfNames.map(n => String(getRowValue(row, n) ?? "").trim());
        if (parts.every(p => p !== "")) {
          const compositeKey = parts.join(COMPOSITE_KEY_SEP);
          if (ocxData.cell?.[compositeKey] != null) display = ocxData.cell[compositeKey];
        }
      } else if (ocxData?.cell?.[raw] != null) {
        display = ocxData.cell[raw];
      }
      if (display === raw && lk[f.FieldName]?.cell?.[raw] != null) display = lk[f.FieldName].cell[raw];

      // ★ 是否顯示為勾選框 (ComboStyle==1)
      const isCheckbox = String(f.ComboStyle ?? "").trim() === "1";
      if (isCheckbox) td.classList.add("text-center", "align-middle");

      const span = document.createElement("span");
      span.className = "cell-view";
      span.dataset.raw = raw == null ? "" : String(raw);
      if (isCheckbox) span.classList.add("d-inline-flex", "justify-content-center", "w-100");

      const inp = document.createElement("input");
      inp.className = isCheckbox ? "form-check-input checkbox-dark cell-edit d-none mx-auto" : "form-control form-control-sm cell-edit d-none";
      inp.name = f.FieldName;

      if (isCheckbox) {
        // ★ 支援多種格式：true/1/"1"/"true"/"True"/bit 欄位
        const checked = raw === true || raw === 1 || raw === "1" ||
                        (typeof raw === "string" && raw.toLowerCase() === "true");
        const viewChk = document.createElement("input");
        viewChk.type = "checkbox";
        viewChk.disabled = true;
        viewChk.tabIndex = -1;
        viewChk.className = "form-check-input checkbox-dark";
        viewChk.checked = checked;
        span.appendChild(viewChk);

        inp.type = "checkbox";
        inp.checked = checked;
        inp.value = checked ? "1" : "0";
        inp.dataset.raw = inp.value;

        // ★ 同步 viewChk 與 inp 的狀態（確保編輯模式切換時同步）
        const syncCheckbox = () => {
          inp.value = inp.checked ? "1" : "0";
          inp.dataset.raw = inp.value;
          viewChk.checked = inp.checked;
        };
        inp.addEventListener("change", syncCheckbox);
        inp.addEventListener("click", syncCheckbox);
      } else {
        const formatted = fmtCell(display, DICT.fmt(f), DICT.type(f));
        span.textContent = formatted;
        inp.dataset.raw = raw == null ? "" : String(raw);
        // 日期欄位：input 也顯示格式化值，raw 留存原始值供儲存
        inp.value = isDateType(DICT.type(f)) && formatted
          ? formatted
          : (display == null ? "" : String(display));
        // ★ 若有 lookup 對照表，記錄並綁定雙擊下拉（編輯模式可選取，瀏覽模式唯讀）
        const fieldLookupMap = oc[f.FieldName]?.dropdown || lk[f.FieldName]?.dropdown;
        const fieldCellMap   = oc[f.FieldName]?.cell    || lk[f.FieldName]?.cell;
        if (fieldLookupMap && Object.keys(fieldLookupMap).length > 0) {
          inp._lookupMap     = fieldLookupMap;  // 下拉用（含所有顯示欄位合併）
          inp._lookupCellMap = fieldCellMap;    // 儲存格顯示用（只用 result0）
          // ★ 記錄條件過濾欄位資訊
          const cond1Field  = (f.LookupCond1Field || '').trim();
          const cond1Source = (f.LookupCond1ResultField || '').trim();
          const cond2Field  = (f.LookupCond2Field || '').trim();
          const cond2Source = (f.LookupCond2ResultField || '').trim();
          const hasCondition = !!(cond1Field || cond2Field);
          const lookupTable  = f.LookupTable || f.OCXLKTableName || '';
          const lookupKey    = f.LookupKeyField || f.KeyFieldName || '';
          const lookupResult = f.LookupResultField || f.OCXLKResultName || '';
          // ★ 非實體 lookup 欄位（有 KeySelfName）：編輯模式下允許從下拉選單選取，
          //   選取後同步更新 key 欄位並觸發 change 事件刷新其他依賴欄位
          const keySelfName = (f.KeySelfName || '').trim();
          td.addEventListener('dblclick', async (e) => {
            e.stopPropagation();
            let isReadOnly = !window._mmdEditing || inp.readOnly || inp.disabled;
            let onSelect = null;
            if (isReadOnly && window._mmdEditing && keySelfName && !inp.disabled) {
              isReadOnly = false;
              onSelect = (key, label) => {
                inp.value = label;
                inp.dataset.raw = key;
                const span = td.querySelector('.cell-view');
                if (span) { span.textContent = label; span.dataset.raw = key; }
                const tr = td.closest('tr');
                if (tr) {
                  const kfLower = keySelfName.toLowerCase();
                  const keyInp = Array.from(tr.querySelectorAll('input'))
                    .find(i => (i.name || '').toLowerCase() === kfLower);
                  if (keyInp) {
                    keyInp.value = key;
                    keyInp.dataset.raw = key;
                    const keyTd = keyInp.closest('td');
                    const keyView = keyTd?.querySelector('.cell-view');
                    if (keyView) { keyView.textContent = key; keyView.dataset.raw = key; }
                    keyInp.dispatchEvent(new Event('input', { bubbles: true }));
                    keyInp.dispatchEvent(new Event('change', { bubbles: true }));
                  }
                }
              };
            }
            // ★ 有條件過濾時，即時查詢 API 取得過濾後的資料
            if (hasCondition && lookupTable && lookupKey && lookupResult) {
              const currentTr = td.closest('tr');
              const getCondVal = (sourceField) => {
                if (!sourceField || !currentTr) return '';
                const sfLower = sourceField.toLowerCase();
                // ★ 優先從 td[data-field] 找 cell-edit 的 dataset.raw（原始 key 值）
                //   因為 inp.value 可能是 lookup 翻譯後的顯示文字，不是實際的 key
                const tdCell = Array.from(currentTr.querySelectorAll('td[data-field]'))
                  .find(t => (t.dataset.field || '').toLowerCase() === sfLower);
                if (tdCell) {
                  const cellInp = tdCell.querySelector('.cell-edit');
                  if (cellInp) return (cellInp.dataset?.raw ?? cellInp.value ?? '').trim();
                }
                // 再找隱藏 input（PK/FK hidden）
                const found = Array.from(currentTr.querySelectorAll('input[type="hidden"]'))
                  .find(i => (i.name || '').toLowerCase() === sfLower);
                if (found) return (found.value ?? '').trim();
                return '';
              };
              const c1Val = getCondVal(cond1Source);
              const c2Val = getCondVal(cond2Source);
              let url = `/api/TableFieldLayout/LookupData?table=${encodeURIComponent(lookupTable)}&key=${encodeURIComponent(lookupKey)}&result=${encodeURIComponent(lookupResult)}`;
              if (cond1Field && c1Val) url += `&cond1Field=${encodeURIComponent(cond1Field)}&cond1Value=${encodeURIComponent(c1Val)}`;
              if (cond2Field && c2Val) url += `&cond2Field=${encodeURIComponent(cond2Field)}&cond2Value=${encodeURIComponent(c2Val)}`;
              try {
                const res = await fetch(url);
                if (!res.ok) return;
                const data = await res.json();
                const filteredMap = {};
                (data || []).forEach(row => {
                  const k = String(row.key ?? '');
                  if (!k) return;
                  filteredMap[k] = combineResults(row);
                });
                if (Object.keys(filteredMap).length > 0) {
                  showLookupDropdown(inp, td, filteredMap, isReadOnly, onSelect);
                }
              } catch { }
              return;
            }
            showLookupDropdown(inp, td, inp._lookupMap, isReadOnly, onSelect);
          });
        }
      }

      // ★★★ 修正：如果該欄位是 PK (IsKey)，或者是唯讀，或者是關聯鍵,都必須鎖定不可編輯 ★★★
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

  // ★ 建立反向索引：key 欄位 → 依賴它的 lookup 結果欄位（供動態刷新用）
  const _keyToLookup = {};
  for (const f of fields) {
    const ocxData = oc[f.FieldName];
    const lkData = lk[f.FieldName];
    if (!ocxData && !lkData) continue;
    const keySelfNames = ocxData?._compositeKeySelfNames
      || (f.KeySelfName ? [f.KeySelfName] : (f.LookupKeyField ? [f.LookupKeyField] : []));
    for (const kn of keySelfNames) {
      const knLower = kn.toLowerCase();
      if (!_keyToLookup[knLower]) _keyToLookup[knLower] = [];
      _keyToLookup[knLower].push({
        resultFieldName: f.FieldName,
        ocxData: ocxData || lkData,
        compositeKeySelfNames: ocxData?._compositeKeySelfNames || null,
        keySelf: f.KeySelfName || f.LookupKeyField || null
      });
    }
  }
  tbody._keyToLookup = _keyToLookup;
  tbody._ocxMaps = oc;

  // ★ 套用已儲存的排序狀態
  const table = tbody.closest('table');
  const tableId = table?.id || '';
  if (SORT_STATE[tableId]) {
    const sortedField = Object.keys(SORT_STATE[tableId]).find(f => SORT_STATE[tableId][f]);
    if (sortedField) {
      const direction = SORT_STATE[tableId][sortedField];
      sortTableByField(tbody, dict, sortedField, direction);

      // 更新表頭的視覺指示器
      const thead = table?.querySelector('thead');
      if (thead) {
        thead.querySelectorAll('th').forEach(th => {
          const field = th.dataset.field;
          const span = th.querySelector('.th-text');
          if (span && field) {
            const fieldDef = dict.find(d => d.FieldName === field);
            const baseText = DICT.header(fieldDef);
            const indicator = field === sortedField ? getSortIndicator(direction) : '';
            span.textContent = baseText + indicator;
          }
        });
      }
    }
  }
};

  // ==============================================================================
  //  建立合計列 (bFooter)
  // ==============================================================================
  const buildFooter = (tableEl, dict, rows) => {
    if (!tableEl) return;

    const old = tableEl.querySelector('tfoot.md-grid-footer');
    if (old) old.remove();

    const visibleFields = dict
      .filter(f => DICT.visible(f))
      .sort((a, b) => DICT.order(a) - DICT.order(b));

    const hasFooter = visibleFields.some(f => +f.bFooter === 1);
    if (!hasFooter) return;

    const tfoot = document.createElement('tfoot');
    tfoot.className = 'md-grid-footer';
    const tr = document.createElement('tr');

    visibleFields.forEach(f => {
      const td = document.createElement('td');
      if (+f.bFooter === 1) {
        let sum = 0;
        rows.forEach(row => {
          if (!row || row.__state === 'deleted') return;
          const raw = row[f.FieldName];
          if (raw == null || raw === '') return;
          const num = parseFloat(String(raw).replace(/,/g, ''));
          if (Number.isFinite(num)) sum += num;
        });
        td.textContent = fmtCell(sum, DICT.fmt(f), DICT.type(f));
        td.style.textAlign = 'right';
        td.style.fontWeight = '600';
        td.style.background = '#f0f4ff';
      }
      tr.appendChild(td);
    });

    tfoot.appendChild(tr);
    tableEl.appendChild(tfoot);
  };

  // ==============================================================================
  //  初始化主從頁（initOne）
  // ==============================================================================
  const initOne = async (cfg) => {

    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const mHead = root.querySelector(`#${cfg.DomId}-m-head`);
    const mBody = root.querySelector(`#${cfg.DomId}-m-body`);
    const btnFirst = document.getElementById(`${cfg.DomId}-btnFirst`);
    const btnPrev = document.getElementById(`${cfg.DomId}-btnPrev`);
    const btnNext = document.getElementById(`${cfg.DomId}-btnNext`);
    const btnLast = document.getElementById(`${cfg.DomId}-btnLast`);
    const btnEdit = document.getElementById(`${cfg.DomId}-btnEdit`);
    const btnAdd = document.getElementById(`${cfg.DomId}-btnAdd`);
    const btnSave = document.getElementById(`${cfg.DomId}-btnSave`);
    const btnDelete = document.getElementById(`${cfg.DomId}-btnDelete`);
    const btnCancel = document.getElementById(`${cfg.DomId}-btnCancel`);
    const queryBtn = document.getElementById(`${cfg.DomId}-btnQuery`);
    const countBox = document.getElementById(`${cfg.DomId}-countBox`);
    const modeLabel = document.getElementById(`${cfg.DomId}-modeLabel`);
    const countLabel = document.getElementById(`${cfg.DomId}-countLabel`);
    const queryModalEl = document.getElementById(`${cfg.DomId}-queryModal`);
    const queryForm = document.getElementById(`${cfg.DomId}-queryForm`);
    const btnClearQuery = document.getElementById(`${cfg.DomId}-btnClearQuery`);
    const btnQuerySubmit = document.getElementById(`${cfg.DomId}-btnQuerySubmit`);

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
    if (window.initColumnDragSort && mHead) {
      initColumnDragSort({ headerRow: mHead, tbody: masterGrid?.querySelector('tbody'), scrollWrap: masterGrid?.closest('.table-responsive') || masterGrid?.parentElement, tableName: cfg.MasterDict || cfg.MasterTable || '', onSaved: null });
    }

    const uniq = (arr) => [...new Set((arr || []).filter(Boolean).map(s => String(s)))];

    // === Master 資料（若 URL 帶有查詢參數則走 Query endpoint 篩選）===
    const masterFilterSql = cfg.MasterFilterSql || '';
    const masterRows = await (async () => {
      if (cfg.MasterApi) return fetch(cfg.MasterApi).then(r => r.json());
      const urlParams = new URLSearchParams(window.location.search);
      const excludeKeys = new Set(['pageIndex', 'pageindex']);
      const filterParams = [];
      for (const [k, v] of urlParams.entries()) {
        if (!excludeKeys.has(k) && v) filterParams.push([k, v]);
      }
      if (filterParams.length > 0) {
        const qp = new URLSearchParams();
        qp.set('table', cfg.MasterTable);
        qp.set('top', String(cfg.MasterTop || 200));
        if (masterFilterSql) qp.set('filter', masterFilterSql);
        filterParams.forEach(([k, v]) => qp.set(k, v));
        return fetch(`/api/CommonTable/Query?${qp}`).then(r => r.json());
      }
      return fetchTopRows(cfg.MasterTable, cfg.MasterTop || 200, masterFilterSql);
    })();

    // Master key fields: prefer cfg.MasterPkFields, fallback to dict IsKey
    const masterKeyFields = uniq([
      ...(masterDict.filter(f => DICT.isKey(f)).map(f => f.FieldName)),
      ...((cfg.MasterPkFields || []))
    ]);
    // Let buildBody inject hidden keys even if not visible
    mBody._keyFields = masterKeyFields;

    // ★ 從 FilterSQL 解析預設值（如 "and t0.FactorType=104" → {FactorType: "104"}）
    if (masterFilterSql) {
      const filterDefaults = {};
      // 匹配 field=value 模式（支援 t0.Field=Value 或 Field=Value）
      const re = /(?:t\d+\.)?(\w+)\s*=\s*(\d+|'[^']*')/gi;
      let m;
      while ((m = re.exec(masterFilterSql)) !== null) {
        filterDefaults[m[1]] = m[2].replace(/^'|'$/g, '');
      }
      mBody._filterDefaults = filterDefaults;
    }

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
      if (window.initColumnDragSort && headTr) {
        initColumnDragSort({ headerRow: headTr, tbody: detailGrid?.querySelector('tbody'), scrollWrap: detailGrid?.closest('.mmd-detail-scroll') || detailGrid?.parentElement, tableName: d.DetailDict || d.DetailTable || '', onSaved: null });
      }
      detailDicts[i] = dict;
    }

    const activeTarget = { type: "master", index: -1 };
    const masterIndicator = root.querySelector(`#${cfg.DomId}-masterIndicator`);
    const detailIndicators = Array.from(root.querySelectorAll(`[id^="${cfg.DomId}-detailIndicator-"]`));
    const detailIndicatorPrefix = `${cfg.DomId}-detailIndicator-`;
    let lastMasterRow = null;
    let lastDetailRow = null;
    window._mmdSelectedRows = window._mmdSelectedRows || {};
    if (!window._mmdSelectedRows[cfg.DomId]) {
      window._mmdSelectedRows[cfg.DomId] = { master: null, details: {} };
    }
    const selectedBucket = window._mmdSelectedRows[cfg.DomId];

// ======================================================================
//   切換 Master → 重新載入全部明細
// ======================================================================
let _detailGen = 0;   // ★ generation counter 防止重複載入
// DetailCascadeMode: 0/未設定=全部從 Master 取 key, 1=串聯式(Detail[N]←Detail[N-1]), 2=扇出式(Detail[1]+←Detail[0])
const cascadeMode = cfg.DetailCascadeMode || 0;
const loadAllDetails = async (row) => {
  const gen = ++_detailGen;   // 取得本次呼叫的 generation
  let prevDetailRows = null;  // 級聯模式下，記錄上一層 Detail 的查詢結果

  for (let i = 0; i < (cfg.Details || []).length; i++) {
    if (gen !== _detailGen) return;   // ★ 已有更新的呼叫，放棄本次
    const d = cfg.Details[i];
    const tbody = document.getElementById(`${cfg.DomId}-detail-${i}-body`);
    if (!tbody) continue;

    // ⭐ 根據 DetailCascadeMode 決定 key 值的來源
    //    mode 0: 全部從 Master row 取值（原始行為）
    //    mode 1: Detail[0]←Master, Detail[N]←Detail[N-1] 的第一筆
    //    mode 2: Detail[0]←Master, Detail[1]+←Detail[0] 的第一筆
    let sourceRow = row; // 預設從 Master 取值
    if (i > 0 && cascadeMode > 0) {
      const parentRows = prevDetailRows;
      if (!parentRows || parentRows.length === 0) {
        // 上層沒有資料 → 本層及後續層都不查詢，顯示空白
        tbody._lastQueryCtx = {};
        tbody.innerHTML = "";
        const dict = detailDicts[i] || [];
        const visibleCols = dict.filter(f => (f.Visible ?? 1) === 1).length || 1;
        const placeholderTr = document.createElement("tr");
        placeholderTr.className = "mmd-placeholder-row";
        placeholderTr.style.cursor = "pointer";
        placeholderTr.dataset.placeholder = "1";
        const td = document.createElement("td");
        td.colSpan = visibleCols;
        td.className = "text-center text-muted";
        td.style.padding = "8px";
        td.innerHTML = "<small>（點擊此處後可使用上方 + 按鈕新增資料）</small>";
        placeholderTr.appendChild(td);
        const detailIndex = i;
        placeholderTr.addEventListener("click", () => {
          activeTarget.type = "detail";
          activeTarget.index = detailIndex;
          tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
          placeholderTr.classList.add("selected");
          updateCountPanel();
        });
        tbody.appendChild(placeholderTr);
        if (cascadeMode === 1) prevDetailRows = null; // 串聯式：本層無資料，後續層也無
        continue;
      }
      sourceRow = parentRows[0];
    }

    const names = [];
    const values = [];
    const ctx = {};

    (d.KeyMap || []).forEach(k => {
      names.push(k.Detail);               // 用明細的欄位當查詢欄位
      values.push(sourceRow[k.Master]);    // 值取來源列的欄位值
      ctx[k.Detail] = sourceRow[k.Master];
    });
    tbody._lastQueryCtx = ctx;

    const rows = await fetchByKeys(d.DetailTable, names, values, d.OrderByField);
    if (gen !== _detailGen) return;   // ★ await 後再檢查一次
    // 級聯模式：記錄本層結果供下一層使用
    if (cascadeMode === 1) prevDetailRows = rows;           // 串聯式：下一層用本層
    else if (cascadeMode === 2 && i === 0) prevDetailRows = rows; // 扇出式：只記 Detail[0]

    // ★ 為 Detail 表格添加 row click 事件處理，實現 Focus 功能
    await buildBody(tbody, detailDicts[i], rows, (row, tr) => {
      // 移除所有行的 selected class
      tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
      // 添加 selected class 到被點擊的行
      tr.classList.add("selected");
      activeTarget.type = "detail";
      activeTarget.index = i;
      updateCountPanel();
      lastDetailRow = row;
      selectedBucket.details[i] = row;
      selectedBucket.lastDetailRow = row;
      selectedBucket.lastDetailIndex = i;

      // ★★★ Detail Focus 聯動功能 ★★★
      //   自動串聯只載入資料，不會改寫 activeTarget，所以 activeTarget 保持在使用者點擊的層級
      if (cfg.EnableDetailFocusCascade) {
        if (layoutMode === 3) {
          loadAllSubDetailsFromFocus(i, row);
        } else {
          loadNextDetailFromFocus(i, row);
        }
      }
    }, true);

    const detailGridEl = document.getElementById(`${cfg.DomId}-detail-${i}-grid`);
    buildFooter(detailGridEl, detailDicts[i], rows);

    // ★ 當 detail 沒有資料時，放一列空白佔位列，讓使用者可以點擊聚焦到單身區塊
    if (rows.length === 0 && tbody.querySelectorAll("tr").length === 0) {
      const dict = detailDicts[i] || [];
      const visibleCols = dict.filter(f => (f.Visible ?? 1) === 1).length || 1;
      const placeholderTr = document.createElement("tr");
      placeholderTr.className = "mmd-placeholder-row";
      placeholderTr.style.cursor = "pointer";
      placeholderTr.dataset.placeholder = "1";
      const td = document.createElement("td");
      td.colSpan = visibleCols;
      td.className = "text-center text-muted";
      td.style.padding = "8px";
      td.innerHTML = "<small>（點擊此處後可使用上方 + 按鈕新增資料）</small>";
      placeholderTr.appendChild(td);
      const detailIndex = i; // 閉包變數
      placeholderTr.addEventListener("click", () => {
        activeTarget.type = "detail";
        activeTarget.index = detailIndex;
        tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
        placeholderTr.classList.add("selected");
        updateCountPanel();
      });
      tbody.appendChild(placeholderTr);
    }

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

      const rows = await fetchByKeys(nextDetail.DetailTable, names, values, nextDetail.OrderByField);

      // 載入下一層 Detail 的資料，同時保留 Focus 聯動功能
      await buildBody(tbody, detailDicts[nextIndex], rows, (row, tr) => {
        tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
        tr.classList.add("selected");
        activeTarget.type = "detail";
        activeTarget.index = nextIndex;
        updateCountPanel();
        selectedBucket.details[nextIndex] = row;
        selectedBucket.lastDetailRow = row;
        selectedBucket.lastDetailIndex = nextIndex;

        // 遞迴：如果還有下一層，繼續聯動
        //   自動串聯只載入資料，不會改寫 activeTarget
        if (cfg.EnableDetailFocusCascade) {
          loadNextDetailFromFocus(nextIndex, row);
        }
      }, true);

      const detailGridEl = document.getElementById(`${cfg.DomId}-detail-${nextIndex}-grid`);
      buildFooter(detailGridEl, detailDicts[nextIndex], rows);

      // ★ 當 detail 沒有資料時，放一列空白佔位列（與 loadAllDetails 一致）
      if (rows.length === 0 && tbody.querySelectorAll("tr").length === 0) {
        const dict = detailDicts[nextIndex] || [];
        const visibleCols = dict.filter(f => (f.Visible ?? 1) === 1).length || 1;
        const placeholderTr = document.createElement("tr");
        placeholderTr.className = "mmd-placeholder-row";
        placeholderTr.style.cursor = "pointer";
        placeholderTr.dataset.placeholder = "1";
        const td = document.createElement("td");
        td.colSpan = visibleCols;
        td.className = "text-center text-muted";
        td.style.padding = "8px";
        td.innerHTML = "<small>（點擊此處後可使用上方 + 按鈕新增資料）</small>";
        placeholderTr.appendChild(td);
        const detailIndex = nextIndex;
        placeholderTr.addEventListener("click", () => {
          activeTarget.type = "detail";
          activeTarget.index = detailIndex;
          tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
          placeholderTr.classList.add("selected");
          updateCountPanel();
        });
        tbody.appendChild(placeholderTr);
      }

      // 編輯模式處理
      if (window._mmdEditing && tbody._editorInstance) {
        if (tbody.offsetParent !== null) {
          tbody._editorInstance.toggleEdit(true);
          tbody._pendingRebind = false;
        } else {
          tbody._pendingRebind = true;
        }
      }

      // ★ 自動串聯後續層級：有資料時用第一筆繼續載入下一層，無資料時清除後續層並顯示 placeholder
      if (rows.length > 0) {
        // 自動選取第一筆，串聯載入下一層
        const firstTr = tbody.querySelector("tr");
        if (firstTr && cfg.EnableDetailFocusCascade) {
          firstTr.classList.add("selected");
          // ★ 不在自動串聯中改寫 activeTarget，讓它只由使用者的實際點擊來設定
          //    這樣按 + 時 getActive() 才能正確回傳使用者真正選取的層級
          selectedBucket.details[nextIndex] = rows[0];
          selectedBucket.lastDetailRow = rows[0];
          selectedBucket.lastDetailIndex = nextIndex;
          await loadNextDetailFromFocus(nextIndex, rows[0]);
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

        const rows = await fetchByKeys(nextDetail.DetailTable, names, values, nextDetail.OrderByField);

        // 載入 SubDetail 的資料（不需要遞迴，因為迴圈已經處理所有層級）
        await buildBody(tbody, detailDicts[nextIndex], rows, (row, tr) => {
          tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
          tr.classList.add("selected");
          activeTarget.type = "detail";
          activeTarget.index = nextIndex;
          updateCountPanel();
          lastDetailRow = row;
          selectedBucket.details[nextIndex] = row;
          selectedBucket.lastDetailRow = row;
          selectedBucket.lastDetailIndex = nextIndex;
          // ★ 不再遞迴聯動，避免串聯式連動
        }, true);

        // ★ 當 detail 沒有資料時，放一列空白佔位列
        if (rows.length === 0 && tbody.querySelectorAll("tr").length === 0) {
          const dict = detailDicts[nextIndex] || [];
          const visibleCols = dict.filter(f => (f.Visible ?? 1) === 1).length || 1;
          const placeholderTr = document.createElement("tr");
          placeholderTr.className = "mmd-placeholder-row";
          placeholderTr.style.cursor = "pointer";
          placeholderTr.dataset.placeholder = "1";
          const td = document.createElement("td");
          td.colSpan = visibleCols;
          td.className = "text-center text-muted";
          td.style.padding = "8px";
          td.innerHTML = "<small>（點擊此處後可使用上方 + 按鈕新增資料）</small>";
          placeholderTr.appendChild(td);
          const detailIndex = nextIndex;
          placeholderTr.addEventListener("click", () => {
            activeTarget.type = "detail";
            activeTarget.index = detailIndex;
            tbody.querySelectorAll('tr').forEach(x => x.classList.remove("selected"));
            placeholderTr.classList.add("selected");
            updateCountPanel();
          });
          tbody.appendChild(placeholderTr);
        }

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
      updateCountPanel();
      lastMasterRow = row;
      selectedBucket.master = row;
      await loadAllDetails(row);
    }, false);

    // ==============================================================================
    //   EditableGrid 初始化（含自動偵測 PK）
    // ==============================================================================

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
            updateCountPanel();
        });
    });

    const getSelectableRows = (tbody) => {
      if (!tbody) return [];
      return Array.from(tbody.querySelectorAll("tr")).filter(r => {
        const text = (r.textContent || "").trim();
        return !text.includes("請點選") && !text.includes("載入中");
      });
    };

    function updateCountPanel() {
      if (!countLabel) return;
      const active = getActive();
      const tbody = active?.tbody;
      const rows = getSelectableRows(tbody);
      const total = rows.length;
      const selected = tbody?.querySelector("tr.selected");
      const idx = selected ? rows.indexOf(selected) + 1 : 0;
      countLabel.textContent = `${idx} / ${total}`;
      if (masterIndicator) {
        masterIndicator.classList.toggle("active", activeTarget.type === "master");
      }
      if (detailIndicators.length) {
        const activeIndex = activeTarget.type === "detail" ? Number(activeTarget.index) : -1;
        detailIndicators.forEach((el) => {
          const raw = (el.id || "").slice(detailIndicatorPrefix.length);
          const idxVal = Number(raw);
          el.classList.toggle("active", idxVal === activeIndex);
        });
      }
    }

    let isDirty = false;

    const setDirty = (dirty) => {
      isDirty = !!dirty;
      if (!window._mmdEditing) return;
      if (btnSave) btnSave.disabled = !isDirty;
      if (btnCancel) btnCancel.disabled = !isDirty;
    };

    const setModePanel = (editing) => {
      if (modeLabel) modeLabel.textContent = editing ? "編輯模式" : "瀏覽模式";
      if (countBox) {
        countBox.classList.toggle("mode-edit", editing);
        countBox.classList.toggle("mode-view", !editing);
      }
      if (btnEdit) {
        btnEdit.innerHTML = editing
          ? '<i class="bi bi-eye"></i>瀏覽'
          : '<i class="bi bi-pencil-square"></i>修改';
      }
      // 按鈕啟用/禁用邏輯
      if (btnAdd) btnAdd.disabled = !editing;
      if (btnDelete) btnDelete.disabled = !editing;
      if (btnSave) btnSave.disabled = !editing || !isDirty;
      if (btnCancel) btnCancel.disabled = !editing || !isDirty;
    };

    const setEditMode = (toEdit) => {
      const on = !!toEdit;
      window._mmdEditing = on;
      masterEditor.toggleEdit(on);
      detailEditors.forEach(ed => ed.toggleEdit(on));
      setModePanel(on);
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

    const navigateRow = (direction) => {
      const t = getActive();
      const tbody = t?.tbody;
      if (!tbody) return;
      const rows = getSelectableRows(tbody);
      if (rows.length === 0) return;
      const current = tbody.querySelector("tr.selected");
      let idx = current ? rows.indexOf(current) : -1;

      switch (direction) {
        case "first": idx = 0; break;
        case "prev": idx = Math.max(0, idx - 1); break;
        case "next": idx = Math.min(rows.length - 1, idx + 1); break;
        case "last": idx = rows.length - 1; break;
      }

      rows.forEach(r => r.classList.remove("selected"));
      if (rows[idx]) {
        rows[idx].classList.add("selected");
        rows[idx].click();
        try { rows[idx].scrollIntoView({ block: "center", behavior: "auto" }); } catch { }
      }
      updateCountPanel();
    };

    let pendingAddedRow = null;
    let pendingAddedTarget = null;

    const normalizeKeyName = (name) => (name || "").toString().trim().toLowerCase();

    const getInputValueByName = (tr, name) => {
      if (!tr || !name) return "";
      const want = normalizeKeyName(name);
      const inputs = tr.querySelectorAll("input");
      for (const inp of inputs) {
        const key = normalizeKeyName(inp.name);
        if (key === want) return inp.value ?? "";
      }
      return "";
    };

    const captureKeyValues = (tr, keyFields) => {
      const values = {};
      (keyFields || []).forEach(k => {
        const v = getInputValueByName(tr, k);
        if (v !== "") values[k] = v;
      });
      return values;
    };

    const findRowByKeys = (tbody, keyValues) => {
      if (!tbody || !keyValues) return null;
      const keys = Object.keys(keyValues);
      if (!keys.length) return null;
      const rows = Array.from(tbody.querySelectorAll("tr"));
      return rows.find(tr => {
        if (!tr.querySelector("input")) return false;
        return keys.every(k => {
          const got = getInputValueByName(tr, k);
          return String(got ?? "").trim() === String(keyValues[k] ?? "").trim();
        });
      }) || null;
    };

    const sortRowsByItemIfNeeded = (rows, keyFields) => {
      const sortKey = Array.isArray(keyFields) && keyFields.length ? keyFields[0] : null;
      if (!sortKey) return rows;
      const getRowValue = (row, field) => {
        if (!row || !field) return "";
        const want = normalizeKeyName(field);
        const hit = Object.keys(row).find(k => normalizeKeyName(k) === want);
        return hit ? row[hit] : "";
      };
      const toNum = (v) => {
        const n = parseFloat(String(v ?? "").trim());
        return Number.isFinite(n) ? n : null;
      };
      return rows.slice().sort((a, b) => {
        const av = getRowValue(a, sortKey);
        const bv = getRowValue(b, sortKey);
        const an = toNum(av);
        const bn = toNum(bv);
        if (an != null && bn != null) return an - bn;
        return String(av ?? "").localeCompare(String(bv ?? ""));
      });
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
      // ★ Master 新增時從 FilterSQL 解析預設值（如 FactorType=104）
      if (target.type === "master" && tbody._filterDefaults) {
        Object.assign(defaults, tbody._filterDefaults);
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

      // ★ FilterSQL 預設欄位：建立 hidden input（如 FactorType=104）
      if (tbody._filterDefaults) {
        const coveredFields = new Set([
          ...pkNames.map(n => n.toLowerCase()),
          ...keyMap.map(k => k.Detail.toLowerCase()),
          ...fields.map(f => f.FieldName.toLowerCase())
        ]);
        Object.entries(tbody._filterDefaults).forEach(([name, val]) => {
          if (!coveredFields.has(name.toLowerCase())) {
            const hid = document.createElement("input");
            hid.type = "hidden";
            hid.name = name;
            hid.value = val;
            hid.className = "cell-edit";
            tr.append(hid);
          }
        });
      }

      fields.forEach(f => {
        const td = document.createElement("td");
        const raw = defaults[f.FieldName] ?? "";

        // ★ 是否顯示為勾選框 (ComboStyle==1)
        const isCheckbox = String(f.ComboStyle ?? "").trim() === "1";
        if (isCheckbox) td.classList.add("text-center", "align-middle");

        const span = document.createElement("span");
        span.className = "cell-view d-none";
        if (isCheckbox) span.classList.add("d-inline-flex", "justify-content-center", "w-100");

        const inp = document.createElement("input");
        inp.name = f.FieldName;

        if (isCheckbox) {
          // ★ 支援多種格式：true/1/"1"/"true"/"True"/bit 欄位
          const checked = raw === true || raw === 1 || raw === "1" ||
                          (typeof raw === "string" && raw.toLowerCase() === "true");
          const viewChk = document.createElement("input");
          viewChk.type = "checkbox";
          viewChk.disabled = true;
          viewChk.tabIndex = -1;
          viewChk.className = "form-check-input checkbox-dark";
          viewChk.checked = checked;
          span.appendChild(viewChk);

          inp.type = "checkbox";
          inp.className = "form-check-input checkbox-dark cell-edit mx-auto";
          inp.checked = checked;
          inp.value = checked ? "1" : "0";
          inp.dataset.raw = inp.value;

          // ★ 同步 viewChk 與 inp 的狀態
          const syncCheckbox = () => {
            inp.value = inp.checked ? "1" : "0";
            inp.dataset.raw = inp.value;
            viewChk.checked = inp.checked;
          };
          inp.addEventListener("change", syncCheckbox);
          inp.addEventListener("click", syncCheckbox);
        } else {
          inp.className = "form-control form-control-sm cell-edit";
          inp.value = (raw ?? "").toString();
          inp.dataset.raw = (raw ?? "").toString();

          // ★ 從快取綁定雙擊下拉選單（與 buildBody 一致）
          const ocxData = getCachedOCXLookup(f);
          const lkData  = getCachedLookup(f);
          const fieldLookupMap = ocxData?.dropdown || lkData?.dropdown;
          const fieldCellMap   = ocxData?.cell    || lkData?.cell;
          if (fieldLookupMap && Object.keys(fieldLookupMap).length > 0) {
            inp._lookupMap     = fieldLookupMap;
            inp._lookupCellMap = fieldCellMap;
            // ★ 記錄條件過濾欄位資訊
            const cond1Field  = (f.LookupCond1Field || '').trim();
            const cond1Source = (f.LookupCond1ResultField || '').trim();
            const cond2Field  = (f.LookupCond2Field || '').trim();
            const cond2Source = (f.LookupCond2ResultField || '').trim();
            const hasCondition = !!(cond1Field || cond2Field);
            const lookupTable  = f.LookupTable || f.OCXLKTableName || '';
            const lookupKey    = f.LookupKeyField || f.KeyFieldName || '';
            const lookupResult = f.LookupResultField || f.OCXLKResultName || '';
            // ★ 非實體 lookup 欄位（有 KeySelfName）：編輯模式下允許從下拉選單選取
            const keySelfName2 = (f.KeySelfName || '').trim();
            td.addEventListener('dblclick', async (e) => {
              e.stopPropagation();
              let isReadOnly = !window._mmdEditing || inp.readOnly || inp.disabled;
              let onSelect = null;
              if (isReadOnly && window._mmdEditing && keySelfName2 && !inp.disabled) {
                isReadOnly = false;
                onSelect = (key, label) => {
                  inp.value = label;
                  inp.dataset.raw = key;
                  const span = td.querySelector('.cell-view');
                  if (span) { span.textContent = label; span.dataset.raw = key; }
                  const tr = td.closest('tr');
                  if (tr) {
                    const kfLower = keySelfName2.toLowerCase();
                    const keyInp = Array.from(tr.querySelectorAll('input'))
                      .find(i => (i.name || '').toLowerCase() === kfLower);
                    if (keyInp) {
                      keyInp.value = key;
                      keyInp.dataset.raw = key;
                      const keyTd = keyInp.closest('td');
                      const keyView = keyTd?.querySelector('.cell-view');
                      if (keyView) { keyView.textContent = key; keyView.dataset.raw = key; }
                      keyInp.dispatchEvent(new Event('input', { bubbles: true }));
                      keyInp.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                  }
                };
              }
              // ★ 有條件過濾時，即時查詢 API 取得過濾後的資料
              if (hasCondition && lookupTable && lookupKey && lookupResult) {
                const currentTr = td.closest('tr');
                const getCondVal = (sourceField) => {
                  if (!sourceField || !currentTr) return '';
                  const sfLower = sourceField.toLowerCase();
                  // ★ 從當前行的 td[data-field] 找 cell-edit 的 dataset.raw（原始 key 值）
                  const tdCell = Array.from(currentTr.querySelectorAll('td[data-field]'))
                    .find(t => (t.dataset.field || '').toLowerCase() === sfLower);
                  if (tdCell) {
                    const cellInp = tdCell.querySelector('.cell-edit');
                    if (cellInp) {
                      const v = (cellInp.dataset?.raw ?? cellInp.value ?? '').trim();
                      if (v) return v;
                    }
                  }
                  // 再找隱藏 input（PK/FK hidden）
                  const found = Array.from(currentTr.querySelectorAll('input[type="hidden"]'))
                    .find(i => (i.name || '').toLowerCase() === sfLower);
                  if (found && (found.value ?? '').trim()) return found.value.trim();
                  // ★ 從當前行的 cell-edit input 找 dataset.raw（新增行沒有 data-field，用 name 找）
                  const anyInp = Array.from(currentTr.querySelectorAll('input.cell-edit'))
                    .find(i => (i.name || '').toLowerCase() === sfLower);
                  if (anyInp) {
                    const v = (anyInp.dataset?.raw ?? anyInp.value ?? '').trim();
                    if (v) return v;
                  }
                  // ★ 從 selectedBucket 已選取行取值（同層 → 父層 → Master）
                  const lookupInRow = (rowData) => {
                    if (!rowData) return '';
                    const rk = Object.keys(rowData).find(k => k.toLowerCase() === sfLower);
                    return rk && rowData[rk] != null ? String(rowData[rk]).trim() : '';
                  };
                  const selfVal = lookupInRow(selectedBucket.details[target.index]);
                  if (selfVal) return selfVal;
                  for (let pi = (target.index ?? 0) - 1; pi >= 0; pi--) {
                    const pv = lookupInRow(selectedBucket.details[pi]);
                    if (pv) return pv;
                  }
                  const mv = lookupInRow(selectedBucket.master);
                  if (mv) return mv;
                  return '';
                };
                const c1Val = getCondVal(cond1Source);
                const c2Val = getCondVal(cond2Source);
                let url = `/api/TableFieldLayout/LookupData?table=${encodeURIComponent(lookupTable)}&key=${encodeURIComponent(lookupKey)}&result=${encodeURIComponent(lookupResult)}`;
                if (cond1Field && c1Val) url += `&cond1Field=${encodeURIComponent(cond1Field)}&cond1Value=${encodeURIComponent(c1Val)}`;
                if (cond2Field && c2Val) url += `&cond2Field=${encodeURIComponent(cond2Field)}&cond2Value=${encodeURIComponent(c2Val)}`;
                try {
                  const res = await fetch(url);
                  if (!res.ok) return;
                  const data = await res.json();
                  const filteredMap = {};
                  (data || []).forEach(row => {
                    const k = String(row.key ?? '');
                    if (!k) return;
                    filteredMap[k] = combineResults(row);
                  });
                  if (Object.keys(filteredMap).length > 0) {
                    showLookupDropdown(inp, td, filteredMap, isReadOnly, onSelect);
                  }
                } catch { }
                return;
              }
              showLookupDropdown(inp, td, inp._lookupMap, isReadOnly, onSelect);
            });
          }
        }

        if (DICT.readonly(f) || f.KeySelfName || (DICT.isKey(f) && tr.dataset.state !== "added")) {
          inp.readOnly = true;
          td.classList.add("mmd-readonly-cell");
        }

        td.append(span);
        td.append(inp);
        if (target.type === "detail" && isDateType(DICT.type(f))) {
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

      // ★ 移除佔位列（如果有的話）
      tbody.querySelectorAll('tr[data-placeholder="1"]').forEach(p => p.remove());

      clearSelected(tbody);
      tr.classList.add("selected");
      // ★ Master 新增列插入在最前面，Detail 仍 append 在最後
      if (target.type === "master") {
        tbody.prepend(tr);
      } else {
        tbody.appendChild(tr);
      }
      updateCountPanel();
      pendingAddedRow = tr;
      pendingAddedTarget = { type: target.type, index: target.index };

      try { tr.scrollIntoView({ block: "nearest" }); } catch { }
      const firstEditable = Array.from(tr.querySelectorAll("input.cell-edit"))
        .find(i => !i.readOnly && i.getAttribute("readonly") !== "readonly");
      if (firstEditable) {
        try { firstEditable.focus(); } catch { }
        try { firstEditable.select?.(); } catch { }
      }
      return tr;
    };

    // 掛載 GRID 鍵盤導航（兩段式點擊、方向鍵、F7/F8、Enter/Escape）
    if (typeof window.initErpGridNav === 'function') {
      window.initErpGridNav(mBody, {
        keyFields : masterPK.map(k => k.toLowerCase()),
        isEditMode: () => !!window._mmdEditing,
        addRow    : null,
        autoSave  : async () => {
          const r = await masterEditor.saveChanges();
          if (r?.ok && !r?.skipped) setDirty(false);
        },
        onRowSelect: (tr) => { if (tr) tr.click(); },
        gridLabel : `${cfg.DomId}-master`,
      });
      for (let _i = 0; _i < (cfg.Details || []).length; _i++) {
        const _dBody = root.querySelector(`#${cfg.DomId}-detail-${_i}-body`);
        if (!_dBody) continue;
        window.initErpGridNav(_dBody, {
          keyFields : (detailKeyFields[_i] || []).map(k => k.toLowerCase()),
          isEditMode: () => !!window._mmdEditing,
          addRow    : async () => {
            const t = { type: 'detail', index: _i, tbody: _dBody, editor: detailEditors[_i], keyFields: detailKeyFields[_i], dict: detailDicts[_i] };
            const row = addRowTo(t);
            return row ? { ok: true } : { ok: false };
          },
          autoSave  : async () => {
            const r = await detailEditors[_i].saveChanges();
            if (r?.ok && !r?.skipped) setDirty(false);
          },
          onRowSelect: (tr) => { if (tr) tr.click(); },
          gridLabel : `${cfg.DomId}-detail-${_i}`,
        });
      }
    }

    // ★ 儲存/刪除後動態刷新資料，並選回前幾階的資料
    //   根據 cascadeMode 決定刷新策略：
    //   Mode 0（預設）：所有 Detail 都從 Master 取 key → 重新載入所有明細即可
    //   Mode 1（串聯）：Detail[N]←Detail[N-1] → 從前一階的已選資料重新載入
    //   Mode 2（扇出）：Detail[1]+←Detail[0] → Detail[0]變更時全部重載，其他時從 Detail[0] 重載
    const reloadAfterChange = async (changedDetailIndex) => {
      if (!lastMasterRow) return;
      if (changedDetailIndex < 0) { location.reload(); return; }

      // 串聯式 (Mode 1) 且非第一階：從前一階重新載入即可，前幾階保持選取
      if (cascadeMode === 1 && changedDetailIndex > 0) {
        const parentRow = selectedBucket.details[changedDetailIndex - 1];
        if (parentRow) {
          await loadNextDetailFromFocus(changedDetailIndex - 1, parentRow);
          activeTarget.type = "detail";
          activeTarget.index = changedDetailIndex - 1;
          updateCountPanel();
          return;
        }
      }

      // 扇出式 (Mode 2) 且非 Detail[0]：從 Detail[0] 的已選資料重新載入
      if (cascadeMode === 2 && changedDetailIndex > 0) {
        const detail0Row = selectedBucket.details[0];
        if (detail0Row) {
          await loadAllSubDetailsFromFocus(0, detail0Row);
          activeTarget.type = "detail";
          activeTarget.index = 0;
          updateCountPanel();
          return;
        }
      }

      // 其他情況（Mode 0、第一階變更、或找不到前一階資料）：重新載入所有明細
      await loadAllDetails(lastMasterRow);
      activeTarget.type = "master";
      activeTarget.index = -1;
      updateCountPanel();
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
        const errMsgs = [];
        if (!rMaster.ok && rMaster.text) errMsgs.push(rMaster.text);
        rDetails.forEach(r => {
          if (!r.ok && r.text) errMsgs.push(r.text);
        });
        const errText = errMsgs.length ? errMsgs.join("\n") : "部分明細未成功";
        Swal?.fire({ icon: "error", title: "儲存失敗", html: `<pre style="text-align:left;white-space:pre-wrap;font-size:20px;">${errText}</pre>` });
        return { ok: false, skipped: false };
      }

      Swal?.fire({ icon: "success", title: "儲存完成", timer: 900, showConfirmButton: false });
      // ★ 儲存成功後：退出編輯模式、清除異動狀態
      const savedTarget = pendingAddedTarget;
      // ★ 若有新增 Master 列，在 reload 前先捕捉 PK 值以便重新選取
      const addedMasterKeys = (pendingAddedRow && (!savedTarget || savedTarget.type === "master"))
        ? captureKeyValues(pendingAddedRow, masterPK)
        : null;
      masterEditor.cancelChanges();
      detailEditors.forEach(ed => ed.cancelChanges());
      setEditMode(false);
      setDirty(false);
      pendingAddedRow = null;
      pendingAddedTarget = null;

      // ★ 動態刷新資料並選回前幾階
      if (savedTarget && savedTarget.type === "detail") {
        await reloadAfterChange(savedTarget.index);
      } else if (!rMaster.skipped) {
        // Master 資料有異動，重新載入頁面（帶上新增列的 PK 以便重新選取）
        const url = new URL(location.href);
        if (addedMasterKeys && Object.keys(addedMasterKeys).length) {
          Object.entries(addedMasterKeys).forEach(([k, v]) => url.searchParams.set(k, v));
          url.searchParams.set("pageIndex", "1");
        }
        location.href = url.toString();
      }

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
        updateCountPanel();
        // 若已無其他待儲存列，則重設 dirty
        if (!root.querySelector('tbody tr[data-state]')) setDirty(false);
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
      setDirty(false);
      updateCountPanel();
      // ★ 動態刷新資料並選回前幾階
      if (target.type === "master") {
        location.reload();
      } else if (target.type === "detail") {
        await reloadAfterChange(target.index);
      }
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
      deleteSubDetailRow,
      saveAll,
      get isDirty() { return isDirty; }
    };

    btnFirst?.addEventListener("click", () => navigateRow("first"));
    btnPrev?.addEventListener("click", () => navigateRow("prev"));
    btnNext?.addEventListener("click", () => navigateRow("next"));
    btnLast?.addEventListener("click", () => navigateRow("last"));

    // 查詢功能（若未提供 ItemId 則停用）
    const itemId = cfg.ItemId || window._mmdItemId || "";
    const masterTable = cfg.MasterTable || cfg.Master || "";
    let queryFieldsLoaded = false;

    if (queryBtn && (!itemId || !masterTable)) {
      queryBtn.disabled = true;
      queryBtn.title = "未設定 ItemId";
    }

    async function loadQueryFields() {
      if (queryFieldsLoaded) return;
      if (!itemId || !masterTable) return;
      try {
        const res = await fetch(`/api/DictSetupApi/QueryFields?itemId=${encodeURIComponent(itemId)}&table=${encodeURIComponent(masterTable)}&lang=TW`);
        if (!res.ok) throw new Error(await res.text());
        const data = await res.json();
        const list = Array.isArray(data) && data.length ? data : [];
        buildQueryForm(list);
        queryFieldsLoaded = true;
      } catch (err) {
        console.warn("load query fields failed", err);
      }
    }

    function buildQueryForm(fields) {
      if (!queryForm) return;
      queryForm.innerHTML = "";

      fields.forEach(f => {
        const colName = f.columnName || f.ColumnName || "";
        if (!colName) return;
        const caption = f.columnCaption || f.ColumnCaption || colName;
        const defVal = f.defaultValue || f.DefaultValue || "";
        const defEq = (f.defaultEqual || f.DefaultEqual || "").toString().toLowerCase();
        const dtRaw = f.dataType ?? f.DataType;
        const dt = Number(dtRaw);

        const wrap = document.createElement("div");
        wrap.className = "col-md-4 col-sm-6";
        const label = document.createElement("label");
        label.className = "form-label text-muted";
        label.textContent = caption;
        const input = document.createElement("input");
        input.className = "form-control form-control-sm";
        input.name = colName;
        if (dt === 1) input.type = "date";
        else if (dt === 2) input.type = "number";
        else input.type = "text";
        if (defVal) input.value = defVal;
        if (defEq === "like") input.placeholder = "模糊查詢";

        wrap.appendChild(label);
        wrap.appendChild(input);
        queryForm.appendChild(wrap);
      });
    }

    queryBtn?.addEventListener("click", async () => {
      await loadQueryFields();
      if (queryModalEl && window.bootstrap) {
        const modal = bootstrap.Modal.getOrCreateInstance(queryModalEl);
        modal.show();
      }
    });

    btnClearQuery?.addEventListener("click", () => {
      if (queryForm) {
        Array.from(queryForm.elements).forEach(el => {
          if (el.tagName === "INPUT") el.value = "";
        });
      }
    });

    btnQuerySubmit?.addEventListener("click", () => {
      if (!queryForm) return;
      const url = new URL(window.location.href);
      Array.from(queryForm.elements).forEach(el => {
        if (el.tagName === "INPUT" && el.name) {
          const val = el.value?.trim() ?? "";
          if (val) url.searchParams.set(el.name, val);
          else url.searchParams.delete(el.name);
        }
      });
      url.searchParams.set("pageIndex", "1");
      window.location.href = url.toString();
    });

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
          if (row) setDirty(true);
          return row || false;
        },
        customSave: async () => { await saveAll(); },
        onDelete: async () => { await deleteSelected(); },
        onCancelPending: (row) => { row?.remove?.(); },
        onCancelEdit: async () => {
          masterEditor.cancelChanges();
          detailEditors.forEach(ed => ed.cancelChanges());
          setEditMode(false);
          setDirty(false);
        }
      });
    } else {
      let pendingRow = null;

      btnEdit?.addEventListener("click", () => setEditMode(!masterEditor.isEdit()));
      btnAdd?.addEventListener("click", () => {
        ensureEditMode();
        if (pendingRow?.remove) pendingRow.remove();
        const row = addRowTo(getActive());
        if (row) {
          pendingRow = row;
          setDirty(true);
        }
      });
      btnDelete?.addEventListener("click", async () => { await deleteSelected(); });
      btnSave?.addEventListener("click", async () => { await saveAll(); pendingRow = null; });
      btnCancel?.addEventListener("click", async () => {
        if (pendingRow?.remove) {
          pendingRow.remove();
          pendingRow = null;
          return;
        }
        masterEditor.cancelChanges();
        detailEditors.forEach(ed => ed.cancelChanges());
        setEditMode(false);
        setDirty(false);
      });
    }

    // 監聽 input/change 事件追蹤異動狀態
    root.addEventListener('input', (e) => {
      if (e.target.closest('.cell-edit')) setDirty(true);
    });
    root.addEventListener('change', (e) => {
      const inp = e.target.closest('.cell-edit');
      if (inp) {
        setDirty(true);
        // ★ 動態刷新：當 key 欄位變更時，更新對應的 lookup 中文名稱
        const tr = inp.closest('tr');
        const tbody = tr?.closest('tbody');
        const keyToLookup = tbody?._keyToLookup;
        const changedField = (inp.name || '').toLowerCase();
        if (tr && keyToLookup) {
          const deps = keyToLookup[changedField];
          if (deps && deps.length > 0) {
            const getFieldVal = (fieldName) => {
              const fn = fieldName.toLowerCase();
              // 優先從 cell-edit input 取值（含剛修改的值），避免讀到 hidden PK 的舊值
              const cellEdit = Array.from(tr.querySelectorAll('input.cell-edit'))
                .find(i => (i.name || '').toLowerCase() === fn);
              if (cellEdit) return (cellEdit.dataset?.raw ?? cellEdit.value ?? '').trim();
              const target = Array.from(tr.querySelectorAll('input'))
                .find(i => (i.name || '').toLowerCase() === fn);
              return target ? (target.value ?? '').trim() : '';
            };
            for (const dep of deps) {
              // 跳過自我參照（避免覆蓋使用者剛選的值）
              if (dep.resultFieldName.toLowerCase() === changedField) continue;
              let lookupKey = '';
              if (dep.compositeKeySelfNames && dep.compositeKeySelfNames.length > 1) {
                const parts = dep.compositeKeySelfNames.map(f => getFieldVal(f));
                if (parts.every(p => p !== '')) lookupKey = parts.join(COMPOSITE_KEY_SEP);
              } else if (dep.keySelf) {
                lookupKey = getFieldVal(dep.keySelf);
              }
              const displayVal = lookupKey ? (dep.ocxData.cell[lookupKey] || dep.ocxData.cell[lookupKey.trim()] || dep.ocxData.cell[lookupKey.trim().toLowerCase()] || '') : '';
              const resultInp = Array.from(tr.querySelectorAll('input'))
                .find(i => (i.name || '').toLowerCase() === dep.resultFieldName.toLowerCase());
              if (resultInp) {
                resultInp.value = displayVal;
                const td = resultInp.closest('td');
                const view = td?.querySelector('.cell-view');
                if (view) view.textContent = displayVal;
              }
            }
          }
        }
      }
    });

    // ★ 頁面載入後：依 URL pageIndex 或 PK 參數自動選取對應的 Master 列
    {
      const urlP = new URLSearchParams(window.location.search);
      const pageIdx = parseInt(urlP.get("pageIndex") || urlP.get("pageindex") || "0", 10);
      const rows = Array.from(mBody.querySelectorAll("tr"));
      let targetRow = null;

      // 優先用 PK 欄位值精確比對
      if (masterKeyFields.length > 0) {
        const pkVals = {};
        masterKeyFields.forEach(k => {
          const v = urlP.get(k);
          if (v) pkVals[k] = v;
        });
        if (Object.keys(pkVals).length > 0) {
          targetRow = rows.find(tr => {
            return Object.entries(pkVals).every(([k, v]) => {
              const inp = Array.from(tr.querySelectorAll("input")).find(i => (i.name || "").toLowerCase() === k.toLowerCase());
              return inp && String(inp.value ?? "").trim() === String(v).trim();
            });
          }) || null;
        }
      }

      // 否則用 pageIndex（1-based）
      if (!targetRow && pageIdx > 0 && pageIdx <= rows.length) {
        targetRow = rows[pageIdx - 1];
      }

      // 預設選第一列
      if (!targetRow && rows.length > 0) {
        targetRow = rows[0];
      }

      if (targetRow) {
        targetRow.click();
        try { targetRow.scrollIntoView({ block: "center", behavior: "auto" }); } catch {}
      }
    }

    setModePanel(false);
    setDirty(false);
    updateCountPanel();
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
