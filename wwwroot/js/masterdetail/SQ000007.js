(function () {
  const ITEM_ID = "SQ000007";
  const itemId = (window._mdItemId || "").toString().toUpperCase();
  if (itemId !== ITEM_ID) return;

  const MODAL_ID = "sq000007EmoFieldModal";

  function ensureModal() {
    let modal = document.getElementById(MODAL_ID);
    if (modal) return modal;

    modal = document.createElement("div");
    modal.id = MODAL_ID;
    modal.className = "modal fade";
    modal.tabIndex = -1;
    modal.setAttribute("aria-hidden", "true");
    modal.setAttribute("data-bs-backdrop", "false");
    modal.innerHTML = `
      <div class="modal-dialog modal-dialog-scrollable" style="--bs-modal-width:520px">
        <div class="modal-content">
          <div class="modal-header py-2">
            <h6 class="modal-title">選擇 EMO 欄位</h6>
            <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body p-0" style="min-height:300px;max-height:60vh;overflow:auto;">
            <div id="${MODAL_ID}-loading" class="text-center py-4 text-muted">載入中...</div>
            <div id="${MODAL_ID}-tree" class="p-2" style="display:none;"></div>
          </div>
          <div class="modal-footer py-1">
            <button type="button" class="btn btn-sm btn-secondary" data-bs-dismiss="modal">取消</button>
            <button type="button" class="btn btn-sm btn-primary" id="${MODAL_ID}-btnOk">確定</button>
          </div>
        </div>
      </div>
    `;
    document.body.appendChild(modal);
    return modal;
  }

  function buildTree(data) {
    const map = {};
    const roots = [];

    data.forEach(d => {
      map[d.itemId] = { ...d, children: [] };
    });
    data.forEach(d => {
      const node = map[d.itemId];
      if (d.superId && map[d.superId]) {
        map[d.superId].children.push(node);
      } else {
        roots.push(node);
      }
    });

    function renderNodes(nodes) {
      if (!nodes.length) return "";
      let html = "<ul class='list-unstyled mb-0' style='padding-left:18px;'>";
      nodes.forEach(n => {
        const hasChildren = n.children.length > 0;
        const toggleIcon = hasChildren
          ? "<span class='tree-toggle me-1' style='cursor:pointer;user-select:none;'>&#9654;</span>"
          : "<span class='me-1' style='width:12px;display:inline-block;'></span>";
        const clickable = !hasChildren
          ? ` class="tree-leaf" style="cursor:pointer;" data-item-id="${esc(n.itemId)}" data-item-name="${esc(n.itemName)}" data-super-id="${esc(n.superId)}"`
          : ` data-item-id="${esc(n.itemId)}" data-item-name="${esc(n.itemName)}"`;
        html += `<li${clickable}>${toggleIcon}<span class="tree-label">${esc(n.itemName)}</span>`;
        if (hasChildren) {
          html += `<div class="tree-children">${renderNodes(n.children)}</div>`;
        }
        html += "</li>";
      });
      html += "</ul>";
      return html;
    }

    return renderNodes(roots);
  }

  function esc(s) {
    const d = document.createElement("div");
    d.textContent = s || "";
    return d.innerHTML;
  }

  async function openDialog(ctx) {
    const masterRow = ctx.masterRow;
    if (!masterRow) {
      alert("請先選取主檔資料列。");
      return;
    }
    const numId = getFieldCI(masterRow, "NumId");
    if (!numId) {
      alert("找不到目前選取列的 NumId。");
      return;
    }

    const modal = ensureModal();
    const bsModal = bootstrap.Modal.getOrCreateInstance(modal);
    const loadingEl = document.getElementById(`${MODAL_ID}-loading`);
    const treeEl = document.getElementById(`${MODAL_ID}-tree`);
    const btnOk = document.getElementById(`${MODAL_ID}-btnOk`);

    loadingEl.style.display = "";
    treeEl.style.display = "none";
    treeEl.innerHTML = "";
    bsModal.show();

    let selectedNode = null;

    try {
      const jwt = localStorage.getItem("jwtId");
      const headers = {};
      if (jwt) headers["X-JWTID"] = jwt;
      const resp = await fetch("/api/SQUdSetNumEMOField/TreeData", { headers });
      if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
      const data = await resp.json();

      treeEl.innerHTML = buildTree(data);
      loadingEl.style.display = "none";
      treeEl.style.display = "";

      // toggle expand/collapse
      treeEl.querySelectorAll(".tree-toggle").forEach(tog => {
        tog.addEventListener("click", function (e) {
          e.stopPropagation();
          const li = this.closest("li");
          const children = li.querySelector(".tree-children");
          if (children) {
            const hidden = children.style.display === "none";
            children.style.display = hidden ? "" : "none";
            this.innerHTML = hidden ? "&#9660;" : "&#9654;";
          }
        });
      });

      // collapse all by default
      treeEl.querySelectorAll(".tree-children").forEach(c => { c.style.display = "none"; });

      // leaf click selection
      treeEl.querySelectorAll(".tree-leaf").forEach(leaf => {
        leaf.addEventListener("click", function () {
          treeEl.querySelectorAll(".tree-leaf").forEach(l => l.style.backgroundColor = "");
          this.style.backgroundColor = "#cce5ff";
          selectedNode = {
            itemId: this.dataset.itemId,
            itemName: this.dataset.itemName,
            superId: this.dataset.superId
          };
          // find parent name
          const parentLi = this.parentElement?.closest("li[data-item-id]");
          if (parentLi) {
            selectedNode.parentName = parentLi.dataset.itemName;
          }
        });
      });
    } catch (err) {
      loadingEl.textContent = "載入失敗: " + (err?.message || err);
      return;
    }

    const newBtnOk = btnOk.cloneNode(true);
    btnOk.parentNode.replaceChild(newBtnOk, btnOk);
    newBtnOk.id = `${MODAL_ID}-btnOk`;

    newBtnOk.addEventListener("click", async () => {
      if (!selectedNode || !selectedNode.parentName) {
        alert("請選擇一個子項目（非根節點）。");
        return;
      }
      const emoValue = `{${selectedNode.parentName}.${selectedNode.itemName}}`;
      try {
        const jwt = localStorage.getItem("jwtId");
        const headers = { "Content-Type": "application/json" };
        if (jwt) headers["X-JWTID"] = jwt;
        const resp = await fetch("/api/SQUdSetNumEMOField/Update", {
          method: "POST",
          headers,
          body: JSON.stringify({ numId, eMOdField: emoValue })
        });
        const result = await resp.json();
        if (!resp.ok || !result.ok) {
          alert("更新失敗: " + (result.error || `HTTP ${resp.status}`));
          return;
        }
        bsModal.hide();
        alert(`已更新 EMOdField = ${emoValue}`);
        const refresh = window.MasterDetailRefresh;
        if (refresh) {
          const domId = ctx.domId;
          const fn = refresh[domId];
          if (typeof fn === "function") {
            await fn();
            return;
          }
        }
        location.reload();
      } catch (err) {
        alert("更新失敗: " + (err?.message || err));
      }
    });
  }

  function getFieldCI(row, fieldName) {
    if (!row || !fieldName) return "";
    const lc = fieldName.toLowerCase();
    for (const k of Object.keys(row)) {
      if (k.toLowerCase() === lc) return (row[k] ?? "").toString().trim();
    }
    return "";
  }

  window.MasterDetailCustomHandlers = window.MasterDetailCustomHandlers || {};
  window.MasterDetailCustomHandlers["btnC1"] = openDialog;
})();
