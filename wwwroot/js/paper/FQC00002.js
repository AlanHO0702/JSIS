// FQC00002 檢驗規範建立 (ME106)
// 使用標準 Tabs 佈局，新增 Ref1-Ref8 面板（對應 Delphi DBEdit1~8 bound to dsMaster1）
// Ref 欄位整合進 Master grid 的 dirty tracking / save 機制
// 對應 Delphi: FQCdQCType\QCType.pas

(function () {
  'use strict';

  const cfgs = window._mmdConfigs || {};
  const domId = Object.keys(cfgs).find(k => (cfgs[k].ItemId || '').toUpperCase() === 'FQC00002');
  if (!domId) return;

  const cfg = cfgs[domId];

  // 啟用串聯式聯動
  cfg.EnableDetailFocusCascade = true;
  cfg.DetailCascadeMode = 1;

  const root = document.getElementById(domId);
  if (!root) return;

  const refFields = ['Ref1','Ref2','Ref3','Ref4','Ref5','Ref6','Ref7','Ref8'];

  // ==========================================
  // 1. 建立 Ref1-Ref8 面板，插入 Master card 與 Tabs 之間
  // ==========================================
  const refPanel = document.createElement('div');
  refPanel.id = `${domId}-refPanel`;
  refPanel.className = 'card shadow-sm border-0 rounded-4 mb-1';
  refPanel.innerHTML = `
    <div class="card-body py-1 px-2">
      <div class="d-flex flex-wrap gap-2 align-items-center">
        ${refFields.map(f => `
          <input type="text" id="${domId}-ref-${f}" data-field="${f}"
                 class="form-control form-control-sm" style="width:120px;" />
        `).join('')}
      </div>
    </div>
  `;

  const masterCard = root.querySelector('.mmd-master-card');
  const navTabs = root.querySelector('.nav-tabs');
  if (masterCard && navTabs) {
    masterCard.after(refPanel);
  }

  // ==========================================
  // 2. Master Row 選取時：更新 Ref 面板 + 注入隱藏 cell-edit
  // ==========================================
  let activeRow = null; // 目前選中的 master <tr>

  function updateRefPanel(rowData) {
    for (const field of refFields) {
      const input = document.getElementById(`${domId}-ref-${field}`);
      if (input) {
        const val = (rowData && rowData[field] != null) ? String(rowData[field]) : '';
        input.value = val;
        input.defaultValue = val; // 記錄原始值，用於比對變更
      }
    }
  }

  // 確保 active master row 有 Ref1-Ref8 的隱藏 cell-edit input
  function ensureHiddenInputs(tr, rowData) {
    if (!tr) return;
    for (const field of refFields) {
      let hidden = tr.querySelector(`input.cell-edit[name="${field}"]`);
      if (!hidden) {
        hidden = document.createElement('input');
        hidden.type = 'hidden';
        hidden.className = 'cell-edit';
        hidden.name = field;
        tr.appendChild(hidden);
      }
      const val = (rowData && rowData[field] != null) ? String(rowData[field]) : '';
      hidden.value = val;
      hidden.defaultValue = val;
    }
  }

  // 監聽 master tbody 點擊，追蹤 active row
  const masterBody = document.getElementById(`${domId}-m-body`);
  if (masterBody) {
    masterBody.addEventListener('click', (e) => {
      const tr = e.target.closest('tr');
      if (tr) activeRow = tr;
    });
  }

  // Hook: masterMultiDetailTemplate.js 的 onMasterFocus
  window._mmdHooks = window._mmdHooks || {};
  window._mmdHooks[domId] = window._mmdHooks[domId] || {};
  window._mmdHooks[domId].onMasterFocus = function (rowData, rowIndex, tr) {
    activeRow = tr || (masterBody ? masterBody.querySelector('tr.table-active, tr.row-active') : null);
    updateRefPanel(rowData);
    ensureHiddenInputs(activeRow, rowData);
  };

  // ==========================================
  // 3. Ref 面板 input → 同步到 master row 隱藏欄位 → 觸發 dirty
  // ==========================================
  for (const field of refFields) {
    const panelInput = document.getElementById(`${domId}-ref-${field}`);
    if (!panelInput) continue;

    panelInput.addEventListener('input', () => {
      if (!activeRow) return;
      // 同步值到 master row 的隱藏 cell-edit
      const hidden = activeRow.querySelector(`input.cell-edit[name="${field}"]`);
      if (hidden) {
        hidden.value = panelInput.value;
        // 觸發 input 事件讓 masterMultiDetailTemplate.js 的 dirty tracking 偵測到
        hidden.dispatchEvent(new Event('input', { bubbles: true }));
      }
    });
  }

  console.log('[FQC00002] 檢驗規範建立 Hooks 已載入');
})();
