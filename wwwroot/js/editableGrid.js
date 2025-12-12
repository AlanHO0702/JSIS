// /wwwroot/js/editableGrid.js
// 讓 table 支援簡易的 編輯 / 檢視 / 儲存 功能
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
      console.warn("makeEditableGrid: wrapper 或 table 未設定");
      return {
        isEdit      : () => false,
        toggleEdit  : () => {},
        saveChanges : async () => ({ ok:false, skipped:true })
      };
    }

    // ===== 狀態 =====
    let editing = false;
    function isEdit() { return editing; }

    // ===== 編輯 / 檢視 切換 =====
    function toggleEdit(toEdit) {
      editing = !!toEdit;
      wrapper.classList.toggle("edit-mode", editing);

      table.querySelectorAll("tbody tr").forEach(tr => {
        tr.querySelectorAll("td").forEach(td => {

          const span = td.querySelector(".cell-view");
          const inp  = td.querySelector(".cell-edit");
          if (!span || !inp) return;

          if (editing) {

            // 進入編輯欄位，記錄 defaultValue
            inp.defaultValue = inp.type === "checkbox"
              ? (inp.checked ? "1" : "0")
              : inp.value;

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

            // 回到顯示欄位
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

    // ===== 收集差異 =====
    function collectChanges() {

      const list = [];

      table.querySelectorAll("tbody tr").forEach(tr => {

        let hasDiff = tr.dataset.state === "added";

        tr.querySelectorAll(".cell-edit").forEach(inp => {
          const dataType = inp.dataset.type || "";
          const oldValRaw = inp.defaultValue ?? "";
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

        // 確保鍵欄位一定帶值：若輸入框空，嘗試從同格 span 或 data-raw 取值
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
        return { ok:false, skipped:true, text:"未設定 TableName" };
      }

      const changes = collectChanges();
      if (changes.length === 0) {
        return { ok:true, skipped:true, text:"無異動" };
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

      // 成功後，同步 defaultValue
      table.querySelectorAll(".cell-edit").forEach(inp => {
        inp.defaultValue = inp.type === "checkbox"
          ? (inp.checked ? "1" : "0")
          : (inp.value ?? "");
      });

      return { ok:true, skipped:false, text:"OK", raw:json };
    }

    // ===== 對外 API =====
    return {
      isEdit,
      toggleEdit,
      saveChanges
    };
  };

})();
