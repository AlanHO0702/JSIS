// /wwwroot/js/erpGridNav.js
// 通用 GRID 鍵盤導航工具
// 提供：兩段式點擊（導航→編輯）、方向鍵、F7/F8 複製、Enter/Escape
//
// 使用方式：
//   initErpGridNav(tbody, {
//     keyFields  : ['papernum', 'item'],  // 複製時跳過的鍵值欄位（lowercase）
//     isEditMode : () => boolean,          // 目前是否在修改模式
//     addRow     : async () => { ok, ... }, // ArrowDown 最後一列時呼叫；null 表示不支援
//     autoSave   : () => void,             // 值變更後自動存檔
//     onRowSelect: (tr) => void,           // 導航切換列時呼叫
//     gridLabel  : 'multitab',            // window.__activeDetailGrid 的標籤
//   });
//
(function () {

  // ── CSS 只注入一次 ─────────────────────────────────────────────
  (function ensureCss() {
    if (document.getElementById('erp-grid-nav-css')) return;
    const style = document.createElement('style');
    style.id = 'erp-grid-nav-css';
    style.textContent = `
.erp-grid-nav td.active-cell {
  outline: 2px solid #0d6efd !important;
  outline-offset: -2px !important;
  box-shadow: inset 0 0 0 2px #0d6efd !important;
}`;
    document.head.appendChild(style);
  })();

  // ── 輔助函數（全域共用）──────────────────────────────────────────
  const mtIsCellVisible = (td) => {
    if (!td || td.getAttribute('data-field') == null) return false;
    return td.offsetParent !== null && getComputedStyle(td).display !== 'none';
  };

  const mtGetBodyRows = (tbody) =>
    Array.from(tbody.querySelectorAll('tr')).filter(tr => tr.querySelector('td[data-field]'));

  const mtGetCellByField = (tr, fieldName) => {
    if (!tr || !fieldName) return null;
    const f = String(fieldName);
    const sel = (window.CSS?.escape)
      ? `td[data-field="${window.CSS.escape(f)}"]`
      : `td[data-field="${f.replace(/"/g, '\\"')}"]`;
    return tr.querySelector(sel);
  };

  const mtGetFirstVisibleCell = (tr) =>
    tr ? (Array.from(tr.querySelectorAll('td[data-field]')).find(mtIsCellVisible) || null) : null;

  const mtSetActiveCell = (td, enterEdit = false) => {
    // 清除所有 active-cell（跨 tbody）
    document.querySelectorAll('td.active-cell').forEach(el => el.classList.remove('active-cell'));
    if (!td) return;
    td.classList.add('active-cell');
    try { td.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: 'auto' }); } catch {}
    if (enterEdit) {
      const inp = td.querySelector('.cell-edit:not(.d-none):not([readonly]):not([disabled])');
      if (inp) { inp.focus(); try { inp.select(); } catch {} }
      else { td.tabIndex = -1; td.focus(); }
    } else {
      td.tabIndex = -1;
      td.focus();
    }
  };

  const mtResolveArrowTarget = (fromTd, key, tbody) => {
    if (!fromTd) return null;
    const fromTr = fromTd.closest('tr');
    if (!fromTr) return null;
    const rows = mtGetBodyRows(tbody);
    const rowIdx = rows.indexOf(fromTr);
    if (rowIdx < 0) return null;
    const visibleCells = Array.from(fromTr.querySelectorAll('td[data-field]')).filter(mtIsCellVisible);
    if (!visibleCells.length) return null;
    const cellIdx = visibleCells.indexOf(fromTd);
    if (cellIdx < 0) return null;
    if (key === 'ArrowLeft' || key === 'ArrowRight') {
      const nextIdx = cellIdx + (key === 'ArrowLeft' ? -1 : 1);
      if (nextIdx < 0 || nextIdx >= visibleCells.length) return null;
      return visibleCells[nextIdx];
    }
    if (key === 'ArrowUp' || key === 'ArrowDown') {
      const nextRowIdx = rowIdx + (key === 'ArrowUp' ? -1 : 1);
      if (nextRowIdx < 0 || nextRowIdx >= rows.length) return null;
      const targetRow = rows[nextRowIdx];
      const byField = mtGetCellByField(targetRow, fromTd.dataset.field || '');
      if (mtIsCellVisible(byField)) return byField;
      const targetVisible = Array.from(targetRow.querySelectorAll('td[data-field]')).filter(mtIsCellVisible);
      if (!targetVisible.length) return null;
      return targetVisible[Math.min(cellIdx, targetVisible.length - 1)];
    }
    return null;
  };

  const mtCopyCellValue = (targetTd, sourceTd) => {
    if (!targetTd || !sourceTd) return false;
    const targetInp = targetTd.querySelector('.cell-edit:not(.cell-date-input)');
    const sourceInp = sourceTd.querySelector('.cell-edit:not(.cell-date-input)');
    if (!targetInp || !sourceInp) return false;
    if (targetInp.readOnly || targetInp.disabled || targetTd.dataset.readonly === '1') return false;
    const nextVal = sourceInp.value ?? '';
    if ((targetInp.value ?? '') === nextVal) return false;
    targetInp.value = nextVal;
    targetInp.dispatchEvent(new Event('input', { bubbles: true }));
    targetInp.dispatchEvent(new Event('change', { bubbles: true }));
    return true;
  };

  const mtCopyCellFromPrevRow = (targetTd, tbody) => {
    if (!targetTd) return false;
    const targetRow = targetTd.closest('tr');
    if (!targetRow) return false;
    const fieldName = targetTd.dataset.field || '';
    if (!fieldName) return false;
    const rows = mtGetBodyRows(tbody);
    const rowIndex = rows.indexOf(targetRow);
    if (rowIndex <= 0) return false;
    const sourceTd = mtGetCellByField(rows[rowIndex - 1], fieldName);
    if (!sourceTd) return false;
    return mtCopyCellValue(targetTd, sourceTd);
  };

  const mtCopyRowFromPrevRow = (targetTd, tbody, keyFieldSet) => {
    if (!targetTd) return false;
    const targetRow = targetTd.closest('tr');
    if (!targetRow) return false;
    const rows = mtGetBodyRows(tbody);
    const rowIndex = rows.indexOf(targetRow);
    if (rowIndex <= 0) return false;
    const sourceRow = rows[rowIndex - 1];
    let changed = false;
    targetRow.querySelectorAll('td[data-field]').forEach(td => {
      const f = (td.dataset.field || '').toLowerCase();
      if (!f || keyFieldSet.has(f)) return;
      const sourceTd = mtGetCellByField(sourceRow, td.dataset.field || '');
      if (!sourceTd) return;
      if (mtCopyCellValue(td, sourceTd)) changed = true;
    });
    return changed;
  };

  // 暴露輔助函數（供外部使用）
  window.mtIsCellVisible      = mtIsCellVisible;
  window.mtGetBodyRows        = mtGetBodyRows;
  window.mtGetCellByField     = mtGetCellByField;
  window.mtGetFirstVisibleCell= mtGetFirstVisibleCell;
  window.mtSetActiveCell      = mtSetActiveCell;
  window.mtResolveArrowTarget = mtResolveArrowTarget;
  window.mtCopyCellValue      = mtCopyCellValue;
  window.mtCopyCellFromPrevRow= mtCopyCellFromPrevRow;
  window.mtCopyRowFromPrevRow = mtCopyRowFromPrevRow;

  // ── 主要初始化函數 ───────────────────────────────────────────────
  window.initErpGridNav = function (tbody, options) {
    if (!tbody) return;

    const opts = options || {};
    const keyFieldSet  = new Set((opts.keyFields || ['papernum', 'item']).map(f => String(f).toLowerCase()));
    const isEditMode   = typeof opts.isEditMode === 'function' ? opts.isEditMode : () => true;
    const addRow       = typeof opts.addRow     === 'function' ? opts.addRow     : null;
    const autoSave     = typeof opts.autoSave   === 'function' ? opts.autoSave   : null;
    const onRowSelect  = typeof opts.onRowSelect=== 'function' ? opts.onRowSelect: null;
    const gridLabel    = opts.gridLabel || 'grid';

    // 為了讓 CSS 選擇器生效，給 tbody 的祖先 table 加 class
    const table = tbody.closest('table');
    if (table && !table.classList.contains('erp-grid-nav')) {
      table.classList.add('erp-grid-nav');
    }

    // 兩段式點擊：第一次 → 導航模式，第二次 → 編輯模式（全選）
    tbody.addEventListener('pointerdown', (e) => {
      const td = e.target.closest('td[data-field]');
      if (!td || !tbody.contains(td)) return;
      // 不攔截 select / checkbox / date / button 等控件本身的點擊
      if (e.target.closest('select, input[type="checkbox"], input[type="date"], button')) return;
      const currentActive = tbody.querySelector('td.active-cell');
      if (currentActive === td) {
        // 已是 active-cell → 進入編輯模式，全選文字
        e.preventDefault();
        mtSetActiveCell(td, true);
        return;
      }
      // 第一次點擊 → 導航模式
      e.preventDefault();
      if (onRowSelect) onRowSelect(td.closest('tr'));
      mtSetActiveCell(td, false);
      if (typeof window.__activeDetailGrid !== 'undefined') {
        window.__activeDetailGrid = gridLabel;
      }
    });

    // 鍵盤事件
    tbody.addEventListener('keydown', async (e) => {
      if (e.altKey || e.ctrlKey || e.metaKey) return;
      const target = e.target;
      const fromTd = target?.closest?.('td[data-field]');
      if (!fromTd || !tbody.contains(fromTd)) return;

      const isEditingCell = target instanceof HTMLInputElement
        || target instanceof HTMLSelectElement
        || target instanceof HTMLTextAreaElement;

      // F7：複製上一列整列資料（跳過鍵值欄位）
      if (e.key === 'F7') {
        if (!isEditMode()) return;
        e.preventDefault();
        if (mtCopyRowFromPrevRow(fromTd, tbody, keyFieldSet)) {
          window.__unsavedChanges = true;
          if (autoSave) autoSave();
        }
        return;
      }

      // F8：複製上一列同欄位值
      if (e.key === 'F8') {
        if (!isEditMode()) return;
        e.preventDefault();
        if (mtCopyCellFromPrevRow(fromTd, tbody)) {
          window.__unsavedChanges = true;
          if (autoSave) autoSave();
        }
        return;
      }

      // Enter：導航→編輯；編輯→存檔+回導航
      if (e.key === 'Enter') {
        e.preventDefault();
        if (!isEditingCell) {
          mtSetActiveCell(fromTd, true);
        } else {
          if (autoSave) autoSave();
          mtSetActiveCell(fromTd, false);
        }
        return;
      }

      // Escape：編輯模式 → 回導航模式
      if (e.key === 'Escape' && isEditingCell) {
        e.preventDefault();
        mtSetActiveCell(fromTd, false);
        return;
      }

      // 方向鍵
      if (!['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].includes(e.key)) return;

      // 編輯模式：左右讓瀏覽器自行處理游標，上下退出編輯再導航
      if (isEditingCell) {
        if (e.key === 'ArrowLeft' || e.key === 'ArrowRight') return;
        target.blur();
      }

      let nextTd = mtResolveArrowTarget(fromTd, e.key, tbody);

      // ArrowDown 在最後一列 → 新增列（若提供 addRow 且在修改模式）
      if (!nextTd && e.key === 'ArrowDown' && addRow) {
        const rows = mtGetBodyRows(tbody);
        const fromTr = fromTd.closest('tr');
        const rowIdx = fromTr ? rows.indexOf(fromTr) : -1;
        if (rowIdx >= 0 && rowIdx === rows.length - 1) {
          if (!isEditMode()) return;
          e.preventDefault();
          const r = await addRow();
          if (r?.ok) {
            const newRows = mtGetBodyRows(tbody);
            const newRow = newRows[newRows.length - 1];
            if (newRow) {
              if (onRowSelect) onRowSelect(newRow);
              const firstCell = mtGetFirstVisibleCell(newRow);
              if (firstCell) mtSetActiveCell(firstCell, false);
            }
          }
        }
        return;
      }

      if (!nextTd) return;
      e.preventDefault();
      // 上下換列時呼叫 autoSave
      if (autoSave && (e.key === 'ArrowUp' || e.key === 'ArrowDown')) {
        const fromTr = fromTd.closest('tr');
        const nextTr = nextTd.closest('tr');
        if (fromTr !== nextTr) autoSave();
      }
      if (onRowSelect) onRowSelect(nextTd.closest('tr'));
      mtSetActiveCell(nextTd, false);
      if (typeof window.__activeDetailGrid !== 'undefined') {
        window.__activeDetailGrid = gridLabel;
      }
    });
  };

})();
