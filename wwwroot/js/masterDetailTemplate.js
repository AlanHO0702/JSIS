(() => {

  // ────────────────────────────────────────
  // 🔧 辭典欄位映射
  // ────────────────────────────────────────
  const DICT_MAP = {
    fieldName: f => f.FieldName || f.ColumnName || f.Field || f.Name,
    headerText: f =>
      f.DisplayLabel ||
      f.DisplayName ||
      f.HeaderText ||
      f.FieldNameCN ||
      f.Alias ||
      f.Label ||
      f.FieldName,
    width: f => {
      const raw =
        f.DisplaySize ??
        f.Width ??
        f.iFieldWidth ??
        f.ColumnWidth ??
        null;
      if (raw == null) return null;

      let n = Number(raw);
      if (isNaN(n) || n <= 0) return null;

      return n * 10; // ⭐ 每字寬 10px
    },
    visible: f => (f.Visible !== false && f.Visible !== 0 && f.iShow !== 0),
    order: f => f.SerialNum ?? f.OrderNo ?? f.Order ?? f.iShowOrder ?? 99999,
    fmt: f => f.FormatStr || f.Format || null,
    dataType: f => f.DataType || null,
    readOnly: f => f.ReadOnly ?? f.iReadOnly ?? f.IsReadOnly ?? 0
  };

  // ────────────────────────────────────────
  // 🔧 辭典 API
  // ────────────────────────────────────────
  const GET_DICT_API = (tbl) => {
    const base = window.FIELD_DICT_GET_API || '/api/TableFieldLayout/GetTableFieldsFull';
    const key  = window.FIELD_DICT_QUERY_KEY || 'table';
    const u = new URL(base, window.location.origin);
    u.searchParams.set(key, tbl);
    if (key !== 'table')     u.searchParams.set('table', tbl);
    if (key !== 'tableName') u.searchParams.set('tableName', tbl);
    return u.toString();
  };

  // 主檔資料
  const GET_MASTER_DEFAULT = (table, top, orderBy, dir) => {
    const p = new URLSearchParams({ table, top: String(top || 200) });
    if (orderBy) {
      p.set("orderBy", orderBy);
      p.set("orderDir", dir || "ASC");
    }
    return `/api/CommonTable/TopRows?${p.toString()}`;
  };

  // 明細資料
  const GET_DETAIL_BY_KEYS = (table, keyNames = [], keyValues = []) => {
    const p = new URLSearchParams({ table });
    keyNames.forEach(n => p.append("keyNames", n));
    keyValues.forEach(v => p.append("keyValues", v ?? ""));
    return `/api/CommonTable/ByKeys?${p.toString()}`;
  };

  // 格式化儲存格
  const fmtCell = (val, fmt, dataType) => {
    if (val == null) return "";
    if (dataType && String(dataType).toLowerCase().includes("date")) {
      const d = new Date(val);
      if (!isNaN(d)) return d.toISOString().slice(0, 10).replace(/-/g, "/");
    }
    if (typeof val === "number") {
      if (fmt && fmt.includes(".000")) return val.toFixed(3);
      if (fmt && fmt.includes(".00")) return val.toFixed(2);
      return val.toLocaleString();
    }
    return String(val);
  };

  // ────────────────────────────────────────
  // 🔧 畫表頭
  // ────────────────────────────────────────
  const buildHead = (theadTr, fields, showRowNo) => {
    theadTr.innerHTML = "";

    if (showRowNo) {
      const th = document.createElement("th");
      th.textContent = "項次";
      th.style.width = "60px";
      theadTr.appendChild(th);
    }

    fields
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



  // ────────────────────────────────────────
  // 🔧 畫表身 — cell-view + cell-edit
  // ────────────────────────────────────────
  const buildBody = (tbody, fields, rows, showRowNo, onRowClick) => {
    tbody.innerHTML = "";

    const visibleFields = fields
      .filter(DICT_MAP.visible)
      .sort((a, b) => DICT_MAP.order(a) - DICT_MAP.order(b));

    rows.forEach((row, idx) => {
      const tr = document.createElement("tr");
      tr.style.cursor = "pointer";

      if (showRowNo) {
        const tdNo = document.createElement("td");
        tdNo.textContent = idx + 1;
        tdNo.className = "text-center";
        tr.appendChild(tdNo);
      }

      visibleFields.forEach(f => {
        const col = DICT_MAP.fieldName(f);
        const raw = row[col];
        const text = fmtCell(raw, DICT_MAP.fmt(f), DICT_MAP.dataType(f));

        const td = document.createElement("td");
        td.dataset.field = col;

        // 顯示
        const span = document.createElement("span");
        span.className = "cell-view";
        span.textContent = text ?? "";

        // 編輯
        const inp = document.createElement("input");
        inp.className = "form-control form-control-sm cell-edit d-none";
        inp.name = col;
        inp.value = raw ?? "";

        const ro = DICT_MAP.readOnly(f);
        const isRO = ro === 1 || ro === "1" || ro === true;

        inp.dataset.readonly = isRO ? "1" : "0";
        if (isRO) {
          inp.classList.add("readonly-cell");
          inp.readOnly = true;
        }

        td.append(span, inp);
        tr.appendChild(td);
      });

      if (onRowClick) tr.addEventListener("click", () => onRowClick(tr, row));

      tbody.appendChild(tr);
    });
  };


  // ────────────────────────────────────────
  // 🔧 主→明細 Key 映射
  // ────────────────────────────────────────
  const pickKeys = (row, keyMap) => {
    const names = [];
    const values = [];
    keyMap.forEach(k => {
      names.push(k.detail);
      values.push(row[k.master]);
    });
    return { names, values };
  };

  // ────────────────────────────────────────
  // 🔧 初始化單一 Master/Detail 區塊
  // ────────────────────────────────────────
  const initOne = async (cfg) => {

    const root = document.getElementById(cfg.DomId);
    if (!root) return;

    const mHead = root.querySelector(`#${cfg.DomId}-m-head`);
    const mBody = root.querySelector(`#${cfg.DomId}-m-body`);
    const dHead = root.querySelector(`#${cfg.DomId}-d-head`);
    const dBody = root.querySelector(`#${cfg.DomId}-d-body`);
    const mTbl  = root.querySelector('.md-master-table');
    const dTbl  = root.querySelector('.md-detail-table');

    // 啟動 F3 辭典定位（可省略）
    const markCtx = (el, tbl) => {
      ['click','pointerdown','mouseenter','focusin'].forEach(ev =>
        el?.addEventListener(ev, () => {
          document.querySelectorAll('.ctx-current')
            .forEach(x => x.classList.remove('ctx-current'));
          el.classList.add('ctx-current');
          window._dictTableName = tbl;
        })
      );
    };
    markCtx(mTbl, cfg.MasterDict || cfg.MasterTable);
    markCtx(dTbl, cfg.DetailDict || cfg.DetailTable);

    // ────────────────────────────────────────
    // 1) 讀辭典
    // ────────────────────────────────────────
    const [mDict, dDict] = await Promise.all([
      fetch(GET_DICT_API(cfg.MasterDict || cfg.MasterTable)).then(r => r.json()),
      fetch(GET_DICT_API(cfg.DetailDict || cfg.DetailTable)).then(r => r.json())
    ]);

    // ────────────────────────────────────────
    // 2) 畫 Master/Detail 表頭
    // ────────────────────────────────────────
    buildHead(mHead, mDict, cfg.ShowRowNumber);
    buildHead(dHead, dDict, cfg.ShowRowNumber);

    // ────────────────────────────────────────
    // 3) 取得主檔資料
    // ────────────────────────────────────────
    const masterUrl = cfg.MasterApi?.trim()
      ? cfg.MasterApi
      : GET_MASTER_DEFAULT(cfg.MasterTable, cfg.MasterTop, cfg.MasterOrderBy, cfg.MasterOrderDir);

    const masterRows = await fetch(masterUrl).then(r => r.json());

    // ────────────────────────────────────────
    // 4) 點主檔 → 載入明細
    // ⭐⭐⭐⭐⭐ 這裡加入自動恢復 Detail 編輯模式 ⭐⭐⭐⭐⭐
    // ────────────────────────────────────────
    const onMasterClick = async (tr, row) => {

      Array.from(mBody.children).forEach(x => x.classList.remove("selected"));
      tr.classList.add("selected");

      const keyMap = (cfg.KeyMap || []).map(k => ({
        master: k.Master,
        detail: k.Detail
      }));
      const { names, values } = pickKeys(row, keyMap);

      const detailUrl =
        (cfg.DetailApi && cfg.DetailApi.includes("{"))
          ? cfg.KeyMap.reduce(
              (u, k) => u.replaceAll(`{${k.Detail}}`, encodeURIComponent(row[k.Master] ?? "")),
              cfg.DetailApi
            )
          : (cfg.DetailApi?.trim()
             ? cfg.DetailApi
             : GET_DETAIL_BY_KEYS(cfg.DetailTable, names, values));

      const detailRows = await fetch(detailUrl).then(r => r.json());

      // Build body
      buildBody(dBody, dDict, detailRows, cfg.ShowRowNumber, () => {});

      // ⭐⭐⭐ 重點：如果現在是編輯模式 → 明細重新進入編輯 ⭐⭐⭐
      if (window._mdEditing && window._detailEditor) {
        window._detailEditor.toggleEdit(true);
      }
    };

    // 畫主檔 body
    buildBody(mBody, mDict, masterRows, cfg.ShowRowNumber, onMasterClick);
  };

  // ────────────────────────────────────────
  // 5) DOM Ready → init
  // ────────────────────────────────────────
  document.addEventListener("DOMContentLoaded", () => {
    if (!window._mdConfigs) return;
    Object.values(window._mdConfigs).forEach(cfg => initOne(cfg));
  });

})();
