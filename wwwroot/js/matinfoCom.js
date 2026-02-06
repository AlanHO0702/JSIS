(() => {
  const tbody = document.getElementById('matinfo-browse-body');
  const theadRow = document.getElementById('matinfo-browse-head');
  const form = document.querySelector('.matinfo-form');
  const modeEl = document.getElementById('singleGridMode');
  const countEl = document.getElementById('singleGridCount');
  const countBox = document.getElementById('singleGridCountBox');
  const browseScrollTop = document.getElementById('matinfo-browse-scroll-top');
  const browseScrollBody = document.getElementById('matinfo-browse-scroll-body');
  const formScrollTop = document.getElementById('matinfo-form-scroll-top');
  const formScrollBody = document.getElementById('matinfo-form-scroll-body');
    const btnQuery = document.getElementById('btnMatInfoQuery');
    const btnEdit = document.getElementById('btnMatInfoEdit');
    const btnCancel = document.getElementById('btnMatInfoCancel');
    const btnAdd = document.getElementById('btnMatInfoAdd');
  const btnToFormal = document.getElementById('btnMatInfoToFormal');
  const btnDetail = document.getElementById('btnMatInfoDetail');
  const btnHistory = document.getElementById('btnMatInfoHistory');
  const btnToEmo = document.getElementById('btnMatInfoToEmo');
  const btnDelete = document.getElementById('btnMatInfoDelete');
  const btnSave = document.getElementById('btnMatInfoSave');
  const btnTabFirst = document.getElementById('btnMatInfoTabFirst');
  const btnTabPrev = document.getElementById('btnMatInfoTabPrev');
  const btnTabNext = document.getElementById('btnMatInfoTabNext');
  const btnTabLast = document.getElementById('btnMatInfoTabLast');
  const unitTable = document.getElementById('matinfo-unit-table');
  const unitWrap = document.getElementById('matinfo-unit-wrap');
  const custTable = document.getElementById('matinfo-cust-table');
  const custWrap = document.getElementById('matinfo-cust-wrap');
  const propTable = document.getElementById('matinfo-prop-table');
  const propWrap = document.getElementById('matinfo-prop-wrap');
  const mapTable = document.getElementById('matinfo-map-table');
  const mapWrap = document.getElementById('matinfo-map-wrap');
  const specTable = document.getElementById('matinfo-spec-table');
  const specWrap = document.getElementById('matinfo-spec-wrap');
  const kitTable = document.getElementById('matinfo-kit-table');
  const kitWrap = document.getElementById('matinfo-kit-wrap');
  const btnUnitFirst = document.getElementById('btnUnitFirst');
  const btnUnitPrev = document.getElementById('btnUnitPrev');
  const btnUnitNext = document.getElementById('btnUnitNext');
  const btnUnitLast = document.getElementById('btnUnitLast');
  const btnUnitAdd = document.getElementById('btnUnitAdd');
  const btnUnitDelete = document.getElementById('btnUnitDelete');
  const btnUnitSave = document.getElementById('btnUnitSave');
  const btnUnitCancel = document.getElementById('btnUnitCancel');
  const btnUnitRefresh = document.getElementById('btnUnitRefresh');
  const btnCustFirst = document.getElementById('btnCustFirst');
  const btnCustPrev = document.getElementById('btnCustPrev');
  const btnCustNext = document.getElementById('btnCustNext');
  const btnCustLast = document.getElementById('btnCustLast');
  const btnCustAdd = document.getElementById('btnCustAdd');
  const btnCustDelete = document.getElementById('btnCustDelete');
  const btnCustSave = document.getElementById('btnCustSave');
  const btnCustCancel = document.getElementById('btnCustCancel');
  const btnCustRefresh = document.getElementById('btnCustRefresh');
  const btnMapFirst = document.getElementById('btnMapFirst');
  const btnMapPrev = document.getElementById('btnMapPrev');
  const btnMapNext = document.getElementById('btnMapNext');
  const btnMapLast = document.getElementById('btnMapLast');
  const btnMapAdd = document.getElementById('btnMapAdd');
  const btnMapDelete = document.getElementById('btnMapDelete');
  const btnMapSave = document.getElementById('btnMapSave');
  const btnMapCancel = document.getElementById('btnMapCancel');
  const btnMapRefresh = document.getElementById('btnMapRefresh');
  const btnMapViewWindow = document.getElementById('btnMapViewWindow');
  const btnMapViewGlyph = document.getElementById('btnMapViewGlyph');
  const btnMapSetPath = document.getElementById('btnMapSetPath');
  const btnSpecFirst = document.getElementById('btnSpecFirst');
  const btnSpecPrev = document.getElementById('btnSpecPrev');
  const btnSpecNext = document.getElementById('btnSpecNext');
  const btnSpecLast = document.getElementById('btnSpecLast');
  const btnSpecSave = document.getElementById('btnSpecSave');
  const btnSpecCancel = document.getElementById('btnSpecCancel');
  const btnSpecRefresh = document.getElementById('btnSpecRefresh');
  const btnSpecReimport = document.getElementById('btnSpecReimport');
  const btnSpecClear = document.getElementById('btnSpecClear');
  const btnSpecPrint = document.getElementById('btnSpecPrint');
  const btnKitFirst = document.getElementById('btnKitFirst');
  const btnKitPrev = document.getElementById('btnKitPrev');
  const btnKitNext = document.getElementById('btnKitNext');
  const btnKitLast = document.getElementById('btnKitLast');
  const btnKitAdd = document.getElementById('btnKitAdd');
  const btnKitDelete = document.getElementById('btnKitDelete');
  const btnKitSave = document.getElementById('btnKitSave');
  const btnKitCancel = document.getElementById('btnKitCancel');
  const btnKitRefresh = document.getElementById('btnKitRefresh');
  const btnSaveHeight = document.getElementById('btnMatInfoSaveHeight');
  const btnCopyPN4YX = document.getElementById('btnMatInfoCopyPN4YX');
  const btnUpdateCustPn = document.getElementById('btnMatInfoUpdateCustPn');
  const glyphImage = document.getElementById('matinfo-glyph-image');
  const glyphEmpty = document.getElementById('matinfo-glyph-empty');
  const browseResizer = document.getElementById('matinfo-browse-resizer');
  const browseBox = document.getElementById('matinfo-browse');
  const logModalEl = document.getElementById('matinfo-log-modal');
  const logModal = window.bootstrap?.Modal ? window.bootstrap.Modal.getOrCreateInstance(logModalEl, { backdrop: "static" }) : null;
  let logBackdrop = null;
  const logMasterTable = document.getElementById('matinfo-log-master');
  const logHistoryTable = document.getElementById('matinfo-log-history');
  const basic2Grid = document.getElementById('matinfo-basic2-grid');
  const basic2Empty = document.getElementById('matinfo-basic2-empty');
  const basic1Grid = document.querySelector('#tab-basic .matinfo-grid');
  const basic1Abs = document.getElementById('matinfo-basic1-abs');
  const basic2Abs = document.getElementById('matinfo-basic2-abs');

  let currentRecord = null;
  let currentKey = null;
    let isNew = false;
    let isEditMode = false;
    let cancelSnapshot = null;
    let preserveCancelSnapshot = false;
    let totalCount = 0;
    let currentIndex = 0;
    let columns = [];
    let dataCache = [];
    let loadedPages = new Set();
    let isSearchMode = false;
    let pendingSelectIndex = null;
    let rowHeight = 24;
    let sortKey = null;
    let sortDir = 'asc';
    let renderScheduled = false;
    const pageSize = 200;
    const bufferRows = 8;
  const detailGrids = [];
  let unitGrid = null;
  let custGrid = null;
  let propGrid = null;
  let mapGrid = null;
  let specGrid = null;
  let kitGrid = null;
  const pageRoot = document.querySelector('.matinfo-page');
  const itemId = pageRoot?.dataset?.itemId || '';
  const itemIdUpper = (itemId || '').toUpperCase();
  const resizableItemIds = new Set(['MG000008', 'CPN00006', 'MG000002', 'CPN00007']);
  const enableColumnResize = resizableItemIds.has(itemIdUpper);
  const columnWidthStorageKey = enableColumnResize ? `matinfo:col-widths:${itemIdUpper}` : null;
    const dictTableName = (() => {
      const id = itemIdUpper;
      if (id === 'MG000008' || id === 'CPN00006') return 'MGN_MINdMatInfo2';
      if (id === 'MG000002' || id === 'CPN00007') return 'MGN_MINdMatInfo40';
      return 'MINdMatInfo';
    })();

    const dictSearchTableName = (() => {
      const id = itemIdUpper;
      if (id === 'MG000008' || id === 'CPN00006') return 'MGN_MINdMatInfo21';
      if (id === 'MG000002' || id === 'CPN00007') return 'MGN_MINdMatInfo41';
      return dictTableName;
    })();

  const dictPanelTableName = (() => {
    const id = itemIdUpper;
    if (id === 'MG000008' || id === 'CPN00006') return 'MGN_MINdMatInfo2PNL';
    if (id === 'MG000002' || id === 'CPN00007') return 'MGN_MINdMatInfo40PNL';
    return dictTableName;
  })();
  const custTabLabel = (() => {
    const id = itemIdUpper;
    return (id === 'MG000008' || id === 'CPN00006') ? '客戶料號' : '廠商料號';
  })();
  const defaultMbFilter = (() => {
    const id = itemIdUpper;
    if (id === 'MG000008' || id === 'CPN00006') return 0;
    if (id === 'MG000002' || id === 'CPN00007') return 1;
    return null;
  })();
  const useCommonTable = dictTableName.trim().toLowerCase() !== 'mindmatinfo';
  const commonKeyFields = ['PartNum', 'Revision'];

  const tabDictTableMap = new Map([
    ['基本資料', dictPanelTableName],
    ['基本資料2', dictPanelTableName],
    ['廠商料號', 'MGNdCustPartNum'],
    ['屬性及備註', 'MGNdMatInfoDtl'],
    ['工程圖', 'MGNdProdMap'],
    ['圖檔', 'MGNdProdMap'],
    ['規格表', 'MGNdSpecData'],
    ['替代單位設定', 'MINdMatUnit'],
    ['組合商品', 'MGNdKitItem']
  ]);

  window._dictTableName = dictTableName;
  document.querySelectorAll('.matinfo-form, .matinfo-tabs').forEach((el) => {
    el.dataset.dictTable = dictPanelTableName;
  });
  document.querySelectorAll('#matinfo-browse, #matinfo-browse-scroll-top, #matinfo-browse-scroll-body').forEach((el) => {
    el.dataset.dictTable = dictTableName;
  });
  const dictCache = { fields: null };
  const dictTypeMap = new Map();
  const formDictCache = { fields: null };
  const formDictTypeMap = new Map();
  const numericTypes = new Set(['int', 'smallint', 'tinyint', 'bigint', 'decimal', 'numeric', 'float', 'real', 'money', 'smallmoney']);
  const dateTypes = new Set(['date', 'datetime', 'smalldatetime', 'datetime2', 'datetimeoffset', 'time']);
  const forcedNumericFields = new Set([
    'llpcs', 'scraprate', 'accept', 'avgdayqnty', 'avgmonqnty', 'leastbag',
    'length', 'width', 'orderqnty', 'stockqnty', 'safeqnty', 'fixqnty',
    'fixdate', 'safeday', 'leadtime', 'usein', 'matclasstype', 'accounttype',
    'mb', 'status', 'istrans', 'isemo', 'ipnstatus', 'tranfactype',
    'stoporder', 'minlot', 'maxqnty', 'minqnty', 'stdusage', 'stdcostup',
    'unitprice1', 'unitprice2', 'unitprice3', 'unitprice4', 'unitprice5',
    'unitprice6', 'unitprice7', 'unitprice8', 'unitprice9', 'unitprice10',
    'notesdecim1', 'notesdecim2', 'notesdecim3', 'notesdecim4', 'notesdecim5',
    'notesdecim6', 'notesdecim7', 'notesdecim8', 'notesdecim9', 'notesdecim10',
    'weight', 'keeptime', 'scrapqnty', 'onwayqnty', 'bakfree', 'minorderqnty',
    'allowxoutrate', 'allowxoutqnty', 'boardarea', 'dheight', 'dvolume',
    'dlengthmax', 'dwidthmax', 'dheightmax', 'dvolumemax', 'dpackqntymax',
    'dvolumeaddon'
  ]);

  const layoutFieldKeys = [
    'iLayRow', 'iLayColumn', 'iLabHeight', 'iLabTop', 'iLabLeft', 'iLabWidth',
    'iFieldHeight', 'iFieldTop', 'iFieldLeft', 'iFieldWidth'
  ];
  const layoutShowWhereKey = 'iShowWhere';

  function getFieldKey(row) {
    return (row?.FieldName || row?.fieldName || '').toString().trim().toLowerCase();
  }

  function mergePanelFields(baseRows, panelRows) {
    const baseList = Array.isArray(baseRows) ? baseRows : [];
    const panelList = Array.isArray(panelRows) ? panelRows : [];
    if (!panelList.length) return baseList;

    const shouldApplyLayoutValue = (val) => {
      if (val === null || val === undefined || val === '') return false;
      const num = Number(val);
      return Number.isFinite(num) && num > 0;
    };

    const panelMap = new Map();
    panelList.forEach((row) => {
      const key = getFieldKey(row);
      if (key) panelMap.set(key, row);
    });

    const seen = new Set();
    const merged = baseList.map((row) => {
      const key = getFieldKey(row);
      if (!key) return row;
      seen.add(key);
      const panel = panelMap.get(key);
      if (!panel) return row;
      const next = { ...row };
      layoutFieldKeys.forEach((k) => {
        const v = getRowValue(panel, k);
        if (shouldApplyLayoutValue(v)) next[k] = v;
      });
      const showWhere = getRowValue(panel, layoutShowWhereKey);
      if (showWhere !== null && showWhere !== undefined && showWhere !== '') {
        next[layoutShowWhereKey] = showWhere;
      }
      const baseLabel = (getRowValue(next, 'DisplayLabel') ?? '').toString().trim();
      const panelLabel = (getRowValue(panel, 'DisplayLabel') ?? '').toString().trim();
      if (!baseLabel && panelLabel) next.DisplayLabel = panelLabel;
      if (getRowValue(next, 'Visible') == null && getRowValue(panel, 'Visible') != null)
        next.Visible = getRowValue(panel, 'Visible');
      if (getRowValue(next, 'ReadOnly') == null && getRowValue(panel, 'ReadOnly') != null)
        next.ReadOnly = getRowValue(panel, 'ReadOnly');
      return next;
    });

    return merged;
  }

  const preferredColumns = [
    'Partnum', 'MatName', 'Unit', 'EngGauge', 'Grade', 'MatClass', 'Revision',
    'StockId', 'PosId', 'UseIn', 'Build_UserId', 'Build_Date', 'Update_UserId',
    'Update_Date', 'Status', 'IsTrans', 'IsEMO'
  ];

  const columnLabels = {
    Partnum: '料號',
    MatName: '品名',
    Unit: '單位',
    EngGauge: '規格',
    Grade: '等級',
    MatClass: '分類',
    Revision: '版次',
    StockId: '倉別',
    PosId: '庫位',
    UseIn: '用途',
    Build_UserId: '建檔者',
    Build_Date: '建檔日期',
    Update_UserId: '更新者',
    Update_Date: '更新日期',
    Status: '狀態',
    IsTrans: '已轉正式',
    IsEMO: '已轉工程'
  };

  const notify = (type, message) => {
    if (window.Swal) {
      const icon = type === 'error' ? 'error' : (type === 'success' ? 'success' : 'info');
      return window.Swal.fire({ icon, title: message });
    }
    alert(message);
  };

  const withJwtHeaders = (init = {}) => {
    const jwt = localStorage.getItem('jwtId');
    const headers = Object.assign({}, init.headers || {});
    if (jwt) headers['X-JWTID'] = jwt;
    return { ...init, headers };
  };

  const applyTabDictTables = () => {
    document.querySelectorAll('.matinfo-form .tab-pane').forEach((pane) => {
      const name = pane.dataset.tabName || '';
      const table = tabDictTableMap.get(name);
      if (table) {
        pane.dataset.dictTable = table;
      } else if (!pane.dataset.dictTable) {
        pane.dataset.dictTable = dictPanelTableName;
      }
    });
    document.querySelectorAll('.matinfo-tabs .nav-link').forEach((btn) => {
      const name = btn.dataset.tabName || btn.textContent?.trim();
      const table = tabDictTableMap.get(name || '');
      if (table) btn.dataset.dictTable = table;
    });
  };

  const applyCustTabLabel = () => {
    const btn = document.querySelector('.matinfo-tabs [data-bs-target="#tab-cust"]');
    if (btn) {
      btn.textContent = custTabLabel;
      btn.setAttribute('aria-label', custTabLabel);
    }
    const pane = document.querySelector('#tab-cust');
    if (pane) {
      const title = pane.querySelector('.matinfo-section-title');
      if (title) title.textContent = custTabLabel;
    }
    const emptyCell = document.querySelector('#tab-cust tbody td');
    if (emptyCell && (emptyCell.textContent || '').includes('尚未接')) {
      emptyCell.textContent = `尚未接${custTabLabel}資料`;
    }
  };

  const setActiveTabContext = (btn) => {
    document.querySelectorAll('.matinfo-form .tab-pane.ctx-current').forEach((pane) => {
      pane.classList.remove('ctx-current');
    });
    const target = btn?.dataset?.bsTarget;
    if (!target) return;
    const pane = document.querySelector(target);
    if (pane) pane.classList.add('ctx-current');
  };

  const createDetailGridManager = (cfg) => {
    const state = {
      rows: [],
      selectedIndex: -1,
      grid: null,
      loading: false
    };

    const getKey = () => ({
      partnum: currentKey?.partnum ?? '',
      revision: currentKey?.revision ?? ''
    });

    const setToolbarEnabled = () => {
      const hasKey = !!getKey().partnum;
      const hasRows = state.rows.length > 0;
      const inEdit = !!isEditMode;
      const navDisabled = !hasKey || !hasRows;
      const setDisabled = (btn, disabled) => { if (btn) btn.disabled = disabled; };
      setDisabled(cfg.toolbar.first, navDisabled);
      setDisabled(cfg.toolbar.prev, navDisabled);
      setDisabled(cfg.toolbar.next, navDisabled);
      setDisabled(cfg.toolbar.last, navDisabled);
      setDisabled(cfg.toolbar.add, !hasKey || !inEdit);
      setDisabled(cfg.toolbar.del, !hasKey || !inEdit || state.selectedIndex < 0);
      setDisabled(cfg.toolbar.save, !hasKey || !inEdit);
      setDisabled(cfg.toolbar.cancel, !hasKey || !inEdit);
      setDisabled(cfg.toolbar.refresh, !hasKey);
    };

    const selectRow = (index) => {
      const rows = Array.from(cfg.table.querySelectorAll('tbody tr'));
      if (!rows.length) return;
      const next = Math.max(0, Math.min(index, rows.length - 1));
      rows.forEach((tr) => tr.classList.remove('is-selected'));
      const target = rows[next];
      target.classList.add('is-selected');
      state.selectedIndex = next;
      target.scrollIntoView({ block: 'nearest' });
      setToolbarEnabled();
    };

    const buildCell = (name, value, readonly, type) => {
      const td = document.createElement('td');
      const span = document.createElement('span');
      span.className = 'cell-view';
      if (type === 'checkbox') {
        const chk = document.createElement('input');
        chk.type = 'checkbox';
        chk.disabled = true;
        chk.checked = value === 1 || value === true || value === '1';
        span.appendChild(chk);
      } else {
        span.textContent = value ?? '';
      }
      const input = document.createElement('input');
      input.className = 'cell-edit d-none';
      input.name = name;
      if (type === 'checkbox') {
        input.type = 'checkbox';
        input.checked = value === 1 || value === true || value === '1';
      } else {
        input.value = value ?? '';
      }
      if (readonly) input.dataset.readonly = '1';
      td.appendChild(span);
      td.appendChild(input);
      return td;
    };

    const renderRows = () => {
      const tbodyEl = cfg.table.querySelector('tbody');
      if (!tbodyEl) return;
      tbodyEl.innerHTML = '';
      if (!state.rows.length) {
        const tr = document.createElement('tr');
        tr.innerHTML = `<td colspan="${cfg.columns.length}">沒有資料</td>`;
        tbodyEl.appendChild(tr);
        applyDetailTableWidths(cfg.table);
        attachDetailTableResizers(cfg.table);
        state.selectedIndex = -1;
        setToolbarEnabled();
        return;
      }

      state.rows.forEach((row, idx) => {
        const tr = document.createElement('tr');
        tr.dataset.index = String(idx);
        if (row.__state === 'deleted') tr.classList.add('is-deleted');
        if (row.__state === 'added') {
          tr.dataset.state = 'added';
          tr.classList.add('table-warning');
        }

        const hiddenPart = document.createElement('input');
        hiddenPart.type = 'hidden';
        hiddenPart.name = 'PartNum';
        hiddenPart.value = getKey().partnum;
        hiddenPart.className = 'mmd-fk-hidden';
        tr.appendChild(hiddenPart);

        const hiddenRev = document.createElement('input');
        hiddenRev.type = 'hidden';
        hiddenRev.name = 'Revision';
        hiddenRev.value = getKey().revision;
        hiddenRev.className = 'mmd-fk-hidden';
        tr.appendChild(hiddenRev);

        cfg.columns.forEach((col) => {
          tr.appendChild(buildCell(col.name, getRowValue(row, col.name), col.readonly, col.type));
        });

        tr.addEventListener('click', () => selectRow(idx));
        tbodyEl.appendChild(tr);
      });

      applyDetailTableWidths(cfg.table);
      attachDetailTableResizers(cfg.table);

      if (state.selectedIndex < 0) {
        selectRow(0);
      } else {
        selectRow(state.selectedIndex);
      }

      if (!state.grid && typeof window.makeEditableGrid === 'function') {
        state.grid = window.makeEditableGrid({
          wrapper: cfg.wrap,
          table: cfg.table,
          tableName: cfg.tableName,
          keyFields: cfg.keyFields
        });
      }
      if (state.grid) state.grid.toggleEdit(isEditMode);
      setToolbarEnabled();
    };

    const loadRows = async () => {
      if (!getKey().partnum) {
        state.rows = [];
        renderRows();
        return;
      }
      state.loading = true;
      try {
        const params = new URLSearchParams();
        params.set('table', cfg.tableName);
        params.append('keyNames', 'PartNum');
        params.append('keyValues', getKey().partnum);
        params.append('keyNames', 'Revision');
        params.append('keyValues', getKey().revision || '');
        const res = await fetch(`/api/CommonTable/ByKeys?${params.toString()}`);
        if (!res.ok) {
          state.rows = [];
          renderRows();
          return;
        }
        const data = await res.json();
        const rows = Array.isArray(data) ? data : [];
        state.rows = cfg.mapRows ? await cfg.mapRows(rows) : rows;
        renderRows();
      } finally {
        state.loading = false;
      }
    };

    const addRow = () => {
      if (!isEditMode || !getKey().partnum) return;
      const row = { PartNum: getKey().partnum, Revision: getKey().revision || '' };
      if (cfg.autoIncField) {
        const max = Math.max(0, ...state.rows.map((r) => Number(r[cfg.autoIncField]) || 0));
        row[cfg.autoIncField] = max + 1;
      }
      cfg.columns.forEach((col) => {
        if (row[col.name] === undefined) row[col.name] = col.default ?? '';
      });
      row.__state = 'added';
      state.rows.push(row);
      renderRows();
      selectRow(state.rows.length - 1);
    };

    const deleteRow = () => {
      if (!isEditMode) return;
      const idx = state.selectedIndex;
      if (idx < 0) return;
      const tr = cfg.table.querySelector(`tbody tr[data-index="${idx}"]`);
      if (!tr) return;
      if (tr.dataset.state === 'added') {
        state.rows.splice(idx, 1);
        renderRows();
        return;
      }
      tr.dataset.state = 'deleted';
      tr.classList.add('is-deleted');
      setToolbarEnabled();
    };

    const saveRows = async () => {
      if (!state.grid) return;
      const tbodyEl = cfg.table.querySelector('tbody');
      if (!tbodyEl) return;
      const missing = [];
      tbodyEl.querySelectorAll('tr').forEach((tr) => {
        if (tr.dataset.state === 'deleted') return;
        cfg.keyFields.forEach((key) => {
          if (key === 'PartNum' || key === 'Revision') return;
          const inp = tr.querySelector(`.cell-edit[name="${key}"]`);
          if (inp && !String(inp.value ?? '').trim()) missing.push(key);
        });
      });
      if (missing.length) {
        notify('error', `請先填寫：${Array.from(new Set(missing)).join(', ')}`);
        return;
      }
      const result = await state.grid.saveChanges();
      if (!result.ok) {
        notify('error', result.text || '儲存失敗');
        return;
      }
      await loadRows();
    };

    const cancelChanges = async () => {
      await loadRows();
    };

    const toggleEdit = (enabled) => {
      if (state.grid) state.grid.toggleEdit(enabled);
      setToolbarEnabled();
    };

    return {
      loadRows,
      addRow,
      deleteRow,
      saveRows,
      cancelChanges,
      selectRow,
      toggleEdit,
      setToolbarEnabled,
      getSelectedIndex: () => state.selectedIndex,
      getSelectedRow: () => (state.selectedIndex >= 0 ? state.rows[state.selectedIndex] : null)
    };
  };

  const applyTopToolbarVisibility = () => {
    const id = (itemId || '').toUpperCase();
    if (id !== 'MG000002' && id !== 'CPN00007') return;
    const keep = new Set([btnTabFirst, btnTabPrev, btnTabNext, btnTabLast, btnQuery, btnAdd, btnDelete, btnEdit]);
    const all = [
      btnTabFirst, btnTabPrev, btnTabNext, btnTabLast, btnQuery, btnEdit, btnCancel, btnAdd,
      btnToFormal, btnDetail, btnHistory, btnToEmo, btnDelete, btnSave
    ];
    all.forEach((btn) => {
      if (!btn) return;
      btn.style.display = keep.has(btn) ? '' : 'none';
    });
    if (btnCopyPN4YX) btnCopyPN4YX.style.display = 'none';
    if (btnUpdateCustPn) btnUpdateCustPn.style.display = 'none';
  };

  const custLookupState = {
    map: null,
    loading: null
  };

  const propLookupState = {
    data: null,
    loading: null
  };

  function positionLookupList(list, anchor) {
    if (!list || !anchor) return;
    const rect = anchor.getBoundingClientRect();
    const spaceBelow = window.innerHeight - rect.bottom;
    const maxHeight = Math.max(140, Math.min(260, spaceBelow - 8));
    const placeBelow = spaceBelow >= 140;
    list.style.position = 'fixed';
    list.style.left = `${Math.max(6, rect.left)}px`;
    list.style.top = placeBelow ? `${rect.bottom}px` : `${Math.max(6, rect.top - maxHeight)}px`;
    list.style.minWidth = `${Math.max(140, rect.width)}px`;
    list.style.maxHeight = `${maxHeight}px`;
  }

  async function buildLookupMapFromApi(table, key, result) {
    if (!table || !key || !result) return new Map();
    const resp = await fetch(`/api/TableFieldLayout/LookupData?table=${encodeURIComponent(table)}&key=${encodeURIComponent(key)}&result=${encodeURIComponent(result)}`);
    if (!resp.ok) return new Map();
    const data = await resp.json();
    const map = new Map();
    const setKeyVariants = (raw, label) => {
      const base = String(raw);
      if (!base) return;
      map.set(base, label);
      const trimmed = base.trim();
      if (trimmed && trimmed !== base) map.set(trimmed, label);
      if (trimmed) {
        map.set(trimmed.toLowerCase(), label);
        map.set(trimmed.toUpperCase(), label);
      }
    };
    (data || []).forEach((row) => {
      const rawKey = row?.key;
      if (rawKey == null) return;
      const keyStr = String(rawKey);
      if (!keyStr) return;
      const labelParts = Object.keys(row || {})
        .filter((p) => p.startsWith('result'))
        .map((p) => row[p])
        .filter((v) => v != null && typeof v !== 'object' && String(v).trim() !== '')
        .map((v) => String(v));
      const label = labelParts.join(' - ');
      setKeyVariants(keyStr, label || keyStr);
    });
    return map;
  }

  async function loadCustCompanyLookup() {
    if (custLookupState.map) return custLookupState.map;
    if (custLookupState.loading) return custLookupState.loading;
    custLookupState.loading = (async () => {
      const res = await fetch(`/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent('MGNdCustPartNum')}&lang=TW`);
      if (!res.ok) return new Map();
      const rows = await res.json();
      const meta = (rows || []).find((r) => {
        const field = (r.FieldName || r.fieldName || '').toString().trim().toLowerCase();
        return field === 'companyid';
      });
      const table = (meta?.LookupTable || meta?.lookupTable || '').toString();
      const key = (meta?.LookupKeyField || meta?.lookupKeyField || '').toString();
      const result = (meta?.LookupResultField || meta?.lookupResultField || '').toString();
      if (!table || !key || !result) return new Map();
      const map = await buildLookupMapFromApi(table, key, result);
      custLookupState.map = map;
      return map;
    })();
    const result = await custLookupState.loading;
    custLookupState.loading = null;
    return result;
  }

  async function loadPropLookups() {
    if (propLookupState.data) return propLookupState.data;
    if (propLookupState.loading) return propLookupState.loading;
    propLookupState.loading = (async () => {
      const res = await fetch(`/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent('MGNdMatInfoDtl')}&lang=TW`);
      if (!res.ok) return { numMap: new Map(), dtlMap: new Map() };
      const rows = await res.json();
      const findMeta = (name) => (rows || []).find((r) => {
        const field = (r.FieldName || r.fieldName || '').toString().trim().toLowerCase();
        return field === name;
      });
      const numMeta = findMeta('numid');
      const dtlMeta = findMeta('dtlnumid');
      const numTable = (numMeta?.LookupTable || numMeta?.lookupTable || '').toString();
      const numKey = (numMeta?.LookupKeyField || numMeta?.lookupKeyField || '').toString();
      const numResult = (numMeta?.LookupResultField || numMeta?.lookupResultField || '').toString();
      const dtlTable = (dtlMeta?.LookupTable || dtlMeta?.lookupTable || '').toString();
      const dtlKey = (dtlMeta?.LookupKeyField || dtlMeta?.lookupKeyField || '').toString();
      const dtlResult = (dtlMeta?.LookupResultField || dtlMeta?.lookupResultField || '').toString();
      const numMap = await buildLookupMapFromApi(numTable, numKey, numResult);
      const dtlMap = await buildLookupMapFromApi(dtlTable, dtlKey, dtlResult);
      const payload = { numMap, dtlMap };
      propLookupState.data = payload;
      return payload;
    })();
    const result = await propLookupState.loading;
    propLookupState.loading = null;
    return result;
  }

  function openCustCompanyLookup(td, input, lookupMap) {
    if (!td || !input || !lookupMap || lookupMap.size === 0) return;
    const existing = document.querySelector('.lookup-dropdown-list');
    if (existing) existing.remove();
    const isReadOnly = input.dataset.readonly === '1' || input.readOnly;
    if (isReadOnly || !isEditMode) return;

    const currentKey = (input.value ?? '').toString().trim();
    const list = document.createElement('div');
    list.className = 'lookup-dropdown-list';

    const addItem = (key, label) => {
      const item = document.createElement('button');
      item.type = 'button';
      item.className = 'lookup-dropdown-item';
      item.dataset.key = key == null ? '' : String(key);
      const suffix = label && label !== key ? ` ${label}` : '';
      item.textContent = `${key ?? ''}${suffix}`.trim() || '(空白)';
      if ((currentKey ?? '') === (key ?? '')) item.classList.add('is-selected');
      list.appendChild(item);
    };

    addItem('', '');
    lookupMap.forEach((label, key) => addItem(key, label));

    const cleanup = () => {
      list.remove();
      document.removeEventListener('mousedown', onDocDown, true);
      window.removeEventListener('resize', onResize, true);
    };

    const applySelection = (key, label) => {
      const tr = td.closest('tr');
      const keyValue = key ?? '';
      input.value = keyValue;
      const view = td.querySelector('.cell-view');
      if (view) view.textContent = keyValue;
      const shortInput = tr?.querySelector('.cell-edit[name="ShortName"]');
      if (shortInput) {
        shortInput.value = label ?? '';
        const shortView = shortInput.closest('td')?.querySelector('.cell-view');
        if (shortView) shortView.textContent = label ?? '';
      }
      if (tr && tr.dataset.state !== 'added') tr.dataset.state = 'modified';
      cleanup();
    };

    const onPick = (e) => {
      const item = e.target.closest('.lookup-dropdown-item');
      if (!item) return;
      e.preventDefault();
      e.stopPropagation();
      const key = item.dataset.key ?? '';
      const label = lookupMap.get(key) ?? '';
      applySelection(key, label);
    };

    const onDocDown = (e) => {
      if (list.contains(e.target)) return;
      cleanup();
    };
    const onResize = () => cleanup();

    list.addEventListener('pointerdown', onPick);
    list.addEventListener('mousedown', onPick);
    list.addEventListener('click', onPick);
    document.body.appendChild(list);
    positionLookupList(list, td);
    document.addEventListener('mousedown', onDocDown, true);
    window.addEventListener('resize', onResize, true);
  }

  if (unitWrap && unitTable) {
    unitGrid = createDetailGridManager({
      tableName: 'MINdMatUnit',
      wrap: unitWrap,
      table: unitTable,
      keyFields: ['PartNum', 'Revision', 'ReplaceUnit', 'UseType'],
      columns: [
        { name: 'ReplaceUnit' },
        { name: 'Ratio' },
        { name: 'UseType' },
        { name: 'Notes' }
      ],
      toolbar: {
        first: btnUnitFirst,
        prev: btnUnitPrev,
        next: btnUnitNext,
        last: btnUnitLast,
        add: btnUnitAdd,
        del: btnUnitDelete,
        save: btnUnitSave,
        cancel: btnUnitCancel,
        refresh: btnUnitRefresh
      }
    });
    detailGrids.push(unitGrid);
  }

  if (custWrap && custTable) {
    custGrid = createDetailGridManager({
      tableName: 'MGNdCustPartNum',
      wrap: custWrap,
      table: custTable,
      keyFields: ['PartNum', 'Revision', 'CompanyId'],
      columns: [
        { name: 'CompanyId' },
        { name: 'ShortName', readonly: true },
        { name: 'CustomerPartNum' },
        { name: 'Description' },
        { name: 'Material' },
        { name: 'EngGauge' },
        { name: 'Specificity' },
        { name: 'TradeNotes' },
        { name: 'ShipNotes' }
      ],
      mapRows: async (rows) => {
        if (!rows.length) return rows;
        const lookupMap = await loadCustCompanyLookup();
        if (!lookupMap || lookupMap.size === 0) return rows;
        return rows.map((row) => {
          const next = { ...row };
          const companyIdRaw = getRowValue(row, 'CompanyId');
          const shortNameRaw = getRowValue(row, 'ShortName');
          const companyId = companyIdRaw == null ? '' : String(companyIdRaw).trim();
          const shortName = shortNameRaw == null ? '' : String(shortNameRaw).trim();
          if (!shortName && companyId && lookupMap.has(companyId)) {
            next.ShortName = lookupMap.get(companyId);
          }
          return next;
        });
      },
      toolbar: {
        first: btnCustFirst,
        prev: btnCustPrev,
        next: btnCustNext,
        last: btnCustLast,
        add: btnCustAdd,
        del: btnCustDelete,
        save: btnCustSave,
        cancel: btnCustCancel,
        refresh: btnCustRefresh
      }
    });
    detailGrids.push(custGrid);

    custTable.addEventListener('dblclick', async (e) => {
      const td = e.target.closest('td');
      if (!td) return;
      const input = td.querySelector('.cell-edit[name="CompanyId"]');
      if (!input) return;
      const lookupMap = await loadCustCompanyLookup();
      if (!lookupMap || lookupMap.size === 0) return;
      openCustCompanyLookup(td, input, lookupMap);
    });
  }

  if (propWrap && propTable) {
    propGrid = createDetailGridManager({
      tableName: 'MGNdMatInfoDtl',
      wrap: propWrap,
      table: propTable,
      keyFields: ['PartNum', 'Revision', 'NumId', 'DtlNumId'],
      columns: [
        { name: 'NumId', readonly: true },
        { name: 'NumName', readonly: true },
        { name: 'DtlNumId', readonly: true },
        { name: 'DtlNumName', readonly: true }
      ],
      mapRows: async (rows) => {
        if (!rows.length) return rows;
        const lookups = await loadPropLookups();
        const numMap = lookups?.numMap || new Map();
        const dtlMap = lookups?.dtlMap || new Map();
        return rows.map((row) => {
          const next = { ...row };
          const numIdRaw = getRowValue(row, 'NumId');
          const dtlIdRaw = getRowValue(row, 'DtlNumId');
          const numId = numIdRaw == null ? '' : String(numIdRaw).trim();
          const dtlId = dtlIdRaw == null ? '' : String(dtlIdRaw).trim();
          const numName = getRowValue(row, 'NumName');
          const dtlName = getRowValue(row, 'DtlNumName');
          if ((!numName || String(numName).trim() === '') && numId && numMap.has(numId)) {
            next.NumName = numMap.get(numId);
          }
          if ((!dtlName || String(dtlName).trim() === '') && dtlId && dtlMap.has(dtlId)) {
            next.DtlNumName = dtlMap.get(dtlId);
          }
          return next;
        });
      },
      toolbar: {}
    });
    detailGrids.push(propGrid);
  }

  if (mapWrap && mapTable) {
    mapGrid = createDetailGridManager({
      tableName: 'MGNdProdMap',
      wrap: mapWrap,
      table: mapTable,
      keyFields: ['PartNum', 'Revision', 'SerialNum'],
      autoIncField: 'SerialNum',
      columns: [
        { name: 'SerialNum', readonly: true },
        { name: 'CrossName' },
        { name: 'Notes' }
      ],
      toolbar: {
        first: btnMapFirst,
        prev: btnMapPrev,
        next: btnMapNext,
        last: btnMapLast,
        add: btnMapAdd,
        del: btnMapDelete,
        save: btnMapSave,
        cancel: btnMapCancel,
        refresh: btnMapRefresh
      }
    });
    detailGrids.push(mapGrid);
  }

  if (specWrap && specTable) {
    specGrid = createDetailGridManager({
      tableName: 'MGNdSpecData',
      wrap: specWrap,
      table: specTable,
      keyFields: ['PartNum', 'Revision', 'DemandId'],
      columns: [
        { name: 'DemandId', readonly: true },
        { name: 'DemandName', readonly: true },
        { name: 'DemandContext' }
      ],
      toolbar: {
        first: btnSpecFirst,
        prev: btnSpecPrev,
        next: btnSpecNext,
        last: btnSpecLast,
        save: btnSpecSave,
        cancel: btnSpecCancel,
        refresh: btnSpecRefresh
      }
    });
    detailGrids.push(specGrid);
  }

  if (kitWrap && kitTable) {
    kitGrid = createDetailGridManager({
      tableName: 'MGNdKitItem',
      wrap: kitWrap,
      table: kitTable,
      keyFields: ['PartNum', 'Revision', 'Item'],
      autoIncField: 'Item',
      columns: [
        { name: 'Item', readonly: true },
        { name: 'KitItemPartNum' },
        { name: 'Lk_MatName', readonly: true },
        { name: 'Lk_Unit', readonly: true },
        { name: 'KitItemQnty' },
        { name: 'Notes' }
      ],
      toolbar: {
        first: btnKitFirst,
        prev: btnKitPrev,
        next: btnKitNext,
        last: btnKitLast,
        add: btnKitAdd,
        del: btnKitDelete,
        save: btnKitSave,
        cancel: btnKitCancel,
        refresh: btnKitRefresh
      }
    });
    detailGrids.push(kitGrid);
  }

  const getUserId = () => {
    return (localStorage.getItem('erpLoginUserId') || window._userId || 'Admin').toString().trim();
  };

  const normalizeFilePath = (value) => {
    if (value == null) return '';
    let text = String(value).trim();
    if ((text.startsWith('"') && text.endsWith('"')) || (text.startsWith("'") && text.endsWith("'"))) {
      text = text.slice(1, -1).trim();
    }
    return text;
  };

  const getMapPathFromRow = (row) => {
    if (!row) return '';
    return normalizeFilePath(getRowValue(row, 'CrossName'));
  };

  const getSelectedMapPath = () => {
    const row = mapGrid?.getSelectedRow?.();
    if (row) return getMapPathFromRow(row);
    return '';
  };

  const setGlyphPreview = (url, errorMsg) => {
    if (!glyphImage || !glyphEmpty) return;
    if (errorMsg) {
      glyphImage.style.display = 'none';
      glyphImage.removeAttribute('src');
      glyphEmpty.textContent = errorMsg;
      glyphEmpty.style.display = '';
      return;
    }
    glyphEmpty.style.display = 'none';
    glyphImage.style.display = '';
    glyphImage.src = url;
  };

  const isSupportedImage = (path) => {
    const lower = path.toLowerCase();
    return lower.endsWith('.jpg') || lower.endsWith('.jpeg') || lower.endsWith('.bmp') || lower.endsWith('.wmf');
  };

  const showGlyphPreview = (path, focusTab) => {
    const filePath = normalizeFilePath(path);
    if (!filePath) {
      notify('error', '沒有圖檔的完整路徑及檔名');
      setGlyphPreview('', '無 圖 檔');
      return;
    }
    if (!isSupportedImage(filePath)) {
      notify('error', '檔案格式只支援 jpg、bmp、wmf');
      setGlyphPreview('', '檔案格式不支援');
      return;
    }
    if (filePath.toLowerCase().endsWith('.wmf')) {
      notify('error', 'WMF 瀏覽器無法預覽，請改用開視窗檢視');
      setGlyphPreview('', 'WMF 無法預覽');
      return;
    }
    if (focusTab) {
      const tabBtn = document.querySelector('.matinfo-tabs [data-bs-target="#tab-glyph"]');
      if (tabBtn) {
        if (window.bootstrap?.Tab) {
          window.bootstrap.Tab.getOrCreateInstance(tabBtn).show();
        } else {
          tabBtn.click();
        }
      }
    }
    const url = `/api/MapPreview/file?path=${encodeURIComponent(filePath)}`;
    setGlyphPreview(url, '');
  };

  const browseHeightStorageKey = itemId ? `matinfo:browse-height:${itemId}` : 'matinfo:browse-height';
  const applyBrowseHeight = (height) => {
    if (!browseScrollBody || !Number.isFinite(height)) return;
    const minHeight = 140;
    const maxHeight = Math.max(minHeight, window.innerHeight - 240);
    const next = Math.max(minHeight, Math.min(maxHeight, Math.round(height)));
    browseScrollBody.style.height = `${next}px`;
    scheduleRender();
    return next;
  };

  const loadBrowseHeight = async () => {
    if (!browseScrollBody) return;
    let height = null;
    try {
      const res = await fetch(`/api/MatInfoUtility/BrowseHeight?itemId=${encodeURIComponent(itemId)}`);
      if (res.ok) {
        const data = await res.json();
        if (Number.isFinite(data?.height)) height = data.height;
      }
    } catch {
      height = null;
    }
    if (!Number.isFinite(height)) {
      const cached = Number(localStorage.getItem(browseHeightStorageKey));
      if (Number.isFinite(cached)) height = cached;
    }
    if (Number.isFinite(height)) applyBrowseHeight(height);
  };

  const saveBrowseHeight = async () => {
    if (!browseScrollBody) return;
    const height = browseScrollBody.clientHeight;
    if (!Number.isFinite(height)) return;
    localStorage.setItem(browseHeightStorageKey, String(height));
    try {
      const res = await fetch('/api/MatInfoUtility/SaveBrowseHeight', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ itemId, height })
      });
      if (!res.ok) {
        notify('error', await res.text());
        return;
      }
      notify('success', '已儲存設定');
    } catch (err) {
      notify('error', err?.message || '儲存失敗');
    }
  };

  const initBrowseResizer = () => {
    if (!browseResizer || !browseScrollBody) return;
    let startY = 0;
    let startHeight = 0;
    const onMove = (e) => {
      const delta = e.clientY - startY;
      applyBrowseHeight(startHeight + delta);
    };
    const onUp = () => {
      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup', onUp);
      document.body.style.userSelect = '';
    };
    browseResizer.addEventListener('mousedown', (e) => {
      e.preventDefault();
      startY = e.clientY;
      startHeight = browseScrollBody.clientHeight;
      document.body.style.userSelect = 'none';
      document.addEventListener('mousemove', onMove);
      document.addEventListener('mouseup', onUp);
    });
  };

  const showLogModal = () => {
    if (!logModalEl) return;
    if (logModal) {
      logModal.show();
      return;
    }
    logModalEl.classList.add('show');
    logModalEl.style.display = 'block';
    logModalEl.removeAttribute('aria-hidden');
    logModalEl.setAttribute('aria-modal', 'true');
    if (!logBackdrop) {
      logBackdrop = document.createElement('div');
      logBackdrop.className = 'modal-backdrop fade show';
      document.body.appendChild(logBackdrop);
    }
  };

  const hideLogModal = () => {
    if (!logModalEl) return;
    if (logModal) {
      logModal.hide();
      return;
    }
    logModalEl.classList.remove('show');
    logModalEl.style.display = 'none';
    logModalEl.setAttribute('aria-hidden', 'true');
    logModalEl.removeAttribute('aria-modal');
    if (logBackdrop) {
      logBackdrop.remove();
      logBackdrop = null;
    }
  };

  function setValue(el, value) {
    if (!el) return;
    if (el.type === 'checkbox') {
      el.checked = value === true || value === 1 || value === '1';
      return;
    }
    el.value = value ?? '';
  }

  function fillForm(record) {
    if (!record) return;
    form.querySelectorAll('[data-field]').forEach((el) => {
      const field = el.getAttribute('data-field');
      if (!field) return;
      setValue(el, record[field] ?? record[field.toLowerCase()] ?? record[field.toUpperCase()]);
    });
  }

  function clearForm() {
    form.querySelectorAll('[data-field]').forEach((el) => {
      if (el.type === 'checkbox') {
        el.checked = false;
      } else {
        el.value = '';
      }
    });
  }

  function setFormEditable(enabled) {
    form.querySelectorAll('[data-field]').forEach((el) => {
      if (el instanceof HTMLInputElement || el instanceof HTMLTextAreaElement) {
        if (el.readOnly || el.hasAttribute('readonly')) return;
        el.disabled = !enabled;
        return;
      }
      if (el instanceof HTMLSelectElement) {
        el.disabled = !enabled;
      }
    });
  }

  function setEditMode(enabled) {
    isEditMode = !!enabled;
    setFormEditable(isEditMode);
    setButtonText(btnEdit, isEditMode ? '保留' : '修改');
    if (modeEl) modeEl.textContent = isEditMode ? '編輯模式' : '瀏覽模式';
    if (countBox) {
      countBox.classList.toggle('mode-edit', isEditMode);
      countBox.classList.toggle('mode-view', !isEditMode);
    }
    if (isEditMode) {
      if (!preserveCancelSnapshot) {
        cancelSnapshot = getFormData();
      }
      preserveCancelSnapshot = false;
    } else {
      cancelSnapshot = null;
      preserveCancelSnapshot = false;
    }
    if (btnSave) btnSave.hidden = isEditMode;
    if (btnCancel) btnCancel.hidden = !isEditMode;
    const lockButtons = [btnQuery, btnAdd, btnToFormal, btnDetail, btnHistory, btnToEmo, btnDelete, btnSave, btnTabFirst, btnTabPrev, btnTabNext, btnTabLast];
    lockButtons.forEach((btn) => { if (btn) btn.disabled = isEditMode; });
    document.querySelector('.matinfo-page')?.classList.toggle('is-edit-mode', isEditMode);
    detailGrids.forEach((grid) => grid.toggleEdit(isEditMode));
  }

  function parseNumber(input) {
    const cleaned = (input ?? '').toString().trim().replace(/,/g, '');
    if (!cleaned) return null;
    if (!/^-?\d+(\.\d+)?$/.test(cleaned)) return null;
    const num = Number(cleaned);
    return Number.isFinite(num) ? num : null;
  }

  function coerceFormValue(field, rawValue, isCheckbox, recordValue) {
    if (isCheckbox) return rawValue ? 1 : 0;
    const value = (rawValue ?? '').toString().trim();
    const key = (field || '').toLowerCase();
    const type = formDictTypeMap.get(key) || '';
    const isRecordNumber = typeof recordValue === 'number' && !Number.isNaN(recordValue);
    const isNumeric = numericTypes.has(type) || forcedNumericFields.has(key) || isRecordNumber;
    const isDate = dateTypes.has(type);
    if (!value) {
      if (isNumeric || isDate) return null;
      return '';
    }
    if (isNumeric) {
      return parseNumber(value);
    }
    return value;
  }

  function getFormData() {
    const data = {};
    form.querySelectorAll('[data-field]').forEach((el) => {
      const field = el.getAttribute('data-field');
      if (!field) return;
      const recordValue = currentRecord ? getValue(currentRecord, field) : null;
      if (el.type === 'checkbox') {
        data[field] = coerceFormValue(field, el.checked, true, recordValue);
        return;
      }
      data[field] = coerceFormValue(field, el.value ?? '', false, recordValue);
    });
    return data;
  }

  function setKeyFieldsEditable(enabled) {
    const partnum = form.querySelector('[data-field="Partnum"]');
    const revision = form.querySelector('[data-field="Revision"]');
    if (partnum) partnum.readOnly = !enabled;
    if (revision) revision.readOnly = !enabled;
  }

  function formatValue(value) {
    if (value === null || value === undefined) return '';
    if (typeof value === 'object') return JSON.stringify(value);
    return `${value}`;
  }

  function formatDateByPattern(date, pattern) {
    if (!(date instanceof Date) || isNaN(date.getTime())) return '';
    const pad = (n) => `${n}`.padStart(2, '0');
    return pattern
      .replace(/yyyy/g, `${date.getFullYear()}`)
      .replace(/MM/g, pad(date.getMonth() + 1))
      .replace(/dd/g, pad(date.getDate()))
      .replace(/HH/g, pad(date.getHours()))
      .replace(/mm/g, pad(date.getMinutes()))
      .replace(/ss/g, pad(date.getSeconds()));
  }

  function formatByFormatStr(value, fmt) {
    if (!fmt) return null;
    const raw = value ?? '';
    if (!raw) return null;
    const dt = new Date(raw);
    if (isNaN(dt.getTime())) return null;
    return formatDateByPattern(dt, fmt);
  }

  const checkboxFields = new Set(['IsTrans', 'IsEMO', 'Status', 'MB', 'StopOrder']);
  function isCheckboxColumn(col) {
    return col?.comboStyle === 1 || (col?.comboStyle == null && checkboxFields.has(col?.key));
  }
  function formatCellHtml(col, value) {
    if (isCheckboxColumn(col)) {
      const checked = value === true || value === 1 || value === '1' || value === 'Y' || value === 'y';
      return `<input type="checkbox" disabled ${checked ? 'checked' : ''}>`;
    }
    const formatted = formatByFormatStr(value, col?.formatStr);
    if (formatted !== null) return formatted;
    return formatValue(value);
  }

  function getValue(item, key) {
    if (!item || !key) return '';
    if (item[key] !== undefined) return item[key];
    const keyLower = key.toLowerCase();
    for (const k of Object.keys(item)) {
      if (k.toLowerCase() === keyLower) return item[k];
    }
    return '';
  }

  function setButtonText(btn, text) {
    if (!btn) return;
    const icon = btn.querySelector('i');
    btn.textContent = text;
    if (icon) {
      btn.prepend(icon);
      btn.insertBefore(document.createTextNode(' '), icon.nextSibling);
    }
  }

  function renderLogTable(tableEl, rows, emptyText) {
    if (!tableEl) return;
    const thead = tableEl.querySelector('thead');
    const tbodyEl = tableEl.querySelector('tbody');
    const list = Array.isArray(rows) ? rows : [];
    if (!list.length) {
      if (thead) thead.innerHTML = '';
      if (tbodyEl) tbodyEl.innerHTML = `<tr><td>${emptyText}</td></tr>`;
      return;
    }
    const keys = Object.keys(list[0] || {});
    if (thead) {
      thead.innerHTML = `<tr>${keys.map(k => `<th>${k}</th>`).join('')}</tr>`;
    }
    if (tbodyEl) {
      tbodyEl.innerHTML = '';
      list.forEach((row) => {
        const tr = document.createElement('tr');
        keys.forEach((key) => {
          const td = document.createElement('td');
          td.textContent = formatValue(getValue(row, key));
          tr.appendChild(td);
        });
        tbodyEl.appendChild(tr);
      });
    }
  }

  async function loadDict() {
    if (dictCache.fields) return dictCache.fields;
    try {
      const res = await fetch(`/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(dictTableName)}&lang=TW`);
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      dictCache.fields = Array.isArray(data) ? data : [];
      dictTypeMap.clear();
      dictCache.fields.forEach((f) => {
        const key = (f.FieldName || f.fieldName || '').toString().trim().toLowerCase();
        const type = (f.DataType || f.dataType || '').toString().trim().toLowerCase();
        if (key) dictTypeMap.set(key, type);
      });
      return dictCache.fields;
    } catch (err) {
      dictCache.fields = [];
      dictTypeMap.clear();
      return dictCache.fields;
    }
  }

  async function loadFormDict() {
    if (formDictCache.fields && formDictCache.fields.length) return formDictCache.fields;
    try {
      const res = await fetch(`/api/TableFieldLayout/GetTableFieldsFull?table=${encodeURIComponent(dictPanelTableName)}&lang=TW`);
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      const fields = Array.isArray(data) ? data : [];
      if (!fields.length) return fields;
      formDictCache.fields = fields;
      formDictTypeMap.clear();
      formDictCache.fields.forEach((f) => {
        const key = (f.FieldName || f.fieldName || '').toString().trim().toLowerCase();
        const type = (f.DataType || f.dataType || '').toString().trim().toLowerCase();
        if (key) formDictTypeMap.set(key, type);
      });
      renderBasic2Fields(formDictCache.fields);
      applyPanelDict(formDictCache.fields);
      return formDictCache.fields;
    } catch (err) {
      formDictTypeMap.clear();
      return [];
    }
  }

  function buildDictMap(rows) {
    const map = new Map();
    (rows || []).forEach((f) => {
      const key = (f.FieldName || f.fieldName || '').toString().trim();
      if (!key) return;
      map.set(key.toLowerCase(), {
        name: key,
        label: (f.DisplayLabel || f.displayLabel || key).toString(),
        visible: Number(f.Visible ?? f.visible ?? 1) === 1,
        readOnly: Number(f.ReadOnly ?? f.readOnly ?? 0) === 1,
        serial: Number(f.SerialNum ?? f.serialNum ?? 9999),
        layRow: Number(f.iLayRow ?? f.iLayrow ?? f.iLayRow ?? 0),
        layCol: Number(f.iLayColumn ?? f.iLaycolumn ?? f.iLayCol ?? 0),
        combo: Number(f.ComboStyle ?? f.comboStyle ?? 0) === 1
      });
    });
    return map;
  }

  function findLabelForField(input) {
    if (!input) return null;
    const parent = input.parentElement;
    if (!parent) return null;
    const prev = input.previousElementSibling;
    if (prev && prev.tagName === 'LABEL') return prev;
    return null;
  }

  function setCheckboxLabelText(labelEl, text) {
    if (!labelEl) return;
    const inputEl = labelEl.querySelector('input[type="checkbox"]');
    labelEl.textContent = '';
    if (inputEl) labelEl.appendChild(inputEl);
    labelEl.appendChild(document.createTextNode(` ${text}`));
  }

  function getRowValue(row, key) {
    if (!row || !key) return null;
    if (row[key] !== undefined) return row[key];
    const lower = key.toLowerCase();
    const hit = Object.keys(row).find(k => k.toLowerCase() === lower);
    return hit ? row[hit] : null;
  }

  function readNum(row, key) {
    const v = getRowValue(row, key);
    if (v === null || v === undefined || v === '') return null;
    const n = Number(v);
    return Number.isFinite(n) ? n : null;
  }

  function getLayoutPair(row) {
    const fieldTop = readNum(row, 'iFieldTop');
    const fieldLeft = readNum(row, 'iFieldLeft');
    const fieldWidth = readNum(row, 'iFieldWidth');
    const fieldHeight = readNum(row, 'iFieldHeight');
    const labTop = readNum(row, 'iLabTop');
    const labLeft = readNum(row, 'iLabLeft');
    const labWidth = readNum(row, 'iLabWidth');
    const labHeight = readNum(row, 'iLabHeight');
    const showWhere = readNum(row, 'iShowWhere');
    return {
      field: {
        top: fieldTop,
        left: fieldLeft,
        width: fieldWidth,
        height: fieldHeight
      },
      label: {
        top: labTop,
        left: labLeft,
        width: labWidth,
        height: labHeight
      },
      showWhere
    };
  }

  const panelLayoutTweaks = {
    MGN_MINdMatInfo2PNL: {
      rowGap: 10,
      fieldLeftPad: 8,
      fieldWidthPad: 10,
      labelWidthPad: 8,
      checkboxLeftShift: -70
    }
  };

  function adjustPanelLayout(row, layout, isCheckbox) {
    if (!layout) return layout;
    const tweaks = panelLayoutTweaks[dictPanelTableName];
    if (!tweaks) return layout;
    const adjusted = {
      field: { ...layout.field },
      label: { ...layout.label },
      showWhere: layout.showWhere
    };
    const rowIndex = readNum(row, 'iLayRow');
    if (rowIndex && rowIndex > 0 && tweaks.rowGap) {
      const rowOffset = (rowIndex - 1) * tweaks.rowGap;
      if (adjusted.field.top != null) adjusted.field.top += rowOffset;
      if (adjusted.label.top != null) adjusted.label.top += rowOffset;
    }
    if (tweaks.fieldLeftPad && adjusted.field.left != null) adjusted.field.left += tweaks.fieldLeftPad;
    if (tweaks.fieldWidthPad && adjusted.field.width != null) adjusted.field.width += tweaks.fieldWidthPad;
    if (tweaks.labelWidthPad && adjusted.label.width != null) adjusted.label.width += tweaks.labelWidthPad;
    if (isCheckbox && tweaks.checkboxLeftShift && adjusted.field.left != null) {
      adjusted.field.left += tweaks.checkboxLeftShift;
    }
    return adjusted;
  }

  function isLayoutBlank(layout) {
    if (!layout) return true;
    const vals = [
      layout.field.top, layout.field.left, layout.field.width, layout.field.height,
      layout.label.top, layout.label.left, layout.label.width, layout.label.height
    ];
    return vals.every(v => {
      const n = Number(v ?? 0);
      return !Number.isFinite(n) || n <= 0;
    });
  }

  function compareByLay(a, b) {
    const ar = readNum(a, 'iLayRow');
    const br = readNum(b, 'iLayRow');
    const ac = readNum(a, 'iLayColumn');
    const bc = readNum(b, 'iLayColumn');
    if (ar != null && br != null && ar !== br) return ar - br;
    if (ac != null && bc != null && ac !== bc) return ac - bc;
    if (ar != null && br == null) return -1;
    if (ar == null && br != null) return 1;
    if (ac != null && bc == null) return -1;
    if (ac == null && bc != null) return 1;
    return (a.SerialNum ?? a.serialNum ?? 9999) - (b.SerialNum ?? b.serialNum ?? 9999);
  }

  function buildFieldElements(row) {
    const fieldName = (row.FieldName || row.fieldName || '').toString().trim();
    const labelText = (row.DisplayLabel || row.displayLabel || fieldName).toString();
    const isCheck = Number(row.ComboStyle ?? row.comboStyle ?? 0) === 1;
    const readOnly = Number(row.ReadOnly ?? row.readOnly ?? 0) === 1;
    const label = document.createElement('label');
    label.textContent = labelText;
    const input = document.createElement('input');
    input.setAttribute('data-field', fieldName);
    if (isCheck) input.type = 'checkbox';
    if (readOnly) {
      if (input.type === 'checkbox') input.disabled = true;
      else input.readOnly = true;
    }
    return { label, input, isCheck };
  }

  function applyAbsStyle(el, pos, extraClass, topPad = 4) {
    if (!el || !pos) return null;
    if (pos.top === null && pos.left === null && pos.width === null && pos.height === null) return null;
    el.classList.add('abs-item');
    if (extraClass) el.classList.add(extraClass);
    el.style.position = 'absolute';
    const isField = el.matches && el.matches('input, select, textarea');
    const fieldGap = isField ? 6 : 0;
    if (pos.top !== null) el.style.top = `${pos.top + topPad}px`;
    if (pos.left !== null) el.style.left = `${pos.left + fieldGap}px`;
    if (pos.width !== null) el.style.width = `${Math.max(0, pos.width - fieldGap)}px`;
    if (pos.height !== null) el.style.height = `${pos.height}px`;
    const estHeight = pos.height ?? (el.tagName === 'LABEL' ? 22 : 24);
    const estTop = (pos.top ?? 0) + topPad;
    return estTop + estHeight;
  }

  function splitPanelFields(rows) {
    const list = (rows || []).filter(r => (r.Visible ?? r.visible ?? 1) == 1);
    const hasShowWhere = list.some(r => (readNum(r, 'iShowWhere') ?? 0) > 1);
    const basic1Fields = new Set();
    basic1Grid?.querySelectorAll('[data-field]').forEach((el) => {
      const field = (el.getAttribute('data-field') || '').toLowerCase();
      if (field) basic1Fields.add(field);
    });

    if (hasShowWhere) {
      const basic1 = list.filter(r => (readNum(r, 'iShowWhere') ?? 0) <= 1);
      const basic2 = list.filter(r => (readNum(r, 'iShowWhere') ?? 0) === 2);
      return { basic1, basic2 };
    }

    const basic1 = list.filter((r) => {
      const fieldName = (r.FieldName || r.fieldName || '').toString().trim().toLowerCase();
      return fieldName && basic1Fields.has(fieldName);
    });
    const basic2 = list.filter((r) => {
      const fieldName = (r.FieldName || r.fieldName || '').toString().trim().toLowerCase();
      return fieldName && !basic1Fields.has(fieldName);
    });
    return { basic1, basic2 };
  }

  function resetBasic1Abs() {
    if (!basic1Grid || !basic1Abs) return;
    while (basic1Abs.firstChild) {
      basic1Grid.appendChild(basic1Abs.firstChild);
    }
  }

  function applyBasic1AbsLayout(rows) {
    if (!basic1Grid || !basic1Abs) return;
    resetBasic1Abs();
    basic1Abs.innerHTML = '';
    basic1Abs.style.minHeight = '';
    basic1Grid.style.display = '';
    const map = new Map();
    const existing = new Set();
    basic1Grid.querySelectorAll('[data-field]').forEach((el) => {
      const field = (el.getAttribute('data-field') || '').toLowerCase();
      if (field) existing.add(field);
    });
    (rows || []).forEach((row) => {
      const name = (row.FieldName || row.fieldName || '').toString().trim().toLowerCase();
      if (name) map.set(name, row);
    });

    let maxBottom = 0;
    let movedControls = 0;
    let gridAdded = 0;
    const moved = new Set();
    basic1Grid.querySelectorAll('[data-field]').forEach((el) => {
      const field = (el.getAttribute('data-field') || '').toLowerCase();
      if (!field) return;
      const row = map.get(field);
      if (!row) return;
      const layout = adjustPanelLayout(row, getLayoutPair(row), el.type === 'checkbox');
      if (isLayoutBlank(layout)) return;
      const isCheckbox = el.type === 'checkbox';
      const labelEl = isCheckbox ? el.closest('label') : findLabelForField(el);
      if (labelEl && !moved.has(labelEl)) {
        if (isCheckbox && !labelEl.contains(el)) labelEl.appendChild(el);
        const usePos = isCheckbox ? layout.field : layout.label;
        const bottom = applyAbsStyle(labelEl, usePos);
        if (bottom) maxBottom = Math.max(maxBottom, bottom);
        basic1Abs.appendChild(labelEl);
        moved.add(labelEl);
      }
      if (!isCheckbox && !moved.has(el)) {
        const bottom = applyAbsStyle(el, layout.field);
        if (bottom) maxBottom = Math.max(maxBottom, bottom);
        basic1Abs.appendChild(el);
        moved.add(el);
        movedControls += 1;
      } else if (isCheckbox && labelEl) {
        labelEl.classList.add('abs-checkbox-label');
        movedControls += 1;
      }
    });

    const missingRows = (rows || [])
      .filter((row) => {
        const fieldName = (row.FieldName || row.fieldName || '').toString().trim().toLowerCase();
        return fieldName && !existing.has(fieldName);
      })
      .sort(compareByLay);

    missingRows.forEach((row) => {
      const { label, input, isCheck } = buildFieldElements(row);
      const layout = adjustPanelLayout(row, getLayoutPair(row), isCheck);
      if (isLayoutBlank(layout)) {
        basic1Grid.appendChild(label);
        basic1Grid.appendChild(input);
        if (isCheck) label.classList.add('abs-checkbox-label');
        gridAdded += 1;
        return;
      }
      if (isCheck) {
        const text = label.textContent;
        label.textContent = '';
        label.appendChild(input);
        label.appendChild(document.createTextNode(` ${text || ''}`));
        label.classList.add('abs-checkbox-label');
        const labelBottom = applyAbsStyle(label, layout.field);
        basic1Abs.appendChild(label);
        maxBottom = Math.max(maxBottom, labelBottom || 0);
      } else {
        const labelBottom = applyAbsStyle(label, layout.label);
        const inputBottom = applyAbsStyle(input, layout.field);
        basic1Abs.appendChild(label);
        basic1Abs.appendChild(input);
        maxBottom = Math.max(maxBottom, labelBottom || 0, inputBottom || 0);
      }
      movedControls += 1;
    });

    if (basic1Abs.children.length > 0) {
      const totalControls = basic1Grid.querySelectorAll('[data-field]').length;
      if (movedControls >= totalControls && gridAdded === 0) {
        basic1Grid.style.display = 'none';
      }
      if (maxBottom > 0) basic1Abs.style.minHeight = `${maxBottom + 12}px`;
    }
  }

  function renderBasic2Fields(rows) {
    if (!basic2Grid || !basic2Abs) return;
    basic2Grid.innerHTML = '';
    basic2Abs.innerHTML = '';
    basic2Abs.style.minHeight = '';

    const { basic1, basic2 } = splitPanelFields(rows);
    applyBasic1AbsLayout(basic1);

    let maxBottom = 0;
    let absAdded = 0;
    let gridAdded = 0;
    let added = 0;
    const list = (basic2 || []).sort(compareByLay);
      list.forEach((row) => {
        const fieldName = (row.FieldName || row.fieldName || '').toString().trim();
        if (!fieldName) return;
        const { label, input, isCheck } = buildFieldElements(row);
        const layout = adjustPanelLayout(row, getLayoutPair(row), isCheck);
        if (isLayoutBlank(layout)) {
          basic2Grid.appendChild(label);
          basic2Grid.appendChild(input);
          if (isCheck) label.classList.add('abs-checkbox-label');
          added += 1;
          gridAdded += 1;
          return;
        }
        if (isCheck) {
          const text = label.textContent;
          label.textContent = '';
        label.appendChild(input);
        label.appendChild(document.createTextNode(` ${text || ''}`));
        label.classList.add('abs-checkbox-label');
        const labelBottom = applyAbsStyle(label, layout.field);
        basic2Abs.appendChild(label);
        maxBottom = Math.max(maxBottom, labelBottom || 0);
      } else {
        const labelBottom = applyAbsStyle(label, layout.label);
        const inputBottom = applyAbsStyle(input, layout.field);
        basic2Abs.appendChild(label);
        basic2Abs.appendChild(input);
        maxBottom = Math.max(maxBottom, labelBottom || 0, inputBottom || 0);
      }
      absAdded += 1;
      added += 1;
    });
    if (basic2Empty) basic2Empty.style.display = added ? 'none' : '';
    if (absAdded > 0 && maxBottom > 0) basic2Abs.style.minHeight = `${maxBottom + 12}px`;
  }

  function applyPanelDict(rows) {
    const map = buildDictMap(rows);
    const panel = document.querySelector('.matinfo-form');
    if (!panel) return;

    panel.querySelectorAll('[data-field]').forEach((el) => {
      const field = (el.getAttribute('data-field') || '').toLowerCase();
      if (!field) return;
      const cfg = map.get(field);
      if (!cfg) return;
      const label = findLabelForField(el);
      if (label) label.textContent = cfg.label;
      if (el.type === 'checkbox') {
        const checkboxLabel = el.closest('label') || label;
        if (checkboxLabel) setCheckboxLabelText(checkboxLabel, cfg.label);
      }
      if (cfg.readOnly) {
        if (el instanceof HTMLSelectElement) el.disabled = true;
        else if (el instanceof HTMLInputElement || el instanceof HTMLTextAreaElement) el.readOnly = true;
      }
      if (!cfg.visible) {
        if (label) label.style.display = 'none';
        el.style.display = 'none';
      } else {
        if (label) label.style.display = '';
        el.style.display = '';
      }
    });

    panel.querySelectorAll('.matinfo-checks').forEach((box) => {
      const labels = Array.from(box.querySelectorAll('label'));
      labels.forEach((label) => {
        const input = label.querySelector('[data-field]');
        const key = (input?.getAttribute('data-field') || '').toLowerCase();
        const cfg = map.get(key);
        label.style.display = cfg?.visible === false ? 'none' : '';
      });
    });

    panel.querySelectorAll('.matinfo-grid').forEach((grid) => {
      const children = Array.from(grid.children);
      for (let i = 0; i < children.length; i += 1) {
        const label = children[i];
        const fieldEl = children[i + 1];
        if (!label || !fieldEl || label.tagName !== 'LABEL') continue;
        const dataEl = fieldEl.matches('[data-field]')
          ? fieldEl
          : fieldEl.querySelector?.('[data-field]');
        const field = (dataEl?.getAttribute('data-field') || '').toLowerCase();
        const cfg = map.get(field);
        label.style.display = cfg?.visible === false ? 'none' : '';
        fieldEl.style.display = cfg?.visible === false ? 'none' : '';
        if (cfg?.layRow > 0 && cfg?.layCol > 0) {
          const labelCol = (cfg.layCol - 1) * 2 + 1;
          const fieldCol = labelCol + 1;
          label.style.gridColumn = `${labelCol} / ${labelCol + 1}`;
          fieldEl.style.gridColumn = `${fieldCol} / ${fieldCol + 1}`;
          label.style.gridRow = `${cfg.layRow}`;
          fieldEl.style.gridRow = `${cfg.layRow}`;
        }
        i += 1;
      }
    });
  }

  const scrollSyncState = new WeakMap();
  function syncScroll(topEl, bodyEl) {
    if (!topEl || !bodyEl) return;
    const inner = topEl.firstElementChild;
    if (!inner) return;
    const bodyTable = () => bodyEl.querySelector('table') || bodyEl;
    const updateWidth = () => {
      inner.style.width = `${bodyTable().scrollWidth}px`;
      topEl.scrollLeft = bodyEl.scrollLeft;
    };

    if (!scrollSyncState.has(topEl)) {
      let lock = false;
      topEl.addEventListener('scroll', () => {
        if (lock) return;
        lock = true;
        bodyEl.scrollLeft = topEl.scrollLeft;
        lock = false;
      });
      bodyEl.addEventListener('scroll', () => {
        if (lock) return;
        lock = true;
        topEl.scrollLeft = bodyEl.scrollLeft;
        lock = false;
      });
      window.addEventListener('resize', updateWidth);
      scrollSyncState.set(topEl, updateWidth);
    }
    updateWidth();
  }

  function buildColumnsFromData(rows) {
    const keySet = new Set();
    rows.forEach((row) => {
      if (!row) return;
      Object.keys(row).forEach((key) => keySet.add(key));
    });

    const ordered = preferredColumns.filter((key) => keySet.has(key));
    const rest = [...keySet].filter((key) => !ordered.includes(key));
    rest.sort((a, b) => a.localeCompare(b));
    return [...ordered, ...rest].map((key) => ({
      key,
      label: columnLabels[key] ?? key,
      width: null,
      comboStyle: checkboxFields.has(key) ? 1 : 0,
      formatStr: null,
      readOnly: false
    }));
  }

  function loadColumnWidthMap() {
    if (!columnWidthStorageKey) return {};
    try {
      const raw = localStorage.getItem(columnWidthStorageKey);
      if (!raw) return {};
      const data = JSON.parse(raw);
      return data && typeof data === 'object' ? data : {};
    } catch {
      return {};
    }
  }

  function saveColumnWidthMap(map) {
    if (!columnWidthStorageKey) return;
    try {
      localStorage.setItem(columnWidthStorageKey, JSON.stringify(map));
    } catch {
      // ignore storage errors
    }
  }

  function applyStoredColumnWidths() {
    if (!enableColumnResize) return;
    const widthMap = loadColumnWidthMap();
    columns.forEach((col) => {
      const key = col?.key;
      const value = key ? Number(widthMap[key]) : null;
      if (key && Number.isFinite(value) && value > 20) {
        col.width = `${Math.round(value)}px`;
      }
    });
  }

  function updateColumnWidthAt(index, width, persist) {
    if (!columns[index] || !theadRow) return;
    const next = Math.max(40, Math.round(width));
    columns[index].width = `${next}px`;
    const th = theadRow.querySelectorAll('th')[index];
    if (th) th.style.width = columns[index].width;
    if (tbody) {
      const rows = tbody.querySelectorAll('tr');
      rows.forEach((row) => {
        const cells = row.children;
        if (!cells || !cells.length) return;
        const cell = cells[index];
        if (cell) cell.style.width = columns[index].width;
      });
    }
    if (persist) {
      const map = loadColumnWidthMap();
      const key = columns[index].key;
      if (key) {
        map[key] = next;
        saveColumnWidthMap(map);
      }
    }
  }

  function getDetailTableStorageKey(table) {
    if (!enableColumnResize || !table) return null;
    const key = table.dataset.resizeKey || table.id;
    if (!key) return null;
    return `matinfo:detail-col-widths:${itemIdUpper}:${key}`;
  }

  function ensureDetailResizeKey(table) {
    if (!table || table.dataset.resizeKey) return;
    if (table.id) {
      table.dataset.resizeKey = table.id;
      return;
    }
    const pane = table.closest('.tab-pane');
    const paneId = pane?.id || 'pane';
    const list = pane ? Array.from(pane.querySelectorAll('table')) : Array.from(document.querySelectorAll('.matinfo-form table'));
    const idx = list.indexOf(table);
    table.dataset.resizeKey = `${paneId}-${idx >= 0 ? idx : list.length}`;
  }

  function loadDetailWidthMap(table) {
    const storageKey = getDetailTableStorageKey(table);
    if (!storageKey) return {};
    try {
      const raw = localStorage.getItem(storageKey);
      if (!raw) return {};
      const data = JSON.parse(raw);
      return data && typeof data === 'object' ? data : {};
    } catch {
      return {};
    }
  }

  function saveDetailWidthMap(table, map) {
    const storageKey = getDetailTableStorageKey(table);
    if (!storageKey) return;
    try {
      localStorage.setItem(storageKey, JSON.stringify(map));
    } catch {
      // ignore storage errors
    }
  }

  function applyDetailTableWidths(table) {
    if (!enableColumnResize || !table) return;
    const widthMap = loadDetailWidthMap(table);
    const ths = Array.from(table.querySelectorAll('thead th'));
    if (!ths.length) return;
    ths.forEach((th, idx) => {
      const value = Number(widthMap[idx]);
      if (!Number.isFinite(value) || value <= 20) return;
      const width = `${Math.round(value)}px`;
      th.style.width = width;
      table.querySelectorAll('tbody tr').forEach((row) => {
        const cell = row.children[idx];
        if (cell) cell.style.width = width;
      });
    });
    lockDetailTableWidths(table);
    refreshDetailTableWidth(table);
  }

  function lockDetailTableWidths(table) {
    if (!table || table.dataset.fixedLayout === '1') return;
    const ths = Array.from(table.querySelectorAll('thead th'));
    if (!ths.length) return;
    let total = 0;
    ths.forEach((th, idx) => {
      const width = Math.round(th.getBoundingClientRect().width || th.offsetWidth || 0);
      if (width <= 0) return;
      th.style.width = `${width}px`;
      table.querySelectorAll('tbody tr').forEach((row) => {
        const cell = row.children[idx];
        if (cell) cell.style.width = `${width}px`;
      });
      total += width;
    });
    if (total <= 0) return;
    table.style.width = `${total}px`;
    table.style.tableLayout = 'fixed';
    table.dataset.fixedLayout = '1';
  }

  function refreshDetailTableWidth(table) {
    if (!table) return;
    const ths = Array.from(table.querySelectorAll('thead th'));
    if (!ths.length) return;
    const total = ths.reduce((sum, th) => {
      const width = Math.round(th.getBoundingClientRect().width || th.offsetWidth || 0);
      return width > 0 ? sum + width : sum;
    }, 0);
    if (total > 0) table.style.width = `${total}px`;
  }

  function updateDetailColumnWidth(table, index, width, persist) {
    if (!enableColumnResize || !table) return;
    const ths = table.querySelectorAll('thead th');
    if (!ths || !ths[index]) return;
    const next = Math.max(20, Math.round(width));
    const th = ths[index];
    const widthValue = `${next}px`;
    th.style.width = widthValue;
    table.querySelectorAll('tbody tr').forEach((row) => {
      const cell = row.children[index];
      if (cell) cell.style.width = widthValue;
    });
    refreshDetailTableWidth(table);
    if (persist) {
      const map = loadDetailWidthMap(table);
      map[index] = next;
      saveDetailWidthMap(table, map);
    }
  }

  function attachDetailTableResizers(table) {
    if (!enableColumnResize || !table) return;
    table.classList.add('matinfo-resizable');
    lockDetailTableWidths(table);
    const ths = Array.from(table.querySelectorAll('thead th'));
    if (!ths.length) return;
    ths.forEach((th, idx) => {
      const existing = th.querySelector('.col-resizer');
      if (existing) return;
      const handle = document.createElement('span');
      handle.className = 'col-resizer';
      handle.addEventListener('click', (e) => e.stopPropagation());
      handle.addEventListener('mousedown', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const startX = e.clientX;
        const startWidth = th.getBoundingClientRect().width || th.offsetWidth || 0;
        const onMove = (ev) => {
          const delta = ev.clientX - startX;
          updateDetailColumnWidth(table, idx, startWidth + delta, false);
        };
        const onUp = () => {
          const finalWidth = th.getBoundingClientRect().width || th.offsetWidth || startWidth;
          updateDetailColumnWidth(table, idx, finalWidth, true);
          document.removeEventListener('mousemove', onMove);
          document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
      });
      th.appendChild(handle);
    });
  }

  function initDetailTableResizers() {
    if (!enableColumnResize) return;
    const tables = Array.from(document.querySelectorAll('.matinfo-form table'));
    tables.forEach((table, idx) => {
      if (!table.querySelector('thead th')) return;
      ensureDetailResizeKey(table);
      applyDetailTableWidths(table);
      attachDetailTableResizers(table);
    });
  }

  function attachColumnResizers() {
    if (!enableColumnResize || !theadRow) return;
    const ths = Array.from(theadRow.querySelectorAll('th'));
    ths.forEach((th, idx) => {
      const existing = th.querySelector('.col-resizer');
      if (existing) existing.remove();
      const handle = document.createElement('span');
      handle.className = 'col-resizer';
      handle.addEventListener('click', (e) => e.stopPropagation());
      handle.addEventListener('mousedown', (e) => {
        e.preventDefault();
        e.stopPropagation();
        const startX = e.clientX;
        const startWidth = th.getBoundingClientRect().width || th.offsetWidth || 0;
        const onMove = (ev) => {
          const delta = ev.clientX - startX;
          updateColumnWidthAt(idx, startWidth + delta, false);
        };
        const onUp = () => {
          updateColumnWidthAt(idx, th.getBoundingClientRect().width || th.offsetWidth || startWidth, true);
          document.removeEventListener('mousemove', onMove);
          document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
      });
      th.appendChild(handle);
    });
  }

  async function buildColumns(rows) {
    const dict = await loadDict();
    const keySet = new Set();
    rows.forEach((row) => {
      if (!row) return;
      Object.keys(row).forEach((key) => keySet.add(key));
    });

    if (dict && dict.length) {
      const dictCols = dict
        .filter(d => (d.Visible ?? d.visible ?? 1) == 1)
        .sort((a, b) => (a.SerialNum ?? a.serialNum ?? 9999) - (b.SerialNum ?? b.serialNum ?? 9999))
        .map(d => {
          const key = d.FieldName || d.fieldName;
          const ds = d.DisplaySize ?? d.displaySize;
          const widthRaw = d.iFieldWidth ?? d.iLabWidth ?? ds;
          const widthVal = d.iFieldWidth ? `${widthRaw}px`
                         : ds ? `${ds}ch`
                         : null;
          return {
            key,
            label: d.DisplayLabel || d.displayLabel || columnLabels[key] || key,
            width: widthVal,
            comboStyle: Number(d.ComboStyle ?? d.comboStyle ?? 0),
            formatStr: d.FormatStr || d.formatStr || null,
            readOnly: Number(d.ReadOnly ?? d.readOnly ?? 0) === 1
          };
        })
        .filter(c => c.key);

      columns = dictCols;
    } else {
      columns = buildColumnsFromData(rows);
    }
    applyStoredColumnWidths();

    theadRow.innerHTML = columns
      .map((col) => {
        const style = col.width ? ` style="width:${col.width}"` : '';
        const cls = [
          isCheckboxColumn(col) ? 'col-checkbox' : '',
          col.key ? 'sortable' : ''
        ].filter(Boolean).join(' ');
        const classAttr = cls ? ` class="${cls}"` : '';
        const keyAttr = col.key ? ` data-key="${col.key}"` : '';
        return `<th${classAttr}${style}${keyAttr}>${col.label ?? col.key}</th>`;
      })
      .join('');
    attachColumnResizers();
    applySortIndicators();
  }

  function applySortIndicators() {
    if (!theadRow) return;
    theadRow.querySelectorAll('th').forEach((th) => {
      th.classList.remove('sorted-asc', 'sorted-desc');
      const key = th.dataset.key;
      if (key && sortKey && key.toLowerCase() === sortKey.toLowerCase()) {
        th.classList.add(sortDir === 'desc' ? 'sorted-desc' : 'sorted-asc');
      }
    });
  }

  function compareSort(a, b) {
    if (a == null && b == null) return 0;
    if (a == null) return sortDir === 'asc' ? -1 : 1;
    if (b == null) return sortDir === 'asc' ? 1 : -1;
    const na = Number(a);
    const nb = Number(b);
    const bothNum = Number.isFinite(na) && Number.isFinite(nb);
    if (bothNum) return sortDir === 'asc' ? na - nb : nb - na;
    const sa = String(a);
    const sb = String(b);
    const cmp = sa.localeCompare(sb, 'zh-Hant');
    return sortDir === 'asc' ? cmp : -cmp;
  }

  function sortSearchRows() {
    if (!sortKey) return;
    const key = sortKey;
    dataCache.sort((a, b) => compareSort(getValue(a, key), getValue(b, key)));
    scheduleRender();
  }

  function updateCount() {
    if (!countEl) return;
    const total = totalCount || 0;
    countEl.textContent = `${currentIndex || 0} / ${total}`;
  }

  function resetVirtualState() {
    totalCount = 0;
    currentIndex = 0;
    dataCache = [];
    loadedPages = new Set();
    isSearchMode = false;
    pendingSelectIndex = null;
    rowHeight = 24;
    columns = [];
    updateCount();
  }

  function scheduleRender() {
    if (renderScheduled) return;
    renderScheduled = true;
    requestAnimationFrame(() => {
      renderScheduled = false;
      renderVirtual();
    });
  }

  async function fetchCommonRecord(partnum, revision) {
    if (!partnum || !revision) return null;
    const params = new URLSearchParams();
    params.set('table', dictTableName);
    params.append('keyNames', commonKeyFields[0]);
    params.append('keyValues', partnum);
    params.append('keyNames', commonKeyFields[1]);
    params.append('keyValues', revision);
    const res = await fetch(`/api/CommonTable/ByKeys?${params.toString()}`);
    if (!res.ok) return null;
    const rows = await res.json();
    return Array.isArray(rows) ? rows[0] : null;
  }

  async function refreshCommonRecord(partnum, revision) {
    const row = await fetchCommonRecord(partnum, revision);
    if (!row) return;
    fillForm(row);
    currentRecord = row;
  }

  function selectRecord(item, index) {
    if (!item) return;
    fillForm(item);
    currentRecord = item;
    currentKey = { partnum: getValue(item, 'Partnum') ?? '', revision: getValue(item, 'Revision') ?? '' };
    isNew = false;
    setKeyFieldsEditable(false);
    setEditMode(false);
    detailGrids.forEach((grid) => grid.loadRows());
    currentIndex = (index ?? 0) + 1;
    updateCount();
    ensureBrowseVisible(index ?? 0);
    scheduleRender();
    if (useCommonTable) {
      refreshCommonRecord(currentKey.partnum, currentKey.revision);
    }
  }

  function ensureIndexLoaded(index) {
    if (isSearchMode) return;
    const page = Math.floor(index / pageSize) + 1;
    loadPage(page);
  }

  function selectRowAt(index) {
    if (!Number.isFinite(index)) return;
    const clamped = Math.max(0, Math.min(index, Math.max(0, totalCount - 1)));
    ensureIndexLoaded(clamped);
    if (dataCache[clamped]) {
      selectRecord(dataCache[clamped], clamped);
      return;
    }
    pendingSelectIndex = clamped;
  }

  function ensureBrowseVisible(index) {
    if (!browseScrollBody || !Number.isFinite(index)) return;
    const clamped = Math.max(0, Math.min(index, Math.max(0, totalCount - 1)));
    const margin = 4;

    const adjustByRect = () => {
      const row = tbody?.querySelector(`tr.matinfo-row[data-index="${clamped}"]`);
      if (!row) return false;
      const bodyRect = browseScrollBody.getBoundingClientRect();
      const rowRect = row.getBoundingClientRect();
      const topEdge = bodyRect.top + margin;
      const bottomEdge = bodyRect.bottom - margin;
      if (rowRect.top < topEdge) {
        browseScrollBody.scrollTop -= (topEdge - rowRect.top);
      } else if (rowRect.bottom > bottomEdge) {
        browseScrollBody.scrollTop += (rowRect.bottom - bottomEdge);
      }
      return true;
    };

    // rough scroll first (in case the row isn't rendered yet), then refine by actual rect
    const rowTop = clamped * rowHeight;
    const rowBottom = rowTop + rowHeight;
    const viewTop = browseScrollBody.scrollTop;
    const viewBottom = viewTop + browseScrollBody.clientHeight;
    if (rowTop < viewTop || rowBottom > viewBottom) {
      const target = Math.max(0, rowTop - Math.max(margin, (browseScrollBody.clientHeight - rowHeight) / 2));
      browseScrollBody.scrollTop = target;
    }
    requestAnimationFrame(() => { adjustByRect(); });
  }

  function renderVirtual() {
    if (!tbody || !theadRow || !browseScrollBody) return;
    if (!columns.length || totalCount <= 0) {
      theadRow.innerHTML = '';
      tbody.innerHTML = '<tr><td colspan="1">沒有資料</td></tr>';
      return;
    }

    const viewportHeight = browseScrollBody.clientHeight || 1;
    const scrollTop = browseScrollBody.scrollTop || 0;
    const startIndex = Math.max(0, Math.floor(scrollTop / rowHeight) - bufferRows);
    const endIndex = Math.min(totalCount - 1, Math.ceil((scrollTop + viewportHeight) / rowHeight) + bufferRows);

    if (!isSearchMode) {
      const startPage = Math.floor(startIndex / pageSize) + 1;
      const endPage = Math.floor(endIndex / pageSize) + 1;
      for (let page = startPage; page <= endPage; page += 1) {
        loadPage(page);
      }
    }

    const topSpace = Math.max(0, startIndex * rowHeight);
    const bottomSpace = Math.max(0, (totalCount - endIndex - 1) * rowHeight);
    const colCount = Math.max(1, columns.length);
    let html = `<tr class="matinfo-spacer"><td colspan="${colCount}" style="height:${topSpace}px"></td></tr>`;
    for (let i = startIndex; i <= endIndex; i += 1) {
      const item = dataCache[i];
      if (!item) {
        html += `<tr class="matinfo-loading"><td colspan="${colCount}">載入中...</td></tr>`;
        continue;
      }
      const activeClass = currentIndex === i + 1 ? ' active' : '';
      html += `<tr class="matinfo-row${activeClass}" data-index="${i}">` +
        columns.map((col) => {
          const cls = [
            isCheckboxColumn(col) ? 'col-checkbox' : '',
            col.readOnly ? 'col-readonly' : ''
          ].filter(Boolean).join(' ');
          const classAttr = cls ? ` class="${cls}"` : '';
          const style = col.width ? ` style="width:${col.width}"` : '';
          return `<td${classAttr}${style}>${formatCellHtml(col, getValue(item, col.key))}</td>`;
        }).join('') +
        `</tr>`;
    }
    html += `<tr class="matinfo-spacer"><td colspan="${colCount}" style="height:${bottomSpace}px"></td></tr>`;
    tbody.innerHTML = html;

    const firstRow = tbody.querySelector('tr.matinfo-row');
    if (firstRow) {
      const h = firstRow.getBoundingClientRect().height;
      if (h > 0 && Math.abs(h - rowHeight) > 1) {
        rowHeight = h;
        scheduleRender();
      }
    }
  }

  async function loadPage(page) {
    if (page < 1 || loadedPages.has(page)) return;
    loadedPages.add(page);
    try {
      const url = (() => {
        if (useCommonTable) {
          const params = new URLSearchParams();
          params.set('table', dictTableName);
          params.set('page', String(page));
          params.set('pageSize', String(pageSize));
          if (defaultMbFilter !== null && defaultMbFilter !== undefined) {
            params.set('mb', String(defaultMbFilter));
          }
          if (sortKey) {
            params.set('orderBy', sortKey);
            params.set('orderDir', sortDir);
          }
          return `/api/CommonTable/Paged?${params.toString()}`;
        }
        const mbParam = (defaultMbFilter !== null && defaultMbFilter !== undefined)
          ? `&mb=${encodeURIComponent(String(defaultMbFilter))}`
          : '';
        return `/api/MindMatInfo/paged?page=${page}&pageSize=${pageSize}${mbParam}`;
      })();
      const res = await fetch(url);
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      const rows = Array.isArray(data?.data) ? data.data : [];
      totalCount = data?.totalCount ?? rows.length;
      updateCount();
      if (dataCache.length !== totalCount) dataCache.length = totalCount;
      const start = (page - 1) * pageSize;
      rows.forEach((row, idx) => {
        dataCache[start + idx] = row;
      });
      if (!columns.length) {
        await buildColumns(rows);
        syncScroll(browseScrollTop, browseScrollBody);
      }
      scheduleRender();
      if (pendingSelectIndex !== null && dataCache[pendingSelectIndex]) {
        const idx = pendingSelectIndex;
        pendingSelectIndex = null;
        selectRowAt(idx);
      }
    } catch (err) {
      loadedPages.delete(page);
      tbody.innerHTML = `<tr><td colspan="6">載入失敗：${err.message ?? err}</td></tr>`;
    }
  }

  async function renderRows(rows, total) {
    const list = Array.isArray(rows) ? rows : [];
    resetVirtualState();
    isSearchMode = true;
    totalCount = total ?? list.length;
    dataCache = list.slice();
    if (list.length) {
      await buildColumns(list);
      syncScroll(browseScrollTop, browseScrollBody);
    }
    updateCount();
    if (sortKey) {
      sortSearchRows();
    } else {
      scheduleRender();
    }
    if (list.length > 0) {
      selectRowAt(0);
    } else {
      theadRow.innerHTML = '';
      clearForm();
      currentRecord = null;
      currentKey = null;
      isNew = false;
      setKeyFieldsEditable(false);
      setEditMode(false);
    }
  }

  async function loadData() {
    resetVirtualState();
    isSearchMode = false;
    try {
      await Promise.all([loadPage(1), loadFormDict()]);
      if (totalCount > 0) {
        selectRowAt(0);
      } else {
        clearForm();
        currentRecord = null;
        currentKey = null;
        isNew = false;
        setKeyFieldsEditable(false);
        setEditMode(false);
        detailGrids.forEach((grid) => grid.loadRows());
      }
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="6">載入失敗：${err.message ?? err}</td></tr>`;
    }
  }

  async function searchByAddedPartnum(partnum) {
    const pn = (partnum || '').toString().trim();
    if (!pn) return false;
    const payload = { PartNumB: pn, PartNumE: pn, Limit: 200 };
    if (defaultMbFilter === 0 || defaultMbFilter === 1) payload.MB = defaultMbFilter;
    const res = await fetch('/api/MatInfoSearch/Search', withJwtHeaders({
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    }));
    if (!res.ok) return false;
    const respJson = await res.json();
    const rows = Array.isArray(respJson) ? respJson : (respJson?.data || respJson?.Data || []);
    if (!Array.isArray(rows) || rows.length === 0) return false;
    await renderRows(rows, rows.length);
    return true;
  }

  tbody?.addEventListener('click', (e) => {
    const row = e.target.closest('tr.matinfo-row');
    if (!row) return;
    const idx = Number(row.dataset.index ?? '-1');
    if (!Number.isFinite(idx) || idx < 0 || idx >= totalCount) return;
    const item = dataCache[idx];
    if (!item) return;
    selectRecord(item, idx);
  });

  browseScrollBody?.addEventListener('scroll', () => {
    scheduleRender();
  });

  theadRow?.addEventListener('click', (e) => {
    const th = e.target.closest('th');
    const key = th?.dataset?.key;
    if (!key) return;
    if (sortKey && sortKey.toLowerCase() === key.toLowerCase()) {
      sortDir = sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      sortKey = key;
      sortDir = 'asc';
    }
    applySortIndicators();
    if (isSearchMode) {
      sortSearchRows();
      selectRowAt(0);
      return;
    }
    if (!useCommonTable) return;
    resetVirtualState();
    loadPage(1);
  });

  async function saveRecord() {
    await loadFormDict();
    const data = getFormData();
    const partnum = (data.Partnum || '').trim();
    const revision = (data.Revision || '').trim();
    if (!partnum) {
      notify('error', '請輸入料號');
      return false;
    }
    if (!revision) {
      notify('error', '請輸入版次');
      return false;
    }

    if (useCommonTable) {
      if (isNew) {
        const exists = await fetchCommonRecord(partnum, revision);
        if (exists) {
          notify('error', '料號已存在');
          return false;
        }
      }
      const resp = await fetch('/api/CommonTable/SaveTableChanges', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          TableName: dictTableName,
          KeyFields: commonKeyFields,
          Data: [data]
        })
      });
      if (!resp.ok) {
        notify('error', await resp.text());
        return false;
      }
      const result = await resp.json();
      const failed = (result?.results || []).find(r => r && r.ok === false);
      if (failed) {
        notify('error', failed.reason || failed.error || '儲存失敗');
        return false;
      }
      notify('success', isNew ? '新增完成' : '儲存完成');
      await loadData();
      return true;
    }

    if (isNew) {
      const resp = await fetch('/api/MindMatInfo', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      });
      if (!resp.ok) {
        notify('error', await resp.text());
        return false;
      }
      notify('success', '新增完成');
      await loadData();
      return true;
    }

    if (!currentKey) {
      notify('error', '請先選擇一筆資料');
      return false;
    }

    const url = `/api/MindMatInfo/${encodeURIComponent(currentKey.partnum)}/${encodeURIComponent(currentKey.revision)}`;
    const resp = await fetch(url, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });
    if (!resp.ok) {
      notify('error', await resp.text());
      return false;
    }
    notify('success', '儲存完成');
    await loadData();
    return true;
  }

  async function deleteRecord() {
    if (!currentKey || !currentKey.partnum || !currentKey.revision) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    const ok = window.Swal
      ? (await window.Swal.fire({ icon: 'warning', title: '確定要刪除?', showCancelButton: true })).isConfirmed
      : confirm('確定要刪除?');
    if (!ok) return;

    if (useCommonTable) {
      const resp = await fetch('/api/CommonTable/SaveTableChanges', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          TableName: dictTableName,
          KeyFields: commonKeyFields,
          Data: [{
            PartNum: currentKey.partnum,
            Revision: currentKey.revision,
            __delete: true
          }]
        })
      });
      if (!resp.ok) {
        notify('error', await resp.text());
        return;
      }
      const result = await resp.json();
      const failed = (result?.results || []).find(r => r && r.ok === false);
      if (failed) {
        notify('error', failed.reason || failed.error || '刪除失敗');
        return;
      }
      notify('success', '刪除完成');
      await loadData();
      return;
    }

    const url = `/api/MindMatInfo/${encodeURIComponent(currentKey.partnum)}/${encodeURIComponent(currentKey.revision)}`;
    const resp = await fetch(url, { method: 'DELETE' });
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    notify('success', '刪除完成');
    await loadData();
  }

  btnQuery?.addEventListener('click', () => {
    if (window.MatInfoSearchHandler?.open) {
      window.MatInfoSearchHandler.open({ itemId, dictTableName: dictSearchTableName });
      return;
    }
    loadData();
  });

  btnEdit?.addEventListener('click', async () => {
    if (isEditMode) {
      const ok = await saveRecord();
      if (ok) setEditMode(false);
      return;
    }
    if (!currentKey || !currentKey.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    setEditMode(true);
  });

  btnCancel?.addEventListener('click', () => {
    if (cancelSnapshot) {
      fillForm(cancelSnapshot);
    } else if (currentRecord) {
      fillForm(currentRecord);
    } else {
      clearForm();
    }
    isNew = false;
    setKeyFieldsEditable(false);
    setEditMode(false);
  });

  btnAdd?.addEventListener('click', () => {
    if (window.MatInfoAddHandler?.open) {
      window.MatInfoAddHandler.open({ itemId, dictTableName });
      return;
    }
    cancelSnapshot = getFormData();
    preserveCancelSnapshot = true;
    clearForm();
    currentRecord = null;
    currentKey = null;
    isNew = true;
    setKeyFieldsEditable(true);
    setEditMode(true);
  });

  btnToFormal?.addEventListener('click', async () => {
    if (!currentKey || !currentKey.partnum || !currentKey.revision) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    const isTrans = getValue(currentRecord, 'IsTrans');
    const status = getValue(currentRecord, 'Status');
    if (isTrans === 1 || isTrans === '1') {
      notify('error', '已轉正式');
      return;
    }
    if (status === 1 || status === '1') {
      notify('error', '已是正式料號');
      return;
    }
    if (window.MatInfoAddHandler?.open) {
      window.MatInfoAddHandler.open({
        itemId,
        dictTableName,
        mode: 'tmpToFormal',
        currOldPN: currentKey.partnum,
        setClass: getValue(currentRecord, 'MatClass'),
        mb: getValue(currentRecord, 'MB'),
        matName: getValue(currentRecord, 'MatName'),
        unit: getValue(currentRecord, 'Unit')
      });
      return;
    }
    notify('error', '轉正式視窗尚未載入');
  });

  btnDetail?.addEventListener('click', () => {
    if (!browseBox) return;
    const hidden = browseBox.classList.toggle('is-hidden');
    setButtonText(btnDetail, hidden ? '瀏覽' : '明細');
  });

  const getTabButtons = () => Array.from(document.querySelectorAll('.matinfo-tabs .nav-link'));
  const openTabAt = (idx) => {
    const tabs = getTabButtons();
    if (!tabs.length) return;
    const target = tabs[Math.max(0, Math.min(idx, tabs.length - 1))];
    if (!target) return;
    if (window.bootstrap?.Tab) {
      const tab = window.bootstrap.Tab.getOrCreateInstance(target);
      tab.show();
      return;
    }
    target.click();
  };
  const moveTab = (dir) => {
    const tabs = getTabButtons();
    if (!tabs.length) return;
    const activeIdx = Math.max(0, tabs.findIndex((btn) => btn.classList.contains('active')));
    const nextIdx = dir === 'prev' ? activeIdx - 1 : activeIdx + 1;
    openTabAt(nextIdx);
  };

  const moveRow = (dir) => {
    if (totalCount <= 0) return;
    const activeIdx = Math.max(0, Math.min((currentIndex || 1) - 1, totalCount - 1));
    let nextIdx = activeIdx;
    if (dir === 'prev') nextIdx = activeIdx - 1;
    else if (dir === 'next') nextIdx = activeIdx + 1;
    else if (dir === 'first') nextIdx = 0;
    else if (dir === 'last') nextIdx = totalCount - 1;
    selectRowAt(nextIdx);
  };

  btnTabFirst?.addEventListener('click', () => moveRow('first'));
  btnTabPrev?.addEventListener('click', () => moveRow('prev'));
  btnTabNext?.addEventListener('click', () => moveRow('next'));
  btnTabLast?.addEventListener('click', () => moveRow('last'));

  btnHistory?.addEventListener('click', async () => {
    if (!currentKey || !currentKey.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    const userId = getUserId();
    const qs = `paperId=${encodeURIComponent(itemId)}&paperNum=${encodeURIComponent(currentKey.partnum)}&userId=${encodeURIComponent(userId)}`;
    const [masterResp, historyResp] = await Promise.all([
      fetch(`/api/UpdateLog/Master?${qs}`, withJwtHeaders()),
      fetch(`/api/UpdateLog/History?${qs}`, withJwtHeaders())
    ]);
    if (!masterResp.ok || !historyResp.ok) {
      notify('error', '讀取歷史失敗');
      return;
    }
    const masterData = await masterResp.json();
    const historyData = await historyResp.json();
    renderLogTable(logMasterTable, masterData?.rows ?? [], '尚無更新記錄');
    renderLogTable(logHistoryTable, historyData?.rows ?? [], '尚無歷史記錄');
    showLogModal();
  });

  logModalEl?.querySelectorAll('[data-bs-dismiss="modal"], .btn-close').forEach((btn) => {
    btn.addEventListener('click', () => {
      hideLogModal();
    });
  });

  btnSave?.addEventListener('click', () => {
    saveRecord();
  });

  btnToEmo?.addEventListener('click', async () => {
    if (!currentKey || !currentKey.partnum || !currentKey.revision) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    const ok = window.Swal
      ? (await window.Swal.fire({ icon: 'warning', title: '確定要轉工程?', showCancelButton: true })).isConfirmed
      : confirm('確定要轉工程?');
    if (!ok) return;

    const chk = await fetch(`/api/MatInfoEmo/Check?partNum=${encodeURIComponent(currentKey.partnum)}`, withJwtHeaders());
    if (chk.ok) {
      const data = await chk.json();
      if (data?.exists) {
        notify('error', '已轉工程');
        return;
      }
    }

    const resp = await fetch('/api/MatInfoEmo/Convert', withJwtHeaders({
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ PartNum: currentKey.partnum, Revision: currentKey.revision })
    }));
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    notify('success', '轉工程資料成功');
    await loadData();
  });

  btnDelete?.addEventListener('click', () => {
    deleteRecord();
  });

  btnUnitFirst?.addEventListener('click', () => unitGrid?.selectRow(0));
  btnUnitPrev?.addEventListener('click', () => unitGrid?.selectRow((unitGrid?.getSelectedIndex() ?? 0) - 1));
  btnUnitNext?.addEventListener('click', () => unitGrid?.selectRow((unitGrid?.getSelectedIndex() ?? 0) + 1));
  btnUnitLast?.addEventListener('click', () => unitGrid?.selectRow(Number.MAX_SAFE_INTEGER));
  btnUnitAdd?.addEventListener('click', () => unitGrid?.addRow());
  btnUnitDelete?.addEventListener('click', () => unitGrid?.deleteRow());
  btnUnitSave?.addEventListener('click', () => unitGrid?.saveRows());
  btnUnitCancel?.addEventListener('click', () => unitGrid?.cancelChanges());
  btnUnitRefresh?.addEventListener('click', () => unitGrid?.loadRows());

  btnCustFirst?.addEventListener('click', () => custGrid?.selectRow(0));
  btnCustPrev?.addEventListener('click', () => custGrid?.selectRow((custGrid?.getSelectedIndex() ?? 0) - 1));
  btnCustNext?.addEventListener('click', () => custGrid?.selectRow((custGrid?.getSelectedIndex() ?? 0) + 1));
  btnCustLast?.addEventListener('click', () => custGrid?.selectRow(Number.MAX_SAFE_INTEGER));
  btnCustAdd?.addEventListener('click', () => custGrid?.addRow());
  btnCustDelete?.addEventListener('click', () => custGrid?.deleteRow());
  btnCustSave?.addEventListener('click', () => custGrid?.saveRows());
  btnCustCancel?.addEventListener('click', () => custGrid?.cancelChanges());
  btnCustRefresh?.addEventListener('click', () => custGrid?.loadRows());

  btnMapFirst?.addEventListener('click', () => mapGrid?.selectRow(0));
  btnMapPrev?.addEventListener('click', () => mapGrid?.selectRow((mapGrid?.getSelectedIndex() ?? 0) - 1));
  btnMapNext?.addEventListener('click', () => mapGrid?.selectRow((mapGrid?.getSelectedIndex() ?? 0) + 1));
  btnMapLast?.addEventListener('click', () => mapGrid?.selectRow(Number.MAX_SAFE_INTEGER));
  btnMapAdd?.addEventListener('click', () => mapGrid?.addRow());
  btnMapDelete?.addEventListener('click', () => mapGrid?.deleteRow());
  btnMapSave?.addEventListener('click', () => mapGrid?.saveRows());
  btnMapCancel?.addEventListener('click', () => mapGrid?.cancelChanges());
  btnMapRefresh?.addEventListener('click', () => mapGrid?.loadRows());
  btnMapViewWindow?.addEventListener('click', () => {
    const path = getSelectedMapPath();
    if (!path) {
      notify('error', '沒有圖檔的完整路徑及檔名');
      return;
    }
    const url = `/api/MapPreview/file?path=${encodeURIComponent(path)}`;
    window.open(url, '_blank');
  });
  btnMapViewGlyph?.addEventListener('click', () => showGlyphPreview(getSelectedMapPath(), true));
  btnMapSetPath?.addEventListener('click', () => {
    if (!isEditMode) {
      notify('error', '請先進入編輯模式');
      return;
    }
    if (!mapTable) return;
    const input = window.prompt('請輸入圖檔完整路徑及檔名');
    if (input == null) return;
    const path = normalizeFilePath(input);
    if (!path) return;
    if (!isSupportedImage(path)) {
      notify('error', '檔案格式只支援 jpg、bmp、wmf');
      return;
    }
    let targetRow = mapTable.querySelector('tbody tr.is-selected');
    if (!targetRow && mapGrid) {
      mapGrid.addRow();
      targetRow = mapTable.querySelector('tbody tr.is-selected');
    }
    if (!targetRow) return;
    const cellInput = targetRow.querySelector('.cell-edit[name="CrossName"]');
    const cellView = targetRow.querySelector('.cell-view');
    if (cellInput) cellInput.value = path;
    if (cellView) cellView.textContent = path;
  });

  btnSpecFirst?.addEventListener('click', () => specGrid?.selectRow(0));
  btnSpecPrev?.addEventListener('click', () => specGrid?.selectRow((specGrid?.getSelectedIndex() ?? 0) - 1));
  btnSpecNext?.addEventListener('click', () => specGrid?.selectRow((specGrid?.getSelectedIndex() ?? 0) + 1));
  btnSpecLast?.addEventListener('click', () => specGrid?.selectRow(Number.MAX_SAFE_INTEGER));
  btnSpecSave?.addEventListener('click', () => specGrid?.saveRows());
  btnSpecCancel?.addEventListener('click', () => specGrid?.cancelChanges());
  btnSpecRefresh?.addEventListener('click', () => specGrid?.loadRows());
  btnSpecReimport?.addEventListener('click', async () => {
    if (!currentKey?.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    if (!isEditMode) {
      notify('error', '請先進入編輯模式');
      return;
    }
    const form = getFormData();
    const specTypeRaw = form?.SpecType ?? getValue(currentRecord, 'SpecType');
    const specTypeValue = Number(specTypeRaw);
    if (!Number.isFinite(specTypeValue)) {
      notify('error', '請先點選規格種類');
      return;
    }
    const resp = await fetch('/api/MatInfoUtility/SpecDataSet', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partNum: currentKey.partnum,
        revision: currentKey.revision,
        specType: specTypeValue
      })
    });
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    await specGrid?.loadRows();
  });
  btnSpecClear?.addEventListener('click', async () => {
    if (!currentKey?.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    if (!isEditMode) {
      notify('error', '請先進入編輯模式');
      return;
    }
    const resp = await fetch('/api/MatInfoUtility/SpecDataClear', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partNum: currentKey.partnum,
        revision: currentKey.revision
      })
    });
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    await specGrid?.loadRows();
  });
  btnSpecPrint?.addEventListener('click', () => {
    notify('warning', '列印功能尚未設定');
  });

  btnKitFirst?.addEventListener('click', () => kitGrid?.selectRow(0));
  btnKitPrev?.addEventListener('click', () => kitGrid?.selectRow((kitGrid?.getSelectedIndex() ?? 0) - 1));
  btnKitNext?.addEventListener('click', () => kitGrid?.selectRow((kitGrid?.getSelectedIndex() ?? 0) + 1));
  btnKitLast?.addEventListener('click', () => kitGrid?.selectRow(Number.MAX_SAFE_INTEGER));
  btnKitAdd?.addEventListener('click', () => kitGrid?.addRow());
  btnKitDelete?.addEventListener('click', () => kitGrid?.deleteRow());
  btnKitSave?.addEventListener('click', () => kitGrid?.saveRows());
  btnKitCancel?.addEventListener('click', () => kitGrid?.cancelChanges());
  btnKitRefresh?.addEventListener('click', () => kitGrid?.loadRows());

  btnSaveHeight?.addEventListener('click', () => saveBrowseHeight());
  btnUpdateCustPn?.addEventListener('click', async () => {
    if (!currentKey?.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    if (!isEditMode) {
      notify('error', '請先進入編輯模式');
      return;
    }
    const newCustPn = window.prompt('輸入新的客戶料號');
    if (newCustPn == null) return;
    const newCustEg = window.prompt('輸入客戶機種(可空白)') ?? '';
    const resp = await fetch('/api/MatInfoUtility/UpdateCustPn', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partNum: currentKey.partnum,
        newCustPn: String(newCustPn).trim(),
        newCustEg: String(newCustEg).trim(),
        userId: getUserId()
      })
    });
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    notify('success', '已完成修改');
    await loadData();
  });
  btnCopyPN4YX?.addEventListener('click', async () => {
    if (!currentKey?.partnum) {
      notify('error', '請先選擇一筆資料');
      return;
    }
    const resp = await fetch('/api/MatInfoUtility/CopyCustomerVersion', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        partNum: currentKey.partnum,
        userId: getUserId()
      })
    });
    if (!resp.ok) {
      notify('error', await resp.text());
      return;
    }
    notify('success', '已完成');
    await loadData();
  });

  syncScroll(browseScrollTop, browseScrollBody);
  syncScroll(formScrollTop, formScrollBody);
  initDetailTableResizers();

  document.querySelectorAll('[data-bs-toggle="tab"]').forEach((btn) => {
    btn.addEventListener('shown.bs.tab', () => {
      syncScroll(formScrollTop, formScrollBody);
      setActiveTabContext(btn);
      if (btn.dataset.bsTarget === '#tab-glyph') {
        showGlyphPreview(getSelectedMapPath(), false);
      }
      const pane = btn.dataset.bsTarget ? document.querySelector(btn.dataset.bsTarget) : null;
      if (pane) {
        const tables = Array.from(pane.querySelectorAll('table'));
        tables.forEach((table) => {
          if (!table.querySelector('thead th')) return;
          ensureDetailResizeKey(table);
          applyDetailTableWidths(table);
          attachDetailTableResizers(table);
        });
      }
    });
  });

  mapTable?.addEventListener('click', () => {
    const glyphPane = document.querySelector('#tab-glyph');
    if (glyphPane?.classList.contains('show')) {
      showGlyphPreview(getSelectedMapPath(), false);
    }
  });

  const openFieldDict = () => {
    const el = document.getElementById('fieldDictModal');
    if (!el) return;
    const target = document.activeElement?.closest?.('[data-dict-table]')?.dataset?.dictTable
      || document.querySelector('.ctx-current[data-dict-table]')?.dataset?.dictTable
      || window._dictTableName
      || dictPanelTableName;
    if (typeof window.showDictModal === 'function') {
      window.showDictModal('fieldDictModal', target);
      return;
    }
    if (typeof window.initFieldDictModal === 'function') {
      window.initFieldDictModal(target, 'fieldDictModal');
    } else if (typeof window.loadFieldDict === 'function') {
      window.loadFieldDict(target);
    }
  };

  document.addEventListener('keydown', (e) => {
    if (e.key !== 'F3') return;
    const target = e.target;
    const inText = target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);
    if (inText) return;
    e.preventDefault();
    openFieldDict();
  });

  window.matinfoReload = loadData;
  window.addEventListener('field-dict-saved', () => {
    formDictCache.fields = null;
    formDictTypeMap.clear();
    loadFormDict();
  });

  function applyTabVisibility() {
    const id = (itemId || '').toUpperCase();
    if (id !== 'MG000008' && id !== 'CPN00006') return;
    const allowed = new Set([
      '基本資料', '基本資料2', '廠商料號', '屬性及備註',
      '工程圖', '圖檔', '規格表', '替代單位設定', '共用件', '組合商品'
    ]);
    const tabs = Array.from(document.querySelectorAll('.matinfo-tabs .nav-link'));
    tabs.forEach((btn) => {
      const name = btn.dataset.tabName || btn.textContent?.trim();
      const show = allowed.has(name || '');
      const li = btn.closest('li');
      if (li) li.style.display = show ? '' : 'none';
      const target = btn.getAttribute('data-bs-target');
      if (target) {
        const pane = document.querySelector(target);
        if (pane) pane.style.display = show ? '' : 'none';
      }
    });
    const activeVisible = document.querySelector('.matinfo-tabs .nav-link.active')?.closest('li')?.style.display !== 'none';
    if (!activeVisible) {
      const first = tabs.find((btn) => {
        const name = btn.dataset.tabName || btn.textContent?.trim();
        return allowed.has(name || '');
      });
      if (first) {
        if (window.bootstrap?.Tab) {
          window.bootstrap.Tab.getOrCreateInstance(first).show();
        } else {
          first.click();
        }
      }
    }
  }
  document.addEventListener('matinfo:add:done', async (e) => {
    const partnum = e.detail?.partnum;
    if (await searchByAddedPartnum(partnum)) return;
    loadData();
  });
  document.addEventListener('matinfo:search:done', (e) => {
    const rows = e.detail?.rows;
    if (Array.isArray(rows)) {
      renderRows(rows, rows.length);
      return;
    }
    loadData();
  });

  setEditMode(false);
  applyTabDictTables();
  applyCustTabLabel();
  applyTopToolbarVisibility();
  applyTabVisibility();
  setActiveTabContext(document.querySelector('.matinfo-tabs .nav-link.active'));
  initBrowseResizer();
  loadBrowseHeight();
  loadData();
})();
