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
       saveUrl:      '/api/TableFieldLayout/SaveSerialOrder',      // 儲存 API (可選)
       fetchFn:      window.busyFetch || fetch                     // fetch 函式 (可選)
     });
   ============================================================ */

(function () {
  'use strict';

  const EDGE_ZONE = 60;
  const SCROLL_SPEED_MAX = 18;

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
    const saveUrl    = opts.saveUrl || '/api/TableFieldLayout/SaveSerialOrder';
    const fetchFn    = opts.fetchFn || (window.busyFetch ? window.busyFetch : fetch.bind(window));

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

    // ---- 確保每個 th 都有 drag-handle ----
    Array.from(headerRow.children).forEach(function (th) {
      if (th.querySelector('.drag-handle')) return;
      var handle = document.createElement('span');
      handle.className = 'drag-handle';
      handle.textContent = '☰';
      th.insertBefore(handle, th.firstChild);
    });

    // ---- SortableJS ----
    Sortable.create(headerRow, {
      animation: 150,
      handle: '.drag-handle',
      ghostClass: 'col-dragging',
      filter: '.col-resizer',
      preventOnFilter: false,

      onStart: function () {
        originalOrder = Array.from(headerRow.children).map(function (h) { return h.dataset.field; });
        startAutoScroll();
      },

      onMove: function (evt) {
        clearDragOver();
        if (evt.related && evt.related !== evt.dragged) {
          evt.related.classList.add('drag-over');
        }
      },

      onEnd: function (evt) {
        stopAutoScroll();
        clearDragOver();

        var newFieldOrder = Array.from(headerRow.children).map(function (h) { return h.dataset.field; });
        var orderChanged = newFieldOrder.some(function (f, i) { return f !== originalOrder[i]; });
        if (!orderChanged) return;

        // 同步 tbody
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

        // 儲存到後端
        var newOrder = newFieldOrder.map(function (field, index) {
          return { fieldName: field, serialNum: index + 1 };
        });

        var fn = fetchFn;
        fn(saveUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ tableName: tableName, fieldOrders: newOrder })
        }, '儲存欄位順序…')
          .then(function (res) { if (!res.ok) throw new Error('更新失敗'); return res.json(); })
          .then(function () {
            if (typeof opts.onSaved === 'function') opts.onSaved();
          })
          .catch(function () {
            if (window.Swal) {
              Swal.fire({ icon: 'error', title: '欄位順序儲存失敗！' });
            }
          });
      }
    });
  }

  // 匯出為全域函式
  window.initColumnDragSort = initColumnDragSort;
})();