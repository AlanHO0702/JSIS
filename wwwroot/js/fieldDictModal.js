// /wwwroot/js/fieldDictModal.js

(function () {

    window.showDictModal = async function (modalId = 'fieldDictModal', tableName = window._dictTableName) {
      const el = document.getElementById(modalId);
      if (!el) { console.warn('?曆??啗冪??Modal ?辣:', modalId); return; }

      await window.initFieldDictModal(tableName, modalId);

      const md = bootstrap.Modal.getOrCreateInstance(el);
      md.show();
  };

  window.__lastMouse = window.__lastMouse || { x: 0, y: 0 };
  document.addEventListener('mousemove', (e) => {
    window.__lastMouse.x = e.clientX;
    window.__lastMouse.y = e.clientY;
  }, { passive: true });

  const GET_API   = window.FIELD_DICT_GET_API  || '/api/TableFieldLayout/GetTableFieldsFull';
  const QUERY_KEY = window.FIELD_DICT_QUERY_KEY || 'table';
  const SAVE_API  = window.FIELD_DICT_SAVE_API || '/api/DictApi/UpdateDictFields';
  const SAVE_OCXMAPS_API = window.FIELD_DICT_SAVE_OCXMAPS_API || '/api/DictOCX/SaveOCXMapsBatch';
  const TYPE_API  = window.FIELD_DICT_TYPE_API || '/api/TableFieldLayout/ColumnTypes';
  const QUIET     = !!window.SUPPRESS_DICT_FETCH_ALERT;
  const GRID_DEFAULTS = {
    labHeight: 22,
    labWidth: 90,
    fieldHeight: 22,
    fieldWidth: 140,
    topOffset: 1,
    leftOffset: 1
  };

  function calcGridLayout(row, col) {
    const r = Number(row);
    const c = Number(col);
    if (!Number.isFinite(r) || !Number.isFinite(c) || r <= 0 || c <= 0) return null;
    const top = ((r - 1) * GRID_DEFAULTS.labHeight) + GRID_DEFAULTS.topOffset;
    const labLeft = ((c - 1) * (GRID_DEFAULTS.labWidth + GRID_DEFAULTS.fieldWidth)) + GRID_DEFAULTS.leftOffset;
    const fieldLeft = labLeft + GRID_DEFAULTS.labWidth;
    return {
      iLabTop: top,
      iLabLeft: labLeft,
      iLabHeight: GRID_DEFAULTS.labHeight,
      iLabWidth: GRID_DEFAULTS.labWidth,
      iFieldTop: top,
      iFieldLeft: fieldLeft,
      iFieldHeight: GRID_DEFAULTS.fieldHeight,
      iFieldWidth: GRID_DEFAULTS.fieldWidth
    };
  }

  window.showDictModal = async function (modalId = 'fieldDictModal', tableName = window._dictTableName) {
    const el = document.getElementById(modalId);
    if (!el) { console.warn('?曆??啗冪??Modal ?辣:', modalId); return; }
    await window.initFieldDictModal(tableName, modalId);

    const displayEl = document.getElementById('dictTableNameDisplay');
    if (displayEl && tableName) {
      displayEl.textContent = `- ${tableName}`;
    }

    new bootstrap.Modal(el).show();
  };

  window.initFieldDictModal = async function (tableName, modalId = 'fieldDictModal') {
    const tname = (tableName || window._dictTableName || '').trim();
    if (!tname) { return; }

    window._dictTableName = tname;

    const scope = document.getElementById(modalId) || document;
    const tbody =
      scope.querySelector('#fieldDictTable tbody') ||
      scope.querySelector('.dictTableBody') ||
      scope.querySelector('tbody[data-role="dict"]');

    if (!tbody) { console.warn('?曆??啗冪??tbody'); return; }

    const loadedFor  = (tbody.getAttribute('data-loaded-for') || '').toLowerCase();
    const want       = tname.toLowerCase();
    const needReload = loadedFor !== want;
    if (needReload) {
      tbody.innerHTML = '';
    }

    const alreadyHasRows = tbody.children && tbody.children.length > 0;

    if (!alreadyHasRows) {
      try {
        const u = new URL(GET_API, window.location.origin);
        u.searchParams.set(QUERY_KEY, tname);
        if (QUERY_KEY !== 'table')     u.searchParams.set('table', tname);
        if (QUERY_KEY !== 'tableName') u.searchParams.set('tableName', tname);

        const res  = await fetch(u.toString());
        if (!res.ok) {
          if (!QUIET) alert('載入辭典欄位失敗');
        } else {
          const 筆 = await res.json();
          筆.sort((a, b) => (Number(a.SerialNum ?? 9999)) - (Number(b.SerialNum ?? 9999)));

          tbody.innerHTML = 筆.map(x => `
          <tr data-tablename="${x.TableName || tname}"
              data-fieldname="${x.FieldName}"
              ondblclick="window.editFieldDetail && window.editFieldDetail('${x.FieldName}')">

            <!-- 蝚?1 甈?摨? + ?梯?甈? -->
            <td style="width:72px">
              <input data-field="SerialNum" type="number"
                    value="${x.SerialNum ?? ''}" class="form-control form-control-sm"
                    style="width:100%; text-align:center;" />

              <span class="d-none extra-fields">
                <input data-field="DisplaySize"  value="${x.DisplaySize  ?? ''}" />
                <input data-field="iLayRow"      value="${x.iLayRow      ?? ''}" />
                <input data-field="iLayColumn"   value="${x.iLayColumn   ?? ''}" />
                <input data-field="iLabHeight"   value="${x.iLabHeight   ?? ''}" />
                <input data-field="iLabTop"      value="${x.iLabTop      ?? ''}" />
                <input data-field="iLabLeft"     value="${x.iLabLeft     ?? ''}" />
                <input data-field="iLabWidth"    value="${x.iLabWidth    ?? ''}" />
                <input data-field="iFieldHeight" value="${x.iFieldHeight ?? ''}" />
                <input data-field="iFieldTop"    value="${x.iFieldTop    ?? ''}" />
                <input data-field="iFieldLeft"   value="${x.iFieldLeft   ?? ''}" />
                <input data-field="iFieldWidth"  value="${x.iFieldWidth  ?? ''}" />
                <input data-field="iShowWhere"   value="${x.iShowWhere   ?? ''}" />

                <input data-field="LookupTable"       value="${x.LookupTable       ?? ''}" />
                <input data-field="LookupKeyField"    value="${x.LookupKeyField    ?? ''}" />
                <input data-field="LookupResultField" value="${x.LookupResultField ?? ''}" />

                <input data-field="IsNotesField" value="${x.IsNotesField ?? ''}" />
                <input data-field="ComboStyle" value="${x.ComboStyle ?? ''}" />

                <input data-field="OCXLKTableName" value="${x.OCXLKTableName ?? ''}" />
                <input data-field="OCXLKResultName" value="${x.OCXLKResultName?? ''}" />
                <input data-field="KeyFieldName" value="${x.KeyFieldName ?? ''}" />
                <input data-field="KeySelfName" value="${x.KeySelfName ?? ''}" />
                <input data-field="KeyMapsJson" value="${encodeURIComponent(x.KeyMapsJson ?? '')}" />

              </span>
            </td>

            <!-- 蝚?2 ~ 9 甈?-->
            <td style="min-width:180px">${x.FieldName ?? ''}</td>

            <td style="min-width:150px">
              <input data-field="DisplayLabel" value="${x.DisplayLabel ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td class="text-center" style="width:40px">
              <input type="checkbox" class="checkbox-dark" data-field="Visible"
                    ${(+x.Visible === 1 ? 'checked' : '')} />
            </td>

            <td class="text-center" style="width:40px">
              <input type="checkbox" class="checkbox-dark" data-field="ReadOnly"
                    ${(+x.ReadOnly === 1 ? 'checked' : '')} />
            </td>

            <td class="text-center" style="width:40px">
              <input type="checkbox" class="checkbox-dark" data-field="ComboStyle"
                    ${(+x.ComboStyle === 1 ? 'checked' : '')} />
            </td>

            <td style="width:120px">
              <input data-field="DataType" value="${x.DataType ?? ''}"
                    class="form-control form-control-sm"
                    readonly
                    style="background:#f2f2f2; color:#8a9399; border:1px solid #dee2e6;"
                    tabindex="-1" />
            </td>

            <td style="width:120px">
              <input data-field="FormatStr" value="${x.FormatStr ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td style="width:160px">
              <input data-field="FieldNote" value="${x.FieldNote ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td style="width:120px">
              <input data-field="EditColor" value="${x.EditColor ?? ''}"
                    class="form-control form-control-sm" placeholder="clYellow / Yellow" />
            </td>

            <!-- 蝚?10 甈???閮剖??? -->
            <td style="width:60px" class="text-center">
              <button type="button" class="btn btn-sm btn-outline-secondary"
                      aria-label="設定"
                      onclick="window.editFieldDetail && window.editFieldDetail('${x.FieldName}')">
                <i class="bi bi-gear"></i>
              </button>
            </td>
          </tr>
        `).join('');

          tbody.setAttribute('data-loaded-for', tname);
        }
      } catch (err) {
        if (!QUIET) alert('載入辭典欄位失敗');
        console.warn('[fieldDictModal] fetch error:', err);
      }
    }

    sortDictTbody(tbody);
    bindSaveButton(tbody);
    initDirtyTracking(tbody);
  };

  function rowSnapshot(tr) {
    const snap = {};
    tr.querySelectorAll('input[data-field]').forEach(inp => {
      const key = inp.getAttribute('data-field');
      if (!key) return;
      snap[key] = (inp.type === 'checkbox') ? (inp.checked ? 1 : 0) : (inp.value ?? '');
    });
    return snap;
  }
  function setRowSnapshot(tr) {
    tr.dataset.dictSnapshot = JSON.stringify(rowSnapshot(tr));
  }
  function isRowDirty(tr) {
    const prev = tr.dataset.dictSnapshot;
    if (!prev) return true; // 瘝?敹怎撠梯???dirty嚗??摮?
    let prevObj = null;
    try { prevObj = JSON.parse(prev); } catch { return true; }
    const cur = rowSnapshot(tr);
    const keys = new Set([...Object.keys(prevObj || {}), ...Object.keys(cur)]);
    for (const k of keys) {
      const a = prevObj?.[k];
      const b = cur[k];
      if ((a ?? '') !== (b ?? '')) return true;
    }
    return false;
  }
  function syncRowDirtyUi(tr) {
    const dirty = isRowDirty(tr);
    tr.dataset.dirty = dirty ? '1' : '0';
    tr.classList.toggle('dict-row-dirty', dirty);
  }
  async function syncDbDataTypes(tbody, dictTableName) {
    if (!tbody || !dictTableName) return { updated: 0 };
    try {
      const cacheKey = String(dictTableName || '').toLowerCase();
      window._dictColumnTypesCache = window._dictColumnTypesCache || {};
      let cols = window._dictColumnTypesCache[cacheKey];
      if (!Array.isArray(cols) || cols.length === 0) {
        const u = new URL(TYPE_API, window.location.origin);
        u.searchParams.set('table', dictTableName);
        const res = await fetch(u.toString());
        if (!res.ok) return { updated: 0 };
        const payload = await res.json().catch(() => ({}));
        cols = Array.isArray(payload?.columns) ? payload.columns : [];
        window._dictColumnTypesCache[cacheKey] = cols;
      }
      const typeMap = new Map();
      cols.forEach(c => {
        const name = (c.ColumnName || c.columnName || c.name || '').toString().trim();
        const type = (c.DataType || c.dataType || '').toString().trim();
        if (name) typeMap.set(name.toLowerCase(), type);
      });
      if (!typeMap.size) return { updated: 0 };

      let updated = 0;
      tbody.querySelectorAll('tr').forEach(tr => {
        const field = (tr.getAttribute('data-fieldname') || '').toString().trim().toLowerCase();
        if (!field) return;
        const dbType = typeMap.get(field);
        if (!dbType) return;
        const inp = tr.querySelector('input[data-field="DataType"]');
        if (!inp) return;
        const cur = (inp.value || '').toString().trim();
        if (cur) return;
        inp.value = dbType;
        updated += 1;
        syncRowDirtyUi(tr);
      });
      return { updated };
    } catch (err) {
      console.warn('[fieldDictModal] syncDbDataTypes failed:', err);
      return { updated: 0 };
    }
  }
  function initDirtyTracking(tbody) {
    if (!tbody) return;

    const alreadyBound = tbody.dataset.dirtyBound === '1';
    if (!alreadyBound) {
      tbody.dataset.dirtyBound = '1';

      tbody.addEventListener('input', (e) => {
        const tr = e.target?.closest?.('tr');
        if (!tr) return;
        syncRowDirtyUi(tr);
      });
      tbody.addEventListener('change', (e) => {
        const tr = e.target?.closest?.('tr');
        if (!tr) return;
        syncRowDirtyUi(tr);
      });
    }

    tbody.querySelectorAll('tr').forEach(tr => {
      setRowSnapshot(tr);
      syncRowDirtyUi(tr);
    });
  }

  window.applyGridLayoutByRowCol = function (tableSelector = '#fieldDictTable tbody') {
    const tbody = document.querySelector(tableSelector);
    if (!tbody) {
      alert('找不到辭典表格內容');
      return;
    }
    if (!confirm('將全部欄位套用格線位置設定，是否繼續？')) return;


    const rows = Array.from(tbody.querySelectorAll('tr'));
    let applied = 0;

    rows.forEach(tr => {
      const getVal = name => tr.querySelector(`input[data-field="${name}"]`)?.value ?? '';
      const row = getVal('iLayRow');
      const col = getVal('iLayColumn');
      const layout = calcGridLayout(row, col);
      if (!layout) return;

      Object.keys(layout).forEach(key => {
        const inp = tr.querySelector(`input[data-field="${key}"]`);
        if (inp) inp.value = String(layout[key]);
      });

      applied += 1;
      syncRowDirtyUi(tr);
    });

    if (applied === 0) {
      alert('沒有可套用的版面設定');
      return;
    }
    alert('已套用 ' + applied + ' 筆');
  };

      function sortDictTbody(tbody) {
    if (!tbody) return;
    const rows = Array.from(tbody.querySelectorAll('tr'));
    rows.sort((a, b) => {
      const aVal = a.querySelector('input[data-field="SerialNum"]')?.value?.trim();
      const bVal = b.querySelector('input[data-field="SerialNum"]')?.value?.trim();
      const aNum = aVal === '' ? 9999 : parseInt(aVal, 10);
      const bNum = bVal === '' ? 9999 : parseInt(bVal, 10);
      if (aNum !== bNum) return aNum - bNum;

      const aName = a.getAttribute('data-fieldname') || '';
      const bName = b.getAttribute('data-fieldname') || '';
      return aName.localeCompare(bName);
    });
    rows.forEach(tr => tbody.appendChild(tr));
  }

  function buildOcxMapsPayload(dirtyRows, dictTableName) {
    const list = [];
    dirtyRows.forEach(tr => {
      const getVal = name => tr.querySelector(`input[data-field="${name}"]`)?.value ?? '';
      const enc = getVal('KeyMapsJson');
      let maps = [];
      try {
        const raw = decodeURIComponent(enc || '');
        maps = raw ? (JSON.parse(raw) || []) : [];
      } catch { maps = []; }
      if (!Array.isArray(maps)) maps = [];

      list.push({
        TableName: dictTableName,
        FieldName: tr.getAttribute('data-fieldname') || '',
        Maps: maps
          .map(m => ({
            KeyFieldName: (m?.KeyFieldName ?? m?.keyFieldName ?? '').toString().trim(),
            KeySelfName: (m?.KeySelfName ?? m?.keySelfName ?? '').toString().trim()
          }))
          .filter(m => m.KeyFieldName || m.KeySelfName)
      });
    });
    return list;
  }

  async function saveAllDictFields(tableSelector = "#fieldDictTable tbody", apiUrl = SAVE_API) {
    if (saveAllDictFields.__busy) return;
    saveAllDictFields.__busy = true;
    document.body.style.cursor = "wait";

    try {
      const tbody = document.querySelector(tableSelector);
      if (!tbody) {
        alert("找不到辭典表格內容");
        return;
      }

      const dictTableName =
        tbody.getAttribute("data-loaded-for") ||
        tbody.dataset.dictTable ||
        window._dictTableName ||
        "";

      if (!dictTableName) {
        alert("找不到辭典表名");
        return;
      }

      await syncDbDataTypes(tbody, dictTableName);

      const allRows = Array.from(tbody.querySelectorAll("tr"));
      const dirtyRows = allRows.filter(tr => tr.dataset.dirty === "1" || isRowDirty(tr));
      console.log("[fieldDictModal] save changed rows:", dirtyRows.length, "/", allRows.length, "table:", dictTableName);

      if (dirtyRows.length === 0) {
        alert("沒有要儲存的變更");
        return;
      }

      const data = dirtyRows.map(tr => {
        const getVal = name => tr.querySelector(`input[data-field="${name}"]`)?.value ?? "";
        const getInt = name => {
          const v = getVal(name);
          return v === "" ? null : parseInt(v, 10);
        };
        const getChk = name => {
          const box = tr.querySelector(`input[type="checkbox"][data-field="${name}"]`);
          if (box) return box.checked ? 1 : 0;
          const raw = tr.querySelector(`input[data-field="${name}"]`)?.value ?? "";
          return raw === "" ? 0 : (parseInt(raw, 10) ? 1 : 0);
        };
        const prev = (() => {
          const raw = tr.dataset.dictSnapshot;
          if (!raw) return null;
          try { return JSON.parse(raw); } catch { return null; }
        })();
        const sameVal = (name, cur) => String(cur ?? "") === String(prev?.[name] ?? "");
        const getChangedInt = name => {
          const v = getVal(name);
          if (sameVal(name, v)) return null;
          return v === "" ? null : parseInt(v, 10);
        };

        return {
          TableName: dictTableName,
          FieldName: tr.getAttribute("data-fieldname") || "",
          SerialNum: getInt("SerialNum"),
          DisplayLabel: getVal("DisplayLabel"),
          Visible: getChk("Visible"),
          ReadOnly: getChk("ReadOnly"),
          DataType: getVal("DataType"),
          FormatStr: getVal("FormatStr"),
          FieldNote: getVal("FieldNote"),
          EditColor: getVal("EditColor"),
          DisplaySize: getChangedInt("DisplaySize"),
          iLayRow: getChangedInt("iLayRow"),
          iLayColumn: getChangedInt("iLayColumn"),
          iLabHeight: getChangedInt("iLabHeight"),
          iLabTop: getChangedInt("iLabTop"),
          iLabLeft: getChangedInt("iLabLeft"),
          iLabWidth: getChangedInt("iLabWidth"),
          iFieldHeight: getChangedInt("iFieldHeight"),
          iFieldTop: getChangedInt("iFieldTop"),
          iFieldLeft: getChangedInt("iFieldLeft"),
          iFieldWidth: getChangedInt("iFieldWidth"),
          iShowWhere: getChangedInt("iShowWhere"),
          LookupTable: getVal("LookupTable"),
          LookupKeyField: getVal("LookupKeyField"),
          LookupResultField: getVal("LookupResultField"),
          IsNotesField: getVal("IsNotesField"),
          ComboStyle: getChk("ComboStyle"),
          OCXLKTableName: getVal("OCXLKTableName"),
          OCXLKResultName: getVal("OCXLKResultName")
        };
      });

      const ocxPayload = buildOcxMapsPayload(dirtyRows, dictTableName);

      const res = await fetch(apiUrl, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data)
      });
      const result = await res.json().catch(() => ({}));
      if (!result?.success) {
        alert(result?.message || "儲存失敗");
        return;
      }

      const res2 = await fetch(SAVE_OCXMAPS_API, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(ocxPayload)
      });
      const result2 = await res2.json().catch(() => ({}));
      if (!result2?.success) {
        alert(result2?.message || "OCX Key 儲存失敗");
        return;
      }

      dirtyRows.forEach(tr => {
        setRowSnapshot(tr);
        syncRowDirtyUi(tr);
      });

      alert("儲存完成，已更新 " + dirtyRows.length + " 筆。");
      window.dispatchEvent(new Event("field-dict-saved"));
      setTimeout(() => location.reload(), 300);
    } catch (err) {
      alert("API 失敗：" + err);
    } finally {
      document.body.style.cursor = "default";
      saveAllDictFields.__busy = false;
    }
  }

  window.saveAllDictFields = saveAllDictFields;

  function bindSaveButton(tbody) {
    const btn = document.getElementById('btnDictSaveAll');
    if (!btn) return;
    btn.onclick = async () => {
      sortDictTbody(tbody);
      await saveAllDictFields('#fieldDictTable tbody', SAVE_API);
    };
  }

  document.addEventListener('DOMContentLoaded', () => {
    const tbody =
      document.querySelector('#fieldDictTable tbody') ||
      document.querySelector('.dictTableBody') ||
      document.querySelector('tbody[data-role="dict"]');
    if (tbody) sortDictTbody(tbody);
  });

  (function bindGlobalF3Once() {
    if (window.__fieldDictF3Bound) return;
    window.__fieldDictF3Bound = true;

    function resolveModalId() {
      if (document.getElementById('fieldDictModal')) return 'fieldDictModal';
      const el =
        document.querySelector('[data-role="field-dict-modal"]') ||
        document.querySelector('#fieldDictTable')?.closest('.modal') ||
        document.querySelector('.modal[id*="FieldDictModal"]') ||
        document.querySelector('.modal[id*="dictModal"]');
      return el?.id || null;
    }

    document.addEventListener('keydown', function (e) {
      if (e.key !== 'F3' || e.repeat) return;
      e.preventDefault();

      const modalId = resolveModalId();
      if (!modalId) { console.warn('?曆??啗冪??Modal ?辣'); return; }

      const focusEl = document.activeElement?.closest?.('[data-dict-table]');
      const pt      = window.__lastMouse || { x: 0, y: 0 };
      const hoverEl = document.elementFromPoint?.(pt.x, pt.y)?.closest?.('[data-dict-table]');
      const current = document.querySelector('.ctx-current[data-dict-table]');

      const tname =
        (focusEl?.dataset?.dictTable) ||
        (hoverEl?.dataset?.dictTable) ||
        (current?.dataset?.dictTable) ||
        window._dictTableName ||
        document.querySelector('[data-dict-table]')?.dataset?.dictTable ||
        document.body?.dataset?.dictTable ||
        document.querySelector('meta[name="dict-table"]')?.content ||
        '';

      if (!tname) { return; }
      window.showDictModal(modalId, tname);
    });

  })();

  if (!window.__dictCtxBound) {
    window.__dictCtxBound = true;

    const setCtxEl = (el) => {
      if (!el) return;
      document.querySelectorAll('.ctx-current').forEach(x => x.classList.remove('ctx-current'));
      el.classList.add('ctx-current');
      window._dictTableName = el.dataset?.dictTable || window._dictTableName || '';
    };

    document.addEventListener('pointerdown', (ev) => {
      const host = ev.target?.closest?.('[data-dict-table]');
      if (host) setCtxEl(host);
    }, true);

    document.addEventListener('focusin', (ev) => {
      const host = ev.target?.closest?.('[data-dict-table]');
      if (host) setCtxEl(host);
    });

    document.addEventListener('mousemove', (e) => {
      window.__lastMouse = { x: e.clientX, y: e.clientY };
    }, { passive: true });
  }

  document.addEventListener('hidden.bs.modal', e => {
    if (e.target.id === 'fieldDictModal') {
      // 蝘駁 backdrop
      document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
      document.body.classList.remove('modal-open');
      document.body.style.overflow = '';

      const overlay = document.getElementById('loadingOverlay');
      if (overlay) {
        overlay.classList.remove('show');
        overlay.style.display = 'none';
      }
      document.body.removeAttribute('aria-busy');
    }
  });

  (function setupFieldDetailModal() {

      const modalEl = document.getElementById('fieldDetailModal');
      if (!modalEl) return;

      const titleSpan = modalEl.querySelector('#fieldDetailTitle');
      const applyBtn = modalEl.querySelector('#btnFieldDetailApply');

      const DETAIL_FIELDS = [
          'DisplaySize',
          'iShowWhere',
          'iLayRow', 'iLayColumn',
          'iLabTop', 'iLabLeft', 'iLabWidth', 'iLabHeight',
          'iFieldTop', 'iFieldLeft', 'iFieldWidth', 'iFieldHeight',
          'LookupTable', 'LookupKeyField', 'LookupResultField',
          'IsNotesField',

          'OCXLKTableName',
          'OCXLKResultName'
      ];

      let currentTr = null;
      let gridAppliedRow = null;
      let gridAppliedCol = null;
      const keyMapBody = modalEl.querySelector('#fdm_keyMapTable tbody');
      const addKeyBtn = modalEl.querySelector('#fdm_addKeyMap');

      function applyGridDefaultsInModal() {
          const rowInp = modalEl.querySelector('[data-detail-field="iLayRow"]');
          const colInp = modalEl.querySelector('[data-detail-field="iLayColumn"]');
          if (!rowInp || !colInp) return;
          const layout = calcGridLayout(rowInp.value, colInp.value);
          if (!layout) return;
          Object.keys(layout).forEach(key => {
              const el = modalEl.querySelector(`[data-detail-field="${key}"]`);
              if (el) el.value = layout[key];
          });
      }

      function normalizeKeyMaps(list) {
          const arr = Array.isArray(list) ? list : [];
          return arr
              .map(m => ({
                  KeyFieldName: (m?.KeyFieldName ?? m?.keyFieldName ?? '').toString().trim(),
                  KeySelfName: (m?.KeySelfName ?? m?.keySelfName ?? '').toString().trim()
              }));
      }

      function renderKeyMapGrid(maps) {
          if (!keyMapBody) return;
          const 筆 = normalizeKeyMaps(maps);
          const show = 筆.length ? 筆 : [{ KeyFieldName: '', KeySelfName: '' }];
          keyMapBody.innerHTML = show.map(m => `
              <tr>
                <td><input type="text" class="form-control form-control-sm fdm-keyfield" value="${m.KeyFieldName ?? ''}"></td>
                <td><input type="text" class="form-control form-control-sm fdm-keyself" value="${m.KeySelfName ?? ''}"></td>
                <td class="text-center">
                  <button type="button" class="btn btn-outline-danger btn-sm fdm-del-keymap" title="?芷">嚗?/button>
                </td>
              </tr>
          `).join('');
      }

      function readKeyMapGrid(includeEmpty = false) {
          if (!keyMapBody) return [];
          const trs = Array.from(keyMapBody.querySelectorAll('tr'));
          const 筆 = trs.map(tr => ({
              KeyFieldName: (tr.querySelector('input.fdm-keyfield')?.value ?? '').toString().trim(),
              KeySelfName: (tr.querySelector('input.fdm-keyself')?.value ?? '').toString().trim()
          }));
          return includeEmpty ? 筆 : 筆.filter(m => m.KeyFieldName || m.KeySelfName);
      }

      if (addKeyBtn && !addKeyBtn.dataset.bound) {
          addKeyBtn.dataset.bound = '1';
          addKeyBtn.addEventListener('click', () => {
              const cur = readKeyMapGrid(true);
              cur.push({ KeyFieldName: '', KeySelfName: '' });
              renderKeyMapGrid(cur);
          });
      }

      if (keyMapBody && !keyMapBody.dataset.bound) {
          keyMapBody.dataset.bound = '1';
          keyMapBody.addEventListener('click', (e) => {
              const btn = e.target?.closest?.('.fdm-del-keymap');
              if (!btn) return;
              btn.closest('tr')?.remove();
              const cur = readKeyMapGrid();
              if (cur.length === 0) renderKeyMapGrid([{ KeyFieldName: '', KeySelfName: '' }]);
          });
      }

      if (!modalEl.dataset.gridBound) {
          modalEl.dataset.gridBound = '1';
          const rowInp = modalEl.querySelector('[data-detail-field="iLayRow"]');
          const colInp = modalEl.querySelector('[data-detail-field="iLayColumn"]');
          const onGridChange = () => {
              applyGridDefaultsInModal();
              gridAppliedRow = rowInp?.value ?? null;
              gridAppliedCol = colInp?.value ?? null;
          };
          rowInp?.addEventListener('input', onGridChange);
          colInp?.addEventListener('input', onGridChange);
      }

      window.editFieldDetail = function (fieldName) {

          const safeName = (window.CSS && CSS.escape)
              ? CSS.escape(fieldName)
              : fieldName.replace(/(["\\])/g, "\\$1");

          const tr = document.querySelector(`.dictTableBody tr[data-fieldname="${safeName}"]`);
          if (!tr) {
              alert('找不到欄位： ' + fieldName);
              return;
          }

          currentTr = tr;

          const displayLabel = tr.querySelector('input[data-field="DisplayLabel"]')?.value ?? '';
          titleSpan.textContent = fieldName + (displayLabel ? ' (' + displayLabel + ')' : '');

          DETAIL_FIELDS.forEach(name => {

              const rowInput = tr.querySelector(`input[data-field="${name}"]`);
              const dlgInput = modalEl.querySelector(`[data-detail-field="${name}"]`);

              if (!dlgInput) return;

              if (name === "IsNotesField") {
                  dlgInput.checked = (rowInput?.value === "1");
              }
              else {
                  dlgInput.value = rowInput?.value ?? "";
              }
          });
          const initRow = tr.querySelector('input[data-field="iLayRow"]')?.value ?? '';
          const initCol = tr.querySelector('input[data-field="iLayColumn"]')?.value ?? '';
          modalEl.dataset.gridInitRow = initRow;
          modalEl.dataset.gridInitCol = initCol;
          gridAppliedRow = initRow;
          gridAppliedCol = initCol;

          try {
              const enc = tr.querySelector('input[data-field="KeyMapsJson"]')?.value ?? '';
              const raw = decodeURIComponent(enc || '');
              const maps = raw ? (JSON.parse(raw) || []) : [];
              if (Array.isArray(maps) && maps.length) {
                  renderKeyMapGrid(maps);
              } else {
                  const kf = tr.querySelector('input[data-field="KeyFieldName"]')?.value ?? '';
                  const ks = tr.querySelector('input[data-field="KeySelfName"]')?.value ?? '';
                  renderKeyMapGrid([{ KeyFieldName: kf, KeySelfName: ks }]);
              }
          } catch {
              const kf = tr.querySelector('input[data-field="KeyFieldName"]')?.value ?? '';
              const ks = tr.querySelector('input[data-field="KeySelfName"]')?.value ?? '';
              renderKeyMapGrid([{ KeyFieldName: kf, KeySelfName: ks }]);
          }

          bootstrap.Modal.getOrCreateInstance(modalEl).show();
      };

      // ==================== 憟 ====================
      if (applyBtn) {
          applyBtn.addEventListener("click", () => {

              if (!currentTr) return;
              const rowInp = modalEl.querySelector('[data-detail-field="iLayRow"]');
              const colInp = modalEl.querySelector('[data-detail-field="iLayColumn"]');
              const curRow = rowInp?.value ?? null;
              const curCol = colInp?.value ?? null;
              if (curRow !== gridAppliedRow || curCol !== gridAppliedCol) {
                  applyGridDefaultsInModal();
                  gridAppliedRow = curRow;
                  gridAppliedCol = curCol;
              }

              DETAIL_FIELDS.forEach(name => {

                  const rowInput = currentTr.querySelector(`input[data-field="${name}"]`);
                  const dlgInput = modalEl.querySelector(`[data-detail-field="${name}"]`);

                  if (!rowInput || !dlgInput) return;

                  if (name === 'IsNotesField') {
                      rowInput.value = dlgInput.checked ? "1" : "0";
                  } else {
                      rowInput.value = dlgInput.value;
                  }
              });

              const maps = readKeyMapGrid(false);
              const kmInp = currentTr.querySelector('input[data-field="KeyMapsJson"]');
              if (kmInp) kmInp.value = encodeURIComponent(JSON.stringify(maps));

              const first = maps[0] || { KeyFieldName: '', KeySelfName: '' };
              const kfInp = currentTr.querySelector('input[data-field="KeyFieldName"]');
              const ksInp = currentTr.querySelector('input[data-field="KeySelfName"]');
              if (kfInp) kfInp.value = first.KeyFieldName || '';
              if (ksInp) ksInp.value = first.KeySelfName || '';

              bootstrap.Modal.getInstance(modalEl)?.hide();
          });
      }

  })();


})();







