// FQC00011 MRB報廢判定單 (ME405)
// 對應 Delphi: FQCdMRBScrapMain (TfrmPaper3LDLL)
//
// 特殊規則：
//   1. 新增 SubDetail 行 → 不走直接 INSERT，改呼叫 SP FQCdMRBScrapNextDLL
//      SP 建立記錄後，再用 Detail1 的 PartNum 補更新（SP 不含 PartNum 參數）
//   2. ClassId 變更 → 查 FQCdDefectClass.Notes → 自動填入 DutyProc
//   3. DefectId 變更 → 查 FQCdDefectInfo.ResProc → 自動填入 DutyProc

(function () {
  'use strict';

  const ITEM_ID = 'FQC00011';
  if (((window._itemId || '').toString().trim().toUpperCase()) !== ITEM_ID) return;

  // ── 工具 ─────────────────────────────────────────────────────────────────

  function getValueCI(obj, key) {
    if (!obj || !key) return undefined;
    if (obj[key] !== undefined) return obj[key];
    const k = Object.keys(obj).find(x => x.toLowerCase() === key.toLowerCase());
    return k !== undefined ? obj[k] : undefined;
  }

  async function fetchFirst(url) {
    try {
      const res = await fetch(url);
      if (!res.ok) return null;
      const data = await res.json();
      if (Array.isArray(data)) return data[0] ?? null;
      if (Array.isArray(data?.value)) return data.value[0] ?? null;
      return data ?? null;
    } catch { return null; }
  }

  // ── PaperNum 從 URL 取得 ──────────────────────────────────────────────────
  function getPaperNumFromUrl() {
    // URL: /DynamicTemplate/Paper3L/FQC00011/{PaperNum}
    const m = location.pathname.match(/\/Paper3L\/[^/]+\/([^/?#]+)/i);
    return m ? decodeURIComponent(m[1]) : '';
  }

  // ── 取得目前選取的 Detail1 row ──────────────────────────────────────────
  function getDetail1Row() {
    const multiTabRows = window._multiTabSelectedRows || {};
    return Object.values(multiTabRows).find(r => r != null) ?? null;
  }

  // ── 取得 SubDetail1 目前最大 Item（從 DOM 讀，避免額外 API 請求） ─────────
  function getCurrentMaxItem() {
    const container = document.getElementById('paper3l-subdetail-container');
    if (!container) return 0;
    let max = 0;
    container.querySelectorAll('tbody tr').forEach(tr => {
      const itemCell = tr.querySelector('td[data-field="Item"] .cell-view');
      const val = Number((itemCell?.textContent ?? '').trim());
      if (Number.isFinite(val) && val > max) max = val;
    });
    return max;
  }

  // ── SP: FQCdMRBScrapNextDLL → 建立新 SubDetail 記錄 ──────────────────────
  // 對應 Delphi qrySubDetail1NewRecord：
  //   maxItem 查詢條件為 PaperNum + LotNum + ProcCode（2022.04.06 修正）
  async function callMRBScrapNextDLL(paperNum, lotNum) {
    const maxItem = getCurrentMaxItem();
    const resp = await fetch('/api/StoredProc/exec', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        key: 'FQCdMRBScrapNextDLL',
        args: { PaperNum: paperNum, Item: maxItem, LotNum: lotNum }
      })
    });
    if (!resp.ok) {
      const err = await resp.text().catch(() => `HTTP ${resp.status}`);
      throw new Error(err);
    }
  }

  // ── 取得 SP 新建的列 Item 編號（查 DB 最大值） ───────────────────────────
  async function fetchNewMaxItem(paperNum, lotNum, procCode) {
    let url = `/api/CommonTable/ByKeys?table=FQCdMRBScrapSub`
      + `&keyNames=PaperNum&keyValues=${encodeURIComponent(paperNum)}`
      + `&keyNames=LotNum&keyValues=${encodeURIComponent(lotNum)}`
      + `&orderBy=Item&orderDir=DESC`;
    if (procCode) {
      url += `&keyNames=ProcCode&keyValues=${encodeURIComponent(procCode)}`;
    }
    const row = await fetchFirst(url);
    return row ? Number(getValueCI(row, 'Item') ?? 0) : 0;
  }

  // ── 用 Detail1 的 PartNum 補更新新建的 SubDetail 列 ─────────────────────
  async function fillPartNumFromDetail1(paperNum, lotNum, newItem, partNum) {
    if (!partNum) return;
    await fetch('/api/CommonTable/SaveTableChanges', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        TableName: 'FQCdMRBScrapSub',
        KeyFields: ['PaperNum', 'Item', 'LotNum'],
        Data: [{
          PaperNum: paperNum,
          Item: newItem,
          LotNum: lotNum,
          PartNum: partNum
        }]
      })
    });
  }

  // ── 攔截 __paper3lSubDetailCollectChanges：注入必填欄位 + 修正 keyFields ──
  // 問題：_Paper3LSubDetail1.cshtml 的 renderTable() 每次資料載入都會重新定義
  //       __paper3lSubDetailCollectChanges，一次性覆寫會被蓋掉。
  // 解法：用 Object.defineProperty setter，讓每次重定義都自動套上修正邏輯。

  function installCollectChangesOverride() {
    let _impl = window.__paper3lSubDetailCollectChanges ?? null;

    function wrapImpl(fn) {
      if (typeof fn !== 'function') return fn;
      return function () {
        const result = fn.call(this);
        if (!result || !Array.isArray(result.changes)) return result;

        // 從 Detail1 補入 FQCdMRBScrapSub 的 NOT NULL 欄位
        const detail1Row = getDetail1Row();
        const partNum  = String(getValueCI(detail1Row, 'PartNum')  ?? '').trim();
        const revision = String(getValueCI(detail1Row, 'Revision') ?? '').trim();
        const layerId  = String(getValueCI(detail1Row, 'LayerId')  ?? '').trim();
        const pop      = String(getValueCI(detail1Row, 'POP')      ?? '0').trim() || '0';

        function isMissing(v) { return v == null || v === ''; }

        result.changes.forEach(row => {
          if (partNum  && isMissing(row.PartNum))  row.PartNum  = partNum;
          if (revision && isMissing(row.Revision)) row.Revision = revision;
          if (layerId  && isMissing(row.LayerId))  row.LayerId  = layerId;
          if (isMissing(row.POP)) row.POP = pop;

          // 清除 key 欄位的千位分隔符（顯示值如 "5,270" → "5270"）
          ['PaperNum', 'Item', 'LotNum'].forEach(k => {
            if (row[k] != null) row[k] = String(row[k]).replace(/,/g, '').trim();
          });
        });

        // FQCdMRBScrapSub 真實 PK = PaperNum + Item + LotNum
        // 對應 Delphi: tblUpdDtl.IndexFieldNames := 'PaperNum;Item;LotNum'
        result.keyFields = ['PaperNum', 'Item', 'LotNum'];

        return result;
      };
    }

    Object.defineProperty(window, '__paper3lSubDetailCollectChanges', {
      configurable: true,
      get() { return _impl; },
      set(fn) { _impl = wrapImpl(fn); }
    });

    // 補包目前已存在的實作
    if (typeof _impl === 'function') {
      _impl = wrapImpl(_impl);
    }
  }

  // ── 覆寫 __paper3lSubDetailAddRow ────────────────────────────────────────
  // 流程：
  //   1. 呼叫 SP FQCdMRBScrapNextDLL（建立含基本欄位的新列）
  //   2. 查出新列的 Item 編號
  //   3. 用 Detail1 選取列的 PartNum 補更新（SP 不帶 PartNum 參數）
  //   4. Refresh 第三階格線

  function installAddRowOverride() {
    window.__paper3lSubDetailAddRow = async function () {
      const paperNum = getPaperNumFromUrl();
      if (!paperNum) { alert('無法取得單號，請確認 URL 格式。'); return { handled: true, ok: false }; }

      const detailRow = getDetail1Row();
      const lotNum   = String(getValueCI(detailRow, 'LotNum')   ?? '').trim();
      const procCode = String(getValueCI(detailRow, 'ProcCode') ?? '').trim();
      const partNum  = String(getValueCI(detailRow, 'PartNum')  ?? '').trim();

      if (!lotNum) {
        alert('請先在上方明細選取一筆批號資料，再新增報廢明細。');
        return { handled: true, ok: false };
      }

      try {
        // 1. SP 建立新列
        await callMRBScrapNextDLL(paperNum, lotNum);

        // 2. 查出 SP 建立的新列 Item 編號
        const newItem = await fetchNewMaxItem(paperNum, lotNum, procCode);

        // 3. 補填 PartNum（來自 Detail1 選取列）
        if (newItem > 0 && partNum) {
          await fillPartNumFromDetail1(paperNum, lotNum, newItem, partNum);
        }
      } catch (err) {
        alert(`新增報廢明細失敗：${err.message ?? err}`);
        return { handled: true, ok: false };
      }

      // 4. 重新整理第三階格線
      if (typeof window.__paper3lSubDetailRefresh === 'function') {
        try { await window.__paper3lSubDetailRefresh(); } catch {}
      }
      return { handled: true, ok: true };
    };
  }

  // ── ClassId / DefectId → DutyProc 自動帶入 ───────────────────────────────
  // 對應 Delphi: gridSubDetail1ColExit + tblUpdDtlAfterOpen prcFdValidate

  function bindAutoFill(container) {
    if (!container) return;

    container.addEventListener('change', async (e) => {
      const input = e.target.closest('input.cell-edit, select.cell-edit');
      if (!input) return;
      const tr = input.closest('tr');
      if (!tr) return;

      const field = (input.name || input.dataset?.field || '').trim();
      if (field !== 'ClassId' && field !== 'DefectId') return;

      function getVal(name) {
        const el = tr.querySelector(
          `input.cell-edit[data-field="${name}"], select.cell-edit[data-field="${name}"]`
        );
        return el ? el.value.trim() : '';
      }

      function setVal(name, val) {
        const el = tr.querySelector(
          `input.cell-edit[data-field="${name}"], select.cell-edit[data-field="${name}"]`
        );
        const view = tr.querySelector(`td[data-field="${name}"] .cell-view`);
        if (el) { el.value = val; el.dispatchEvent(new Event('input', { bubbles: true })); }
        if (view) view.textContent = val;
      }

      if (field === 'ClassId') {
        const classId = getVal('ClassId');
        if (!classId) return;
        const row = await fetchFirst(
          `/api/CommonTable/ByKeys?table=FQCdDefectClass`
          + `&keyNames=ClassId&keyValues=${encodeURIComponent(classId)}`
        );
        const notes = getValueCI(row, 'Notes');
        if (notes != null) setVal('DutyProc', String(notes));
      }

      if (field === 'DefectId') {
        const classId  = getVal('ClassId');
        const defectId = getVal('DefectId');
        if (!classId || !defectId) return;
        const row = await fetchFirst(
          `/api/CommonTable/ByKeys?table=FQCdDefectInfo`
          + `&keyNames=ClassId&keyNames=DefectId`
          + `&keyValues=${encodeURIComponent(classId)}&keyValues=${encodeURIComponent(defectId)}`
        );
        const resProc = getValueCI(row, 'ResProc');
        if (resProc != null) setVal('DutyProc', String(resProc));
      }
    });
  }

  // ── 初始化 ────────────────────────────────────────────────────────────────

  function tryInit() {
    // installCollectChangesOverride 用 defineProperty，越早執行越好，
    // 不需等 __paper3lSubDetailAddRow 就先安裝 setter
    installCollectChangesOverride();

    // installAddRowOverride 需等 __paper3lSubDetailAddRow 掛上後才能覆寫
    if (typeof window.__paper3lSubDetailAddRow !== 'function') {
      setTimeout(function waitAddRow() {
        if (typeof window.__paper3lSubDetailAddRow !== 'function') {
          setTimeout(waitAddRow, 300);
          return;
        }
        installAddRowOverride();
      }, 300);
    } else {
      installAddRowOverride();
    }

    // 綁定 ClassId/DefectId 自動帶入到 SubDetail1 容器
    const container = document.getElementById('paper3l-subdetail-container');
    if (container) {
      bindAutoFill(container);
    } else {
      // 容器尚未渲染，用 MutationObserver 等待
      const obs = new MutationObserver(() => {
        const c = document.getElementById('paper3l-subdetail-container');
        if (c) { obs.disconnect(); bindAutoFill(c); }
      });
      obs.observe(document.body, { childList: true, subtree: true });
    }

    console.log('[FQC00011] ME405 MRB報廢判定單 hooks 已載入');
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', tryInit);
  } else {
    tryInit();
  }
})();
