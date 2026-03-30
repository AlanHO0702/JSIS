(function () {
  if (window.OrderTailCancelHelper) return;

  const readPaperNum = (ctx) => {
    const v = (ctx && ctx.paperNum ? ctx.paperNum : (window._paperNum || "")).toString().trim();
    if (v) return v;
    if (typeof window.getPaperNum === "function") {
      try { return (window.getPaperNum() || "").toString().trim(); } catch { }
    }
    return "";
  };

  const withJwtHeaders = (init) => {
    const base = init || {};
    const headers = Object.assign({}, base.headers || {});
    const jwt = localStorage.getItem("jwtId");
    if (jwt) headers["X-JWTID"] = jwt;
    return Object.assign({}, base, { headers });
  };

  const readSelectedItem = () => {
    const active = document.activeElement && document.activeElement.closest ? document.activeElement.closest(".erp-table tbody tr") : null;
    const row = document.querySelector(".erp-table tbody tr.row-selected") || active;
    if (!row) return "";
    const dataItem = (row.dataset && row.dataset.item ? row.dataset.item : "").toString().trim();
    if (dataItem) return dataItem;
    const itemInput = row.querySelector("input[data-field='Item'], input[data-field='item']");
    return (itemInput && (itemInput.value || itemInput.getAttribute("value")) ? (itemInput.value || itemInput.getAttribute("value")) : "").toString().trim();
  };

  const refreshAfterSuccess = async () => {
    if (typeof window.__refreshHeaderData === "function") {
      try { await window.__refreshHeaderData(); } catch { }
    }
    if (typeof window.__refreshMultiTab === "function") {
      try { await window.__refreshMultiTab(); } catch { }
    }
    if (typeof window.__paper3lSubDetailRefresh === "function") {
      try { await window.__paper3lSubDetailRefresh(); } catch { }
    }
  };

  const open = async (ctx, options) => {
    if (!window.Swal) {
      alert("缺少 Swal，無法開啟尾數取消視窗。");
      return;
    }

    const opt = options || {};
    const paperNum = readPaperNum(ctx);
    const paperId = ((ctx && ctx.headerTable) || window._headerTableName || opt.defaultPaperId || "").toString().trim();
    if (!paperNum) return Swal.fire({ icon: "info", title: "請先選擇單號" });
    if (!paperId) return Swal.fire({ icon: "error", title: "缺少單據別", text: "無法執行尾數取消" });

    const showPlus = !!opt.allowPlusCancel && /^MPHdOrderMain$/i.test(paperId);

    const result = await Swal.fire({
      title: "尾數取消",
      html: `
        <div class="swal2-tailcancel">
          <style>
            .swal2-tailcancel .tc-row{ display:flex; gap:8px; align-items:center; justify-content:center; margin-bottom:8px; }
            .swal2-tailcancel .tc-label{ min-width:50px; text-align:right; }
            .swal2-tailcancel .tc-input{ width:200px; margin:0; }
          </style>
          <div style="display:flex; gap:12px; justify-content:center; margin-bottom:8px;">
            <label style="display:flex; gap:6px; align-items:center;">
              <input type="radio" name="tcMode" value="all" checked>全部尾數取消
            </label>
            <label style="display:flex; gap:6px; align-items:center;">
              <input type="radio" name="tcMode" value="single">單筆尾數取消
            </label>
          </div>
          <div class="tc-row">
            <label class="tc-label">項次</label>
            <input id="tcItem" class="swal2-input tc-input" placeholder="項次" disabled>
          </div>
          <div class="tc-row">
            <label class="tc-label">備註</label>
            <input id="tcNotes" class="swal2-input tc-input" placeholder="取消備註(可空白)">
          </div>
          <div style="display:flex; gap:8px; align-items:center; justify-content:center; margin-bottom:8px;">
            <label style="display:flex; gap:6px; align-items:center;">
              <input type="checkbox" id="tcRecompute" checked>重算金額
            </label>
          </div>
          ${showPlus ? `
          <div style="display:flex; gap:8px; align-items:center; justify-content:center;">
            <label style="display:flex; gap:6px; align-items:center;">
              <input type="checkbox" id="tcPlus">來源請購單一併尾數取消
            </label>
          </div>` : ""}
        </div>
      `,
      showCancelButton: true,
      confirmButtonText: "確定",
      cancelButtonText: "取消",
      focusConfirm: false,
      didOpen: () => {
        const itemInput = document.getElementById("tcItem");
        if (itemInput) {
          const selectedItem = readSelectedItem();
          if (selectedItem) itemInput.value = selectedItem;
        }
        document.querySelectorAll("input[name='tcMode']").forEach(function (r) {
          r.addEventListener("change", function () {
            const mode = (document.querySelector("input[name='tcMode']:checked") || {}).value || "all";
            if (itemInput) {
              itemInput.disabled = mode !== "single";
              if (mode === "single" && !itemInput.value) {
                const selectedItem = readSelectedItem();
                if (selectedItem) itemInput.value = selectedItem;
              }
            }
          });
        });
      },
      preConfirm: () => {
        const mode = (document.querySelector("input[name='tcMode']:checked") || {}).value || "all";
        const itemRaw = (document.getElementById("tcItem") || {}).value || "";
        const notes = (document.getElementById("tcNotes") || {}).value || "";
        const recompute = !!((document.getElementById("tcRecompute") || {}).checked);
        const plusCancel = !!((document.getElementById("tcPlus") || {}).checked);

        if (mode === "single") {
          const v = itemRaw.trim();
          if (!v) {
            Swal.showValidationMessage("項次必須輸入");
            return false;
          }
          if (!/^\d+$/.test(v)) {
            Swal.showValidationMessage("項次必須為數字");
            return false;
          }
          return { mode, item: parseInt(v, 10), notes, recompute, plusCancel };
        }

        return { mode, item: 1, notes, recompute, plusCancel };
      }
    });
    if (!result || !result.value) return;
    const form = result.value;

    const modeLabel = form.mode === "single" ? "單筆" : "全部";
    const confirm = await Swal.fire({
      icon: "question",
      title: "確定" + modeLabel + "尾數取消?",
      showCancelButton: true,
      confirmButtonText: "確定",
      cancelButtonText: "取消"
    });
    if (!confirm.isConfirmed) return;

    const resp = await fetch("/api/OrderSubButton/OrderTailCancel", withJwtHeaders({
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        paperId: paperId,
        paperNum: paperNum,
        mode: form.mode,
        item: form.item,
        reCompute: form.recompute ? 1 : 0,
        notes: form.notes || "",
        plusCancel: form.plusCancel || false
      })
    }));
    const data = await resp.json();
    if (!data.ok) return Swal.fire({ icon: "error", title: "執行失敗", text: data.error || "" });

    const doneMsg = data.plusDone ? "尾數取消完成，來源請購項目已處理" : "尾數取消完成";
    await Swal.fire({ icon: "success", title: doneMsg, timer: 1200, showConfirmButton: false });
    await refreshAfterSuccess();
  };

  const register = (itemId, buttonName, options) => {
    if (!itemId || !buttonName) return;
    window.ActionRailCustomHandlers = window.ActionRailCustomHandlers || {};
    window.ActionRailCustomHandlers[itemId] = window.ActionRailCustomHandlers[itemId] || {};
    window.ActionRailCustomHandlers[itemId][buttonName] = (ctx) => open(ctx, options || {});
  };

  window.OrderTailCancelHelper = { open, register };
})();
