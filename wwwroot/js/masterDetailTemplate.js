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
    const map = {};

    rows.forEach(r => {
      // /LookupData 會回傳 { key, result0, result1, ... }
      map[r.key] = r.result0;
    });

    LOOKUP_CACHE[key] = map;
    return map;
  }

  // -----------------------------
  // 🧩 OCX Lookup（第二層，非實體欄位用）
  // -----------------------------
  const OCX_CACHE = {};

  async function loadOCXLookup(f) {
    const key = `${f.OCXLKTableName}|${f.KeyFieldName}|${f.OCXLKResultName}`;

    if (OCX_CACHE[key]) return OCX_CACHE[key];

    // 只要這三個沒齊，就視為沒設定 OCX
    if (!f.OCXLKTableName || !f.KeyFieldName || !f.OCXLKResultName) {
      return (OCX_CACHE[key] = null);
    }

    // 這裡的 key：用「Table Key 欄位」(KeyFieldName)
    // 這個欄位會對應到主表的某個欄位（通常是 KeySelfName）
    const url = `/api/TableFieldLayout/LookupData`
      + `?table=${encodeURIComponent(f.OCXLKTableName)}`
      + `&key=${encodeURIComponent(f.KeyFieldName)}`
      + `&result=${encodeURIComponent(f.OCXLKResultName)}`;

    const rows = await fetch(url).then(r => r.json());
    const map = {};

    rows.forEach(r => {
      // 一樣用 { key, result0 }
      map[r.key] = r.result0;
    });

    OCX_CACHE[key] = map;
    return map;
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
  // 🧩 欄寬存取（拖曳後寫回辭典 + localStorage）
  // -----------------------------
  const WIDTH_SAVE_URL = "/api/TableFieldLayout/SaveDetailLayout";
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

  const persistWidths = async (table, ths) => {
    const cols = ths.map(th => ({
      fieldName: th.dataset.field || "",
      width: Math.round(th.getBoundingClientRect().width)
    }));
    const payload = { tableName: normalizeTableName(table), cols };
    localStorage.setItem(savedWidthKey(table), JSON.stringify(cols));
    try {
      await fetch(WIDTH_SAVE_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
    } catch { /* ignore */ }
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
    let saveTimer = null;
    const debounceSave = () => {
      clearTimeout(saveTimer);
      saveTimer = setTimeout(() => persistWidths(tableName, ths), 350);
    };

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
      debounceSave();
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
      if (!isNaN(d)) return d.toISOString().slice(0, 10).replace(/-/g, "/");
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

          // OCX Lookup（優先）
          if (ocxMaps[col] && ocxMaps[col][raw] != null) {
              display = ocxMaps[col][raw];
          }
          // 一般 Lookup（次之）
          else if (lookupMaps[col] && lookupMaps[col][raw] != null) {
              display = lookupMaps[col][raw];
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
          if (isCheckbox) span.classList.add("d-inline-flex", "justify-content-center", "w-100");

          // 編輯欄位 input
          const inp = document.createElement("input");
          inp.className = isCheckbox ? "form-check-input checkbox-dark cell-edit d-none mx-auto" : "form-control form-control-sm cell-edit d-none";
          inp.name = col;

          if (isCheckbox) {
              const checked = raw === true || raw === 1 || raw === "1";
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
              inp.addEventListener("change", () => {
                  inp.value = inp.checked ? "1" : "0";
                  inp.dataset.raw = inp.value;
                  viewChk.checked = inp.checked;
              });
          } else {
              const valText = display == null ? "" : fmtCell(display, DICT_MAP.fmt(f), DICT_MAP.dataType(f));
              span.textContent = valText;
              inp.value = display == null ? "" : display;
              inp.dataset.raw = raw == null ? "" : raw;
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

      // 附加隱藏 PK（就算辭典未顯示也要能存檔）
      (keyFields || []).forEach(k => {
        if (!k) return;
        const pk = document.createElement("input");
        pk.type = "hidden";
        pk.name = k;
        pk.className = "mmd-pk-hidden";
        const val = row[k] ?? row[k.toLowerCase()] ?? "";
        pk.value = val == null ? "" : val;
        tr.appendChild(pk);
      });


      if (onRowClick) tr.addEventListener("click", () => onRowClick(tr, row));
      tbody.appendChild(tr);
    });
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

  // ------------------------------
  // 🧩 初始化單一 MasterDetail
  // ------------------------------
  const initOne = async (cfg) => {
    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const masterName = cfg.MasterDict || cfg.MasterTable;
    const detailName = cfg.DetailDict || cfg.DetailTable;

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

    let masterData = [];
    let detailData = [];
    let currentMasterRow = null;
    let lastArea = "master";

    const setArea = (area) => { lastArea = area === "detail" ? "detail" : "master"; };
    let addMode = false;

    const setAddMode = (on) => {
      addMode = !!on;
      confirmBtn?.classList.toggle("d-none", !addMode);
      cancelBtn?.classList.toggle("d-none", !addMode);
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

    const renderMaster = () => {
      mBody.innerHTML = "";
      buildBody(
        mBody,
        mDict,
        masterData,
        cfg.ShowRowNumber,
        onMasterClick,
        cfg,
        [], // master key 由 editableGrid 處理
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
      forceAllEditable(mBody);
      const firstEditable = mBody.querySelector('tr[data-state="added"] .cell-edit:not(.readonly-cell)');
      firstEditable?.focus();
    };

    const renderDetail = () => {
      dBody.innerHTML = "";
      buildBody(
        dBody,
        dDict,
        detailData,
        false,
        () => {},
        cfg,
        cfg.DetailKeyFields || [],
        window._mdEditing || addMode,
        true
      );
      if (window._detailEditor && (window._mdEditing || addMode)) {
        window._detailEditor.toggleEdit(false);
        window._detailEditor.toggleEdit(true);
      }
      dWrapper?.scrollTo({ top: 0, behavior: "auto" });
      forceAllEditable(dBody);
      forceRowEditable(dBody);
      const firstEditable = dBody.querySelector('tr[data-state="added"] .cell-edit:not(.readonly-cell)');
      firstEditable?.focus();
    };

    const addMasterRow = () => {
      if (!ensureEditMode()) { alert("請先點『編輯』再新增"); return; }
      window._mdEditing = true;
      const row = { __state: "added" };
      // 預填主鍵欄位為空字串
      mDict.filter(f => (f.IsKey ?? 0) === 1).forEach(f => { row[f.FieldName] = ""; });
      masterData.unshift(row);
      currentMasterRow = row;
      renderMaster();
      const firstRow = mBody.querySelector("tr");
      firstRow?.click();
      setAddMode(true);
      detailData = [];
      renderDetail();
      window._masterEditor?.toggleEdit(true);
      window._detailEditor?.toggleEdit(true);
    };

    const addDetailRow = () => {
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
      detailData.unshift(row);
      renderDetail();
      const first = dBody.querySelector("tr");
      first?.classList.add("selected");
      setAddMode(true);
      window._masterEditor?.toggleEdit(true);
      window._detailEditor?.toggleEdit(true);
    };

    const cancelAdd = () => {
      masterData = masterData.filter(r => r.__state !== "added");
      detailData = detailData.filter(r => r.__state !== "added");
      currentMasterRow = masterData[0] || null;
      renderMaster();
      if (currentMasterRow) {
        const tr = mBody.querySelector("tr");
        tr?.click();
      } else {
        dBody.innerHTML = `<tr><td class="text-center text-muted p-3">請點選上方一筆資料</td></tr>`;
      }
      setAddMode(false);
      renderDetail(); // 確保單身的暫存新增列被清掉
    };

    let savingAdd = false;
    const confirmAdd = async () => {
      if (savingAdd) return;
      savingAdd = true;
      ensureEditMode();
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
      const masterRows = await fetch(masterUrl).then(r => r.json());
      masterData = Array.isArray(masterRows) ? masterRows : [];
      renderMaster();
      const first = mBody.querySelector("tr");
      if (first) first.click();
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
    const masterUrl =
      cfg.MasterApi?.trim()
        ? cfg.MasterApi
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

    // 畫主檔
    masterData = Array.isArray(masterRows) ? masterRows : [];
    if (!cfg.MasterOrderBy) {
      masterData = sortByKeys(masterData, mDict, cfg.MasterKeyFields || []);
    }
    renderMaster();
    const first = mBody.querySelector("tr");
    if (first) first.click();
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
