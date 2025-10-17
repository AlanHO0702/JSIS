// /wwwroot/js/fieldDictModal.js
// 完整版：載入 → 排序 → 綁定 → 儲存（含 TableName/DataType/FormatStr 等欄位）

(function(){

  // ===== 顯示（會先載入再 show） =====
  window.showDictModal = async function(modalId = 'fieldDictModal', tableName = window._dictTableName) {
    const el = document.getElementById(modalId);
    if (!el) { console.warn('找不到辭典 Modal 元件:', modalId); return; }
    await window.initFieldDictModal(tableName, modalId);
    new bootstrap.Modal(el).show();
  };

  // ===== 初始化：撈資料、排序、綁到 tbody =====
  window.initFieldDictModal = async function(tableName, modalId = 'fieldDictModal') {
    const tname = tableName || window._dictTableName || '';
    if (!tname) { alert('沒有指定辭典表名'); return; }

    const url = `/api/TableFieldLayout/GetTableFields?tableName=${encodeURIComponent(tname)}`;
    const res = await fetch(url);
    if (!res.ok) { alert("載入辭典欄位失敗"); return; }

    const rows = await res.json();
    // 依序號，空值放最後
    rows.sort((a,b) => (Number(a.SerialNum ?? 9999)) - (Number(b.SerialNum ?? 9999)));

    const scope = document.getElementById(modalId) || document;
    const tbody =
      scope.querySelector('#fieldDictTable tbody') ||
      scope.querySelector('.dictTableBody') ||
      scope.querySelector('tbody[data-role="dict"]');

    if (!tbody) { console.warn('找不到辭典 tbody'); return; }

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
          <input type="checkbox" data-field="Visible" ${x.Visible === 1 ? 'checked' : ''} />
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

    // 初次載入後做一次穩定排序（若使用者手動改序號也能按鈕再次排序）
    sortDictTbody(tbody);
    // 綁定「全部儲存」按鈕
    bindSaveButton(tbody);
  };

  // ===== 排序 tbody（依 SerialNum，其次 FieldName） =====
  function sortDictTbody(tbody){
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

  // ===== 儲存動作（含 TableName fallback 與 DataType/FormatStr 送出） =====
  function saveAllDictFields(tableSelector = '#fieldDictTable tbody', apiUrl = '/api/DictApi/UpdateDictFields') {
    document.body.style.cursor = "wait";

    const rows = document.querySelectorAll(`${tableSelector} tr`);
    const data = Array.from(rows).map(tr => {
      const serialNumValue = tr.querySelector('input[data-field="SerialNum"]')?.value ?? '';
      const visibleValue   = tr.querySelector('input[data-field="Visible"]')?.checked ? 1 : 0;
      return {
        TableName: tr.getAttribute('data-tablename') || window._dictTableName || '', // ✅ 關鍵：fallback
        FieldName: tr.getAttribute('data-fieldname') || '',
        DisplayLabel: tr.querySelector('input[data-field="DisplayLabel"]')?.value ?? '',
        DataType:     tr.querySelector('input[data-field="DataType"]')?.value ?? '',
        FormatStr:    tr.querySelector('input[data-field="FormatStr"]')?.value ?? '',
        FieldNote:    tr.querySelector('input[data-field="FieldNote"]')?.value ?? '',
        SerialNum: serialNumValue === '' ? null : parseInt(serialNumValue, 10),
        Visible: visibleValue,
        // 若未使用可留著，後端會忽略 null
        LookupResultField: (()=>{
          const el = tr.querySelector('input[data-field="LookupResultField"]');
          if (!el) return null;
          const v = el.value.trim();
          return v === "" ? null : v;
        })()
      };
    });

    // Debug
    console.log('UpdateDictFields payload:', data);

    fetch(apiUrl, {
      method: 'POST',
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    })
    .then(res => res.json())
    .then(result => {
      document.body.style.cursor = "default";
      if (result.success) {
        alert("全部儲存成功！");
        window.dispatchEvent(new Event('field-dict-saved'));
      } else {
        alert(result.message || "儲存失敗！");
      }
    })
    .catch(err => {
      document.body.style.cursor = "default";
      alert("API 失敗: " + err);
    });
  }

  // ===== 綁定「全部儲存」按鈕（依你 Modal 內按鈕 id 調整） =====
  function bindSaveButton(tbody){
    const btn = document.getElementById('btnDictSaveAll');
    if (!btn) return;
    btn.onclick = () => {
      // 儲存前再排一次順序（可選）
      sortDictTbody(tbody);
      saveAllDictFields('#fieldDictTable tbody', '/api/DictApi/UpdateDictFields');
    };
  }

  // （可選）頁面載入就把既有 DOM 的辭典表做一次排序
  document.addEventListener('DOMContentLoaded', () => {
    const tbody =
      document.querySelector('#fieldDictTable tbody') ||
      document.querySelector('.dictTableBody') ||
      document.querySelector('tbody[data-role="dict"]');
    if (tbody) sortDictTbody(tbody);
  });

})();
