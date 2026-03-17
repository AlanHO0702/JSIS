(function () {
  'use strict';

  var ITEM_ID = 'AT000002';
  var LOG_TYPE = 'ClassInvType';
  var SP_SUFFIX = 'AT000002';

  /* ── 動態載入 ccsUpdateLog.js（若尚未載入） ── */
  function ensureUpdateLogScript() {
    return new Promise(function (resolve) {
      if (typeof window.openSharedUpdateLog === 'function') { resolve(); return; }
      var s = document.createElement('script');
      s.src = '/js/ccsUpdateLog.js?v=' + Date.now();
      s.onload = resolve;
      s.onerror = function () { console.error('載入 ccsUpdateLog.js 失敗'); resolve(); };
      document.head.appendChild(s);
    });
  }

  /* ── 取得當前 active tab 的 grid table 元素 ── */
  function getActiveGrid() {
    return document.querySelector('.tab-pane.show.active table[id^="grid-"]');
  }

  /* ── 從 active grid 取得 tableName（data-dict-table）── */
  function getActiveTableName() {
    var grid = getActiveGrid();
    return grid ? (grid.dataset.dictTable || '').trim() : '';
  }

  /* ── 從 active grid 取得 key fields（data-key-fields）── */
  function getKeyFields() {
    var grid = getActiveGrid();
    if (!grid) return [];
    var raw = (grid.dataset.keyFields || '').trim();
    return raw ? raw.split(',').map(function (s) { return s.trim(); }) : [];
  }

  /* ── 取得選取列的資料 ── */
  function getSelectedRowData() {
    var grid = getActiveGrid();
    if (!grid) return null;
    var tr = grid.querySelector('tbody tr.selected-row');
    if (!tr) return null;
    var row = {};
    tr.querySelectorAll('input.cell-edit[name]').forEach(function (inp) {
      if (!inp.name) return;
      row[inp.name] = inp.type === 'checkbox' ? (inp.checked ? '1' : '0') : (inp.value || '');
    });
    return Object.keys(row).length ? row : null;
  }

  /* ── 從選取列取得 key 值（對應 Delphi 的 LocateKeys）── */
  function getSelectedKeyValue() {
    var row = getSelectedRowData();
    if (!row) return '';
    var keyFields = getKeyFields();
    if (keyFields.length) {
      var rowKeys = Object.keys(row);
      for (var i = 0; i < keyFields.length; i++) {
        var target = keyFields[i].toLowerCase();
        var hit = rowKeys.find(function (k) { return k.toLowerCase() === target; });
        if (hit && row[hit]) return String(row[hit]).trim();
      }
    }
    /* fallback: 第一個欄位 */
    var keys = Object.keys(row);
    return keys.length ? String(row[keys[0]] || '').trim() : '';
  }

  /* ── 開啟紀錄 ── */
  async function openLog() {
    await ensureUpdateLogScript();
    if (typeof window.openSharedUpdateLog !== 'function') {
      alert('紀錄功能尚未載入，請稍後再試。');
      return;
    }

    var paperNum = getSelectedKeyValue();
    if (!paperNum) {
      alert('請先選取一筆資料');
      return;
    }

    var paperId = getActiveTableName();
    await window.openSharedUpdateLog({
      paperId: paperId,
      paperNum: paperNum,
      itemId: ITEM_ID,
      logType: LOG_TYPE
    });
  }

  /* ── 新增「紀錄」按鈕 ── */
  function addLogButton() {
    if (document.getElementById('btnUpdateLog')) return;

    var bar = document.querySelector('.toolbar-main .d-flex');
    if (!bar) return;

    var btn = document.createElement('button');
    btn.type = 'button';
    btn.id = 'btnUpdateLog';
    btn.className = 'btn toolbar-btn';
    btn.style.cssText = 'color:#6c5ce7; border-color:#b8b0e6;';
    btn.innerHTML = '<i class="bi bi-clock-history"></i>紀錄';
    btn.addEventListener('click', openLog);

    bar.appendChild(btn);
  }

  /* ── 註冊 MultiGridCustomHandlers（供資料庫設定的自訂按鈕使用） ── */
  window.MultiGridCustomHandlers = window.MultiGridCustomHandlers || {};
  window.MultiGridCustomHandlers[ITEM_ID] = window.MultiGridCustomHandlers[ITEM_ID] || {};
  window.MultiGridCustomHandlers[ITEM_ID]['紀錄'] = async function (ctx) {
    await ensureUpdateLogScript();
    if (typeof window.openSharedUpdateLog !== 'function') {
      alert('紀錄功能尚未載入，請稍後再試。');
      return;
    }
    var paperId = getActiveTableName();
    await window.openSharedUpdateLog({
      paperId: paperId,
      paperNum: ctx.paperNum || '',
      itemId: ITEM_ID,
      logType: LOG_TYPE
    });
  };

  /* ── 隱藏查詢按鈕 ── */
  function hideQueryButton() {
    var btn = document.getElementById('btnMultiGridQuery');
    if (btn) btn.classList.add('d-none');
  }

  /* ══════════════════════════════════════════════════════════════
     BeforePost 修改紀錄：攔截儲存動作，記錄欄位層級的修改差異
     對應 Delphi qryMaster1BeforePost 中呼叫
     CURdTableUpdateLogUpdate_AT000002 的邏輯
     ══════════════════════════════════════════════════════════════ */

  /* 收集某一列中所有已變更的欄位（只記錄使用者實際修改的欄位） */
  function collectFieldChanges(tr) {
    var changes = [];
    if (!tr) return changes;
    tr.querySelectorAll('input.cell-edit[name]').forEach(function (inp) {
      if (!inp.name) return;
      if (inp.dataset.readonly === '1') return;
      if (inp.classList.contains('key-field-hidden')) return;
      if (inp.type === 'hidden') return;

      var oldVal = inp.defaultValue ?? '';
      var newVal = inp.type === 'checkbox'
        ? (inp.checked ? '1' : '0')
        : (inp.value ?? '');

      if (oldVal === newVal) return;

      changes.push({
        FieldName: inp.name,
        OldValue: oldVal || null,
        NewValue: newVal || null
      });
    });
    return changes;
  }

  /* 取得一列的 key 值 */
  function getRowKeyValue(tr, keyFields) {
    if (!tr || !keyFields || !keyFields.length) return '';
    var inputs = tr.querySelectorAll('input.cell-edit[name]');
    for (var i = 0; i < keyFields.length; i++) {
      var target = keyFields[i].toLowerCase();
      for (var j = 0; j < inputs.length; j++) {
        if ((inputs[j].name || '').toLowerCase() === target) {
          var val = inputs[j].type === 'checkbox'
            ? (inputs[j].checked ? '1' : '0')
            : (inputs[j].value || '');
          if (val.trim()) return val.trim();
        }
      }
    }
    // fallback: 第一個 input
    if (inputs.length) {
      var v = inputs[0].type === 'checkbox'
        ? (inputs[0].checked ? '1' : '0')
        : (inputs[0].value || '');
      return v.trim();
    }
    return '';
  }

  /* 送出修改紀錄到 API */
  async function recordChangesToServer(tableName, keyNum, changes) {
    if (!tableName || !keyNum || !changes || !changes.length) return;
    try {
      var jwt = localStorage.getItem('jwtId');
      var headers = { 'Content-Type': 'application/json' };
      if (jwt) headers['X-JWTID'] = jwt;

      await fetch('/api/UpdateLog/RecordChanges', {
        method: 'POST',
        headers: headers,
        body: JSON.stringify({
          TableName: tableName,
          KeyNum: keyNum,
          UserId: localStorage.getItem('erpLoginUserId') || '',
          SpSuffix: SP_SUFFIX,
          Changes: changes
        })
      });
    } catch (err) {
      console.error('記錄修改紀錄失敗:', err);
    }
  }

  /* 攔截 mgSave 按鈕，在儲存前先記錄修改 */
  function hookSaveButton() {
    var mgSave = document.getElementById('mgSave');
    if (!mgSave || mgSave.dataset.logHooked === '1') return;
    mgSave.dataset.logHooked = '1';

    mgSave.addEventListener('click', async function () {
      var grid = getActiveGrid();
      if (!grid) return;

      var tableName = grid.dataset.tableName || grid.dataset.dictTable || '';
      var keyFields = (grid.dataset.keyFields || '').split(',').map(function (s) { return s.trim(); }).filter(Boolean);

      // 收集所有已修改列（排除新增列）的欄位變更
      var rows = grid.querySelectorAll('tbody tr');
      var promises = [];

      rows.forEach(function (tr) {
        if (tr.dataset.state === 'added') return;    // 新增列不記錄 BeforePost 差異
        if (tr.dataset.state === 'deleted') return;   // 刪除列不記錄

        var changes = collectFieldChanges(tr);
        if (changes.length === 0) return;

        var keyNum = getRowKeyValue(tr, keyFields);
        if (!keyNum) return;

        promises.push(recordChangesToServer(tableName, keyNum, changes));
      });

      if (promises.length > 0) {
        await Promise.all(promises);
      }
    }, true); // useCapture=true，確保在原本的 click handler 之前執行
  }

  /* ── 初始化 ── */
  function init() {
    hideQueryButton();
    addLogButton();
    hookSaveButton();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();