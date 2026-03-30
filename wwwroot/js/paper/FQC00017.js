// FQC00017 IQC品檢單 (ME202)
// 對應 Delphi: FQCdFMEOutMain (PaperOrgDLL)
//
// 在 PaperDetail 頁面的 _MultiTabDetail 下方新增 3 個子頁籤：
//   - QC結果 (FQCdFMEOutResult{PowerType}) ← 單身 PaperNum + SubItem=Item
//   - QC項目 (FQCdFMEOutItem{PowerType})   ← 單身 PaperNum + SubItem=Item
//   - 料號屬性 (MGNdVMatInfoDtl)            ← 單身 PartNum
//
// 參考 FME00011_ThirdTab.cshtml 的寫法

(function () {
  'use strict';

  const ITEM_ID = 'FQC00017';
  if (((window._itemId || '').toString().trim().toUpperCase()) !== ITEM_ID) return;
  if (!document.querySelector('.multi-tab-detail')) return;

  // sub-tab 定義
  const SUB_TABS = [
    { id: 'fqc_qcresult', title: 'QC結果', dictBase: 'FQCdFMEOutResult', keyType: 'paperItem' },
    { id: 'fqc_qcitem',   title: 'QC項目', dictBase: 'FQCdFMEOutItem',   keyType: 'paperItem' },
    { id: 'fqc_matinfo',  title: '料號屬性', dictBase: 'MGNdVMatInfoDtl', keyType: 'partNum' }
  ];

  let powerType = 0;
  let detailTabId = '';
  let currentPaperNum = '';
  let currentDetailItem = '';
  let currentDetailPartNum = '';
  const dictFieldsCache = {};
  const dataCache = {};

  // ---- 工具函數 ----
  function esc(v) {
    return String(v ?? '')
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  }
  function toArray(x) {
    if (Array.isArray(x)) return x;
    if (x && Array.isArray(x.value)) return x.value;
    return [];
  }
  function fieldValue(row, name) {
    if (!row || !name) return '';
    if (row[name] != null) return row[name];
    const hit = Object.keys(row).find(k => k.toLowerCase() === name.toLowerCase());
    return hit ? row[hit] : '';
  }
  function normalizeKey(name) {
    return (name || '').toString().trim().toLowerCase();
  }
  function isNumberType(dt) {
    const t = (dt || '').toLowerCase();
    return t.includes('int') || t.includes('decimal') || t.includes('numeric')
      || t.includes('money') || t.includes('float') || t.includes('real');
  }
  function isDateType(dt) {
    const t = (dt || '').toLowerCase();
    return t.includes('date') || t.includes('time');
  }
  function pad2(v) { return String(v).padStart(2, '0'); }
  function formatDate(raw) {
    if (raw == null) return '';
    const s = String(raw).trim();
    if (!s) return '';
    const m = s.match(/^(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})/);
    if (m) return `${m[1]}/${pad2(m[2])}/${pad2(m[3])}`;
    const d = new Date(s);
    if (Number.isNaN(d.getTime())) return s;
    return `${d.getFullYear()}/${pad2(d.getMonth() + 1)}/${pad2(d.getDate())}`;
  }
  function formatCell(raw, col, row) {
    if (isDateType(col?.dataType)) return formatDate(raw);
    return raw == null ? '' : raw;
  }

  // ---- 取得 PowerType ----
  async function fetchPowerType() {
    try {
      const res = await fetch(`/api/DynamicTable/PaperTypes/${encodeURIComponent('FQCdFMEOutMain')}?itemId=${ITEM_ID}`);
      if (!res.ok) return 0;
      const arr = await res.json();
      if (Array.isArray(arr) && arr.length > 0) return Number(arr[0].PowerType ?? arr[0].powerType ?? 0);
      // fallback: 從 CURdSysItems 讀取
      const res2 = await fetch(`/api/CommonTable/ByKeys?table=CURdSysItems&keyNames=ItemId&keyValues=${ITEM_ID}`);
      if (!res2.ok) return 0;
      const items = await res2.json();
      if (Array.isArray(items) && items.length > 0) return Number(items[0].PowerType ?? items[0].powerType ?? 0);
      return 0;
    } catch { return 0; }
  }

  // ---- 取得辭典欄位 ----
  async function fetchDictFields(dictTable) {
    if (dictFieldsCache[dictTable]) return dictFieldsCache[dictTable];
    try {
      const res = await fetch(`/api/TableFieldLayout/DictFields?table=${encodeURIComponent(dictTable)}&lang=TW`);
      if (!res.ok) return [];
      const raw = toArray(await res.json());
      const fields = raw
        .filter(r => (r?.Visible ?? r?.visible ?? 1) == 1)
        .sort((a, b) => Number(a?.SerialNum ?? a?.serialNum ?? 0) - Number(b?.SerialNum ?? b?.serialNum ?? 0));
      dictFieldsCache[dictTable] = fields;
      return fields;
    } catch { return []; }
  }

  // ---- 取得實際表名（含 PowerType 後綴） ----
  function resolveTableName(tab) {
    if (tab.keyType === 'partNum') return tab.dictBase; // 料號屬性不帶後綴
    return powerType > 0 ? `${tab.dictBase}${powerType}` : tab.dictBase;
  }

  // ---- 查詢子明細資料 ----
  async function fetchSubData(tab, paperNum, itemVal, partNum) {
    const tableName = resolveTableName(tab);
    let url;
    if (tab.keyType === 'paperItem') {
      if (!paperNum || !itemVal) return [];
      url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(tableName)}`
        + `&keyNames=PaperNum&keyNames=SubItem`
        + `&keyValues=${encodeURIComponent(paperNum)}&keyValues=${encodeURIComponent(itemVal)}`;
    } else if (tab.keyType === 'partNum') {
      if (!partNum) return [];
      url = `/api/CommonTable/ByKeys?table=${encodeURIComponent(tableName)}`
        + `&keyNames=PartNum`
        + `&keyValues=${encodeURIComponent(partNum)}`;
    } else {
      return [];
    }
    try {
      const res = await fetch(url);
      if (!res.ok) return [];
      return toArray(await res.json());
    } catch { return []; }
  }

  // ---- 建立 Tab DOM ----
  function ensureTabDom() {
    const wrap = document.querySelector('.multi-tab-detail');
    if (!wrap) return false;
    const nav = wrap.querySelector('ul.nav.nav-tabs');
    const content = wrap.querySelector('div.tab-content');
    if (!nav || !content) return false;

    for (const tab of SUB_TABS) {
      if (!document.getElementById(`tab-${tab.id}`)) {
        const li = document.createElement('li');
        li.className = 'nav-item';
        li.setAttribute('role', 'presentation');
        li.innerHTML = `
          <button class="nav-link" id="tab-${tab.id}"
                  data-bs-toggle="tab" data-bs-target="#content-${tab.id}"
                  data-tab-id="${tab.id}" type="button" role="tab">${tab.title}</button>`;
        nav.appendChild(li);
      }
      if (!document.getElementById(`content-${tab.id}`)) {
        const pane = document.createElement('div');
        pane.className = 'tab-pane fade';
        pane.id = `content-${tab.id}`;
        pane.setAttribute('role', 'tabpanel');
        pane.innerHTML = `<div class="table-container" data-tab-id="${tab.id}"></div>`;
        content.appendChild(pane);
      }
    }
    return true;
  }

  // ---- 渲染子表格 ----
  function renderSubGrid(tabDef, rows, dictFields) {
    const pane = document.getElementById(`content-${tabDef.id}`);
    if (!pane) return;
    const container = pane.querySelector('.table-container');
    if (!container) return;

    if (!dictFields.length) {
      container.innerHTML = '<div class="text-center text-muted py-3">欄位辭典未設定</div>';
      return;
    }

    const cols = dictFields.map(f => ({
      name: (f.FieldName || f.fieldName || '').toString(),
      label: (f.DisplayLabel || f.displayLabel || f.FieldName || f.fieldName || '').toString(),
      dataType: (f.DataType || f.dataType || '').toString(),
      width: Number(f.FieldWidth || f.fieldWidth || 0),
      displaySize: Number(f.DisplaySize || f.displaySize || 0)
    })).filter(x => x.name);

    const header = cols.map(c => {
      const align = isNumberType(c.dataType) ? 'align-right' : 'align-left';
      return `<th class="${align}" data-field="${esc(c.name)}"><span class="th-text">${esc(c.label)}</span><span class="col-resizer" draggable="false"></span></th>`;
    }).join('');

    const bodyRows = rows.length
      ? rows.map((r, ri) => {
          const tds = cols.map(c => {
            const align = isNumberType(c.dataType) ? 'align-right' : 'align-left';
            const raw = fieldValue(r, c.name);
            const display = formatCell(raw, c, r);
            return `<td class="${align}" data-field="${esc(c.name)}"><span class="cell-view">${esc(String(display))}</span></td>`;
          }).join('');
          const sel = ri === 0 ? ' row-selected' : '';
          return `<tr data-state="unchanged" data-row-index="${ri}"${sel ? ' class="row-selected"' : ''}>${tds}</tr>`;
        }).join('')
      : `<tr><td colspan="${Math.max(cols.length, 1)}" class="text-center text-muted py-2">(無資料)</td></tr>`;

    container.innerHTML = `
      <div class="erp-table-wrapper">
        <div class="erp-table-scroll-body">
          <table class="erp-table">
            <thead><tr>${header}</tr></thead>
            <tbody>${bodyRows}</tbody>
          </table>
        </div>
      </div>`;

    // 套用預設欄寬
    const table = container.querySelector('table.erp-table');
    if (table) {
      const ths = Array.from(table.querySelectorAll('thead th'));
      cols.forEach((c, i) => {
        const px = c.width > 0 ? c.width : (c.displaySize > 0 ? c.displaySize * 10 : 0);
        if (px > 0 && ths[i]) {
          ths[i].style.width = `${px}px`;
        }
      });
    }

    const tbody = table ? table.querySelector('tbody') : null;
    if (tbody) {
      tbody.addEventListener('click', (ev) => {
        const tr = ev.target.closest('tr[data-row-index]');
        if (!tr) return;
        tbody.querySelectorAll('tr.row-selected').forEach(r => r.classList.remove('row-selected'));
        tr.classList.add('row-selected');
      });
    }
  }

  // ---- 載入並渲染指定 sub-tab ----
  async function loadAndRender(tabDef) {
    const dictTable = resolveTableName(tabDef);
    const fields = await fetchDictFields(dictTable);
    const rows = await fetchSubData(tabDef, currentPaperNum, currentDetailItem, currentDetailPartNum);
    dataCache[tabDef.id] = rows;
    renderSubGrid(tabDef, rows, fields);
  }

  // ---- 載入全部 sub-tabs ----
  async function loadAllSubTabs() {
    await Promise.all(SUB_TABS.map(t => loadAndRender(t)));
  }

  // ---- 偵測 DETAIL1 的 tabId ----
  function detectDetailTabId() {
    const tabs = Array.isArray(window._multiTabTabs) ? window._multiTabTabs : [];
    if (tabs.length > 0) detailTabId = (tabs[0]?.Id || '').toString().trim();
  }

  // ---- 綁定單身明細 row 選取事件 ----
  function bindDetailRowEvent() {
    window.addEventListener('multiTabRowSelected', (e) => {
      const d = e?.detail || {};
      const tabId = (d.tabId || '').toString().trim();
      if (!tabId || (detailTabId && tabId !== detailTabId)) return;
      const row = d.rowData;
      if (!row) return;

      const newItem = String(fieldValue(row, 'Item') || '').trim();
      const newPartNum = String(fieldValue(row, 'PartNum') || '').trim();

      if (newItem !== currentDetailItem || newPartNum !== currentDetailPartNum) {
        currentDetailItem = newItem;
        currentDetailPartNum = newPartNum;
        loadAllSubTabs();
      }
    });
  }

  // ---- 綁定 sub-tab shown 事件（lazy load） ----
  function bindSubTabShown() {
    for (const tab of SUB_TABS) {
      const btn = document.getElementById(`tab-${tab.id}`);
      if (!btn) continue;
      btn.addEventListener('shown.bs.tab', () => {
        // 如果已有快取資料就不重複載入
        if (!dataCache[tab.id]) loadAndRender(tab);
      });
    }
  }

  // ---- 同步初始選取狀態 ----
  function syncInitialSelection() {
    currentPaperNum = (window._paperNum || '').toString().trim();
    if (!currentPaperNum) {
      // 嘗試從 URL 取得 paperNum
      const m = location.pathname.match(/\/Paper\/[^/]+\/([^/]+)/);
      if (m) currentPaperNum = decodeURIComponent(m[1]);
    }
    if (!detailTabId) return;
    const row = window._multiTabSelectedRows?.[detailTabId];
    if (row) {
      currentDetailItem = String(fieldValue(row, 'Item') || '').trim();
      currentDetailPartNum = String(fieldValue(row, 'PartNum') || '').trim();
    }
  }

  function selectedSubRow(tabId) {
    const rows = Array.isArray(dataCache[tabId]) ? dataCache[tabId] : [];
    if (rows.length === 0) return null;
    const pane = document.getElementById(`content-${tabId}`);
    const selected = pane?.querySelector('tbody tr.row-selected[data-row-index]');
    const idx = Number(selected?.dataset?.rowIndex);
    if (Number.isInteger(idx) && idx >= 0 && idx < rows.length) return rows[idx];
    return rows[0];
  }

  function currentDetailRow() {
    const sel = window._multiTabSelectedRows;
    if (!sel || typeof sel !== 'object') return null;
    if (detailTabId && sel[detailTabId]) return sel[detailTabId];
    const firstKey = Object.keys(sel).find(k => sel[k]);
    return firstKey ? sel[firstKey] : null;
  }

  async function fetchQCItemRows() {
    const tab = SUB_TABS.find(t => t.id === 'fqc_qcitem');
    if (!tab) return [];
    const rows = await fetchSubData(tab, currentPaperNum, currentDetailItem, currentDetailPartNum);
    dataCache[tab.id] = rows;
    return rows;
  }

  async function fetchQCTypeSubRows(qcType, qcTypeItem) {
    const type = String(qcType || '').trim();
    const item = Number(qcTypeItem);
    if (!type || !Number.isFinite(item)) return [];
    const url = `/api/CommonTable/ByKeys?table=${encodeURIComponent('FQCdQCTypeSubDtl')}`
      + `&keyNames=QCType&keyNames=Item`
      + `&keyValues=${encodeURIComponent(type)}&keyValues=${encodeURIComponent(String(item))}`
      + `&orderBy=${encodeURIComponent('SerialNum')}`;
    try {
      const res = await fetch(url);
      if (!res.ok) return [];
      return toArray(await res.json());
    } catch {
      return [];
    }
  }

  async function execCopySpec(args) {
    const resp = await fetch('/api/StoredProc/exec', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        key: 'FQCdFMEOutCopySpec',
        args
      })
    });
    const raw = await resp.text();
    let data;
    try { data = JSON.parse(raw); } catch { data = { ok: false, error: raw }; }
    if (!(resp.ok && data.ok)) throw new Error(data.error || `HTTP ${resp.status}`);
  }

  async function runCopySpec(ctx) {
    if (ctx?.paperNum) currentPaperNum = String(ctx.paperNum).trim();
    if (!currentPaperNum) currentPaperNum = (window._paperNum || '').toString().trim();
    if (!currentPaperNum) {
      alert('找不到單號，無法執行複製檢驗規格。');
      return;
    }

    const dRow = currentDetailRow();
    const detailItem = String(fieldValue(dRow, 'Item') || currentDetailItem || '').trim();
    if (!detailItem) {
      alert('請先選取單身資料，再執行複製檢驗規格。');
      return;
    }
    currentDetailItem = detailItem;
    currentDetailPartNum = String(fieldValue(dRow, 'PartNum') || currentDetailPartNum || '').trim();

    const qcRows = await fetchQCItemRows();
    if (qcRows.length === 0) {
      alert('沒有「QC項目」資料，無法複製檢驗規格。');
      return;
    }

    const qcRow = selectedSubRow('fqc_qcitem') || qcRows[0];
    const subItemRaw = fieldValue(qcRow, 'SubItem') || detailItem;
    const qcTypeItemRaw = fieldValue(qcRow, 'QCTypeItem');
    const qcType = String(fieldValue(qcRow, 'QCType') || '').trim();
    const subItem = Number(subItemRaw);
    const qcTypeItem = Number(qcTypeItemRaw);
    if (!Number.isFinite(subItem) || !Number.isFinite(qcTypeItem) || !qcType) {
      alert('QC項目資料不完整（SubItem/QCTypeItem/QCType），無法複製檢驗規格。');
      return;
    }

    const subRows = await fetchQCTypeSubRows(qcType, qcTypeItem);
    if (subRows.length === 0) {
      alert('沒有「檢驗子項」資料，無法複製檢驗規格。');
      return;
    }
    const serialNum = Number(fieldValue(subRows[0], 'SerialNum'));
    if (!Number.isFinite(serialNum)) {
      alert('檢驗子項缺少 SerialNum，無法複製檢驗規格。');
      return;
    }

    await execCopySpec({
      PaperNum: currentPaperNum,
      SubItem: subItem,
      QCTypeItem: qcTypeItem,
      QCType: qcType,
      SerialNum: serialNum
    });

    await Promise.all(SUB_TABS.filter(t => normalizeKey(t.id) === 'fqc_qcitem' || normalizeKey(t.id) === 'fqc_qcresult')
      .map(t => loadAndRender(t)));
    alert('複製檢驗規格完成。');
  }

  function registerCustomHandlers() {
    window.ActionRailCustomHandlers = window.ActionRailCustomHandlers || {};
    const map = window.ActionRailCustomHandlers[ITEM_ID] || {};
    map.btnCopySpec = runCopySpec;
    map.btnCopyspec = runCopySpec;
    map.BtnCopySpec = runCopySpec;
    window.ActionRailCustomHandlers[ITEM_ID] = map;
  }

  // ---- 初始化 ----
  async function init() {
    powerType = await fetchPowerType();
    detectDetailTabId();
    if (!ensureTabDom()) return;
    registerCustomHandlers();
    bindDetailRowEvent();
    bindSubTabShown();
    syncInitialSelection();
    if (currentDetailItem) {
      await loadAllSubTabs();
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => init().catch(err => console.error('[FQC00017]', err)));
  } else {
    init().catch(err => console.error('[FQC00017]', err));
  }
})();








