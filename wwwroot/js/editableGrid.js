// /wwwroot/js/editableGrid.js
// 讓任何 table 都能套用 修改 / 保留 / 儲存 功能
(function () {

  window.makeEditableGrid = function (options) {

    const wrapper   = options.wrapper;
    const table     = options.table;
    const tableName = options.tableName || "";
    const keyFields = Array.isArray(options.keyFields)
      ? options.keyFields.map(x => String(x))
      : [];
    const saveUrl   = options.saveUrl || "/api/CommonTable/SaveTableChanges";

    if (!wrapper || !table) {
      console.warn("makeEditableGrid: wrapper 或 table 未設定");
      return {
        isEdit      : () => false,
        toggleEdit  : () => {},
        saveChanges : async () => ({ ok:false, skipped:true })
      };
    }

    // ===== 編輯模式狀態 =====
    let editing = false;
    function isEdit() { return editing; }

    // ===== 編輯 / 保留 模式切換 =====
    function toggleEdit(toEdit) {
      editing = !!toEdit;
      wrapper.classList.toggle("edit-mode", editing);

      table.querySelectorAll("tbody tr").forEach(tr => {
        tr.querySelectorAll("td").forEach(td => {

          const span = td.querySelector(".cell-view");
          const inp  = td.querySelector(".cell-edit");
          if (!span || !inp) return;

          if (editing) {

            // 進入編輯模式，紀錄 defaultValue
            inp.defaultValue = inp.value;

            span.classList.add("d-none");
            inp.classList.remove("d-none");

            if (inp.dataset.readonly === "1") {
              inp.classList.add("readonly-cell");
              inp.setAttribute("readonly", "readonly");
              span.classList.add("readonly-cell");
            } else {
              inp.classList.remove("readonly-cell");
              inp.removeAttribute("readonly");
              inp.removeAttribute("tabindex");
            }

          } else {

            // 回到顯示模式
            span.textContent = inp.value;
            span.classList.remove("d-none");
            inp.classList.add("d-none");

          }
        });
      });
    }

    // ===== 收集變更 =====
    function collectChanges() {

      const list = [];

      table.querySelectorAll("tbody tr").forEach(tr => {

        let hasDiff = false;

        tr.querySelectorAll(".cell-edit").forEach(inp => {
          const oldVal = inp.defaultValue ?? "";
          const newVal = inp.value ?? "";
          if (inp.dataset.readonly === "1") return;
          if (oldVal !== newVal) hasDiff = true;
        });

        if (!hasDiff) return;

        const rowAll = {};

        // 編輯欄位
        tr.querySelectorAll(".cell-edit").forEach(inp => {
          if (!inp.name) return;
          rowAll[inp.name] = inp.value ?? "";
        });

        // PK 欄位
        tr.querySelectorAll(".mmd-pk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowAll[inp.name] = inp.value ?? "";
        });

        // FK 欄位（KeyMap）
        tr.querySelectorAll(".mmd-fk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowAll[inp.name] = inp.value ?? "";
        });

        list.push(rowAll);
      });

      return list;
    }

    // ===== 儲存 =====
    async function saveChanges() {

      if (!tableName) {
        return { ok:false, skipped:true, text:"未設定 TableName" };
      }

      const changes = collectChanges();
      if (changes.length === 0) {
        return { ok:true, skipped:true, text:"沒有變更" };
      }

      const payload = { TableName: tableName, Data: changes };
      if (keyFields.length) payload.KeyFields = keyFields;

      let resp, txt;
      try {
        resp = await fetch(saveUrl, {
          method: "POST",
          headers: { "Content-Type":"application/json" },
          body: JSON.stringify(payload)
        });
        txt = await resp.text();
      } catch (err) {
        console.error("saveChanges 發生錯誤:", err);
        return { ok:false, skipped:false, text:String(err) };
      }

      let json = null;
      try { json = txt ? JSON.parse(txt) : null; } catch {}

      if (!resp.ok || (json && json.success === false)) {
        const msg =
          (json && (json.message || json.error)) ||
          txt ||
          "儲存失敗";
        console.error("saveChanges fail:", msg);
        return { ok:false, skipped:false, text:msg, raw:json };
      }

      // 成功 → 更新 defaultValue
      table.querySelectorAll(".cell-edit").forEach(inp => {
        inp.defaultValue = inp.value;
      });

      return { ok:true, skipped:false, text:"OK", raw:json };
    }

    // ===== 對外提供 =====
    return {
      isEdit,
      toggleEdit,
      saveChanges
    };
  };

})();
