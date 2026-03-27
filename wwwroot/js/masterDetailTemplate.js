// wwwroot/js/masterDetailTemplate.js
(() => {

  // -----------------------------
  // 🧩 全域 Lookup 快取（一般 Lookup）
  // -----------------------------
  const LOOKUP_CACHE = {};

  async function loadLookup(f) {
    const key = `${f.LookupTable}|${f.LookupKeyField}|${f.LookupResultField}`;

    if (LOOKUP_CACHE[key]) return LOOKUP_CACHE[key];

    if (!f.LookupTable || !f.LookupKeyField || !f.LookupResultField) {
      return (LOOKUP_CACHE[key] = null);
    }

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
    return (LOOKUP_CACHE[key] = { cell, dropdown });
  }

  // -----------------------------
  // 🧩 OCX Lookup（第二層，非實體欄位用）
  // -----------------------------
  const OCX_CACHE = {};
  const COMPOSITE_KEY_SEP = '\x1f';

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

  async function loadOCXLookup(f) {
    const composite = parseKeyMaps(f);
    const compositeKeyFields = composite ? composite.keyFieldNames.join(',') : null;
    const effectiveKeyField = compositeKeyFields || f.KeyFieldName;

    const key = `${f.OCXLKTableName}|${effectiveKeyField}|${f.OCXLKResultName}`;

    if (OCX_CACHE[key]) return OCX_CACHE[key];

    if (!f.OCXLKTableName || !effectiveKeyField || !f.OCXLKResultName) {
      return (OCX_CACHE[key] = null);
    }

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
    if (composite) {
      result._compositeKeySelfNames = composite.keySelfNames;
    }
    return (OCX_CACHE[key] = result);
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

  // -----------------------------
  // 🧩 Dictionary Helper
  // -----------------------------
  const DICT_MAP = {
    fieldName: f => f.FieldName,
    headerText: f => f.DisplayLabel || f.FieldName,
    order: f => f.SerialNum ?? 99999,
    width: f => {
      const n = Number(f.DisplaySize || f.iFieldWidth || 0);
      return n > 0 ? n * 10 : null;   // 一字寬 10px
    },
    visible: f => (f.Visible ?? 1) == 1,
    fmt: f => f.FormatStr || null,
    dataType: f => f.DataType || null,
    readonly: f => (f.ReadOnly ?? 0) == 1
  };

  // -----------------------------
  // 🧩 Lookup Dropdown（雙擊下拉選單）
  // -----------------------------
  function ensureLookupDropdownCss() {
    if (document.getElementById('md-lookup-dd-css')) return;
    const style = document.createElement('style');
    style.id = 'md-lookup-dd-css';
    style.textContent = `
.md-lookup-dd{background:#fff;border:1px solid #b0c4de;border-radius:6px;box-shadow:0 4px 18px rgba(0,0,0,.18);max-height:300px;display:flex;flex-direction:column;overflow:hidden;font-size:.85rem}
.md-lookup-search{border:none;border-bottom:1px solid #dde6f0;padding:5px 9px;outline:none;font-size:.85rem;min-width:120px}
.md-lookup-list{overflow-y:auto;max-height:252px}
.md-lookup-item{padding:4px 10px;cursor:pointer;white-space:nowrap}
.md-lookup-item:hover,.md-lookup-item:focus{background:#e8f0ff;outline:none}
.md-lookup-item.selected{background:#d0e4ff;font-weight:500}
.md-lookup-dd[data-readonly="1"] .md-lookup-item{cursor:default;color:#555}
.md-lookup-dd[data-readonly="1"] .md-lookup-item:hover,.md-lookup-dd[data-readonly="1"] .md-lookup-item:focus{background:#f5f5f5}
`;
    document.head.appendChild(style);
  }

  function showLookupDropdown(inp, td, lookupMap, readOnly) {
    ensureLookupDropdownCss();
    const existing = document.getElementById('md-lookup-dd');
    if (existing) existing.remove();

    const entries = Object.entries(lookupMap);
    if (!entries.length) return;

    const currentRaw = readOnly
      ? (td.querySelector('.cell-view')?.dataset?.raw ?? inp.dataset.raw ?? "")
      : (inp.dataset.raw ?? "");

    const dd = document.createElement('div');
    dd.id = 'md-lookup-dd';
    dd.className = 'md-lookup-dd';
    if (readOnly) dd.dataset.readonly = '1';

    const search = document.createElement('input');
    search.type = 'text';
    search.placeholder = readOnly ? '搜尋（唯讀）…' : '搜尋…';
    search.className = 'md-lookup-search';
    dd.appendChild(search);

    const list = document.createElement('div');
    list.className = 'md-lookup-list';
    dd.appendChild(list);

    const renderItems = (filter) => {
      list.innerHTML = '';
      const q = (filter || '').toLowerCase();
      entries.forEach(([key, label]) => {
        if (q && !label.toLowerCase().includes(q) && !key.toLowerCase().includes(q)) return;
        const item = document.createElement('div');
        item.className = 'md-lookup-item';
        item.tabIndex = -1;
        item.textContent = label;
        item.dataset.key = key;
        if (String(currentRaw) === String(key)) item.classList.add('selected');
        if (!readOnly) {
          item.addEventListener('mousedown', (e) => {
            e.preventDefault();
            inp.value = key;
            inp.dataset.raw = key;
            const span = td.querySelector('.cell-view');
            if (span) {
              span.dataset.raw = key;
              span.textContent = inp._lookupCellMap?.[key] ?? label;
            }
            inp.dispatchEvent(new Event('input', { bubbles: true }));
            inp.dispatchEvent(new Event('change', { bubbles: true }));
            dd.remove();
          });
        }
        list.appendChild(item);
      });
    };

    renderItems('');
    search.addEventListener('input', () => renderItems(search.value));

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
        const first = list.querySelector('.md-lookup-item');
        if (first) first.focus();
        e.preventDefault();
      }
    });

    list.addEventListener('keydown', (e) => {
      const focused = document.activeElement;
      if (e.key === 'ArrowDown') { focused?.nextElementSibling?.focus(); e.preventDefault(); }
      else if (e.key === 'ArrowUp') { (focused?.previousElementSibling ?? search).focus(); e.preventDefault(); }
      else if (e.key === 'Enter') { focused?.dispatchEvent(new MouseEvent('mousedown', { bubbles: true })); e.preventDefault(); }
      else if (e.key === 'Escape') { dd.remove(); inp.focus(); }
    });
  }

  // -----------------------------
  // 🧩 欄寬存取（從辭典讀取初始寬度）
  // -----------------------------
  const normalizeTableName = (name) => (name || "").replace(/^dbo\./i, "").trim().toLowerCase();
  const savedWidthKey = (table) => `colwidth:${normalizeTableName(table)}`;

  const loadSavedWidthMap = (table) => {
    try {
      const raw = localStorage.getItem(savedWidthKey(table));
      if (!raw) return {};
      const arr = JSON.parse(raw) || [];
      const map = {};
      arr.forEach(c => {
        const k = (c.fieldName || "").toLowerCase();
        if (k && c.width) map[k] = Number(c.width);
      });
      return map;
    } catch { return {}; }
  };

  const enableColumnResize = (tableEl, tableName) => {
    if (!tableEl || !tableName) return;
    const ths = Array.from(tableEl.querySelectorAll("thead th"));
    if (!ths.length) return;

    const saved = loadSavedWidthMap(tableName);
    ths.forEach(th => {
      const k = (th.dataset.field || "").toLowerCase();
      const w = saved[k];
      if (w) th.style.width = `${w}px`;
      if (!th.querySelector(".md-col-resizer")) {
        const handle = document.createElement("span");
        handle.className = "md-col-resizer";
        th.appendChild(handle);
      }
    });

    let isDown = false, startX = 0, startW = 0, th = null;

    ths.forEach(h => {
      const handle = h.querySelector(".md-col-resizer");
      if (!handle) return;
      handle.addEventListener("mousedown", (e) => {
        e.preventDefault();
        isDown = true;
        th = h;
        startX = e.pageX;
        startW = th.getBoundingClientRect().width;
        document.body.classList.add("resizing");
        th.classList.add("resizing");
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
      document.body.classList.remove("resizing");
      th?.classList.remove("resizing");
      // 欄寬不自動儲存，按 F9 統一儲存順序及寬度
      th = null;
    });
  };

  // -----------------------------
  // 🧩 日期 / 數字格式化
  // -----------------------------
  const fmtCell = (val, fmt, dataType) => {
    if (val == null || val === "") return "";

    if (dataType && String(dataType).toLowerCase().includes("date")) {
      const d = new Date(val);
      if (isNaN(d)) return String(val);
      if (fmt) {
        return fmt
          .replace("yyyy", String(d.getFullYear()))
          .replace("MM", String(d.getMonth() + 1).padStart(2, "0"))
          .replace("dd", String(d.getDate()).padStart(2, "0"))
          .replace("HH", String(d.getHours()).padStart(2, "0"))
          .replace("mm", String(d.getMinutes()).padStart(2, "0"))
          .replace("ss", String(d.getSeconds()).padStart(2, "0"));
      }
      return d.toISOString().slice(0, 10).replace(/-/g, "/");
    }

    if (typeof val === "number") {
      if (fmt) {
        if (fmt.includes(".000")) return val.toFixed(3);
        if (fmt.includes(".00")) return val.toFixed(2);
      }
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
    if (!td || !inp) return;
    td.style.position = "relative";

    const picker = document.createElement("input");
    picker.type = "date";
    picker.className = "date-picker-input";
    picker.style.position = "absolute";
    picker.style.opacity = "0";
    picker.style.pointerEvents = "none";
    picker.style.width = "1px";
    picker.style.height = "1px";

    inp.addEventListener("click", () => {
      if (!window._mdEditing || inp.readOnly) return;
      picker.value = toDateInputValue(inp.value);
      if (picker.showPicker) picker.showPicker();
      else picker.focus();
    });

    picker.addEventListener("change", () => {
      const v = toDisplayDate(picker.value);
      inp.value = v;
      inp.dataset.raw = v;
    });

    td.appendChild(picker);
  };

  // -----------------------------
  // 🧩 建立表頭
  // -----------------------------
  const buildHead = (theadTr, dict, showRowNo, tableName) => {
    theadTr.innerHTML = "";
    const savedWidth = loadSavedWidthMap(tableName);

    dict
      .filter(DICT_MAP.visible)
      .sort((a, b) => DICT_MAP.order(a) - DICT_MAP.order(b))
      .forEach(f => {
        const th = document.createElement("th");
        th.textContent = DICT_MAP.headerText(f);
        th.dataset.field = f.FieldName;

        const w = savedWidth[(f.FieldName || "").toLowerCase()] || DICT_MAP.width(f);
        if (w) th.style.width = w + "px";

        const handle = document.createElement("span");
        handle.className = "md-col-resizer";
        th.appendChild(handle);

        theadTr.appendChild(th);
      });
  };

  // -----------------------------
  // 🧩 建立表身 (含 Lookup + OCX)
  // -----------------------------
  const buildBody = async (tbody, dict, rows, showRowNo, onRowClick, cfg, keyFields = [], isEditMode = false, isDetail = false) => {
    tbody.innerHTML = "";

    const fields = dict
      .filter(f => (f.IsKey ?? 0) === 1 || DICT_MAP.visible(f)) // include keys even if not visible
      .sort((a, b) => DICT_MAP.order(a) - DICT_MAP.order(b));

    // 先把所有欄位的 Lookup / OCX map 都載完（各欄位只打一次 API）
    const lookupMaps = {};
    const ocxMaps = {};

    for (const f of fields) {
      lookupMaps[f.FieldName] = await loadLookup(f);
      ocxMaps[f.FieldName]    = await loadOCXLookup(f);
    }

    rows.forEach((row, idx) => {
      const tr = document.createElement("tr");
      tr.__rowData = row;
      tr.style.cursor = "pointer";
      if (row && row.__state) {
        tr.dataset.state = row.__state;
        if (row.__state === "added") tr.classList.add("table-warning");
      }

      const isNewRow = (row && row.__state === "added");

        fields.forEach(f => {

          const col = f.FieldName;

          // 取得原始資料
          let raw = row[col];
          if (raw == null) raw = ""; // 避免 undefined/null 顯示

          // 非實體欄位 → 改抓 KeySelfName
          if ((raw == null || raw === "") && f.KeySelfName) {
              raw = row[f.KeySelfName];
          }

          let display = raw;

          // OCX Lookup（優先）— 支援複合鍵
          const ocxData = ocxMaps[col];
          if (ocxData?._compositeKeySelfNames) {
              const parts = ocxData._compositeKeySelfNames.map(n => String(row[n] ?? "").trim());
              if (parts.every(p => p !== "")) {
                  const compositeKey = parts.join(COMPOSITE_KEY_SEP);
                  if (ocxData.cell?.[compositeKey] != null) display = ocxData.cell[compositeKey];
              }
          } else if (ocxData?.cell?.[raw] != null) {
              display = ocxData.cell[raw];
          }
          // 一般 Lookup（次之）
          else if (lookupMaps[col]?.cell?.[raw] != null) {
              display = lookupMaps[col].cell[raw];
          }

          // 建立 TD
          const td = document.createElement("td");
          td.dataset.field = col;

          // 是否顯示為勾選框 (ComboStyle==1)
          const isCheckbox = String(f.ComboStyle ?? "").trim() === "1";
          if (isCheckbox) td.classList.add("text-center", "align-middle");

          // 顯示欄位
          const span = document.createElement("span");
          span.className = "cell-view";
          span.dataset.raw = raw == null ? "" : String(raw);
          if (isCheckbox) span.classList.add("d-inline-flex", "justify-content-center", "w-100");

          // 編輯欄位 input
          const inp = document.createElement("input");
          inp.className = isCheckbox ? "form-check-input checkbox-dark cell-edit d-none mx-auto" : "form-control form-control-sm cell-edit d-none";
          inp.name = col;

          if (isCheckbox) {
              const checked = raw === true || raw === 1 || raw === "1";
              const chkSize = "width:16px;height:16px;min-width:16px;min-height:16px;max-width:16px;max-height:16px;";
              const viewChk = document.createElement("input");
              viewChk.type = "checkbox";
              viewChk.disabled = true;
              viewChk.tabIndex = -1;
              viewChk.className = "form-check-input checkbox-dark";
              viewChk.style.cssText = chkSize;
              viewChk.checked = checked;
              span.appendChild(viewChk);

              inp.type = "checkbox";
              inp.style.cssText = chkSize;
              inp.checked = checked;
              inp.value = checked ? "1" : "0";
              inp.dataset.raw = inp.value;
              inp.addEventListener("change", () => {
                  inp.value = inp.checked ? "1" : "0";
                  inp.dataset.raw = inp.value;
                  viewChk.checked = inp.checked;
              });
          } else {
              const valText = display == null ? "" : fmtCell(display, DICT_MAP.fmt(f), DICT_MAP.dataType(f));
              span.textContent = valText;
              // 日期欄位：inp.value 也使用格式化後的值（不含多餘的時分秒）
              if (isDateType(DICT_MAP.dataType(f))) {
                inp.value = valText;
              } else {
                inp.value = display == null ? "" : display;
              }
              inp.dataset.raw = raw == null ? "" : raw;

              // ★ 若有 lookup 對照表，記錄並綁定雙擊下拉（編輯模式可選取，瀏覽模式唯讀）
              const fieldDropdownMap = ocxMaps[col]?.dropdown || lookupMaps[col]?.dropdown;
              const fieldCellMap     = ocxMaps[col]?.cell    || lookupMaps[col]?.cell;
              if (fieldDropdownMap && Object.keys(fieldDropdownMap).length > 0) {
                inp._lookupMap     = fieldDropdownMap;
                inp._lookupCellMap = fieldCellMap;
                td.addEventListener('dblclick', (e) => {
                  e.stopPropagation();
                  const isReadOnly = !window._mdEditing || inp.readOnly || inp.disabled;
                  showLookupDropdown(inp, td, inp._lookupMap, isReadOnly);
                });
              }
          }

          // Lookup 或 Readonly → 灰底且不可編輯
         // ---- 是否唯讀（非實體 lookup + 辭典唯讀）----
          const isVirtualLookup = !!f.KeySelfName;
          // 新增列：只要不是虛擬欄位，一律允許編輯（含原本 readonly 欄位）
          let ro = isNewRow
            ? isVirtualLookup
            : (DICT_MAP.readonly(f) || isVirtualLookup);

          // 記錄 readonly 屬性給 editableGrid 用
          inp.dataset.readonly = ro ? "1" : "0";

          if (ro) {
            inp.readOnly = true;
            inp.classList.add("readonly-cell");   // 灰底
          } else {
            inp.readOnly = false;
            inp.classList.remove("readonly-cell");
          }

          if (isEditMode && inp.dataset.readonly !== "1") {
            span.classList.add("d-none");
            inp.classList.remove("d-none");
          }

          td.append(span, inp);
          if (isDetail && isDateType(DICT_MAP.dataType(f))) {
            attachDatePicker(td, inp);
          }
          tr.appendChild(td);
      });

      const renderedInputNames = new Set(
        Array.from(tr.querySelectorAll("input[name]"))
          .map(i => (i.name || "").toLowerCase())
          .filter(Boolean)
      );

      // 附加隱藏 PK（就算辭典未顯示也要能存檔）
      (keyFields || []).forEach(k => {
        if (!k) return;
        const key = String(k).toLowerCase();
        if (renderedInputNames.has(key)) return;
        const pk = document.createElement("input");
        pk.type = "hidden";
        pk.name = k;
        pk.className = "mmd-pk-hidden";
        const val = row[k] ?? row[k.toLowerCase()] ?? "";
        pk.value = val == null ? "" : val;
        tr.appendChild(pk);
        renderedInputNames.add(key);
      });

      // 新增明細列：補齊 KeyMap 對應的 FK（辭典未開欄位時也能存檔）
      if (isDetail && isNewRow) {
        (cfg?.KeyMap || []).forEach(km => {
          const fkName = km?.Detail;
          if (!fkName) return;
          const fkKey = String(fkName).toLowerCase();
          if (renderedInputNames.has(fkKey)) return;

          let fkVal = row[fkName];
          if (fkVal == null || fkVal === "") {
            const actualKey = Object.keys(row || {}).find(x => x.toLowerCase() === fkKey);
            if (actualKey) fkVal = row[actualKey];
          }

          const fk = document.createElement("input");
          fk.type = "hidden";
          fk.name = fkName;
          fk.className = "mmd-fk-hidden";
          fk.value = fkVal == null ? "" : fkVal;
          tr.appendChild(fk);
          renderedInputNames.add(fkKey);
        });
      }


      if (onRowClick) tr.addEventListener("click", () => onRowClick(tr, row));
      tbody.appendChild(tr);
    });

    // ★ 建立反向索引：key 欄位 → 依賴它的 lookup 結果欄位（供動態刷新用）
    const _keyToLookup = {};
    for (const f of fields) {
      const ocxData = ocxMaps[f.FieldName];
      const lkData = lookupMaps[f.FieldName];
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

    // ★ 動態刷新：監聽 change 事件，當 key 欄位變更時更新對應 lookup 中文名稱
    if (!tbody._lookupRefreshBound) {
      tbody._lookupRefreshBound = true;
      tbody.addEventListener('change', (e) => {
        const inp = e.target.closest('.cell-edit');
        if (!inp) return;
        const tr = inp.closest('tr');
        const keyToLookup = tbody._keyToLookup;
        if (!tr || !keyToLookup) return;
        const changedField = (inp.name || '').toLowerCase();
        const deps = keyToLookup[changedField];
        if (!deps || !deps.length) return;
        const getFieldVal = (fieldName) => {
          const fn = fieldName.toLowerCase();
          const target = Array.from(tr.querySelectorAll('input'))
            .find(i => (i.name || '').toLowerCase() === fn);
          return target ? (target.value ?? '').trim() : '';
        };
        for (const dep of deps) {
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
      });
    }
  };

  // -----------------------------
  // 🧩 建立合計列 (bFooter)
  // -----------------------------
  const buildFooter = (tableEl, dict, rows) => {
    if (!tableEl) return;

    // 移除舊的 tfoot
    const old = tableEl.querySelector('tfoot.md-grid-footer');
    if (old) old.remove();

    const visibleFields = dict
      .filter(f => DICT_MAP.visible(f))
      .sort((a, b) => DICT_MAP.order(a) - DICT_MAP.order(b));

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
        td.textContent = fmtCell(sum, DICT_MAP.fmt(f), DICT_MAP.dataType(f));
        td.style.textAlign = 'right';
        td.style.fontWeight = '600';
        td.style.background = '#f0f4ff';
      }
      tr.appendChild(td);
    });

    tfoot.appendChild(tr);
    tableEl.appendChild(tfoot);
  };

  // ------------------------------
  // 🧩 取得明細 Key
  // ------------------------------
  const pickKeys = (row, keyMap) => {
    const names = [];
    const values = [];
    keyMap.forEach(k => {
      // cfg.KeyMap 內的屬性命名：{ Master: "...", Detail: "..." }
      names.push(k.Detail);
      values.push(row[k.Master]);
    });
    return { names, values };
  };

  // ------------------------------
  // 🧩 資料排序：優先用傳入 keyFields；否則用辭典的 IsKey；數字欄位採數值排序
  // ------------------------------
  const sortByKeys = (rows, dict, keyFields = []) => {
    const keys = (keyFields && keyFields.length)
      ? keyFields
      : dict.filter(f => (f.IsKey ?? 0) === 1).map(f => f.FieldName);
    if (!keys.length) return rows;

    const isNumberKey = (key) => {
      const col = dict.find(d => (d.FieldName || "").toLowerCase() === (key || "").toLowerCase());
      return col?.DataType?.toLowerCase().includes("int") || col?.DataType?.toLowerCase().includes("number");
    };

    return [...rows].sort((a, b) => {
      for (const k of keys) {
        const av = a?.[k];
        const bv = b?.[k];
        if (isNumberKey(k)) {
          const na = Number(av ?? 0);
          const nb = Number(bv ?? 0);
          if (na !== nb) return na - nb;
        } else {
          const sa = (av ?? "").toString();
          const sb = (bv ?? "").toString();
          if (sa !== sb) return sa < sb ? -1 : 1;
        }
      }
      return 0;
    });
  };

  const parseDateValue = (val) => {
    if (val == null) return NaN;
    if (typeof val === "object") {
      val = val.value ?? val.Value ?? "";
    }
    const str = String(val || "").trim();
    if (!str) return NaN;
    const direct = Date.parse(str);
    if (Number.isFinite(direct)) return direct;
    const match = str.match(/^(\d{4})[\/-](\d{1,2})[\/-](\d{1,2})(?:\s+(\d{1,2}):(\d{1,2})(?::(\d{1,2}))?)?/);
    if (match) {
      const year = Number(match[1]);
      const month = Number(match[2]) - 1;
      const day = Number(match[3]);
      const hour = Number(match[4] || 0);
      const min = Number(match[5] || 0);
      const sec = Number(match[6] || 0);
      return new Date(year, month, day, hour, min, sec).getTime();
    }
    return NaN;
  };

  const getRowValue = (row, fieldName) => {
    if (!row || !fieldName) return "";
    const key = Object.keys(row).find(k => k.toLowerCase() === fieldName.toLowerCase());
    return key ? row[key] : "";
  };

  const getSortKey = (val, dataType) => {
    if (val == null || val === "") return { type: "empty", value: "" };
    if (isDateType(dataType)) {
      const d = parseDateValue(val);
      if (Number.isFinite(d)) return { type: "date", value: d };
    }
    const str = String(val).trim();
    const num = Number(str.replace(/,/g, ""));
    if (Number.isFinite(num) && /^-?\d+(\.\d+)?$/.test(str.replace(/,/g, ""))) {
      return { type: "number", value: num };
    }
    if (/[\/:\-]/.test(str)) {
      const d = parseDateValue(str);
      if (Number.isFinite(d)) return { type: "date", value: d };
    }
    return { type: "string", value: str.toLowerCase() };
  };

  const compareSortKey = (a, b, order) => {
    if (a.type === "empty" && b.type === "empty") return 0;
    if (a.type === "empty") return 1;
    if (b.type === "empty") return -1;
    const dir = order === "desc" ? -1 : 1;
    if (a.type === b.type && (a.type === "number" || a.type === "date")) {
      return (a.value - b.value) * dir;
    }
    return a.value > b.value ? dir : a.value < b.value ? -dir : 0;
  };

  const sortRowsByField = (rows, fieldName, dict, order) => {
    if (!rows || !rows.length) return rows;
    const col = dict.find(d => (d.FieldName || "").toLowerCase() === fieldName.toLowerCase());
    const dataType = col ? DICT_MAP.dataType(col) : null;
    const mapped = rows.map(row => {
      const raw = getRowValue(row, fieldName);
      return { row, key: getSortKey(raw, dataType) };
    });
    mapped.sort((a, b) => compareSortKey(a.key, b.key, order));
    return mapped.map(x => x.row);
  };

  // ------------------------------
  // 🧩 從 URL 讀取查詢參數
  // ------------------------------
  const getQueryParams = (excludeKeys = []) => {
    const params = new URLSearchParams(window.location.search);
    const result = {};
    const excludeSet = new Set(excludeKeys.map(k => k.toLowerCase()));
    params.forEach((v, k) => {
      // 排除分頁參數和系統參數
      if (excludeSet.has(k.toLowerCase())) return;
      if (k.toLowerCase() === 'pageindex' || k.toLowerCase() === 'pagesize') return;
      if (k.toLowerCase() === 'tab') return;
      result[k] = v;
    });
    return result;
  };

  const buildQueryString = (params) => {
    const pairs = Object.entries(params).filter(([k, v]) => v != null && v !== '');
    if (!pairs.length) return '';
    return '&' + pairs.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`).join('&');
  };

  // ------------------------------
  // 🧩 初始化單一 MasterDetail
  // ------------------------------
  const initOne = async (cfg) => {
    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const masterName = cfg.MasterDict || cfg.MasterTable;
    const detailName = cfg.DetailDict || cfg.DetailTable;

    // 讀取 URL 查詢參數
    const urlQueryParams = getQueryParams();

    const mHead = root.querySelector(`#${cfg.DomId}-m-head`);
    const mBody = root.querySelector(`#${cfg.DomId}-m-body`);
    const dHead = root.querySelector(`#${cfg.DomId}-d-head`);
    const dBody = root.querySelector(`#${cfg.DomId}-d-body`);

    const mWrapper = root.querySelector(`#${cfg.DomId}-masterWrapper`);
    const dWrapper = root.querySelector(`#${cfg.DomId}-detailWrapper`);
    const masterTbl = root.querySelector(".md-master-table");
    const detailTbl = root.querySelector(".md-detail-table");
    const addBtn    = document.getElementById(`${cfg.DomId}-btnAdd`);
    const confirmBtn= document.getElementById(`${cfg.DomId}-btnConfirm`);
    const cancelBtn = document.getElementById(`${cfg.DomId}-btnCancel`);
    const editBtn   = document.getElementById(`${cfg.DomId}-btnEdit`);

    const masterKeyFields = (cfg.MasterKeyFields && cfg.MasterKeyFields.length)
      ? cfg.MasterKeyFields
      : (cfg.KeyMap || []).map(k => k.Master).filter(Boolean);

    let masterData = [];
    let detailData = [];
    let currentMasterRow = null;
    let lastArea = "master";

    const setArea = (area) => { lastArea = area === "detail" ? "detail" : "master"; };
    let addMode = false;

    const setAddMode = (on) => {
      addMode = !!on;
      confirmBtn?.classList.toggle("d-none", !addMode);
      // cancel 按鈕在編輯模式下由 toolbar 的 disabled 狀態控制，不用 d-none 隱藏
      if (addMode && cancelBtn) cancelBtn.disabled = false;
    };

    // 追蹤「最後點擊/聚焦」區域（決定新增要落在哪），避免滑過就被判定
    ["click","focusin"].forEach(ev => {
      mWrapper?.addEventListener(ev, () => setArea("master"));
      dWrapper?.addEventListener(ev, () => setArea("detail"));
      masterTbl?.addEventListener(ev, () => setArea("master"));
      detailTbl?.addEventListener(ev, () => setArea("detail"));
    });

    // F3 辭典情境綁定
    const markCtx = (el, tbl) => {
      ["click", "pointerdown", "mouseenter"].forEach(ev =>
        el?.addEventListener(ev, () => {
          document.querySelectorAll(".ctx-current")
            .forEach(x => x.classList.remove("ctx-current"));
          el.classList.add("ctx-current");
          window._dictTableName = tbl;
        })
      );
    };

    markCtx(masterTbl, cfg.MasterDict || cfg.MasterTable);
    markCtx(detailTbl, cfg.DetailDict || cfg.DetailTable);

    // 讀辭典（完整欄位版）
    const mDict = await fetch(
      `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(masterName)}`
    ).then(r => r.json());

    const dDict = await fetch(
      `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(detailName)}`
    ).then(r => r.json());

    buildHead(mHead, mDict, false, masterName);
    buildHead(dHead, dDict, false, detailName);
    enableColumnResize(masterTbl, masterName);
    enableColumnResize(detailTbl, detailName);

    // 欄位拖曳排序
    if (window.initColumnDragSort) {
      [
        { grid: masterTbl, tableName: masterName },
        { grid: detailTbl, tableName: detailName }
      ].forEach(({ grid, tableName }) => {
        const hRow = grid?.querySelector('thead tr');
        if (hRow) {
          initColumnDragSort({
            headerRow: hRow,
            tbody: grid.querySelector('tbody'),
            scrollWrap: grid.closest('.table-responsive') || grid.parentElement,
            tableName: tableName,
            onSaved: null
          });
        }
      });
    }

    const bindHeaderSort = (tableEl, dict, isMaster) => {
      if (!tableEl) return;
      const head = tableEl.querySelector("thead");
      if (!head) return;
      head.querySelectorAll("th").forEach(th => {
        th.style.cursor = "default";
        th.title = "點擊排序";
        th.addEventListener("click", (e) => {
          if (e.target?.classList?.contains("md-col-resizer")) return;
          const field = th.dataset.field;
          if (!field) return;
          const current = th.dataset.sortOrder || "desc";
          const nextOrder = current === "desc" ? "asc" : "desc";
          head.querySelectorAll("th").forEach(h => { h.dataset.sortOrder = ""; });
          th.dataset.sortOrder = nextOrder;

          if (isMaster) {
            masterData = sortRowsByField(masterData, field, dict, nextOrder);
            currentMasterRow = null;
            renderMaster().then(() => {
              const first = mBody.querySelector("tr");
              first?.click();
            });
          } else {
            detailData = sortRowsByField(detailData, field, dict, nextOrder);
            renderDetail();
          }
        });
      });
    };

    bindHeaderSort(masterTbl, mDict, true);
    bindHeaderSort(detailTbl, dDict, false);

    const ensureEditMode = () => {
      if (!window._mdEditing) return false;
      if (window._masterEditor) window._masterEditor.toggleEdit(true);
      if (window._detailEditor) window._detailEditor.toggleEdit(true);
      return true;
    };

    const forceRowEditable = (tbody) => {
      tbody?.querySelectorAll('tr[data-state="added"]').forEach(tr => {
        tr.querySelectorAll('.cell-view').forEach(span => span.classList.add('d-none'));
        tr.querySelectorAll('.cell-edit').forEach(inp => {
          inp.classList.remove('d-none');
          if (inp.dataset.readonly !== "1") {
            inp.removeAttribute('readonly');
          }
        });
      });
    };

    // 確保整個表格進入編輯視圖（顯示 input、隱藏 span）
    const forceAllEditable = (tbody) => {
      tbody?.querySelectorAll('tr').forEach(tr => {
        tr.querySelectorAll('.cell-view').forEach(span => span.classList.add('d-none'));
        tr.querySelectorAll('.cell-edit').forEach(inp => {
          inp.classList.remove('d-none');
          if (inp.dataset.readonly !== "1") inp.removeAttribute('readonly');
        });
      });
    };

    const renderMaster = async () => {
      mBody.innerHTML = "";
      await buildBody(
        mBody,
        mDict,
        masterData,
        cfg.ShowRowNumber,
        onMasterClick,
        cfg,
        masterKeyFields,
        window._mdEditing || addMode,
        false
      );
      if (!currentMasterRow) {
        const first = mBody.querySelector("tr");
        if (first) first.click();
      }
      if (window._masterEditor && (window._mdEditing || addMode)) {
        window._masterEditor.toggleEdit(false);
        window._masterEditor.toggleEdit(true);
      }
      mWrapper?.scrollTo({ top: 0, behavior: "auto" });
      if (window._mdEditing || addMode) {
        forceAllEditable(mBody);
        const firstEditable = mBody.querySelector('tr[data-state="added"] .cell-edit:not(.readonly-cell)');
        firstEditable?.focus();
      }
    };

    // Detail 列點擊高亮
    const onDetailClick = (tr, row) => {
      Array.from(dBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");
      const evt = new CustomEvent("md-detail-selected", { detail: { domId: cfg.DomId, rowData: row } });
      document.dispatchEvent(evt);
    };

    const renderDetail = async () => {
      dBody.innerHTML = "";
      // ★ 合併 DetailKeyFields 與 KeyMap Detail 欄位，確保複合 PK 的所有欄位都有 hidden input
      const detailPkFields = [...new Set([
        ...(cfg.DetailKeyFields || []),
        ...(cfg.KeyMap || []).map(k => k.Detail).filter(Boolean)
      ])].filter(Boolean);
      await buildBody(
        dBody,
        dDict,
        detailData,
        false,
        onDetailClick,
        cfg,
        detailPkFields,
        window._mdEditing || addMode,
        true
      );

      buildFooter(detailTbl, dDict, detailData);

      // ★ 當 detail 沒有資料時，放一列空白佔位列，讓使用者可以點擊聚焦到單身區塊
      if (detailData.length === 0 && dBody.querySelectorAll("tr").length === 0) {
        const visibleCols = dDict.filter(f => (f.Visible ?? 1) === 1).length || 1;
        const placeholderTr = document.createElement("tr");
        placeholderTr.className = "md-placeholder-row";
        placeholderTr.style.cursor = "pointer";
        placeholderTr.dataset.placeholder = "1";
        const td = document.createElement("td");
        td.colSpan = visibleCols;
        td.className = "text-center text-muted";
        td.style.padding = "8px";
        td.innerHTML = "<small>（點擊此處後可使用上方 + 按鈕新增資料）</small>";
        placeholderTr.appendChild(td);
        placeholderTr.addEventListener("click", () => {
          setArea("detail");
          Array.from(dBody.children).forEach(x => x.classList.remove("selected"));
          placeholderTr.classList.add("selected");
        });
        dBody.appendChild(placeholderTr);
      }

      if (window._detailEditor && (window._mdEditing || addMode)) {
        window._detailEditor.toggleEdit(false);
        window._detailEditor.toggleEdit(true);
      }
      dWrapper?.scrollTo({ top: 0, behavior: "auto" });
      if (window._mdEditing || addMode) {
        forceAllEditable(dBody);
        forceRowEditable(dBody);
        const firstEditable = dBody.querySelector('tr[data-state="added"] .cell-edit:not(.readonly-cell)');
        firstEditable?.focus();
      }
    };

    const addMasterRow = async () => {
      if (!ensureEditMode()) { alert("請先點『編輯』再新增"); return; }
      window._mdEditing = true;
      const row = { __state: "added" };
      // 預填主鍵欄位為空字串
      mDict.filter(f => (f.IsKey ?? 0) === 1).forEach(f => { row[f.FieldName] = ""; });
      masterData.unshift(row);
      currentMasterRow = row;
      await renderMaster();
      const firstRow = mBody.querySelector("tr");
      firstRow?.click();
      setAddMode(true);
      detailData = [];
      await renderDetail();
      window._masterEditor?.toggleEdit(true);
      window._detailEditor?.toggleEdit(true);
    };

    const addDetailRow = async () => {
      if (!currentMasterRow) {
        const firstTr = mBody.querySelector("tr");
        if (firstTr) {
          firstTr.click();
        }
      }
      if (!currentMasterRow) return; // 仍然沒有資料，直接跳出
      if (!ensureEditMode()) { alert("請先點『編輯』再新增"); return; }
      window._mdEditing = true;
      const row = { __state: "added" };
      // 帶入主檔鍵值
      (cfg.KeyMap || []).forEach(k => {
        row[k.Detail] = currentMasterRow[k.Master] ?? "";
      });
      // 確保 DetailKeyFields 都存在
      (cfg.DetailKeyFields || []).forEach(k => {
        if (row[k] == null) row[k] = "";
      });

      // 序號自動 +1：DetailKeyFields 扣掉 KeyMap 對應欄位後，若剩一個且為數值型態則自動遞增
      const mdDetailKeys = new Set((cfg.KeyMap || []).map(k => k.Detail.toLowerCase()));
      const seqCandidates = (cfg.DetailKeyFields || []).filter(k => !mdDetailKeys.has(k.toLowerCase()));
      if (seqCandidates.length === 1) {
        const seqField = seqCandidates[0];
        const col = dDict.find(d => (d.FieldName || "").toLowerCase() === seqField.toLowerCase());
        const dt = (col?.DataType || "").toLowerCase();
        if (dt.includes("int") || dt.includes("number")) {
          const maxVal = detailData
            .filter(r => r !== row)
            .reduce((mx, r) => Math.max(mx, Number(r[seqField]) || 0), 0);
          row[seqField] = maxVal + 1;
        }
      }

      // UseId 自動帶入登入公司別
      const useIdCol = dDict.find(d => (d.FieldName || "").toLowerCase() === "useid");
      if (useIdCol && (row[useIdCol.FieldName] == null || row[useIdCol.FieldName] === "")) {
        row[useIdCol.FieldName] = window._useId || window.DEFAULT_USEID || localStorage.getItem("erpUseId") || "";
      }

      detailData.push(row);
      await renderDetail();
      const allTrs = Array.from(dBody.querySelectorAll("tr"));
      const last = allTrs[allTrs.length - 1];
      if (last) {
        last.classList.add("selected");
        try { last.scrollIntoView({ block: "nearest", behavior: "auto" }); } catch {}
      }
      setAddMode(true);
      window._masterEditor?.toggleEdit(true);
      window._detailEditor?.toggleEdit(true);
    };

    const cancelAdd = async () => {
      if (!addMode) return;   // 非新增模式時不執行，交給 toolbar 的取消 handler 處理
      setAddMode(false);
      // 清除 addMode 後交給 toolbar 的取消 handler 統一執行 refreshData
    };

    let savingAdd = false;
    const confirmAdd = async () => {
      if (savingAdd) return;
      savingAdd = true;
      ensureEditMode();

      // ★ 儲存前記住新增列的 key 值，以便儲存後定位
      const getInputValue = (tr, fieldName) => {
        if (!tr || !fieldName) return "";
        const want = fieldName.toLowerCase();
        const inp = Array.from(tr.querySelectorAll("input")).find(i => (i.name || "").toLowerCase() === want);
        return inp?.value ?? "";
      };
      const addedMasterTr = mBody.querySelector('tr[data-state="added"]');
      const addedMasterKeys = {};
      if (addedMasterTr) {
        masterKeyFields.forEach(k => {
          addedMasterKeys[k] = getInputValue(addedMasterTr, k);
        });
      }
      const addedDetailTr = dBody.querySelector('tr[data-state="added"]');
      const addedDetailKeys = {};
      if (addedDetailTr && cfg.DetailKeyFields) {
        cfg.DetailKeyFields.forEach(k => {
          addedDetailKeys[k] = getInputValue(addedDetailTr, k);
        });
      }

      const normKey = (v) => String(v ?? "").trim();
      if (addedDetailTr && (cfg.DetailKeyFields || []).length) {
        const hasDuplicateDetailKey = detailData.some(r => {
          if (!r || r.__state === "added" || r.__state === "deleted") return false;
          return cfg.DetailKeyFields.every(k => normKey(getRowValue(r, k)) === normKey(addedDetailKeys[k]));
        });

        if (hasDuplicateDetailKey) {
          const dupKeyText = cfg.DetailKeyFields.map(k => normKey(addedDetailKeys[k])).join(", ");
          Swal.fire({
            icon: "warning",
            title: "鍵值重複",
            text: `此鍵值已存在（${dupKeyText}），請改用其他鍵值。`
          });
          savingAdd = false;
          return;
        }
      }

      const me = window._masterEditor;
      const de = window._detailEditor;
      const r1 = me ? await me.saveChanges() : { ok: true, skipped: true };
      const r2 = de ? await de.saveChanges() : { ok: true, skipped: true };

      if (!r1.ok || !r2.ok) {
        const err = !r1.ok ? r1 : r2;
        Swal.fire({ icon: "error", title: "儲存失敗", text: err.text || "新增儲存失敗" });
        savingAdd = false;
        return;
      }

      setAddMode(false);
      // 重新載入主檔，確保鍵值與資料同步
      const freshMasterRows = await fetch(masterUrl).then(r => r.json());
      masterData = Array.isArray(freshMasterRows) ? freshMasterRows : [];
      await renderMaster();

      // ★ 儲存後找到剛剛新增的那一筆並選中
      const findRowByKeys = (rows, keyFields, keyValues) => {
        if (!rows?.length || !keyFields?.length || !Object.keys(keyValues).length) return null;
        return rows.find(row => {
          return keyFields.every(k => {
            const rowVal = String(row[k] ?? "").trim();
            const targetVal = String(keyValues[k] ?? "").trim();
            return rowVal === targetVal;
          });
        }) || null;
      };

      let targetRow = null;
      if (Object.keys(addedMasterKeys).length > 0) {
        targetRow = findRowByKeys(masterData, masterKeyFields, addedMasterKeys);
      }
      if (!targetRow && currentMasterRow) {
        targetRow = findRowByKeys(masterData, masterKeyFields, currentMasterRow);
      }

      if (targetRow) {
        const targetTr = Array.from(mBody.querySelectorAll("tr")).find(tr => tr.__rowData === targetRow);
        if (targetTr) {
          targetTr.click();
          try { targetTr.scrollIntoView({ block: "center", behavior: "auto" }); } catch {}
        } else {
          const first = mBody.querySelector("tr");
          if (first) first.click();
        }
      } else {
        const first = mBody.querySelector("tr");
        if (first) first.click();
      }

      Swal.fire({ icon: "success", title: "新增完成", timer: 1000, showConfirmButton: false });
      savingAdd = false;
    };

    addBtn?.addEventListener("click", () => {
      if (lastArea === "detail") addDetailRow();
      else addMasterRow();
    });
    confirmBtn?.addEventListener("click", confirmAdd);
    cancelBtn?.addEventListener("click", cancelAdd);

    // 主檔資料
    // 如果有 URL 查詢參數，使用 Query API；否則使用 TopRows API
    const queryStr = buildQueryString(urlQueryParams);
    const hasQueryParams = Object.keys(urlQueryParams).length > 0;

    const masterUrl =
      cfg.MasterApi?.trim()
        ? cfg.MasterApi + (cfg.MasterApi.includes('?') ? queryStr : (queryStr ? '?' + queryStr.substring(1) : ''))
        : hasQueryParams
          ? `/api/CommonTable/Query?table=${encodeURIComponent(cfg.MasterTable)}&top=${cfg.MasterTop || 200}`
              + (cfg.MasterOrderBy ? `&orderBy=${encodeURIComponent(cfg.MasterOrderBy)}` : "")
              + (cfg.MasterOrderDir ? `&orderDir=${encodeURIComponent(cfg.MasterOrderDir)}` : "")
              + queryStr
          : `/api/CommonTable/TopRows?table=${encodeURIComponent(cfg.MasterTable)}&top=${cfg.MasterTop || 200}`
              + (cfg.MasterOrderBy ? `&orderBy=${encodeURIComponent(cfg.MasterOrderBy)}` : "")
              + (cfg.MasterOrderDir ? `&orderDir=${encodeURIComponent(cfg.MasterOrderDir)}` : "");

    const masterRows = await fetch(masterUrl).then(r => r.json());

    // 主檔點選 → 載入明細
    async function onMasterClick(tr, row) {
      Array.from(mBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");
      currentMasterRow = row;

      const keyMap = cfg.KeyMap || [];
      const { names, values } = pickKeys(row, keyMap);

        const detailUrl =
          cfg.DetailApi?.trim()
            ? cfg.DetailApi
            : `/api/CommonTable/ByKeys?table=${encodeURIComponent(cfg.DetailTable)}`
                + names.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
                + values.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("")
                + (cfg.DetailOrderBy ? `&orderBy=${encodeURIComponent(cfg.DetailOrderBy)}` : "")
                + (cfg.DetailOrderDir ? `&orderDir=${encodeURIComponent(cfg.DetailOrderDir)}` : "");

      const detailRows = await fetch(detailUrl).then(r => r.json());

      // 若 API 沒帶回鍵值，手動塞入（辭典欄位即使不可視也要有鍵）
      if (Array.isArray(detailRows) && names.length === values.length) {
        detailRows.forEach(dr => {
          names.forEach((n, i) => {
            if (dr[n] == null) dr[n] = values[i] ?? "";
          });
        });
      }

      detailData = Array.isArray(detailRows) ? detailRows : [];
      if (!cfg.DetailOrderBy) {
        detailData = sortByKeys(detailData, dDict, cfg.DetailKeyFields || []);
      }
      renderDetail();

      // 若畫面目前在「修改中」，點主檔時要讓明細維持編輯狀態
      if (window._mdEditing && window._detailEditor) {
        window._detailEditor.toggleEdit(true);
      }

      const evt = new CustomEvent("md-master-selected", { detail: { domId: cfg.DomId, rowData: row } });
      document.dispatchEvent(evt);
    }

    const pickMasterRowByKeys = (rows, prevRow) => {
      if (!prevRow || !rows?.length) return null;
      const keys = masterKeyFields.length
        ? masterKeyFields
        : (cfg.KeyMap || []).map(k => k.Master).filter(Boolean);
      if (!keys.length) return null;
      const prevKey = keys.map(k => String(getRowValue(prevRow, k) ?? "")).join("|");
      return rows.find(r => keys.map(k => String(getRowValue(r, k) ?? "")).join("|") === prevKey) || null;
    };

    const refreshData = async () => {
      try {
        // 記住捲動位置
        const prevMasterScroll = mWrapper?.scrollTop || 0;
        const prevDetailScroll = dWrapper?.scrollTop || 0;

        const freshRows = await fetch(masterUrl).then(r => r.json());
        masterData = Array.isArray(freshRows) ? freshRows : [];
        if (!cfg.MasterOrderBy) {
          masterData = sortByKeys(masterData, mDict, masterKeyFields);
        }
        await renderMaster();

        const matched = pickMasterRowByKeys(masterData, currentMasterRow);
        const targetRow = matched || masterData[0] || null;
        if (!targetRow) {
          dBody.innerHTML = `<tr><td class="text-center text-muted p-3">請點選上方一筆資料</td></tr>`;
          return;
        }

        const tr = Array.from(mBody.querySelectorAll("tr")).find(r => r.__rowData === targetRow)
          || mBody.querySelector("tr");
        if (tr) {
          await onMasterClick(tr, targetRow);
        }

        // 還原捲動位置
        if (mWrapper) mWrapper.scrollTop = prevMasterScroll;
        if (dWrapper) dWrapper.scrollTop = prevDetailScroll;
      } catch (err) {
        console.warn("masterDetail refresh failed", err);
      }
    };

    // 畫主檔
    masterData = Array.isArray(masterRows) ? masterRows : [];
    if (!cfg.MasterOrderBy) {
      masterData = sortByKeys(masterData, mDict, masterKeyFields);
    }
    renderMaster();

    // 掛載 GRID 鍵盤導航（兩段式點擊、方向鍵、F7/F8、Enter/Escape）
    if (typeof window.initErpGridNav === 'function') {
      window.initErpGridNav(mBody, {
        keyFields : masterKeyFields.map(k => k.toLowerCase()),
        isEditMode: () => !!window._mdEditing,
        addRow    : null,
        autoSave  : null,
        onRowSelect: (tr) => { if (tr && tr.__rowData) onMasterClick(tr, tr.__rowData); },
        gridLabel : `${cfg.DomId}-master`,
      });
      window.initErpGridNav(dBody, {
        keyFields : (cfg.DetailKeyFields || []).map(k => k.toLowerCase()),
        isEditMode: () => !!window._mdEditing,
        addRow    : async () => { await addDetailRow(); return { ok: true }; },
        autoSave  : null,
        onRowSelect: (tr) => { if (tr && tr.__rowData) onDetailClick(tr, tr.__rowData); },
        gridLabel : `${cfg.DomId}-detail`,
      });
    }

    // ★ 檢查 sessionStorage 是否有儲存前記住的選中列
    let targetTr = null;
    const savedKeysJson = sessionStorage.getItem(`md-selected-${cfg.DomId}`);
    if (savedKeysJson) {
      sessionStorage.removeItem(`md-selected-${cfg.DomId}`); // 用完就刪
      try {
        const savedKeys = JSON.parse(savedKeysJson);
        const targetRow = masterData.find(row => {
          return masterKeyFields.every(k => {
            const rowVal = String(row[k] ?? "").trim();
            const savedVal = String(savedKeys[k] ?? "").trim();
            return rowVal === savedVal;
          });
        });
        if (targetRow) {
          targetTr = Array.from(mBody.querySelectorAll("tr")).find(tr => tr.__rowData === targetRow);
        }
      } catch {}
    }

    if (targetTr) {
      targetTr.click();
      try { targetTr.scrollIntoView({ block: "center", behavior: "auto" }); } catch {}
    } else {
      const first = mBody.querySelector("tr");
      if (first) first.click();
    }

    window.MasterDetailRefresh = window.MasterDetailRefresh || {};
    window.MasterDetailRefresh[cfg.DomId] = refreshData;
  };

  // -------------------------------------------------
  // 🧩 DOM Ready → 初始化全部 MasterDetail 區塊
  // -------------------------------------------------
  document.addEventListener("DOMContentLoaded", () => {
    // checkbox 外觀加深
    if (!document.getElementById("md-checkbox-dark-style")) {
      const style = document.createElement("style");
      style.id = "md-checkbox-dark-style";
      style.textContent = `
        .checkbox-dark {
          accent-color: #2c3e50;
          border: 1px solid #2c3e50 !important;
        }
      `;
      document.head.appendChild(style);
    }

    if (!window._mdConfigs) return;
    Object.values(window._mdConfigs).forEach(cfg => initOne(cfg));
  });

})();
