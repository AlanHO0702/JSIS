/* ============================================================
   columnDragSort.js — 欄位拖曳排序共用模組
   依賴：SortableJS (window.Sortable)

   使用方式：
     initColumnDragSort({
       headerRow:    document.getElementById('table-header-row'),  // thead > tr
       tbody:        document.getElementById('dataTbody'),         // tbody (可選)
       scrollWrap:   document.querySelector('.table-wrap'),        // 水平捲動容器 (可選)
       scrollSync:   document.getElementById('hscrollBottom'),     // 同步捲軸 (可選)
       tableName:    'SPOdOrderMain',                              // 字典表名
       fieldsArray:  window._tableFields,                          // 欄位陣列 [{FieldName}] (可選)
       onSaved:      () => doQuery(currentPage),                   // 儲存成功後回呼 (可選)
       saveUrl:      '/api/TableFieldLayout/SaveSerialOrder',      // 儲存順序 API (可選)
       saveWidthUrl: '/api/DictSetupApi/FieldWidth/Save',          // 儲存寬度 API (可選)
       fetchFn:      window.busyFetch || fetch                     // fetch 函式 (可選)
     });

   F9 鍵：儲存所有已註冊表格的欄位順序與寬度到後端
   未按 F9 時，拖曳排序僅為暫時性，重新整理或離開頁面後恢復辭典初始排序
   ============================================================ */

(function () {
  'use strict';

  const EDGE_ZONE = 60;
  const SCROLL_SPEED_MAX = 18;
  const DEFAULT_SAVE_ORDER_URL = '/api/TableFieldLayout/SaveSerialOrder';
  const DEFAULT_SAVE_WIDTH_URL = '/api/DictSetupApi/FieldWidth/Save';

  // 已註冊的所有表格（供 F9 統一儲存）
  const _registry = [];

  /**
   * @param {Object} opts
   */
  function initColumnDragSort(opts) {
    if (!opts || !opts.headerRow || !window.Sortable) return;

    const headerRow  = opts.headerRow;
    const tbody      = opts.tbody || headerRow.closest('table')?.querySelector('tbody');
    const scrollWrap = opts.scrollWrap || headerRow.closest('.table-wrap') || headerRow.closest('.erp-table-wrapper');
    const scrollSync = opts.scrollSync || null;
    const tableName  = opts.tableName || window._tableName || '';
    const saveUrl    = opts.saveUrl || DEFAULT_SAVE_ORDER_URL;
    const saveWidthUrl = opts.saveWidthUrl || DEFAULT_SAVE_WIDTH_URL;
    const fetchFn    = opts.fetchFn || (window.busyFetch ? window.busyFetch : fetch.bind(window));

    // 註冊到全域清單
    _registry.push({ headerRow, tbody, tableName, saveUrl, saveWidthUrl, fetchFn, onSaved: opts.onSaved });

    let autoScrollRAF = null;
    let lastMouseX = 0;
    let originalOrder = [];

    // ---- 自動捲動 ----
    function autoScrollTick() {
      if (!scrollWrap) { stopAutoScroll(); return; }
      const rect = scrollWrap.getBoundingClientRect();
      let delta = 0;
      if (lastMouseX < rect.left + EDGE_ZONE) {
        delta = -Math.round(SCROLL_SPEED_MAX * (1 - Math.max(0, lastMouseX - rect.left) / EDGE_ZONE));
      } else if (lastMouseX > rect.right - EDGE_ZONE) {
        delta = Math.round(SCROLL_SPEED_MAX * (1 - Math.max(0, rect.right - lastMouseX) / EDGE_ZONE));
      }
      if (delta !== 0) {
        scrollWrap.scrollLeft += delta;
        if (scrollSync) scrollSync.scrollLeft = scrollWrap.scrollLeft;
      }
      autoScrollRAF = requestAnimationFrame(autoScrollTick);
    }
    function startAutoScroll() { if (!autoScrollRAF) autoScrollRAF = requestAnimationFrame(autoScrollTick); }
    function stopAutoScroll()  { if (autoScrollRAF) { cancelAnimationFrame(autoScrollRAF); autoScrollRAF = null; } }

    document.addEventListener('mousemove', function (e) { lastMouseX = e.clientX; });

    function clearDragOver() {
      headerRow.querySelectorAll('.drag-over').forEach(function (el) { el.classList.remove('drag-over'); });
    }

    // ---- SortableJS ----
    Sortable.create(headerRow, {
      animation: 150,
      forceFallback: true,
      fallbackOnBody: true,
      ghostClass: 'col-dragging',
      filter: '.col-resizer',
      preventOnFilter: false,

      onStart: function () {
        originalOrder = Array.from(headerRow.children).map(function (h) { return h.dataset.field; });
        document.body.classList.add('col-sorting');
        document.body.style.userSelect = 'none';
        startAutoScroll();
      },

      onMove: function (evt) {
        clearDragOver();
        if (evt.related && evt.related !== evt.dragged) {
          evt.related.classList.add('drag-over');
        }
      },

      onEnd: function (evt) {
        document.body.classList.remove('col-sorting');
        document.body.style.userSelect = '';
        stopAutoScroll();
        clearDragOver();

        var newFieldOrder = Array.from(headerRow.children).map(function (h) { return h.dataset.field; });
        var orderChanged = newFieldOrder.some(function (f, i) { return f !== originalOrder[i]; });
        if (!orderChanged) return;

        // 同步 tbody（僅 DOM 操作，不儲存到後端）
        if (tbody) {
          Array.from(tbody.rows).forEach(function (tr) {
            var cells = Array.from(tr.children);
            if (evt.oldIndex < cells.length && evt.newIndex < cells.length) {
              var moved = cells[evt.oldIndex];
              var target = cells[evt.newIndex];
              if (evt.oldIndex < evt.newIndex) {
                target.after(moved);
              } else {
                target.before(moved);
              }
            }
          });
        }

        // 更新 fieldsArray 順序（如有提供）
        if (opts.fieldsArray && Array.isArray(opts.fieldsArray)) {
          var fieldMap = {};
          opts.fieldsArray.forEach(function (f) { fieldMap[f.FieldName || f.fieldName || f] = f; });
          var reordered = newFieldOrder.map(function (fn) { return fieldMap[fn]; }).filter(Boolean);
          opts.fieldsArray.length = 0;
          reordered.forEach(function (f) { opts.fieldsArray.push(f); });
        }

        // 不自動儲存到後端 — 按 F9 才會儲存
      }
    });
  }

  // ---- F9：儲存所有已註冊表格的欄位順序與寬度 ----
  async function saveAllLayouts() {
    if (!_registry.length) return;

    var hasError = false;
    var savedCount = 0;

    for (var i = 0; i < _registry.length; i++) {
      var entry = _registry[i];
      var headerRow = entry.headerRow;
      var tableName = entry.tableName;
      var fetchFn   = entry.fetchFn;

      if (!tableName || !headerRow) continue;

      var ths = Array.from(headerRow.children);

      // 儲存欄位順序
      var fieldOrders = ths.map(function (th, idx) {
        return { fieldName: th.dataset.field, serialNum: idx + 1 };
      });

      try {
        var res = await fetchFn(entry.saveUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ tableName: tableName, fieldOrders: fieldOrders })
        }, '儲存欄位順序…');
        if (!res.ok) throw new Error('儲存順序失敗');
      } catch (e) {
        hasError = true;
      }

      // 儲存欄位寬度
      for (var j = 0; j < ths.length; j++) {
        var th = ths[j];
        var field = th.dataset.field;
        var width = Math.round(th.getBoundingClientRect().width);
        if (!field || !width) continue;
        try {
          await fetch(entry.saveWidthUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ tableName: tableName, fieldName: field, widthPx: width })
          });
        } catch (e) {
          hasError = true;
        }
      }

      savedCount++;

      // 呼叫 onSaved 回呼
      if (typeof entry.onSaved === 'function') {
        try { entry.onSaved(); } catch (e) { /* ignore */ }
      }
    }

    // 顯示結果通知
    if (window.Swal) {
      if (hasError) {
        Swal.fire({ icon: 'error', title: '欄位配置儲存失敗！' });
      } else {
        Swal.fire({ icon: 'success', title: '欄位順序及寬度已儲存', timer: 1500, showConfirmButton: false });
      }
    }
  }

  // ---- 全域 F9 鍵盤事件 ----
  var _f9Bound = false;
  function bindF9() {
    if (_f9Bound) return;
    _f9Bound = true;
    document.addEventListener('keydown', function (e) {
      if (e.key === 'F9') {
        e.preventDefault();
        if (!window.Swal) { saveAllLayouts(); return; }
        Swal.fire({
          title: '是否要儲存欄位順序及寬度？',
          icon: 'question',
          showCancelButton: true,
          confirmButtonText: '是',
          cancelButtonText: '否',
          reverseButtons: false
        }).then(function (result) {
          if (result.isConfirmed) saveAllLayouts();
        });
      }
    });
  }
  bindF9();

  // 匯出為全域函式
  window.initColumnDragSort = initColumnDragSort;
  window.saveColumnLayout = saveAllLayouts;
})();