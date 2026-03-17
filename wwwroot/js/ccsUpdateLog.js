(function () {
  var dictCache = Object.create(null);

  function withJwtHeaders(init) {
    var jwt = localStorage.getItem('jwtId');
    var headers = Object.assign({}, (init && init.headers) || {});
    if (jwt) headers['X-JWTID'] = jwt;
    return Object.assign({}, init || {}, { headers: headers });
  }

  function showPrompt(message, type) {
    var text = message == null ? '' : String(message);
    if (typeof window.ccsPrompt === 'function') return window.ccsPrompt(text, type || 'info');
    if (window.Swal && typeof window.Swal.fire === 'function') {
      var icon = type === 'error' ? 'error'
        : type === 'warning' ? 'warning'
        : type === 'success' ? 'success'
        : 'info';
      return window.Swal.fire({ icon: icon, title: text, confirmButtonText: '確定' });
    }
    window.alert(text);
  }

  function ensureModal() {
    var modalEl = document.getElementById('updateLogModal');
    if (modalEl) {
      // 移除舊 modal，重新建立（確保結構為最新版本）
      var oldInstance = window.bootstrap && bootstrap.Modal.getInstance(modalEl);
      if (oldInstance) oldInstance.dispose();
      modalEl.remove();
    }

    modalEl = document.createElement('div');
    modalEl.className = 'modal fade';
    modalEl.id = 'updateLogModal';
    modalEl.tabIndex = -1;
    modalEl.setAttribute('aria-hidden', 'true');
    modalEl.innerHTML = [
      '<div class="modal-dialog modal-xl update-log-dialog">',
      '  <div class="modal-content update-log-modal">',
      '    <div class="modal-header">',
      '      <h5 class="modal-title">修改歷史</h5>',
      '      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>',
      '    </div>',
      '    <div class="modal-body">',
      '      <div class="ul-toolbar">',
      '        <button type="button" class="btn btn-sm btn-outline-secondary ul-refresh">重新整理</button>',
      '      </div>',
      '      <div class="ul-layout">',
      '        <section class="ul-top">',
      '          <div class="ul-title">更新記錄</div>',
      '          <div class="table-responsive ul-scroll">',
      '            <table class="table table-sm table-bordered mb-0 ul-master"><thead></thead><tbody></tbody></table>',
      '          </div>',
      '        </section>',
      '        <section class="ul-bottom">',
      '          <div class="ul-left">',
      '            <div class="ul-title">歷史記錄</div>',
      '            <div class="table-responsive ul-scroll">',
      '              <table class="table table-sm table-bordered mb-0 ul-history"><thead></thead><tbody></tbody></table>',
      '            </div>',
      '          </div>',
      '          <div class="ul-right">',
      '            <div class="ul-title">差異內容</div>',
      '            <textarea class="form-control ul-diff" rows="8" readonly></textarea>',
      '          </div>',
      '        </section>',
      '      </div>',
      '    </div>',
      '  </div>',
      '</div>'
    ].join('');
    document.body.appendChild(modalEl);

    var existingSt = document.getElementById('updateLogModalStyle');
    if (existingSt) existingSt.remove();
    var st = document.createElement('style');
    st.id = 'updateLogModalStyle';
    st.textContent = [
      '#updateLogModal .update-log-dialog{--bs-modal-width:1240px;max-width:1240px;width:1240px;}',
      '#updateLogModal .update-log-modal{max-width:1240px;width:1240px;height:calc(88vh);display:flex;flex-direction:column;}',
      '#updateLogModal .modal-body{flex:1 1 auto;min-height:0;overflow:hidden;display:flex;flex-direction:column;padding:12px;}',
      '#updateLogModal .ul-toolbar{display:flex;align-items:center;gap:8px;margin-bottom:8px;}',
      '#updateLogModal .ul-layout{display:grid;grid-template-rows:40% 60%;gap:10px;flex:1 1 auto;min-height:0;}',
      '#updateLogModal .ul-top,#updateLogModal .ul-left,#updateLogModal .ul-right{display:flex;flex-direction:column;min-height:0;}',
      '#updateLogModal .ul-bottom{display:grid;grid-template-columns:55% 45%;gap:10px;min-height:0;}',
      '#updateLogModal .ul-title{font-size:.95rem;font-weight:700;color:#2f3a48;margin-bottom:4px;}',
      '#updateLogModal .ul-scroll{flex:1 1 auto;min-height:0;border:1px solid #d6dbe3;border-radius:4px;overflow:auto;background:#fff;}',
      '#updateLogModal table{font-size:13px;table-layout:fixed;width:100%;}',
      '#updateLogModal thead th{position:sticky;top:0;background:#f3f6fb;z-index:1;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;user-select:none;border-right:1px solid #d6dbe3;padding:4px 8px;}',
      '#updateLogModal thead th .ul-resizer{position:absolute;right:0;top:0;bottom:0;width:5px;cursor:col-resize;background:transparent;}',
      '#updateLogModal thead th .ul-resizer:hover{background:#aab;}',
      '#updateLogModal tbody td{text-overflow:ellipsis;overflow:hidden;white-space:nowrap;padding:3px 8px;}',
      '#updateLogModal tbody tr{cursor:pointer;}',
      '#updateLogModal tbody tr:hover td{background:#eef3fa;}',
      '#updateLogModal tbody tr.ul-row-selected td{background:#c6e0f5 !important;}',
      '#updateLogModal .ul-diff{flex:1 1 auto;min-height:0;font-size:13px;line-height:1.45;border:1px solid #d6dbe3;border-radius:4px;resize:none;background:#fcfcfd;}',
      '@media (max-width: 1400px){#updateLogModal .update-log-dialog{max-width:96vw;width:96vw;}#updateLogModal .update-log-modal{max-width:96vw;width:96vw;}}'
    ].join('');
    document.head.appendChild(st);

    return modalEl;
  }

  /* ── 從 row 中取值（不區分大小寫） ── */
  function getCI(row, names) {
    var obj = row || {};
    var keys = Object.keys(obj);
    for (var i = 0; i < names.length; i++) {
      var target = String(names[i] || '').toLowerCase();
      var hit = keys.find(function (k) { return String(k).toLowerCase() === target; });
      if (hit !== undefined) return obj[hit];
    }
    return '';
  }

  /* ── 日期格式化 ── */
  function fmtDate(val) {
    if (!val) return '';
    var s = String(val);
    var d = new Date(s);
    if (isNaN(d.getTime())) return s;
    var pad = function (n) { return n < 10 ? '0' + n : String(n); };
    return d.getFullYear() + '/' + pad(d.getMonth() + 1) + '/' + pad(d.getDate()) +
      ' ' + pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds());
  }

  /* ══════════════════════════════════════════════════
     固定欄位定義（對應 Delphi UpdateLog 表單）
     ══════════════════════════════════════════════════ */

  // 上方「更新記錄」= Delphi dbgHis（History）
  var COLS_TOP = [
    { key: 'DisplayLabel', label: '資料表名稱', width: 200, fallback: ['DisplayLabel', 'TableName'] },
    { key: 'KeyNum',       label: '鍵值',       width: 120, fallback: ['KeyNum'] },
    { key: 'UserId',       label: '作業者代號', width: 120, fallback: ['UserId'] }
  ];

  // 下方「歷史記錄」= Delphi JSdDBGrid1（Master detail）
  var COLS_BOTTOM = [
    { key: 'DisplayLabel', label: '資料表名稱', width: 180, fallback: ['DisplayLabel', 'TableName'] },
    { key: 'KeyNum',       label: '鍵值',       width: 100, fallback: ['KeyNum'] },
    { key: 'UpdateTime',   label: '更新時間',   width: 170, fallback: ['UpdateTime'], format: fmtDate },
    { key: 'UserId',       label: '作業者代號', width: 100, fallback: ['UserId'] },
    { key: 'UserName',     label: '作業者',     width: 100, fallback: ['UserName', 'UserId'] }
  ];

  /* ── 建立表格（含可拖拉欄寬） ── */
  function buildFixedTable(tableEl, rows, colDefs, onClick) {
    var thead = tableEl.querySelector('thead');
    var tbody = tableEl.querySelector('tbody');
    thead.innerHTML = '';
    tbody.innerHTML = '';

    var list = Array.isArray(rows) ? rows : [];

    // colgroup
    var existingCg = tableEl.querySelector('colgroup');
    if (existingCg) existingCg.remove();
    var cg = document.createElement('colgroup');
    colDefs.forEach(function (c) {
      var col = document.createElement('col');
      col.style.width = c.width + 'px';
      cg.appendChild(col);
    });
    tableEl.insertBefore(cg, thead);

    // thead
    var trH = document.createElement('tr');
    colDefs.forEach(function (c, ci) {
      var th = document.createElement('th');
      th.textContent = c.label;
      th.style.position = 'relative';
      th.style.width = c.width + 'px';
      th.style.minWidth = '40px';

      // 拖拉 resizer
      var resizer = document.createElement('span');
      resizer.className = 'ul-resizer';
      resizer.addEventListener('mousedown', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var startX = e.clientX;
        var startW = th.getBoundingClientRect().width;
        var colEl = cg.children[ci];
        var onMove = function (ev) {
          var newW = Math.max(40, startW + ev.clientX - startX);
          th.style.width = newW + 'px';
          if (colEl) colEl.style.width = newW + 'px';
        };
        var onUp = function () {
          document.removeEventListener('mousemove', onMove);
          document.removeEventListener('mouseup', onUp);
          document.body.style.cursor = '';
        };
        document.body.style.cursor = 'col-resize';
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
      });
      th.appendChild(resizer);
      trH.appendChild(th);
    });
    thead.appendChild(trH);

    // tbody
    if (!list.length) {
      var emptyTr = document.createElement('tr');
      var emptyTd = document.createElement('td');
      emptyTd.colSpan = colDefs.length;
      emptyTd.className = 'text-muted small text-center';
      emptyTd.textContent = '尚無資料';
      emptyTr.appendChild(emptyTd);
      tbody.appendChild(emptyTr);
      return;
    }

    list.forEach(function (row, idx) {
      var tr = document.createElement('tr');
      tr.dataset.idx = String(idx);
      colDefs.forEach(function (c) {
        var td = document.createElement('td');
        var val = getCI(row, c.fallback || [c.key]);
        var display = c.format ? c.format(val) : (val == null ? '' : String(val));
        td.textContent = display;
        td.title = display;
        tr.appendChild(td);
      });
      if (onClick) tr.addEventListener('click', function () { onClick(idx); });
      tbody.appendChild(tr);
    });
  }

  /* ── 從 master 資料中取得不重複的「更新記錄」摘要 ── */
  function buildSummaryRows(masterRows) {
    var seen = {};
    var result = [];
    (masterRows || []).forEach(function (row) {
      var tbl = getCI(row, ['DisplayLabel', 'TableName']) || '';
      var key = getCI(row, ['KeyNum']) || '';
      var uid = getCI(row, ['UserId']) || '';
      var k = tbl + '|' + key + '|' + uid;
      if (!seen[k]) {
        seen[k] = true;
        result.push(row);
      }
    });
    return result;
  }

  /* ── API 呼叫 ── */
  async function fetchLog(paperId, paperNum, logType) {
    var params = { paperId: paperId, paperNum: paperNum, userId: '' };
    if (logType) params.logType = logType;
    var qs = new URLSearchParams(params).toString();

    var mr = await fetch('/api/UpdateLog/Master?' + qs, withJwtHeaders());
    var hr = await fetch('/api/UpdateLog/History?' + qs, withJwtHeaders());
    var mj = await mr.json().catch(function () { return {}; });
    var hj = await hr.json().catch(function () { return {}; });

    return {
      ok: mr.ok || hr.ok,
      master: (mr.ok && mj && mj.ok !== false) ? (mj.rows || []) : [],
      history: (hr.ok && hj && hj.ok !== false) ? (hj.rows || []) : [],
      error: (mj && (mj.error || mj.message)) || (hj && (hj.error || hj.message)) || ''
    };
  }

  /* ── 主入口 ── */
  window.openSharedUpdateLog = async function (opt) {
    var paperNum = (opt && opt.paperNum) ? String(opt.paperNum).trim() : '';
    var itemId = (opt && opt.itemId) ? String(opt.itemId).trim() : '';
    var paperId = (opt && opt.paperId) ? String(opt.paperId).trim() : '';
    var logType = (opt && opt.logType) ? String(opt.logType).trim() : '';

    if (!paperNum) {
      showPrompt('請先選取一筆資料', 'warning');
      return;
    }

    var primaryPaperId = paperId || itemId;
    var secondaryPaperId = (itemId && itemId !== primaryPaperId) ? itemId : '';

    var a = await fetchLog(primaryPaperId, paperNum, logType);
    var needFallback = !!secondaryPaperId && (!a.ok || ((a.master || []).length === 0 && (a.history || []).length === 0));
    var b = needFallback ? await fetchLog(secondaryPaperId, paperNum, logType) : null;

    // master = Delphi qryMaster1（下方歷史記錄明細）
    var masterRows = a.master.length ? a.master : ((b && b.master) || []);
    // history = Delphi qryHis（上方更新記錄摘要）
    var historyRows = a.history.length ? a.history : ((b && b.history) || []);

    if (!a.ok && !(b && b.ok)) {
      showPrompt('讀取記錄失敗：' + (a.error || (b && b.error) || 'unknown'), 'error');
      return;
    }

    // 上方「更新記錄」：若 history 沒資料，從 master 取不重複摘要
    var topRows = historyRows.length ? historyRows : buildSummaryRows(masterRows);
    // 下方「歷史記錄」：全部明細
    var bottomRows = masterRows;

    var modalEl = ensureModal();
    var masterTable = modalEl.querySelector('.ul-master');
    var historyTable = modalEl.querySelector('.ul-history');
    var diffEl = modalEl.querySelector('.ul-diff');

    // 上方：更新記錄（3 欄）
    buildFixedTable(masterTable, topRows, COLS_TOP, null);

    // 下方：歷史記錄（5 欄）+ 點擊顯示差異
    buildFixedTable(historyTable, bottomRows, COLS_BOTTOM, function (idx) {
      historyTable.querySelectorAll('tbody tr').forEach(function (tr) { tr.classList.remove('ul-row-selected'); });
      var rowEl = historyTable.querySelector('tbody tr[data-idx="' + idx + '"]');
      if (rowEl) rowEl.classList.add('ul-row-selected');
      var row = bottomRows[idx] || {};
      diffEl.value = getCI(row, ['Difference']) || '';
    });

    // 預設選取第一筆歷史記錄
    if (bottomRows.length) {
      diffEl.value = getCI(bottomRows[0], ['Difference']) || '';
      var tr0 = historyTable.querySelector('tbody tr[data-idx="0"]');
      if (tr0) tr0.classList.add('ul-row-selected');
    } else {
      diffEl.value = '';
    }

    // ★ 綁定「重新整理」按鈕（從 Doc2 補回）
    var refreshBtn = modalEl.querySelector('.ul-refresh');
    if (refreshBtn) {
      refreshBtn.addEventListener('click', function () {
        window.openSharedUpdateLog(opt);
      });
    }

    if (window.bootstrap) {
      var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
      modal.show();
    }
  };

  window.openCcsUpdateLog = window.openSharedUpdateLog;
})();