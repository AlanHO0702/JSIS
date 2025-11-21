// /wwwroot/js/editableGrid.js
// 讓任何一張表 (table + wrapper + tableName + keyFields) 都能套用 修改/保留/儲存 功能
(function () {
  /**
   * options:
   *  - wrapper: 外層 DOM (用來掛 edit-mode)
   *  - table:   <table> DOM
   *  - tableName: 儲存時要送給後端的 TableName
   *  - keyFields: [ 'PaperType', ... ] 送給後端的 KeyFields (選用)
   *  - saveUrl: 預設 /api/CommonTable/SaveTableChanges
   */
  window.makeEditableGrid = function (options) {
    const wrapper   = options.wrapper;
    const table     = options.table;
    const tableName = options.tableName || '';
    const keyFields = Array.isArray(options.keyFields)
      ? options.keyFields.map(s => String(s))
      : [];
    const saveUrl   = options.saveUrl || '/api/CommonTable/SaveTableChanges';

    if (!table || !wrapper) {
      console.warn('makeEditableGrid: wrapper 或 table 沒傳好', { wrapper, table });
      return {
        isEdit: () => false,
        toggleEdit: () => {},
        saveChanges: async () => ({ ok: false, skipped: true, text: 'wrapper/table not found' })
      };
    }

    let editing = false;

    function isEdit() {
      return editing;
    }

    function toggleEdit(toEdit) {
      editing = !!toEdit;

      wrapper.classList.toggle('edit-mode', editing);

      table.querySelectorAll('tbody tr').forEach(tr => {
        tr.querySelectorAll('td').forEach(td => {
          const span = td.querySelector('.cell-view');
          const inp  = td.querySelector('.cell-edit');
          if (!span || !inp) return;

          if (editing) {
            // 進入編輯模式前，記住 defaultValue
            if (inp.defaultValue === undefined || inp.defaultValue === null) {
              inp.defaultValue = inp.value;
            } else {
              // 再進一次編輯模式時，以目前值為新基準
              inp.defaultValue = inp.value;
            }

            span.classList.add('d-none');
            inp.classList.remove('d-none');

            if (inp.dataset.readonly === '1') {
              inp.classList.add('readonly-cell');
              inp.setAttribute('readonly', 'readonly');
              //inp.setAttribute('tabindex', '-1');
            } else {
              inp.classList.remove('readonly-cell');
              inp.removeAttribute('readonly');
              inp.removeAttribute('tabindex');
            }
          } else {
            // 回到保留模式：把 input 值顯示回 span
            span.textContent = inp.value;
            span.classList.remove('d-none');
            inp.classList.add('d-none');
          }
        });
      });
    }

    function collectChanges() {
      const list = [];
      table.querySelectorAll('tbody tr').forEach(tr => {
        let hasDiff = false;

        tr.querySelectorAll('.cell-edit').forEach(inp => {
          const oldVal = inp.defaultValue ?? '';
          const newVal = inp.value ?? '';
          if (inp.dataset.readonly === '1') return;
          if (newVal !== oldVal) hasDiff = true;
        });

        if (!hasDiff) return;

        const rowAll = {};
        tr.querySelectorAll('.cell-edit').forEach(inp => {
          const name = inp.name;
          if (!name) return;
          rowAll[name] = inp.value ?? '';
        });
        list.push(rowAll);
      });
      return list;
    }

    async function saveChanges() {
      if (!tableName) {
        console.warn('makeEditableGrid: 沒有設定 tableName，略過儲存');
        return { ok: false, skipped: true, text: '未設定 TableName' };
      }

      const changes = collectChanges();
      if (changes.length === 0) {
        return { ok: true, skipped: true, text: '沒有變更' };
      }

      const payload = {
        TableName: tableName,
        Data:      changes
      };
      if (keyFields.length) {
        payload.KeyFields = keyFields;   // ⭐ 關鍵：把 keyFields 傳給後端
      }

      let resp, text;
      try {
        resp = await fetch(saveUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
        text = await resp.text();
      } catch (err) {
        console.error('makeEditableGrid saveChanges error:', err);
        return { ok: false, skipped: false, text: String(err) };
      }

      let json = null;
      try {
        json = text ? JSON.parse(text) : null;
      } catch {
        // 就當純文字錯誤訊息
      }

      if (!resp.ok || (json && json.success === false)) {
        const msg =
          (json && (json.message || json.error)) ||
          text ||
          '儲存失敗';
        console.error('makeEditableGrid saveChanges fail:', msg, json);
        return { ok: false, skipped: false, text: msg, raw: json };
      }

      // 儲存成功 → 把現在的值變成新的 defaultValue
      table.querySelectorAll('.cell-edit').forEach(inp => {
        inp.defaultValue = inp.value;
      });

      return { ok: true, skipped: false, text: 'OK', raw: json };
    }

    return {
      isEdit,
      toggleEdit,
      saveChanges
    };
  };
})();
