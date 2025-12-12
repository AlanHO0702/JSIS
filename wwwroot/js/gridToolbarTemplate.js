(function (w) {
  /**
   * 通用按鈕樣板控制：新增/儲存/刪除/取消
   * opts:
   *  name: string
   *  grid: optional editableGrid 實體
   *  tbody, cols, keyFields: 若未提供 customAdd，需提供以便新增空列
   *  btnToggle, btnAdd, btnSave, btnDelete, btnCancel: 按鈕 DOM
   *  newDefaults: object | () => object
   *  beforeAdd/afterAdd/beforeSave/afterSave: hooks，return false 會中止
   *  reload: async fn（取消或儲存後可用來還原資料）
   *  onDelete: async fn for實際刪除
   *  customAdd/customSave/customToggle/onCancelPending/onCancelEdit: 取代預設行為
   */
  function setupGridController(opts) {
    const state = { pendingRow: null, lock: false };

    function setPending(row) {
      state.pendingRow = row;
      state.lock = !!row;
    }

    function cancelPendingRow() {
      if (!state.pendingRow) return false;
      if (opts.onCancelPending) opts.onCancelPending(state.pendingRow);
      state.pendingRow.remove?.();
      setPending(null);
      return true;
    }

    function defaultAdd() {
      if (!opts.tbody || !opts.cols) return null;
      const defaults = typeof opts.newDefaults === 'function' ? opts.newDefaults() : (opts.newDefaults || {});
      const row = w.addRow ? w.addRow(opts.tbody, opts.cols, defaults, opts.keyFields || []) : null;
      return row;
    }

    async function defaultSave() {
      if (!opts.grid) return;
      await w.saveGrid(opts.grid, opts.label || opts.name || '');
      if (opts.reload) await opts.reload();
    }

    function defaultToggle() {
      if (opts.grid && w.toggleEdit) w.toggleEdit(opts.grid, opts.btnToggle);
    }

    opts.btnToggle?.addEventListener('click', () => {
      if (opts.customToggle) opts.customToggle();
      else defaultToggle();
    });

    opts.btnAdd?.addEventListener('click', () => {
      if (opts.beforeAdd && opts.beforeAdd() === false) return;
      cancelPendingRow();
      const row = opts.customAdd ? opts.customAdd() : defaultAdd();
      if (row === false) return;
      if (row?.querySelector) w.showEditRow?.(row);
      setPending(row || true);
      if (opts.afterAdd) opts.afterAdd(row);
    });

    opts.btnSave?.addEventListener('click', async () => {
      if (opts.beforeSave && opts.beforeSave() === false) return;
      if (opts.customSave) await opts.customSave();
      else await defaultSave();
      setPending(null);
      if (!opts.customToggle && opts.grid && w.toggleEdit) w.toggleEdit(opts.grid, opts.btnToggle, false);
      if (opts.afterSave) opts.afterSave();
    });

    opts.btnCancel?.addEventListener('click', async () => {
      if (cancelPendingRow()) return;
      if (opts.onCancelEdit) {
        await opts.onCancelEdit();
      } else {
        if (opts.grid && w.toggleEdit) w.toggleEdit(opts.grid, opts.btnToggle, false);
        if (opts.reload) await opts.reload();
      }
      setPending(null);
    });

    opts.btnDelete?.addEventListener('click', async () => {
      if (!opts.onDelete) return;
      await opts.onDelete();
    });

    return { setPending, state };
  }

  w.createGridController = setupGridController;
  w.setupGridController = setupGridController;
})(window);
