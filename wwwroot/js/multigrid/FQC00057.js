(function () {
  'use strict';

  var ITEM_ID = 'FQC00057';
  var itemId = (window._multiGridItemId || '').toString().toUpperCase();
  if (itemId !== ITEM_ID) return;

  var BAR_ID = 'fqc00057FilterBar';

  function withJwtHeaders(init) {
    var base = init || {};
    var headers = Object.assign({}, base.headers || {});
    var jwt = localStorage.getItem('jwtId');
    if (jwt) headers['X-JWTID'] = jwt;
    return Object.assign({}, base, { headers: headers });
  }

  function normalizeStatus(raw) {
    var s = (raw || '').toString().trim().toLowerCase();
    if (s === 'inactive' || s === '0') return 'inactive';
    if (s === 'all' || s === '2' || s === 'any') return 'all';
    return 'active';
  }

  function readStateFromUrl() {
    var url = new URL(window.location.href);
    var mode = (url.searchParams.get('bProcMode') || '').toLowerCase();
    if (mode !== 'like') mode = '=';
    return {
      status: normalizeStatus(url.searchParams.get('status')),
      bProcMode: mode,
      bProcCode: (url.searchParams.get('bProcCode') || '').trim()
    };
  }

  function buildTargetUrl(state) {
    var url = new URL(window.location.href);

    url.searchParams.set('status', state.status || 'active');

    if ((state.bProcCode || '').trim()) {
      url.searchParams.set('bProcCode', state.bProcCode.trim());
      url.searchParams.set('bProcMode', state.bProcMode === 'like' ? 'like' : '=');
    } else {
      url.searchParams.delete('bProcCode');
      url.searchParams.delete('bProcMode');
    }

    url.searchParams.set('pageIndex', '1');
    url.searchParams.delete('page');

    var keys = Array.from(url.searchParams.keys());
    keys.forEach(function (k) {
      if (/^pageIndex_/i.test(k) || /^pageSize_/i.test(k)) {
        url.searchParams.delete(k);
      }
    });

    return url.toString();
  }

  function ensureStyle() {
    if (document.getElementById('fqc00057-filter-style')) return;
    var style = document.createElement('style');
    style.id = 'fqc00057-filter-style';
    style.textContent = [
      '.fqc00057-filter-bar{display:flex;align-items:center;gap:10px;flex-wrap:wrap;padding:6px 10px;margin:0 0 8px;background:#f8fafc;border:1px solid #d9e2ef;border-radius:8px}',
      '.fqc00057-radio-group{display:flex;align-items:center;gap:14px;margin-right:8px}',
      '.fqc00057-radio-group label{display:inline-flex;align-items:center;gap:4px;font-size:.88rem;color:#1f2937;cursor:pointer}',
      '.fqc00057-proc-group{display:flex;align-items:center;gap:6px}',
      '.fqc00057-proc-group .lbl{font-size:.88rem;color:#1f2937;white-space:nowrap}',
      '.fqc00057-proc-group select,.fqc00057-proc-group input{height:28px;font-size:.85rem}',
      '.fqc00057-proc-group select{width:72px}',
      '.fqc00057-proc-group input{width:260px}',
      '@media (max-width:767.98px){.fqc00057-proc-group input{width:180px}}'
    ].join('');
    document.head.appendChild(style);
  }

  function ensureFilterBar() {
    if (document.getElementById(BAR_ID)) return document.getElementById(BAR_ID);

    var toolbar = document.querySelector('.multigrid-toolbar');
    if (!toolbar) return null;

    var bar = document.createElement('div');
    bar.id = BAR_ID;
    bar.className = 'fqc00057-filter-bar';
    bar.innerHTML = [
      '<div class="fqc00057-radio-group">',
      '  <label><input type="radio" name="fqc00057-status" value="active"> 生效</label>',
      '  <label><input type="radio" name="fqc00057-status" value="inactive"> 失效</label>',
      '  <label><input type="radio" name="fqc00057-status" value="all"> 不限</label>',
      '</div>',
      '<div class="fqc00057-proc-group">',
      '  <span class="lbl">製程大站</span>',
      '  <select id="fqc00057-bproc-mode" class="form-select form-select-sm">',
      '    <option value="=">=</option>',
      '    <option value="like">like</option>',
      '  </select>',
      '  <input id="fqc00057-bproc-code" class="form-control form-control-sm" list="fqc00057-bproc-list" autocomplete="off" />',
      '  <datalist id="fqc00057-bproc-list"></datalist>',
      '</div>',
      '<button type="button" id="fqc00057-btn-get" class="btn toolbar-btn">資料重取</button>'
    ].join('');

    toolbar.insertAdjacentElement('afterend', bar);
    return bar;
  }

  async function loadProcLookup() {
    var list = document.getElementById('fqc00057-bproc-list');
    if (!list || list.dataset.loaded === '1') return;

    try {
      var url = '/api/TableFieldLayout/LookupData?table=FQCdProcInfo&key=BProcCode&result=BProcName';
      var rows = await fetch(url, withJwtHeaders()).then(function (r) { return r.json(); });
      if (!Array.isArray(rows)) return;

      var frag = document.createDocumentFragment();
      rows.forEach(function (row) {
        var code = (row && row.key != null ? String(row.key) : '').trim();
        if (!code) return;
        var name = (row && row.result0 != null ? String(row.result0) : '').trim();
        var option = document.createElement('option');
        option.value = code;
        option.label = name ? (code + ' ' + name) : code;
        frag.appendChild(option);
      });
      list.appendChild(frag);
      list.dataset.loaded = '1';
    } catch (err) {
      console.error('[FQC00057] 載入製程大站失敗', err);
    }
  }

  function collectState() {
    var checked = document.querySelector('input[name="fqc00057-status"]:checked');
    var modeEl = document.getElementById('fqc00057-bproc-mode');
    var codeEl = document.getElementById('fqc00057-bproc-code');
    return {
      status: checked ? checked.value : 'active',
      bProcMode: modeEl && modeEl.value === 'like' ? 'like' : '=',
      bProcCode: codeEl ? codeEl.value.trim() : ''
    };
  }

  function applyFilter() {
    var target = buildTargetUrl(collectState());
    window.location.href = target;
  }

  function bindEvents() {
    var getBtn = document.getElementById('fqc00057-btn-get');
    var modeEl = document.getElementById('fqc00057-bproc-mode');
    var codeEl = document.getElementById('fqc00057-bproc-code');
    var radios = document.querySelectorAll('input[name="fqc00057-status"]');

    if (getBtn) getBtn.addEventListener('click', applyFilter);

    if (codeEl) {
      codeEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
          e.preventDefault();
          applyFilter();
        }
      });
    }

    if (modeEl) {
      modeEl.addEventListener('change', function () {
        if (!(codeEl && codeEl.value.trim())) return;
        applyFilter();
      });
    }

    radios.forEach(function (r) {
      r.addEventListener('change', applyFilter);
    });
  }

  function applyInitialState() {
    var state = readStateFromUrl();

    var statusRadio = document.querySelector('input[name="fqc00057-status"][value="' + state.status + '"]');
    if (statusRadio) statusRadio.checked = true;

    var modeEl = document.getElementById('fqc00057-bproc-mode');
    if (modeEl) modeEl.value = state.bProcMode;

    var codeEl = document.getElementById('fqc00057-bproc-code');
    if (codeEl) codeEl.value = state.bProcCode;
  }

  function hideDefaultQueryButton() {
    var btn = document.getElementById('btnMultiGridQuery');
    if (btn) btn.classList.add('d-none');
  }

  async function init() {
    ensureStyle();
    var bar = ensureFilterBar();
    if (!bar) return;

    hideDefaultQueryButton();
    applyInitialState();
    bindEvents();
    await loadProcLookup();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () { void init(); });
  } else {
    void init();
  }
})();
