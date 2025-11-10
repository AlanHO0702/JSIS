// /wwwroot/js/fieldDictModal.js
// 載入 → 排序 → 綁定 → 儲存（支援伺服端已渲染 tbody、可配置 API）

(function () {
  // 可在頁面先設：window.FIELD_DICT_GET_API / window.FIELD_DICT_SAVE_API / window.SUPPRESS_DICT_FETCH_ALERT
  const GET_API  = window.FIELD_DICT_GET_API  || '/api/TableFieldLayout/GetTableFields';
  const SAVE_API = window.FIELD_DICT_SAVE_API || '/api/DictApi/UpdateDictFields';
  const QUIET    = !!window.SUPPRESS_DICT_FETCH_ALERT;

  // ===== 顯示（會先 init 再 show） =====
  window.showDictModal = async function (modalId = 'fieldDictModal', tableName = window._dictTableName) {
    const el = document.getElementById(modalId);
    if (!el) { console.warn('找不到辭典 Modal 元件:', modalId); return; }
    await window.initFieldDictModal(tableName, modalId);
    new bootstrap.Modal(el).show();
  };

  // ===== 初始化：撈資料（若 tbody 已有資料就不覆蓋）、排序、綁定 =====
  window.initFieldDictModal = async function (tableName, modalId = 'fieldDictModal') {
    const tname = (tableName || window._dictTableName || '').trim();
    if (!tname) { alert('沒有指定辭典表名'); return; }

    const scope = document.getElementById(modalId) || document;
    const tbody =
      scope.querySelector('#fieldDictTable tbody') ||
      scope.querySelector('.dictTableBody') ||
      scope.querySelector('tbody[data-role="dict"]');

    if (!tbody) { console.warn('找不到辭典 tbody'); return; }

    const alreadyHasRows = tbody.children && tbody.children.length > 0;

    // 若頁面已經 server-side 渲染出資料，就**不再覆蓋**；只在空的時候才去撈 API
    if (!alreadyHasRows) {
      try {
        const url = `${GET_API}?tableName=${encodeURIComponent(tname)}`;
        const res = await fetch(url);
        if (!res.ok) {
          if (!QUIET) alert('載入辭典欄位失敗');
        } else {
          const rows = await res.json();
          // 依序號，空值放最後
          rows.sort((a, b) => (Number(a.SerialNum ?? 9999)) - (Number(b.SerialNum ?? 9999)));

          tbody.innerHTML = rows.map(x => `
            <tr data-tablename="${x.TableName || tname}" data-fieldname="${x.FieldName}">
              <td style="width:80px">
                <input data-field="SerialNum" value="${x.SerialNum ?? ''}" class="form-control form-control-sm" />
              </td>
              <td style="min-width:180px">
                <input data-field="FieldName" value="${x.FieldName ?? ''}" class="form-control form-control-sm" readonly />
              </td>
              <td style="min-width:220px">
                <input data-field="DisplayLabel" value="${x.DisplayLabel ?? ''}" class="form-control form-control-sm" />
              </td>
              <td class="text-center" style="width:60px">
                <input type="checkbox" data-field="Visible" ${(+x.Visible === 1 ? 'checked' : '')} />
              </td>
              <td class="text-center" style="width:60px">
                <input type="checkbox" data-field="ReadOnly" ${(+x.ReadOnly === 1 ? 'checked' : '')} />
              </td>
              <td style="width:140px">
                <input data-field="DataType" value="${x.DataType ?? ''}" class="form-control form-control-sm" />
              </td>
              <td style="width:160px">
                <input data-field="FormatStr" value="${x.FormatStr ?? ''}" class="form-control form-control-sm" />
              </td>
              <td>
                <input data-field="FieldNote" value="${x.FieldNote ?? ''}" class="form-control form-control-sm" />
              </td>
            </tr>
          `).join('');
        }
      } catch (err) {
        if (!QUIET) alert('載入辭典欄位失敗');
        console.warn('[fieldDictModal] fetch error:', err);
      }
    } else {
      // 已有資料（Razor 產生），什麼都不做
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

  const rows = document.querySelectorAll(`${tableSelector} tr`);
  const data = Array.from(rows).map(tr => {
    const getVal = name => tr.querySelector(`input[data-field="${name}"]`)?.value ?? '';
    const getInt = name => {
      const v = getVal(name);
      return v === '' ? null : parseInt(v, 10);
    };
    const getChk = name => tr.querySelector(`input[data-field="${name}"]`)?.checked ? 1 : 0;

    return {
      // === 基本欄位 ===
      TableName: tr.getAttribute('data-tablename') || window._dictTableName || '',
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

      document.addEventListener('keydown', (e) => {
        if (e.key !== 'F3' || e.repeat) return;
        e.preventDefault();

        const modalId = resolveModalId();
        if (!modalId) { console.warn('找不到辭典 Modal 元件 (自動偵測失敗)'); return; }

        const tname =
          (window._dictTableName && String(window._dictTableName).trim()) ||
          document.body?.dataset?.dictTable ||
          document.querySelector('meta[name="dict-table"]')?.content || '';

        if (!tname) { alert('沒有指定辭典表名 (window._dictTableName)'); return; }
        window.showDictModal(modalId, tname);
      });
    })();

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

})();


