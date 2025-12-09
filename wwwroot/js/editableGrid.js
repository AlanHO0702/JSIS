// /wwwroot/js/editableGrid.js
// 讓任何 table 都能套用 修改 / 保留 / 儲存 / 新增 / 刪除 功能
(function () {

  window.makeEditableGrid = function (options) {

    const wrapper   = options.wrapper;
    const table     = options.table;
    const tableName = options.tableName || "";
    const keyFields = Array.isArray(options.keyFields)
      ? options.keyFields.map(x => String(x))
      : [];
    const keyFieldSet = new Set(keyFields.map(k => k.toLowerCase()));
    const saveUrl   = options.saveUrl   || "/api/CommonTable/SaveTableChanges";
    const insertUrl = options.insertUrl || "/api/CommonTable/InsertTableRows";
    const deleteUrl = options.deleteUrl || "/api/CommonTable/DeleteTableRows";

    if (!wrapper || !table) {
      console.warn("makeEditableGrid: wrapper 或 table 未設定");
      return {
        isEdit      : () => false,
        toggleEdit  : () => {},
        saveChanges : async () => ({ ok:false, skipped:true }),
        insertRows  : async () => ({ ok:false, skipped:true }),
        deleteRows  : async () => ({ ok:false, skipped:true })
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
          const oldVal = inp.defaultValue ?? "";
          const newVal = inp.type === "checkbox"
            ? (inp.checked ? "1" : "0")
            : (inp.value ?? "");
          const isReadonly = inp.dataset.readonly === "1";
          const isKey = keyFieldSet.has((inp.name || "").toLowerCase());
          if (!isReadonly && oldVal !== newVal) hasDiff = true;
          if (!isKey && isReadonly) return;
        });

        if (!hasDiff) return;

        const rowAll = {};

        // 編輯欄位
        tr.querySelectorAll(".cell-edit").forEach(inp => {
            if (!inp.name) return;
            rowAll[inp.name] = inp.type === "checkbox"
              ? (inp.checked ? "1" : "0")
              : (inp.value ?? "");
        });

        // 確保鍵欄位一定帶值：若輸入框空，嘗試從同格 span 或 data-raw 取值
        keyFieldSet.forEach(k => {
          const inp = Array.from(tr.querySelectorAll('.cell-edit')).find(i => (i.name || '').toLowerCase() === k);
          const span = inp?.previousElementSibling;
          if (inp && (rowAll[inp.name] === "" || rowAll[inp.name] == null)) {
            const val = span?.textContent?.trim() || inp.dataset.raw || "";
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

    // 收集「目前被選取的那一筆」資料
    function collectSelectedData(operation) {
      const list = [];

      if (operation === "insert") {
        const rowObj = {};
        
        // 根據第一筆列的結構產生空資料
        const firstTr = table.querySelector("tbody tr");
        if (!firstTr) return [{}]; // 表格為空，回傳空物件
        
        // 收集可編輯欄位名稱（設為空字串）
        firstTr.querySelectorAll(".cell-edit").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = "";
        });
        
        // 收集隱藏的 PK 欄位（設為空字串）
        firstTr.querySelectorAll(".mmd-pk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = "";
        });
        
        // 收集隱藏的 FK / KeyMap 欄位（設為空字串）
        firstTr.querySelectorAll(".mmd-fk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = "";
        });
        
        list.push(rowObj);
        return list;
      } else {
        // 嘗試以 tr 上的狀態判定優先
        let tr = table.querySelector("tbody tr.selected, tbody tr[data-selected='1']");

        // 若沒有以 tr 標記，嘗試找 checked 的選取 input（radio/checkbox）
        if (!tr) {
          const selInput = table.querySelector("tbody input[type=radio]:checked, tbody input[type=checkbox]:checked");
          if (selInput) tr = selInput.closest("tr");
        }

        if (!tr) {
          return null; // 找不到選取列則回傳 null
        }

        const rowObj = {};

        // 收集可編輯欄位的值（與 collectChanges 行為一致，跳過沒有 name 的欄位）
        tr.querySelectorAll(".cell-edit").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = inp.value ?? "";
        });

        // 收集隱藏的 PK 欄位
        tr.querySelectorAll(".mmd-pk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = inp.value ?? "";
        });

        // 收集隱藏的 FK / KeyMap 欄位
        tr.querySelectorAll(".mmd-fk-hidden").forEach(inp => {
          if (!inp.name) return;
          rowObj[inp.name] = inp.value ?? "";
        });
        list.push(rowObj);

        return list;
      }
    }

    // ===== 儲存 / 新增 / 刪除 共用 helper =====
    async function doPost(url, payload) {
      let resp, txt;
      try {
      resp = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload)
      });
      txt = await resp.text();
      } catch (err) {
      console.error("doPost 發生錯誤:", err);
      return { ok: false, skipped: false, text: String(err), raw: null, resp: null };
      }

      let json = null;
      try { json = txt ? JSON.parse(txt) : null; } catch {}

      if (!resp.ok || (json && json.success === false)) {
      const msg =
        (json && (json.message || json.error)) ||
        txt ||
        "操作失敗";
      console.error("doPost fail:", msg);
      return { ok: false, skipped: false, text: msg, raw: json, resp };
      }

      return { ok: true, skipped: false, text: "OK", raw: json, resp };
    }

    // ===== 儲存 =====
    async function saveChanges() {
      if (!tableName) {
        return { ok:false, skipped:true, text:"未設定 TableName" };
      }
      
      if (!keyFields.length) {
        return { ok:false, skipped:true, text:"未設定 KeyFields，無法儲存" };
      }

      const changes = collectChanges();
      if (changes.length === 0) {
        return { ok:true, skipped:true, text:"沒有變更" };
      }
      
      const payload = { TableName: tableName, Data: changes, KeyFields: keyFields };

      const result = await doPost(saveUrl, payload);
      if (!result.ok) return result;

      // 成功後，同步 defaultValue
      table.querySelectorAll(".cell-edit").forEach(inp => {
        inp.defaultValue = inp.type === "checkbox"
          ? (inp.checked ? "1" : "0")
          : (inp.value ?? "");
      });

      return result;
    }

    // ===== 新增 =====
    // rows: Array of objects { colName: value, ... }
    async function insertRows() {
      if (!tableName) {
        return { ok:false, skipped:true, text:"未設定 TableName" };
      }
      
      // 預設插入一筆空資料
      const insertData = collectSelectedData("insert");
      
      const payload = { TableName: tableName, Data: insertData };
      if (keyFields.length) payload.KeyFields = keyFields;

      const result = await doPost(insertUrl, payload);
      if (!result.ok) return result;

      // 成功 → 把 table 中的 input defaultValue 同步（若 UI 有對應 row）
      table.querySelectorAll(".cell-edit").forEach(inp => {
      inp.defaultValue = inp.value;
      });

      return result;
    }

    // ===== 刪除 =====
    // keys: Array of objects，格式為 PK 欄位名稱 => 值 (須與 keyFields 一致)
    async function deleteRows() {
      if (!tableName) {
        return { ok:false, skipped:true, text:"未設定 TableName" };
      }

      if (!keyFields.length) {
        return { ok:false, skipped:true, text:"未設定 KeyFields，無法刪除" };
      }
      
      const selectedData = collectSelectedData("delete");
      // 傳給後端的 Payload 仍維持 Data 字段以保持一致性
      const payload = { TableName: tableName, Data: selectedData, KeyFields: keyFields };

      const result = await doPost(deleteUrl, payload);
      if (!result.ok) return result;
      
      return result;
    }

    // ===== 對外 API =====
    return {
      isEdit,
      toggleEdit,
      saveChanges,
      insertRows,
      deleteRows
    };
  };

})();