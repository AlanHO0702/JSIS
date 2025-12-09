// CUR00018 專用自訂按鈕邏輯（系統模組設定）
// 會註冊到 window.SingleGridCustomHandlers，並同時曝光舊的全域函式名稱以相容現有 onclick。

(function () {
  const getSelectedRow = () => {
    return window.SELECTED_ROW
      || document.querySelector('tr.selected-row')
      || document.querySelector('tr.table-active')
      || null;
  };

  const readRow = (tr) => {
    if (!tr) return {};
    const obj = {};
    tr.querySelectorAll('td[data-field]').forEach(td => {
      const field = td.dataset.field || '';
      if (!field) return;
      const inp = td.querySelector('.cell-edit');
      const span = td.querySelector('.cell-view');
      obj[field] = inp?.value ?? span?.textContent ?? '';
    });
    // 舊頁面未加 data-field 時，備援用第一欄視為 SystemId
    if (!obj.SystemId && tr.children.length > 0) {
      obj.SystemId = tr.children[0].innerText?.trim?.() || '';
    }
    return obj;
  };

  const ensureRow = () => {
    const tr = getSelectedRow();
    if (!tr) {
      alert('請先選擇一筆系統資料');
      return null;
    }
    return tr;
  };

  const getCtx = () => {
    const tr = ensureRow();
    if (!tr) return null;
    const row = readRow(tr);
    const systemCode = row.SystemId || row.systemId || row.SystemID || '';
    return { tr, row, systemCode };
  };

  const ensurePreviewModal = () => {
    let modalEl = document.getElementById('imagePreviewModal');
    if (!modalEl) {
      const wrapper = document.createElement('div');
      wrapper.innerHTML = `
      <div class="modal fade" id="imagePreviewModal" tabindex="-1">
        <div class="modal-dialog modal-xl modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">圖檔預覽</h5>
              <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body text-center">
              <img id="previewImage" src="" alt="預覽圖檔" style="max-width:100%; max-height:75vh; border:1px solid #ccc;">
            </div>
          </div>
        </div>
      </div>`;
      document.body.appendChild(wrapper.firstElementChild);
      modalEl = document.getElementById('imagePreviewModal');
    }
    return modalEl;
  };

  const openPreview = (url) => {
    const modalEl = ensurePreviewModal();
    const img = modalEl.querySelector('#previewImage');
    if (!img) return alert('找不到預覽區塊');
    img.src = url;
    if (modalEl && window.bootstrap) {
      window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
    }
  };

  async function uploadImage() {
    const ctx = getCtx();
    if (!ctx || !ctx.systemCode) return alert('請先選擇系統代碼');
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.bmp';
    input.onchange = async (e) => {
      const file = e.target.files?.[0];
      if (!file) return;
      const formData = new FormData();
      formData.append('SystemCode', ctx.systemCode);
      formData.append('File', file);
      const res = await fetch('/api/SystemGraph/Upload', { method: 'POST', body: formData });
      if (res.ok) { alert('上傳完成'); location.reload(); }
      else alert(await res.text());
    };
    input.click();
  }

  async function viewImage() {
    const ctx = getCtx();
    if (!ctx || !ctx.systemCode) return alert('請先選擇系統代碼');
    const fileName = ctx.row.GraphName || ctx.row.graphName || ctx.row.FileName || ctx.row.filename || '';
    if (!fileName) return alert('沒有圖檔檔名');
    openPreview(`/api/SystemGraph/GetImage/${encodeURIComponent(fileName)}`);
  }

  async function deleteImage() {
    const ctx = getCtx();
    if (!ctx || !ctx.systemCode) return alert('請先選擇系統代碼');
    if (!confirm('確定刪除圖檔？')) return;
    const res = await fetch(`/api/SystemGraph/Delete?systemCode=${encodeURIComponent(ctx.systemCode)}`, { method: 'DELETE' });
    if (res.ok) { alert('刪除完成'); location.reload(); }
    else alert(await res.text());
  }

  async function uploadManual() {
    const ctx = getCtx();
    if (!ctx || !ctx.systemCode) return alert('請先選擇系統代碼');
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.doc,.docx';
    input.onchange = async (e) => {
      const file = e.target.files?.[0];
      if (!file) return;
      const formData = new FormData();
      formData.append('SystemCode', ctx.systemCode);
      formData.append('File', file);
      const res = await fetch('/api/SystemGraph/UploadManual', { method: 'POST', body: formData });
      if (res.ok) { alert('上傳完成'); location.reload(); }
      else alert(await res.text());
    };
    input.click();
  }

  async function downloadManual() {
    const ctx = getCtx();
    if (!ctx) return;
    const fileName = ctx.row.ManualName || ctx.row.manualName || '';
    if (!fileName) return alert('沒有使用手冊檔名');
    window.open(`/api/SystemGraph/GetManual/${encodeURIComponent(fileName)}`, '_blank');
  }

  async function deleteManual() {
    const ctx = getCtx();
    if (!ctx || !ctx.systemCode) return alert('請先選擇系統代碼');
    if (!confirm('確定刪除使用手冊？')) return;
    const res = await fetch(`/api/SystemGraph/DeleteManual?systemCode=${encodeURIComponent(ctx.systemCode)}`, { method: 'DELETE' });
    if (res.ok) { alert('刪除完成'); location.reload(); }
    else alert(await res.text());
  }

  // 預留 SOP 相關按鈕（若後端提供對應 API，填上即可）
  async function uploadSop() { alert('尚未實作：上傳 SOP'); }
  async function viewSop() { alert('尚未實作：檢視 SOP'); }
  async function deleteSop() { alert('尚未實作：刪除 SOP'); }

  // 註冊到 SingleGrid handlers
  window.SingleGridCustomHandlers = window.SingleGridCustomHandlers || {};
  window.SingleGridCustomHandlers["CUR00018"] = {
    btnC1: uploadImage,
    btnC2: viewImage,
    btnC3: deleteImage,
    btnC4: uploadManual,
    btnC5: downloadManual,
    btnC6: deleteManual,
    btnC7: uploadSop,
    btnC8: viewSop,
    btnC9: deleteSop
  };

  // 也提供全域函式，給舊的 onclick 呼叫
  window.uploadImage = uploadImage;
  window.viewImage = viewImage;
  window.deleteImage = deleteImage;
  window.uploadManual = uploadManual;
  window.downloadManual = downloadManual;
  window.deleteManual = deleteManual;
})();
