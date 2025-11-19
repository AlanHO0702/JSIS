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

  // -----------------------------
  // 🧩 建立表頭
  // -----------------------------
  const buildHead = (theadTr, dict, showRowNo) => {
    theadTr.innerHTML = "";

    if (showRowNo) {
      const th = document.createElement("th");
      th.textContent = "項次";
      th.style.width = "60px";
      theadTr.appendChild(th);
    }

    dict
      .filter(DICT_MAP.visible)
      .sort((a, b) => DICT_MAP.order(a) - DICT_MAP.order(b))
      .forEach(f => {
        const th = document.createElement("th");
        th.textContent = DICT_MAP.headerText(f);

        const w = DICT_MAP.width(f);
        if (w) th.style.width = w + "px";

        theadTr.appendChild(th);
      });
  };

  // -----------------------------
  // 🧩 建立表身 (含 Lookup + OCX)
  // -----------------------------
  const buildBody = async (tbody, dict, rows, showRowNo, onRowClick, cfg) => {
    tbody.innerHTML = "";

    const fields = dict
      .filter(DICT_MAP.visible)
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

      if (showRowNo) {
        const tdNo = document.createElement("td");
        tdNo.className = "text-center";
        tdNo.textContent = idx + 1;
        tr.appendChild(tdNo);
      }

      fields.forEach(f => {
        const col = f.FieldName;

        // 1️⃣ 先拿原始值（實體欄位）
        let code = row[col];

        // 2️⃣ 若是「非實體欄位」，欄位本身沒有值，就改抓 KeySelfName 指向的欄位
        if ((code == null || code === "") && f.KeySelfName) {
          code = row[f.KeySelfName];
        }

        let display = code;

        // 3️⃣ 先吃 OCX Lookup（如果有設定非實體欄位）
        const ocxMap = ocxMaps[col];
        if (ocxMap && code != null && ocxMap[code] != null) {
          display = ocxMap[code];
        }
        else {
          // 4️⃣ 沒 OCX 或找不到，再吃一般 Lookup
          const lkMap = lookupMaps[col];
          if (lkMap && code != null && lkMap[code] != null) {
            display = lkMap[code];
          }
        }

        const td = document.createElement("td");
        td.dataset.field = col;

        // 顯示文字
        const span = document.createElement("span");
        span.className = "cell-view";
        span.textContent = fmtCell(display, DICT_MAP.fmt(f), DICT_MAP.dataType(f));

        // 編輯值：維持原始值（code 或 row[col]），不要用顯示文字
        const inp = document.createElement("input");
        inp.className = "form-control form-control-sm cell-edit d-none";
        inp.name = col;
        inp.value = row[col] ?? "";   // 原始欄位值

        if (DICT_MAP.readonly(f)) {
          inp.readOnly = true;
          inp.classList.add("readonly-cell");
        }

        td.append(span, inp);
        tr.appendChild(td);
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
  // 🧩 初始化單一 MasterDetail
  // ------------------------------
  const initOne = async (cfg) => {
    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const mHead = root.querySelector(`#${cfg.DomId}-m-head`);
    const mBody = root.querySelector(`#${cfg.DomId}-m-body`);
    const dHead = root.querySelector(`#${cfg.DomId}-d-head`);
    const dBody = root.querySelector(`#${cfg.DomId}-d-body`);

    const masterTbl = root.querySelector(".md-master-table");
    const detailTbl = root.querySelector(".md-detail-table");

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
      `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(cfg.MasterDict || cfg.MasterTable)}`
    ).then(r => r.json());

    const dDict = await fetch(
      `/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(cfg.DetailDict || cfg.DetailTable)}`
    ).then(r => r.json());

    buildHead(mHead, mDict, cfg.ShowRowNumber);
    buildHead(dHead, dDict, cfg.ShowRowNumber);

    // 主檔資料
    const masterUrl =
      cfg.MasterApi?.trim()
        ? cfg.MasterApi
        : `/api/CommonTable/TopRows?table=${encodeURIComponent(cfg.MasterTable)}&top=${cfg.MasterTop || 200}`;

    const masterRows = await fetch(masterUrl).then(r => r.json());

    // 主檔點選 → 載入明細
    const onMasterClick = async (tr, row) => {
      Array.from(mBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");

      const keyMap = cfg.KeyMap || [];
      const { names, values } = pickKeys(row, keyMap);

      const detailUrl =
        cfg.DetailApi?.trim()
          ? cfg.DetailApi
          : `/api/CommonTable/ByKeys?table=${encodeURIComponent(cfg.DetailTable)}`
              + names.map(n => `&keyNames=${encodeURIComponent(n)}`).join("")
              + values.map(v => `&keyValues=${encodeURIComponent(v ?? "")}`).join("");

      const detailRows = await fetch(detailUrl).then(r => r.json());

      await buildBody(
        dBody,
        dDict,
        detailRows,
        cfg.ShowRowNumber,
        () => {},
        cfg
      );

      // 若畫面目前在「修改中」，點主檔時要讓明細維持編輯狀態
      if (window._mdEditing && window._detailEditor) {
        window._detailEditor.toggleEdit(true);
      }
    };

    // 畫主檔
    await buildBody(
      mBody,
      mDict,
      masterRows,
      cfg.ShowRowNumber,
      onMasterClick,
      cfg
    );
  };

  // -------------------------------------------------
  // 🧩 DOM Ready → 初始化全部 MasterDetail 區塊
  // -------------------------------------------------
  document.addEventListener("DOMContentLoaded", () => {
    if (!window._mdConfigs) return;
    Object.values(window._mdConfigs).forEach(cfg => initOne(cfg));
  });

})();
