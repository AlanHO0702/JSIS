(function () {
  var dictCache = Object.create(null);

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

    var st = document.createElement('style');
    st.textContent = [
      '#updateLogModal .update-log-dialog{--bs-modal-width:1240px;max-width:1240px;width:1240px;}',
      '#updateLogModal .update-log-modal{max-width:1240px;width:1240px;height:calc(88vh);display:flex;flex-direction:column;}',
      '#updateLogModal .modal-body{flex:1 1 auto;min-height:0;overflow:hidden;display:flex;flex-direction:column;padding:12px;}',
      '#updateLogModal .ul-toolbar{display:flex;align-items:center;gap:8px;margin-bottom:8px;}',
      '#updateLogModal .ul-layout{display:grid;grid-template-rows:58% 42%;gap:10px;flex:1 1 auto;min-height:0;}',
      '#updateLogModal .ul-top,#updateLogModal .ul-left,#updateLogModal .ul-right{display:flex;flex-direction:column;min-height:0;}',
      '#updateLogModal .ul-bottom{display:grid;grid-template-columns:48% 52%;gap:10px;min-height:0;}',
      '#updateLogModal .ul-title{font-size:.95rem;font-weight:700;color:#2f3a48;margin-bottom:4px;}',
      '#updateLogModal .ul-scroll{flex:1 1 auto;min-height:0;border:1px solid #d6dbe3;border-radius:4px;overflow:auto;background:#fff;}',
      '#updateLogModal table{font-size:13px;table-layout:fixed;}',
      '#updateLogModal thead th{position:sticky;top:0;background:#f3f6fb;z-index:1;white-space:nowrap;}',
      '#updateLogModal tbody td{text-overflow:ellipsis;overflow:hidden;white-space:nowrap;}',
      '#updateLogModal tbody tr.ul-row-selected td{background:#fff3cd !important;}',
      '#updateLogModal .ul-diff{flex:1 1 auto;min-height:0;font-size:13px;line-height:1.45;border:1px solid #d6dbe3;border-radius:4px;resize:none;background:#fcfcfd;}',
      '@media (max-width: 1400px){#updateLogModal .update-log-dialog{max-width:96vw;width:96vw;}#updateLogModal .update-log-modal{max-width:96vw;width:96vw;}}'
    ].join('');
    document.head.appendChild(st);

    return modalEl;
  }

  function getCI(row, names) {
    var obj = row || {};
    var keys = Object.keys(obj);
    for (var i = 0; i < names.length; i++) {
      var target = String(names[i] || '').toLowerCase();
      var hit = keys.find(function (k) { return String(k).toLowerCase() === target; });
      if (hit) return obj[hit];
    }
    return '';
  }

  async function loadDict(tableName) {
    var tn = String(tableName || '').trim();
    if (!tn) return [];
    if (dictCache[tn]) return dictCache[tn];

    try {
      var resp = await fetch('/api/TableFieldLayout/DictFields?table=' + encodeURIComponent(tn) + '&lang=TW', withJwtHeaders());
      if (!resp.ok) {
        dictCache[tn] = [];
        return [];
      }
      var rows = await resp.json().catch(function () { return []; });
      if (!Array.isArray(rows)) {
        dictCache[tn] = [];
        return [];
      }

      var dict = rows
        .filter(function (x) {
          var v = x.Visible != null ? x.Visible : x.visible;
          return v === 1 || v === true;
        })
        .sort(function (a, b) {
          var sa = Number(a.SerialNum != null ? a.SerialNum : a.serialNum) || 9999;
          var sb = Number(b.SerialNum != null ? b.SerialNum : b.serialNum) || 9999;
          return sa - sb;
        })
        .map(function (x) {
          var fieldName = x.FieldName != null ? x.FieldName : x.fieldName;
          var label = x.DisplayLabel != null ? x.DisplayLabel : (x.displayLabel != null ? x.displayLabel : fieldName);
          return { fieldName: fieldName, displayLabel: label || fieldName };
        })
        .filter(function (x) { return !!x.fieldName; });

      dictCache[tn] = dict;
      return dict;
    } catch {
      dictCache[tn] = [];
      return [];
    }
  }

  function buildTable(tableEl, rows, dict, onClick) {
    var thead = tableEl.querySelector('thead');
    var tbody = tableEl.querySelector('tbody');
    thead.innerHTML = '';
    tbody.innerHTML = '';

    var list = Array.isArray(rows) ? rows : [];
    var fallbackCols = list.length
      ? Object.keys(list[0] || {}).map(function (k) { return { fieldName: k, displayLabel: k }; })
      : [];
    var cols = (Array.isArray(dict) && dict.length > 0) ? dict : fallbackCols;

    if (cols.length > 0) {
      thead.innerHTML = '<tr>' + cols.map(function (c) {
        return '<th class="text-nowrap">' + String(c.displayLabel || c.fieldName || '') + '</th>';
      }).join('') + '</tr>';
    }

    if (!list.length) {
      var colspan = Math.max(cols.length, 1);
      tbody.innerHTML = '<tr><td class="text-muted small text-center" colspan="' + colspan + '">尚無資料</td></tr>';
      return;
    }

    list.forEach(function (row, idx) {
      var tr = document.createElement('tr');
      tr.dataset.idx = String(idx);
      cols.forEach(function (col) {
        var td = document.createElement('td');
        td.className = 'text-nowrap';
        var val = getCI(row, [col.fieldName]);
        td.textContent = val == null ? '' : String(val);
        tr.appendChild(td);
      });
      if (onClick) tr.addEventListener('click', function () { onClick(idx); });
      tbody.appendChild(tr);
    });
  }

  async function fetchLog(paperId, paperNum) {
    var qs = new URLSearchParams({
      paperId: paperId,
      paperNum: paperNum,
      userId: ''
    }).toString();

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

  window.openSharedUpdateLog = async function (opt) {
    var paperNum = (opt && opt.paperNum) ? String(opt.paperNum).trim() : '';
    var itemId = (opt && opt.itemId) ? String(opt.itemId).trim() : '';
    var paperId = (opt && opt.paperId) ? String(opt.paperId).trim() : '';

    if (!paperNum) {
      alert('請先選取一筆資料');
      return;
    }

    var primaryPaperId = paperId || itemId;
    var secondaryPaperId = (itemId && itemId !== primaryPaperId) ? itemId : '';

    var a = await fetchLog(primaryPaperId, paperNum);
    var needFallback = !!secondaryPaperId && (!a.ok || ((a.master || []).length === 0 && (a.history || []).length === 0));
    var b = needFallback ? await fetchLog(secondaryPaperId, paperNum) : null;

    var masterRows = a.master.length ? a.master : ((b && b.master) || []);
    var historyRows = a.history.length ? a.history : ((b && b.history) || []);

    if (!a.ok && !(b && b.ok)) {
      alert('讀取記錄失敗：' + (a.error || (b && b.error) || 'unknown'));
      return;
    }

    var dictMaster = await loadDict('CURdTableUpdateLog');
    var dictHistory = await loadDict('CURdTableUpdateLogHis');

    var modalEl = ensureModal();
    var masterTable = modalEl.querySelector('.ul-master');
    var historyTable = modalEl.querySelector('.ul-history');
    var diffEl = modalEl.querySelector('.ul-diff');

    buildTable(masterTable, masterRows, dictMaster, function (idx) {
      masterTable.querySelectorAll('tbody tr').forEach(function (tr) { tr.classList.remove('ul-row-selected'); });
      var rowEl = masterTable.querySelector('tbody tr[data-idx="' + idx + '"]');
      if (rowEl) rowEl.classList.add('ul-row-selected');
      var row = masterRows[idx] || {};
      diffEl.value = getCI(row, ['Difference']) || '';
    });

    buildTable(historyTable, historyRows, dictHistory, null);

    if (masterRows.length) {
      var row0 = masterRows[0] || {};
      diffEl.value = getCI(row0, ['Difference']) || '';
      var tr0 = masterTable.querySelector('tbody tr[data-idx="0"]');
      if (tr0) tr0.classList.add('ul-row-selected');
    } else {
      diffEl.value = '';
    }

    var refreshBtn = modalEl.querySelector('.ul-refresh');
    if (refreshBtn && !refreshBtn.dataset.bound) {
      refreshBtn.dataset.bound = '1';
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
