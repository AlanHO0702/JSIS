// /wwwroot/js/fieldDictModal.js
// 載入 → 排序 → 綁定 → 儲存（支援伺服端已渲染 tbody、可配置 API）

(function () {

    window.showDictModal = async function (modalId = 'fieldDictModal', tableName = window._dictTableName) {
      const el = document.getElementById(modalId);
      if (!el) { console.warn('找不到辭典 Modal 元件:', modalId); return; }

      await window.initFieldDictModal(tableName, modalId);

      const md = bootstrap.Modal.getOrCreateInstance(el);
      md.show();
  };

  // 記錄滑鼠位置，F3 時可用來判斷就近的卡片
  window.__lastMouse = window.__lastMouse || { x: 0, y: 0 };
  document.addEventListener('mousemove', (e) => {
    window.__lastMouse.x = e.clientX;
    window.__lastMouse.y = e.clientY;
  }, { passive: true });

  // 可在頁面先設：window.FIELD_DICT_GET_API / window.FIELD_DICT_SAVE_API / window.SUPPRESS_DICT_FETCH_ALERT
  const GET_API   = window.FIELD_DICT_GET_API  || '/api/TableFieldLayout/GetTableFieldsFull';
  const QUERY_KEY = window.FIELD_DICT_QUERY_KEY || 'table';
  const SAVE_API  = window.FIELD_DICT_SAVE_API || '/api/DictApi/UpdateDictFields';
  const QUIET     = !!window.SUPPRESS_DICT_FETCH_ALERT;



  // ===== 初始化：撈資料（若 tbody 已有資料就不覆蓋）、排序、綁定 =====
  window.initFieldDictModal = async function (tableName, modalId = 'fieldDictModal') {
    const tname = (tableName || window._dictTableName || '').trim();
    if (!tname) { alert('沒有指定辭典表名'); return; }

    // ⭐ 記錄目前正在編輯的辭典表名，給儲存時統一使用
    window._dictTableName = tname;

    const scope = document.getElementById(modalId) || document;
    const tbody =
      scope.querySelector('#fieldDictTable tbody') ||
      scope.querySelector('.dictTableBody') ||
      scope.querySelector('tbody[data-role="dict"]');

    if (!tbody) { console.warn('找不到辭典 tbody'); return; }

    // ▶︎ 只要欲載入的表名與目前不一致，就先清空並強制重載
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
          const rows = await res.json();
          rows.sort((a, b) => (Number(a.SerialNum ?? 9999)) - (Number(b.SerialNum ?? 9999)));

          tbody.innerHTML = rows.map(x => `
          <tr data-tablename="${x.TableName || tname}"
              data-fieldname="${x.FieldName}"
              ondblclick="window.editFieldDetail && window.editFieldDetail('${x.FieldName}')">

            <!-- 第 1 欄：序號 + 隱藏欄位 -->
            <td style="width:60px">
              <input data-field="SerialNum" type="number"
                    value="${x.SerialNum ?? ''}" class="form-control form-control-sm" />

              <span class="d-none extra-fields">
                <input data-field="DisplaySize"  value="${x.DisplaySize  ?? ''}" />
                <input data-field="iLabHeight"   value="${x.iLabHeight   ?? ''}" />
                <input data-field="iLabTop"      value="${x.iLabTop      ?? ''}" />
                <input data-field="iLabLeft"     value="${x.iLabLeft     ?? ''}" />
                <input data-field="iLabWidth"    value="${x.iLabWidth    ?? ''}" />
                <input data-field="iFieldHeight" value="${x.iFieldHeight ?? ''}" />
                <input data-field="iFieldTop"    value="${x.iFieldTop    ?? ''}" />
                <input data-field="iFieldLeft"   value="${x.iFieldLeft   ?? ''}" />
                <input data-field="iFieldWidth"  value="${x.iFieldWidth  ?? ''}" />

                <input data-field="LookupTable"       value="${x.LookupTable       ?? ''}" />
                <input data-field="LookupKeyField"    value="${x.LookupKeyField    ?? ''}" />
                <input data-field="LookupResultField" value="${x.LookupResultField ?? ''}" />

                <input data-field="IsNotesField" value="${x.IsNotesField ?? ''}" />

                <input data-field="OCXLKTableName" value="${x.OCXLKTableName ?? ''}" />
                <input data-field="OCXLKResultName" value="${x.OCXLKResultName?? ''}" />
                <input data-field="KeyFieldName" value="${x.KeyFieldName ?? ''}" />
                <input data-field="KeySelfName" value="${x.KeySelfName ?? ''}" />

              </span>
            </td>

            <!-- 第 2 ~ 9 欄 -->
            <td style="min-width:180px">${x.FieldName ?? ''}</td>

            <td style="min-width:180px">
              <input data-field="DisplayLabel" value="${x.DisplayLabel ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td class="text-center" style="width:40px">
              <input type="checkbox" data-field="Visible"
                    ${(+x.Visible === 1 ? 'checked' : '')} />
            </td>

            <td class="text-center" style="width:40px">
              <input type="checkbox" data-field="ReadOnly"
                    ${(+x.ReadOnly === 1 ? 'checked' : '')} />
            </td>

            <td style="width:50px">
              <input data-field="iShowWhere" value="${x.iShowWhere ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td style="width:140px">
              <input data-field="DataType" value="${x.DataType ?? ''}"
                    class="form-control form-control-sm"
                    readonly
                    style="background:#f2f2f2; color:#8a9399; border:1px solid #dee2e6;"
                    tabindex="-1" />
            </td>

            <td style="width:160px">
              <input data-field="FormatStr" value="${x.FormatStr ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <td>
              <input data-field="FieldNote" value="${x.FieldNote ?? ''}"
                    class="form-control form-control-sm" />
            </td>

            <!-- 第 10 欄：⚙ 設定按鈕 -->
            <td style="width:60px" class="text-center">
              <button type="button" class="btn btn-sm btn-outline-secondary"
                      onclick="window.editFieldDetail && window.editFieldDetail('${x.FieldName}')">
                ⚙
              </button>
            </td>
          </tr>
        `).join('');

          // 標記目前已載入哪一張辭典表
          tbody.setAttribute('data-loaded-for', tname);
        }
      } catch (err) {
        if (!QUIET) alert('載入辭典欄位失敗');
        console.warn('[fieldDictModal] fetch error:', err);
      }
    }

    // 穩定排序 + 綁保存
    sortDictTbody(tbody);
    bindSaveButton(tbody);
  };

  // ===== 排序（SerialNum → FieldName） =====
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

  // ===== 儲存（完整版本：含欄位位置 + 搜尋設定 + 備註等） =====
  function saveAllDictFields(tableSelector = '#fieldDictTable tbody', apiUrl = SAVE_API) {
    document.body.style.cursor = 'wait';

    const tbody = document.querySelector(tableSelector);
    if (!tbody) {
      document.body.style.cursor = 'default';
      alert('找不到辭典 tbody');
      return;
    }

    // ⭐ 這裡統一決定「要存到哪一個辭典 TableName」
    const dictTableName =
      tbody.getAttribute('data-loaded-for') ||
      tbody.dataset.dictTable ||
      window._dictTableName ||
      '';

    if (!dictTableName) {
      document.body.style.cursor = 'default';
      alert('找不到要儲存的辭典表名');
      return;
    }

    const rows = tbody.querySelectorAll('tr');
    const data = Array.from(rows).map(tr => {
      const getVal = name => tr.querySelector(`input[data-field="${name}"]`)?.value ?? '';
      const getInt = name => {
        const v = getVal(name);
        return v === '' ? null : parseInt(v, 10);
      };
      const getChk = name => tr.querySelector(`input[data-field="${name}"]`)?.checked ? 1 : 0;

      return {
        // === 基本欄位 ===
        TableName: dictTableName,  // ⭐ 一律用目前的辭典表名
        FieldName: tr.getAttribute('data-fieldname') || '',

        SerialNum: getInt('SerialNum'),
        DisplayLabel: getVal('DisplayLabel'),
        Visible: getChk('Visible'),
        ReadOnly: getChk('ReadOnly'),
        DataType: getVal('DataType'),
        FormatStr: getVal('FormatStr'),
        FieldNote: getVal('FieldNote'),

        // === 版面欄位 ===
        DisplaySize: getInt('DisplaySize'),
        iLabHeight: getInt('iLabHeight'),
        iLabTop: getInt('iLabTop'),
        iLabLeft: getInt('iLabLeft'),
        iLabWidth: getInt('iLabWidth'),
        iFieldHeight: getInt('iFieldHeight'),
        iFieldTop: getInt('iFieldTop'),
        iFieldLeft: getInt('iFieldLeft'),
        iFieldWidth: getInt('iFieldWidth'),
        iShowWhere: getInt('iShowWhere'),

        // === 查詢設定 ===
        LookupTable: getVal('LookupTable'),
        LookupKeyField: getVal('LookupKeyField'),
        LookupResultField: getVal('LookupResultField'),

        // === 其他欄位 ===
        IsNotesField: getVal('IsNotesField')
      };
    });

    // 送出 API
    fetch(apiUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    })
      .then(res => res.json())
      .then(result => {
        document.body.style.cursor = 'default';
        if (result?.success) {
          alert('全部儲存成功！');
          window.dispatchEvent(new Event('field-dict-saved'));
          setTimeout(() => location.reload(), 300);
        } else {
          alert(result?.message || '儲存失敗！');
        }
      })
      .catch(err => {
        document.body.style.cursor = 'default';
        alert('API 失敗: ' + err);
      });
  }

  // 讓外面 onclick="saveAllDictFields('#xxx .dictTableBody')" 可直接用
  window.saveAllDictFields = saveAllDictFields;

  // ===== 綁「全部儲存」按鈕（若有固定 id） =====
  function bindSaveButton(tbody) {
    const btn = document.getElementById('btnDictSaveAll');
    if (!btn) return;
    btn.onclick = () => {
      sortDictTbody(tbody);
      saveAllDictFields('#fieldDictTable tbody', SAVE_API);
    };
  }

  // 頁面載入就把現有 DOM 做一次排序（如果有的話）
  document.addEventListener('DOMContentLoaded', () => {
    const tbody =
      document.querySelector('#fieldDictTable tbody') ||
      document.querySelector('.dictTableBody') ||
      document.querySelector('tbody[data-role="dict"]');
    if (tbody) sortDictTbody(tbody);
  });

  // ===== 全域 F3 快捷鍵（自動找 modalId） =====
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
      if (!modalId) { console.warn('找不到辭典 Modal 元件'); return; }

      // ✅ 優先使用：目前焦點 → 滑鼠底下 → ctx-current → 其他 fallback
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

      if (!tname) { alert('沒有指定辭典表名'); return; }
      window.showDictModal(modalId, tname);
    });

  })();

  // ===== 全域情境橋接：不管點/聚焦/滑鼠，隨時更新目前表 =====
  if (!window.__dictCtxBound) {
    window.__dictCtxBound = true;

    const setCtxEl = (el) => {
      if (!el) return;
      document.querySelectorAll('.ctx-current').forEach(x => x.classList.remove('ctx-current'));
      el.classList.add('ctx-current');
      window._dictTableName = el.dataset?.dictTable || window._dictTableName || '';
    };

    // 任何點擊 / 聚焦到含 data-dict-table 的元素，都切情境
    document.addEventListener('pointerdown', (ev) => {
      const host = ev.target?.closest?.('[data-dict-table]');
      if (host) setCtxEl(host);
    }, true);

    document.addEventListener('focusin', (ev) => {
      const host = ev.target?.closest?.('[data-dict-table]');
      if (host) setCtxEl(host);
    });

    // 記錄滑鼠座標給 F3 使用
    document.addEventListener('mousemove', (e) => {
      window.__lastMouse = { x: e.clientX, y: e.clientY };
    }, { passive: true });
  }

  document.addEventListener('hidden.bs.modal', e => {
    if (e.target.id === 'fieldDictModal') {
      // 移除 backdrop
      document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
      document.body.classList.remove('modal-open');
      document.body.style.overflow = '';

      // ✅ 強制隱藏 overlay，不論 busy 狀態
      const overlay = document.getElementById('loadingOverlay');
      if (overlay) {
        overlay.classList.remove('show');
        overlay.style.display = 'none';
      }
      // 移除 body 鎖定
      document.body.removeAttribute('aria-busy');
    }
  });

    // ===== 欄位詳細設定：打開 modal 編輯隱藏欄位 =====
  (function setupFieldDetailModal() {

      const modalEl = document.getElementById('fieldDetailModal');
      if (!modalEl) return;

      const titleSpan = modalEl.querySelector('#fieldDetailTitle');
      const applyBtn = modalEl.querySelector('#btnFieldDetailApply');

      // 需同步的欄位（含 OCX）
      const DETAIL_FIELDS = [
          'DisplaySize',
          'iLabTop', 'iLabLeft', 'iLabWidth', 'iLabHeight',
          'iFieldTop', 'iFieldLeft', 'iFieldWidth', 'iFieldHeight',
          'LookupTable', 'LookupKeyField', 'LookupResultField',
          'IsNotesField',

          // ★ 第二層 OCX 設定
          'OCXLKTableName',
          'OCXLKResultName',
          'KeyFieldName',
          'KeySelfName'
      ];

      let currentTr = null;

      // ==================== 打開欄位設定 ====================
      window.editFieldDetail = function (fieldName) {

          const safeName = (window.CSS && CSS.escape)
              ? CSS.escape(fieldName)
              : fieldName.replace(/(["\\])/g, "\\$1");

          // 找 TR
          const tr = document.querySelector(`.dictTableBody tr[data-fieldname="${safeName}"]`);
          if (!tr) {
              alert("找不到欄位列：" + fieldName);
              return;
          }

          currentTr = tr;

          // 標題：欄位名稱 (顯示名稱)
          const displayLabel = tr.querySelector('input[data-field="DisplayLabel"]')?.value ?? '';
          titleSpan.textContent = fieldName + (displayLabel ? `（${displayLabel}）` : '');

          // TR → Modal
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

          bootstrap.Modal.getOrCreateInstance(modalEl).show();
      };

      // ==================== 套用 ====================
      if (applyBtn) {
          applyBtn.addEventListener("click", () => {

              if (!currentTr) return;

              // Modal → TR
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

              bootstrap.Modal.getInstance(modalEl)?.hide();
          });
      }

  })();


})();
