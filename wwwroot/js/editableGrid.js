// /wwwroot/js/editableGrid.js
// 霈?table ?舀蝪⊥???蝺刻摩 / 瑼Ｚ? / ?脣? ?
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

    // ===== ???=====
    let editing = false;
    function isEdit() { return editing; }

    // ===== 蝺刻摩 / 瑼Ｚ? ?? =====
    function toggleEdit(toEdit) {
      editing = !!toEdit;
      wrapper.classList.toggle("edit-mode", editing);

      table.querySelectorAll("tbody tr").forEach(tr => {
        tr.querySelectorAll("td").forEach(td => {

          const span = td.querySelector(".cell-view");
          const inp  = td.querySelector(".cell-edit");
          if (!span || !inp) return;

          if (editing) {

            // ?脣蝺刻摩甈?嚗???defaultValue
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

            // ?憿舐內甈?
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

    // ===== ?園?撌桃 =====
    function collectChanges() {

      const list = [];

      table.querySelectorAll("tbody tr").forEach(tr => {

        // ===== ?芷???芷?KeyFields + __delete =====
        if (tr.dataset.state === "deleted") {
          const rowDel = {};
          // ??閰血? cell-edit ???踹? DOM ??憛?hidden嚗?
          tr.querySelectorAll(".cell-edit").forEach(inp => {
            if (!inp.name) return;
            if (keyFieldSet.has((inp.name || "").toLowerCase())) {
              rowDel[inp.name] = inp.type === "checkbox" ? (inp.checked ? "1" : "0") : (inp.value ?? "");
            }
          });
          // ?? hidden PK / FK
          tr.querySelectorAll(".mmd-pk-hidden, .mmd-fk-hidden").forEach(inp => {
            if (!inp.name) return;
            if (rowDel[inp.name] === undefined || rowDel[inp.name] === "") rowDel[inp.name] = inp.value ?? "";
          });
          // ?敺? KeyFields嚗?撩嚗?
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

        // 蝺刻摩甈?
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

        // 蝣箔??菜?雿?摰葆?潘??亥撓?交?蝛綽??岫敺???span ??data-raw ??
        keyFieldSet.forEach(k => {
          const inp = Array.from(tr.querySelectorAll('.cell-edit')).find(i => (i.name || '').toLowerCase() === k);
          const span = inp?.previousElementSibling;
          if (inp && (rowAll[inp.name] === "" || rowAll[inp.name] == null)) {
            const val = span?.textContent?.trim() || inp.value || inp.dataset.raw || inp.defaultValue || "";
            rowAll[inp.name] = val;
          }
        });

        // PK ?梯?甈?
        tr.querySelectorAll(".mmd-pk-hidden").forEach(inp => {
          if (!inp.name) return;
          if (rowAll[inp.name] === undefined || rowAll[inp.name] === "") {
            rowAll[inp.name] = inp.value ?? "";
          }
        });

        // FK ?梯?甈? (KeyMap)
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

    // ===== ?脣? =====
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

      // ??敺??郊 defaultValue
      table.querySelectorAll(".cell-edit").forEach(inp => {
        inp.defaultValue = inp.type === "checkbox"
          ? (inp.checked ? "1" : "0")
          : (inp.value ?? "");
        if (inp.type === "checkbox") {
          inp.defaultChecked = inp.checked;
        }
      });

      // ?芷????敺宏??
      table.querySelectorAll('tbody tr[data-state="deleted"]').forEach(tr => tr.remove());
      // ?啣?????敺圾?斤???閮?
      table.querySelectorAll('tbody tr[data-state="added"]').forEach(tr => {
        delete tr.dataset.state;
        tr.classList.remove("table-warning");
      });

      return { ok:true, skipped:false, text:"OK", raw:json };
    }

    // ===== 撠? API =====
    return {
      isEdit,
      toggleEdit,
      saveChanges
    };
  };

})();



