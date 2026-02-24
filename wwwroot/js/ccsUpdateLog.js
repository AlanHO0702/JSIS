(function () {
  function withJwtHeaders(init) {
    var jwt = localStorage.getItem('jwtId');
    var headers = Object.assign({}, (init && init.headers) || {});
    if (jwt) headers['X-JWTID'] = jwt;
    return Object.assign({}, init || {}, { headers: headers });
  }

  function ensureModal() {
    var modalEl = document.getElementById('updateLogModal');
    if (modalEl) return modalEl;

    modalEl = document.createElement('div');
    modalEl.className = 'modal fade';
    modalEl.id = 'updateLogModal';
    modalEl.tabIndex = -1;
    modalEl.setAttribute('aria-hidden', 'true');
    modalEl.innerHTML = [
      '<div class="modal-dialog modal-xl update-log-dialog">',
      '  <div class="modal-content update-log-modal">',
      '    <div class="modal-header">',
      '      <h5 class="modal-title">修改紀錄</h5>',
      '      <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>',
      '    </div>',
      '    <div class="modal-body">',
      '      <div class="d-flex align-items-center gap-2 mb-2">',
      '        <button type="button" class="btn btn-sm btn-outline-secondary ul-refresh">重新整理</button>',
      '      </div>',
      '      <div class="ul-grid">',
      '        <div class="ul-pane">',
      '          <div class="ul-title"><span>更新記錄</span></div>',
      '          <div class="table-responsive ul-scroll">',
      '            <table class="table table-sm table-bordered mb-0 ul-master"><thead></thead><tbody></tbody></table>',
      '          </div>',
      '        </div>',
      '        <div class="ul-pane">',
      '          <div class="ul-title"><span>歷史記錄</span></div>',
      '          <div class="table-responsive ul-scroll ul-history-wrap">',
      '            <table class="table table-sm table-bordered mb-0 ul-history"><thead></thead><tbody></tbody></table>',
      '          </div>',
      '          <textarea class="form-control ul-diff" rows="6" readonly></textarea>',
      '        </div>',
      '      </div>',
      '    </div>',
      '  </div>',
      '</div>'
    ].join('');

    document.body.appendChild(modalEl);

    var st = document.createElement('style');
    st.textContent = [
      '#updateLogModal .update-log-dialog{--bs-modal-width:1200px;max-width:1200px;width:1200px;}',
      '#updateLogModal .update-log-modal{max-width:1200px;width:1200px;height:calc(86vh);display:flex;flex-direction:column;}',
      '#updateLogModal .modal-body{flex:1 1 auto;min-height:0;overflow:hidden;display:flex;flex-direction:column;}',
      '#updateLogModal .ul-grid{display:grid;grid-template-rows:1fr 1fr;gap:10px;flex:1 1 auto;min-height:0;}',
      '#updateLogModal .ul-pane{display:flex;flex-direction:column;min-height:0;}',
      '#updateLogModal .ul-title{font-size:.95rem;font-weight:700;color:#374151;margin-bottom:4px;}',
      '#updateLogModal .ul-scroll{flex:1 1 auto;min-height:0;border:1px solid #dee2e6;border-radius:6px;overflow:auto;}',
      '#updateLogModal .ul-history-wrap{margin-bottom:8px;}',
      '#updateLogModal table{font-size:13px;}',
      '#updateLogModal tbody tr.ul-row-selected td{background:#fff3cd !important;}',
      '#updateLogModal .ul-diff{font-size:13px;}'
    ].join('');
    document.head.appendChild(st);

    return modalEl;
  }

  function buildTable(tableEl, rows, onClick) {
    var thead = tableEl.querySelector('thead');
    var tbody = tableEl.querySelector('tbody');
    thead.innerHTML = '';
    tbody.innerHTML = '';

    var list = Array.isArray(rows) ? rows : [];
    if (!list.length) {
      tbody.innerHTML = '<tr><td class="text-muted small">尚無資料</td></tr>';
      return;
    }

    var cols = Object.keys(list[0] || {});
    thead.innerHTML = '<tr>' + cols.map(function (c) { return '<th class="text-nowrap">' + c + '</th>'; }).join('') + '</tr>';
    list.forEach(function (row, idx) {
      var tr = document.createElement('tr');
      tr.dataset.idx = String(idx);
      cols.forEach(function (c) {
        var td = document.createElement('td');
        td.className = 'text-nowrap';
        td.textContent = row[c] == null ? '' : String(row[c]);
        tr.appendChild(td);
      });
      if (onClick) tr.addEventListener('click', function () { onClick(idx); });
      tbody.appendChild(tr);
    });
  }

  async function fetchLog(paperId, paperNum) {
    var qs = new URLSearchParams({ paperId: paperId, paperNum: paperNum, userId: '' }).toString();
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

  window.openCcsUpdateLog = async function (opt) {
    var paperNum = (opt && opt.paperNum) ? String(opt.paperNum).trim() : '';
    var paperId = (opt && opt.paperId) ? String(opt.paperId).trim() : '';
    var itemId = (opt && opt.itemId) ? String(opt.itemId).trim() : '';

    if (!paperNum) {
      alert('請先選取一筆資料');
      return;
    }

    var a = await fetchLog(paperId || itemId, paperNum);
    var needFallback = !!(paperId && itemId && paperId !== itemId);
    var needTryItemId = needFallback && (!a.ok || ((a.master || []).length === 0 && (a.history || []).length === 0));
    var b = needTryItemId ? await fetchLog(itemId, paperNum) : null;
    var masterRows = a.master.length ? a.master : ((b && b.master) || []);
    var historyRows = a.history.length ? a.history : ((b && b.history) || []);

    if (!a.ok && !(b && b.ok)) {
      alert('讀取記錄失敗：' + (a.error || (b && b.error) || 'unknown'));
      return;
    }

    var modalEl = ensureModal();
    var masterTable = modalEl.querySelector('.ul-master');
    var historyTable = modalEl.querySelector('.ul-history');
    var diffEl = modalEl.querySelector('.ul-diff');

    buildTable(masterTable, masterRows, function (idx) {
      masterTable.querySelectorAll('tbody tr').forEach(function (tr) { tr.classList.remove('ul-row-selected'); });
      var rowEl = masterTable.querySelector('tbody tr[data-idx="' + idx + '"]');
      if (rowEl) rowEl.classList.add('ul-row-selected');
      var row = masterRows[idx] || {};
      diffEl.value = row.Difference || row.difference || '';
    });
    buildTable(historyTable, historyRows, null);

    if (masterRows.length) {
      var row0 = masterRows[0] || {};
      diffEl.value = row0.Difference || row0.difference || '';
      var tr0 = masterTable.querySelector('tbody tr[data-idx="0"]');
      if (tr0) tr0.classList.add('ul-row-selected');
    } else {
      diffEl.value = '';
    }

    var refreshBtn = modalEl.querySelector('.ul-refresh');
    if (refreshBtn && !refreshBtn.dataset.bound) {
      refreshBtn.dataset.bound = '1';
      refreshBtn.addEventListener('click', function () {
        window.openCcsUpdateLog(opt);
      });
    }

    if (window.bootstrap) {
      var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
      modal.show();
    }
  };
})();
