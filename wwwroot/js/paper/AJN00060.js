// AJN00060 傳票批次處理
// 功能：隱藏新增/列印按鈕 + 批次審核/退審/作廢

(function () {
  'use strict';

  const getUserId = () =>
    (window._userId || localStorage.getItem('erpLoginUserId') || 'admin').toString().trim();

  const tableName = window._tableName || 'AJNdJourMain';

  // ==========================================
  // 隱藏不需要的按鈕 (PowerType<>0)
  // 列表頁: btnAddNew / btnPrint
  // 單身頁: btnSubAddNew / btnPrint
  // ==========================================
  function hideButtons() {
    document.getElementById('btnAddNew')?.classList.add('d-none');
    document.getElementById('btnSubAddNew')?.classList.add('d-none');
    document.getElementById('btnPrint')?.classList.add('d-none');
  }

  // ==========================================
  // 插入批次操作按鈕到 toolbar
  // ==========================================
  function addBatchButtons() {
    const toolbar = document.querySelector('.title-row > div');
    if (!toolbar) return;

    const btnHtml = `
      <button type="button" id="btnBatchApprove" class="btn toolbar-btn success-outline" title="將所有未審核/退審的傳票批次審核">
        <i class="bi bi-check2-all"></i>批次審核
      </button>
      <button type="button" id="btnBatchReject" class="btn toolbar-btn" style="color:#cc8a00; border-color:#f0d98c;" title="將所有已審核的傳票批次退審">
        <i class="bi bi-arrow-counterclockwise"></i>批次退審
      </button>
      <button type="button" id="btnBatchVoid" class="btn toolbar-btn danger" title="將所有未審核/退審的傳票批次作廢">
        <i class="bi bi-x-octagon"></i>批次作廢
      </button>
    `;
    toolbar.insertAdjacentHTML('beforeend', btnHtml);
  }

  // ==========================================
  // 取得目前列表中所有 PaperNum
  // ==========================================
  function getAllPaperNums() {
    return Array.from(document.querySelectorAll('#dataTbody tr[data-paper-num]'))
      .map(tr => tr.getAttribute('data-paper-num'))
      .filter(Boolean);
  }

  // ==========================================
  // 查詢各傳票的 Finished 狀態
  // ==========================================
  async function queryFinishedStatus(paperNums) {
    if (paperNums.length === 0) return [];

    const inClause = paperNums.map(p => `'${p.replace(/'/g, "''")}'`).join(',');
    const resp = await fetch('/api/StoredProc/queryDirect', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        TableName: 'AJNdJourMain',
        Columns: ['PaperNum', 'Finished'],
        WhereClause: `PaperNum IN (${inClause})`
      })
    });
    if (!resp.ok) throw new Error('查詢傳票狀態失敗');
    const result = await resp.json();
    return result.data || result;
  }

  // ==========================================
  // 執行單筆 PaperAction
  // ==========================================
  async function doPaperAction(paperNum, eoc, aftFinished) {
    const resp = await fetch('/api/PaperAction/DoAction', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        PaperId: tableName,
        PaperNum: paperNum,
        UserId: getUserId(),
        EOC: eoc,
        AftFinished: aftFinished
      })
    });
    if (!resp.ok) {
      const err = await resp.json().catch(() => ({}));
      throw new Error(err.message || err.error || `HTTP ${resp.status}`);
    }
  }

  // ==========================================
  // 批次操作主函式
  // finishedFilter: 符合條件的 Finished 值陣列
  // eoc / aftFinished: 傳給 CURdPaperAction 的參數
  // ==========================================
  async function batchAction(eoc, aftFinished, finishedFilter, actionName) {
    const paperNums = getAllPaperNums();
    if (paperNums.length === 0) {
      Swal.fire({ icon: 'warning', title: '目前列表無資料' });
      return;
    }

    // 先查詢各傳票的 Finished 狀態
    let rows;
    try {
      rows = await queryFinishedStatus(paperNums);
    } catch (e) {
      Swal.fire({ icon: 'error', title: '查詢失敗', text: e.message });
      return;
    }

    const eligible = rows.filter(r => finishedFilter.includes(r.Finished));
    if (eligible.length === 0) {
      Swal.fire({ icon: 'info', title: `無符合條件的傳票可${actionName}` });
      return;
    }

    // 確認對話框
    const confirm = await Swal.fire({
      icon: 'question',
      title: `確定要${actionName}？`,
      text: `共 ${eligible.length} 筆傳票符合條件`,
      showCancelButton: true,
      confirmButtonText: '確定',
      cancelButtonText: '取消'
    });
    if (!confirm.isConfirmed) return;

    // 逐筆執行
    let success = 0, failed = 0;
    const errors = [];

    for (const row of eligible) {
      try {
        await doPaperAction(row.PaperNum, eoc, aftFinished);
        success++;
      } catch (e) {
        failed++;
        errors.push(`${row.PaperNum}: ${e.message}`);
      }
    }

    // 結果通知
    const resultText = failed > 0
      ? `成功 ${success} 筆，失敗 ${failed} 筆\n${errors.slice(0, 5).join('\n')}${errors.length > 5 ? '\n...' : ''}`
      : `成功 ${success} 筆`;

    await Swal.fire({
      icon: failed > 0 ? 'warning' : 'success',
      title: `${actionName}完成`,
      text: resultText
    });

    // 動態刷新列表（不重新整理頁面）
    reloadGrid();
  }

  // ==========================================
  // 動態刷新 Grid（不重新整理頁面）
  // ==========================================
  async function reloadGrid() {
    try {
      // 取得目前查詢條件（從 localStorage 還原）
      const tn = String(window._tableName || '').trim().toLowerCase();
      const fk = tn ? `orderListQueryFilters:${tn}` : 'orderListQueryFilters';
      const pk = tn ? `orderListPageNumber:${tn}` : 'orderListPageNumber';
      const savedFilters = JSON.parse(localStorage.getItem(fk) || '[]');
      const savedPage = parseInt(localStorage.getItem(pk) || '1', 10);

      const itemId = (window._itemId || '').toString().trim();

      // 確保有分頁參數
      let filters = savedFilters.filter(f => f.Field !== 'page' && f.Field !== 'pageSize');
      filters.push({ Field: 'page', Op: '', Value: String(savedPage) });
      filters.push({ Field: 'pageSize', Op: '', Value: '50' });

      const resp = await fetch('/api/DynamicTable/PagedQuery', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          table: window._tableName,
          itemId: itemId,
          filters: filters,
          skipLookup: false
        })
      });
      if (!resp.ok) throw new Error('HTTP ' + resp.status);
      const result = await resp.json();

      // 更新 lookup 資料
      if (result.lookupMapData && Object.keys(result.lookupMapData).length > 0) {
        window._lookupMapData = result.lookupMapData;
      }

      // 使用框架的 renderTable 刷新表格
      if (typeof renderTable === 'function') {
        renderTable(result.data);
      }
      if (typeof renderPagination === 'function') {
        renderPagination(result.totalCount, 50, savedPage);
      }
      if (typeof renderOrderCount === 'function') {
        renderOrderCount(result.totalCount, 50, savedPage);
      }
    } catch (e) {
      console.error('reloadGrid 失敗:', e);
    }
  }

  // ==========================================
  // 綁定按鈕事件
  // ==========================================
  function bindButtons() {
    // 批次審核：Finished=0(未審) 或 3(退審) -> EOC=1, AftFinished=1
    document.getElementById('btnBatchApprove')?.addEventListener('click', () =>
      batchAction(1, 1, [0, 3], '批次審核'));

    // 批次退審：Finished=1(已審) -> EOC=3, AftFinished=3
    document.getElementById('btnBatchReject')?.addEventListener('click', () =>
      batchAction(3, 3, [1], '批次退審'));

    // 批次作廢：Finished=0(未審) 或 3(退審) -> EOC=2, AftFinished=2
    document.getElementById('btnBatchVoid')?.addEventListener('click', () =>
      batchAction(2, 2, [0, 3], '批次作廢'));
  }

  // ==========================================
  // 初始化
  // ==========================================
  function init() {
    hideButtons();
    addBatchButtons();
    bindButtons();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();