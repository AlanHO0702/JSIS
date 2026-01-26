// /wwwroot/js/editableGrid.js
// 通用 table 可編輯/儲存/刪除功能
(function () {

  window.makeEditableGrid = function (options) {

    const wrapper   = options.wrapper;
    const table     = options.table;
    const tableName = options.tableName || "";
    const keyFields = Array.isArray(options.keyFields)
      ? options.keyFields.map(x => String(x))
      : [];
    const keyFieldSet = new Set(keyFields.map(k => k.toLowerCase()));
    const saveUrl   = options.saveUrl || "/api/CommonTable/SaveTableChanges";

    if (!wrapper || !table) {
      console.warn("makeEditableGrid: wrapper or table not set.");
      return {
        isEdit      : () => false,
        toggleEdit  : () => {},
        saveChanges : async () => ({ ok:false, skipped:true })
      };
    }

    // ===== 狀態 =====
    let editing = false;
    function isEdit() { return editing; }

    // ===== 編輯模式切換 =====
    function toggleEdit(toEdit) {
      editing = !!toEdit;
      wrapper.classList.toggle("edit-mode", editing);

      table.querySelectorAll("tbody tr").forEach(tr => {
        tr.querySelectorAll("td").forEach(td => {

          const span = td.querySelector(".cell-view");
          const inp  = td.querySelector(".cell-edit");
          if (!span || !inp) return;

          if (editing) {

            // ★ 進入編輯模式時，從 viewChk 同步 checked 狀態到 inp（確保狀態一致）
            if (inp.type === "checkbox") {
              const viewChk = span.querySelector('input[type="checkbox"]');
              if (viewChk) {
                inp.checked = viewChk.checked;
                inp.value = viewChk.checked ? "1" : "0";
              }
            }

            // 記錄進入編輯時的 defaultValue
            inp.defaultValue = inp.type === "checkbox"
              ? (inp.checked ? "1" : "0")
              : inp.value;
            if (inp.type === "checkbox") {
              inp.defaultChecked = inp.checked;
            }

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

            // 退出編輯模式
            if (inp.type === "checkbox") {
              let viewChk = span.querySelector('input[type="checkbox"]');
              if (!viewChk) {
                viewChk = document.createElement("input");
                viewChk.type = "checkbox";
                viewChk.disabled = true;
                viewChk.tabIndex = -1;
                viewChk.className = "form-check-input";
                span.innerHTML = "";
                span.appendChild(viewChk);
              }
              viewChk.checked = !!inp.checked;
              inp.value = inp.checked ? "1" : "0";
            } else {
              span.textContent = inp.value;
            }
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

        // ===== 刪除列：只送 KeyFields + __delete =====
        if (tr.dataset.state === "deleted") {
          const rowDel = {};
          // 先從 cell-edit 收集（含 DOM 上的 hidden）
          tr.querySelectorAll(".cell-edit").forEach(inp => {
            if (!inp.name) return;
            if (keyFieldSet.has((inp.name || "").toLowerCase())) {
              rowDel[inp.name] = inp.type === "checkbox" ? (inp.checked ? "1" : "0") : (inp.value ?? "");
            }
          });
          // 再檢查 hidden PK / FK
          tr.querySelectorAll(".mmd-pk-hidden, .mmd-fk-hidden").forEach(inp => {
            if (!inp.name) return;
            if (rowDel[inp.name] === undefined || rowDel[inp.name] === "") rowDel[inp.name] = inp.value ?? "";
          });
          // 補齊 KeyFields
          keyFieldSet.forEach(k => {
            const has = Object.keys(rowDel).some(n => (n || "").toLowerCase() === k);
            if (has) return;
            const inp = Array.from(tr.querySelectorAll('.cell-edit')).find(i => (i.name || '').toLowerCase() === k);
            const span = inp?.previousElementSibling;
            const name = inp?.name || k;
            const val = span?.textContent?.trim() || inp?.value || inp?.dataset?.raw || inp?.defaultValue || "";
            rowDel[name] = val;
          });

          rowDel.__delete = true;
          list.push(rowDel);
          return;
        }

        let hasDiff = tr.dataset.state === "added";

        tr.querySelectorAll(".cell-edit").forEach(inp => {
          const dataType = inp.dataset.type || "";
          const oldValRaw = inp.type === "checkbox"
            ? (inp.defaultChecked ? "1" : "0")
            : (inp.defaultValue ?? "");
          const newVal = (() => {
            if (inp.type === "checkbox") return inp.checked ? "1" : "0";
            if (dataType === "number") {
              const raw = (inp.value ?? "").trim();
              return raw === "" ? "0" : raw;
            }
            return inp.value ?? "";
          })();
          const oldVal = (dataType === "number" && (oldValRaw === "" || oldValRaw == null)) ? null : oldValRaw;
          const isReadonly = inp.dataset.readonly === "1";
          const isKey = keyFieldSet.has((inp.name || "").toLowerCase());
          if (!isReadonly && String(oldVal) !== String(newVal)) hasDiff = true;
          if (!isKey && isReadonly) return;
        });

        if (!hasDiff) return;
        const rowAll = {};

        // 編輯欄位
        tr.querySelectorAll(".cell-edit").forEach(inp => {
            if (!inp.name) return;
            const dataType = inp.dataset.type || "";
            if (inp.type === "checkbox") {
              rowAll[inp.name] = inp.checked ? "1" : "0";
            } else if (dataType === "number") {
              const raw = (inp.value ?? "").trim();
              rowAll[inp.name] = raw === "" ? "0" : raw;
            } else {
              rowAll[inp.name] = inp.value ?? "";
            }
        });

        // 補齊 keyFields
        keyFieldSet.forEach(k => {
          const inp = Array.from(tr.querySelectorAll('.cell-edit')).find(i => (i.name || '').toLowerCase() === k);
          const span = inp?.previousElementSibling;
          if (inp && (rowAll[inp.name] === "" || rowAll[inp.name] == null)) {
            const val = span?.textContent?.trim() || inp.value || inp.dataset.raw || inp.defaultValue || "";
            rowAll[inp.name] = val;
          }
        });

        // PK 隱藏欄位
        tr.querySelectorAll(".mmd-pk-hidden").forEach(inp => {
          if (!inp.name) return;
          if (rowAll[inp.name] === undefined || rowAll[inp.name] === "") {
            rowAll[inp.name] = inp.value ?? "";
          }
        });

        // FK 隱藏欄位 (KeyMap)
        tr.querySelectorAll(".mmd-fk-hidden").forEach(inp => {
          if (!inp.name) return;
          if (rowAll[inp.name] === undefined || rowAll[inp.name] === "") {
            rowAll[inp.name] = inp.value ?? "";
          }
        });

        list.push(rowAll);
      });

      return list;
    }

    // ===== 儲存 =====
    async function saveChanges() {

      if (!tableName) {
        return { ok:false, skipped:true, text:"tableName not set" };
      }

      const changes = collectChanges();
      if (changes.length === 0) {
        return { ok:true, skipped:true, text:"no changes" };
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
        console.error("saveChanges error:", err);
        return { ok:false, skipped:false, text:String(err) };
      }

      let json = null;
      try { json = txt ? JSON.parse(txt) : null; } catch {}

      if (!resp.ok || (json && json.success === false)) {
        const msg =
          (json && (json.message || json.error)) ||
          txt || "save failed";
        console.error("saveChanges error:", msg);
        return { ok:false, skipped:false, text:msg, raw:json };
      }

      // 儲存成功，更新 defaultValue
      table.querySelectorAll(".cell-edit").forEach(inp => {
        inp.defaultValue = inp.type === "checkbox"
          ? (inp.checked ? "1" : "0")
          : (inp.value ?? "");
        if (inp.type === "checkbox") {
          inp.defaultChecked = inp.checked;
        }
      });

      // 刪除列：移除 DOM
      table.querySelectorAll('tbody tr[data-state="deleted"]').forEach(tr => tr.remove());
      // 新增列：移除標記
      table.querySelectorAll('tbody tr[data-state="added"]').forEach(tr => {
        delete tr.dataset.state;
        tr.classList.remove("table-warning");
      });

      return { ok:true, skipped:false, text:"OK", raw:json };
    }

    // ===== 刪除選中的列 =====
    async function deleteRows() {
      if (!editing) return { ok: false, text: "請先進入編輯模式" };

      const selected = table.querySelector("tbody tr.selected");
      if (!selected) return { ok: false, text: "請先選擇要刪除的資料列" };

      // 如果是新增的列（還沒存到資料庫），直接移除
      if (selected.dataset.state === "added") {
        selected.remove();
        return { ok: true, text: "已移除未儲存的新增列" };
      }

      // 收集 key 欄位值
      const rowDel = {};
      selected.querySelectorAll(".cell-edit").forEach(inp => {
        if (!inp.name) return;
        if (keyFieldSet.has((inp.name || "").toLowerCase())) {
          rowDel[inp.name] = inp.type === "checkbox" ? (inp.checked ? "1" : "0") : (inp.value ?? "");
        }
      });
      // 也檢查 hidden PK / FK
      selected.querySelectorAll(".mmd-pk-hidden, .mmd-fk-hidden").forEach(inp => {
        if (!inp.name) return;
        if (rowDel[inp.name] === undefined || rowDel[inp.name] === "") rowDel[inp.name] = inp.value ?? "";
      });
      rowDel.__delete = true;

      if (!tableName) {
        return { ok: false, text: "tableName 未設定" };
      }

      // 直接呼叫 API 刪除
      const payload = { TableName: tableName, Data: [rowDel] };
      if (keyFields.length) payload.KeyFields = keyFields;

      let resp, txt;
      try {
        resp = await fetch(saveUrl, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(payload)
        });
        txt = await resp.text();
      } catch (err) {
        console.error("deleteRows error:", err);
        return { ok: false, text: String(err) };
      }

      let json = null;
      try { json = txt ? JSON.parse(txt) : null; } catch {}

      if (!resp.ok || (json && json.success === false)) {
        const msg = (json && (json.message || json.error)) || txt || "刪除失敗";
        return { ok: false, text: msg };
      }

      // 刪除成功，從 DOM 移除該列
      const nextRow = selected.nextElementSibling || selected.previousElementSibling;
      selected.remove();

      // 選擇下一列或上一列
      if (nextRow && nextRow.tagName === "TR") {
        table.querySelectorAll("tbody tr").forEach(tr => tr.classList.remove("selected"));
        nextRow.classList.add("selected");
      }

      return { ok: true, text: "刪除成功" };
    }

    // ===== 新增一列（預設不做事，讓 masterDetailTemplate.js 處理） =====
    function insertRows() {
      return { ok: true, skipped: true, text: "由外部處理" };
    }

    // ===== 匯出 API =====
    return {
      isEdit,
      toggleEdit,
      saveChanges,
      deleteRows,
      insertRows
    };
  };

})();
