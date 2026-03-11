// AA000033 總領用傳票（物資轉傳票）
// 功能：期別/公司別篩選面板 + 結轉、退審、取消按鈕

(function () {
  'use strict';

  // ==========================================
  // 工具函式
  // ==========================================
  const getUserId = () =>
    (window._userId || localStorage.getItem('erpLoginUserId') || 'admin').toString().trim();

  const getUseId = () =>
    (window._useId || localStorage.getItem('erpUseId') || 'A001').toString().trim();

  // ==========================================
  // 建立篩選面板 (期別 + 公司別)
  // ==========================================
  function buildFilterPanel() {
    const cardBody = document.querySelector('.card-body');
    if (!cardBody) return;

    const panel = document.createElement('div');
    panel.id = 'mtlToAAMPanel';
    panel.className = 'd-flex align-items-end flex-wrap border rounded bg-light';
    panel.style.cssText = 'padding:2px 6px; gap:3px;';
    panel.innerHTML = `
      <div class="d-flex flex-column">
        <label class="fw-bold" style="font-size:14px; line-height:1.5; margin:0;">期別</label>
        <input type="text" id="mtlHisId" class="form-control" style="width:90px; height:24px; font-size:14px; padding:0 2px;" placeholder="YYYY.MM" />
      </div>
      <div class="d-flex flex-column">
        <label class="fw-bold" style="font-size:14px; line-height:1.5; margin:0;">公司別</label>
        <select id="mtlUseId" class="form-select" style="width:140px; height:24px; font-size:14px; padding:0 2px;">
          <option value="">-- 請選擇 --</option>
        </select>
      </div>
      <input type="hidden" id="mtlType" value="0" />
    `;

    // 插入到 toolbar 之後（查詢按鈕那列的下方）
    const toolbar = cardBody.querySelector('.top-toolbar');
    if (toolbar) {
      toolbar.insertAdjacentElement('afterend', panel);
    } else {
      cardBody.prepend(panel);
    }
  }

  // ==========================================
  // 載入公司別下拉選單資料 (AJNdAccUse)
  // ==========================================
  async function loadAccUseOptions() {
    try {
      const resp = await fetch('/api/TableFieldLayout/LookupData?table=AJNdAccUse&key=UseId&result=Usename');
      if (!resp.ok) return;
      const list = await resp.json();
      const sel = document.getElementById('mtlUseId');
      if (!sel || !Array.isArray(list)) return;

      list.forEach(item => {
        const opt = document.createElement('option');
        opt.value = item.key ?? '';
        opt.textContent = `${item.key ?? ''} ${item.result0 ?? ''}`.trim();
        sel.appendChild(opt);
      });

      // 預設選擇登入者的 UseId
      const defaultUseId = getUseId();
      if (defaultUseId) sel.value = defaultUseId;
    } catch (e) {
      console.error('載入公司別失敗:', e);
    }
  }

  // ==========================================
  // 載入預設期別 (下一期 = Max(HisId) + 1 個月)
  // ==========================================
  async function loadDefaultHisId() {
    try {
      const resp = await fetch('/api/StoredProc/queryDirect', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          TableName: 'AJNdMTL2AAM',
          Columns: ['MAX(HisId) as MaxHisId'],
          WhereClause: '1=1'
        })
      });
      if (!resp.ok) return;
      const data = await resp.json();
      const rows = data.data || data;
      const maxHisId = rows?.[0]?.MaxHisId ?? '';

      const input = document.getElementById('mtlHisId');
      if (!input) return;

      if (maxHisId) {
        // HisId 格式: "YYYY.MM"，算下一個月
        input.value = calcNextMonth(maxHisId);
      } else {
        // 無資料時用當前年月
        const now = new Date();
        const y = now.getFullYear();
        const m = String(now.getMonth() + 1).padStart(2, '0');
        input.value = `${y}.${m}`;
      }
    } catch (e) {
      console.error('載入預設期別失敗:', e);
    }
  }

  function calcNextMonth(hisId) {
    // hisId 格式: "YYYY.MM"
    const parts = hisId.trim().split('.');
    if (parts.length < 2) return hisId;
    let y = parseInt(parts[0], 10);
    let m = parseInt(parts[1], 10);
    m += 1;
    if (m > 12) { m = 1; y += 1; }
    return `${y}.${String(m).padStart(2, '0')}`;
  }

  // ==========================================
  // 取得面板當前值
  // ==========================================
  function getPanelValues() {
    return {
      hisId: (document.getElementById('mtlHisId')?.value || '').trim(),
      useId: (document.getElementById('mtlUseId')?.value || '').trim(),
      type: parseInt(document.getElementById('mtlType')?.value || '0', 10)
    };
  }

  function validatePanel() {
    const v = getPanelValues();
    if (!v.hisId) {
      Swal.fire({ icon: 'error', title: '請輸入轉帳期別！' });
      return null;
    }
    if (!v.useId) {
      Swal.fire({ icon: 'error', title: '請輸入轉帳公司別！' });
      return null;
    }
    return v;
  }

  // ==========================================
  // 呼叫 SP (透過 StoredProc/exec)
  // ==========================================
  async function callSP(key, args) {
    const resp = await fetch('/api/StoredProc/exec', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Key: key, Args: args })
    });
    const result = await resp.json();
    if (!resp.ok || !result.ok) {
      throw new Error(result.error || '執行失敗');
    }
    return result;
  }

  // ==========================================
  // 重新載入 Grid 資料（從 View AJNdV_MTL2AAM 查詢）
  // ==========================================
  async function reloadGrid() {
    try {
      const v = getPanelValues();
      const conditions = [];
      if (v.hisId) conditions.push(`HisId='${v.hisId}'`);
      if (v.useId) conditions.push(`UseId='${v.useId}'`);

      const resp = await fetch('/api/StoredProc/queryDirect', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          TableName: 'AJNdV_MTL2AAM',
          WhereClause: conditions.length > 0 ? conditions.join(' AND ') : '1=1'
        })
      });
      if (!resp.ok) throw new Error('HTTP ' + resp.status);
      const result = await resp.json();
      const items = result.data || result;

      // 使用框架的 updateTableBody + applyLookupToTable（含 Lookup、格式化、cell-edit 等完整結構）
      if (typeof window._sgUpdateTableBody === 'function' && window._sgTableFields) {
        window._sgUpdateTableBody(Array.isArray(items) ? items : [], window._sgTableFields, '（無資料）');
        if (typeof window._sgApplyLookup === 'function') {
          window._sgApplyLookup();
        }
      }
    } catch (e) {
      console.error('reloadGrid 失敗:', e);
    }
  }

  // ==========================================
  // 按鈕 Handler: 結轉 (AJNdMTL2AAMConfirm)
  // ==========================================
  async function handleConfirm() {
    const v = validatePanel();
    if (!v) return;

    const confirm = await Swal.fire({
      icon: 'question',
      title: `確定要轉物資至傳票？ (${v.hisId})`,
      showCancelButton: true,
      confirmButtonText: '確定',
      cancelButtonText: '取消'
    });
    if (!confirm.isConfirmed) return;

    try {
      await callSP('AJNdMTL2AAMConfirm', {
        HisId: v.hisId,
        UseId: v.useId,
        UserId: getUserId(),
        Type: v.type
      });
      await Swal.fire({ icon: 'success', title: '物資轉傳票作業完成！' });
      reloadGrid();
    } catch (e) {
      Swal.fire({ icon: 'error', title: '執行失敗', text: e.message });
    }
  }

  // ==========================================
  // 按鈕 Handler: 退審 (AJNdMTL2AAMScrap, Scrap=0)
  // ==========================================
  async function handleCancel() {
    const v = validatePanel();
    if (!v) return;

    const confirm = await Swal.fire({
      icon: 'question',
      title: `確定要取消轉物資至傳票？ (${v.hisId})`,
      showCancelButton: true,
      confirmButtonText: '確定',
      cancelButtonText: '取消'
    });
    if (!confirm.isConfirmed) return;

    try {
      await callSP('AJNdMTL2AAMScrap', {
        HisId: v.hisId,
        UseId: v.useId,
        UserId: getUserId(),
        Type: v.type,
        Scrap: 0
      });
      await Swal.fire({ icon: 'success', title: '取消物資轉傳票作業完成！' });
      reloadGrid();
    } catch (e) {
      Swal.fire({ icon: 'error', title: '執行失敗', text: e.message });
    }
  }

  // ==========================================
  // 按鈕 Handler: 取消 (AJNdMTL2AAMScrap, Scrap=1)
  // ==========================================
  async function handleDelete() {
    const v = validatePanel();
    if (!v) return;

    const confirm = await Swal.fire({
      icon: 'question',
      title: `確定要取消轉物資至傳票？ (${v.hisId})`,
      showCancelButton: true,
      confirmButtonText: '確定',
      cancelButtonText: '取消'
    });
    if (!confirm.isConfirmed) return;

    try {
      await callSP('AJNdMTL2AAMScrap', {
        HisId: v.hisId,
        UseId: v.useId,
        UserId: getUserId(),
        Type: v.type,
        Scrap: 1
      });
      await Swal.fire({ icon: 'success', title: '取消物資轉傳票作業完成！' });
      reloadGrid();
    } catch (e) {
      Swal.fire({ icon: 'error', title: '執行失敗', text: e.message });
    }
  }

  // ==========================================
  // 綁定按鈕事件
  // ==========================================
  function bindButtons() {
    document.getElementById('btnMtlConfirm')?.addEventListener('click', handleConfirm);
    document.getElementById('btnMtlCancel')?.addEventListener('click', handleCancel);
    document.getElementById('btnMtlDelete')?.addEventListener('click', handleDelete);
  }

  // ==========================================
  // 初始化：建立面板 & 載入預設值 & 綁定按鈕
  // ==========================================
  function init() {
    // 隱藏此作業不需要的 toolbar 按鈕
    document.getElementById('btnEditToggle')?.classList.add('d-none');
    document.getElementById('btnExportExcel')?.classList.add('d-none');

    // 將結轉/退審/取消按鈕插入 toolbar
    const toolbarMain = document.querySelector('.toolbar-main .d-flex');
    if (toolbarMain) {
      const btnHtml = `
        <button type="button" id="btnMtlConfirm" class="btn toolbar-btn" style="color:#0d6efd; border-color:#abc4f8;"><i class="bi bi-check2-circle"></i>結轉</button>
        <button type="button" id="btnMtlCancel" class="btn toolbar-btn" style="color:#cc8a00; border-color:#f0d98c;"><i class="bi bi-arrow-counterclockwise"></i>退審</button>
        <button type="button" id="btnMtlDelete" class="btn toolbar-btn" style="color:#cc3a3a; border-color:#efb3b3;"><i class="bi bi-x-circle"></i>取消</button>
      `;
      toolbarMain.insertAdjacentHTML('beforeend', btnHtml);
    }

    buildFilterPanel();
    bindButtons();
    loadAccUseOptions();
    loadDefaultHisId();
  }

  // DOM ready 時初始化
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();