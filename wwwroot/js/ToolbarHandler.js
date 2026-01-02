// 放在檔案最前面
async function fetchWithBusy(url, init, msg) {
  if (window.http)      return await window.http(url, init, msg);
  if (window.busyFetch) return await window.busyFetch(url, init, msg);

  // fallback：沒有全域包裝也強制秀遮罩
  if (window.showBusy) window.showBusy(msg || '處理中…');
  try { return await fetch(url, init); }
  finally { if (window.hideBusy) window.hideBusy(); }
}


// 共用封鎖狀態（如需）
window.BLOCK_STATUSES = window.BLOCK_STATUSES || ['已確認', '已結案', '已作廢'];

// CSS.escape polyfill
if (typeof CSS === 'undefined' || typeof CSS.escape !== 'function') {
  window.CSS = window.CSS || {};
  CSS.escape = s => String(s).replace(/([^\w-])/g, '\\$1');
}

  class ToolbarHandler {
  constructor(opts) {
    this.searchBtnId         = opts.searchBtnId;
    this.addBtnId            = opts.addBtnId;
    this.deleteBtnId         = opts.deleteBtnId;

    this.modalId             = opts.modalId;
    this.formId              = opts.formId;
    this.addApiUrl           = opts.addApiUrl;
    this.deleteApiUrlFn      = opts.deleteApiUrlFn; // function(id)
    this.detailRouteTemplate = opts.detailRouteTemplate;
    this.tableName           = opts.tableName;
    this.pageSize            = opts.pageSize || 50;

    this.renderTable         = opts.renderTable;
    this.renderPagination    = opts.renderPagination;
    this.renderOrderCount    = opts.renderOrderCount;
    this.restoreSearchForm   = opts.restoreSearchForm;
    this.pagedQueryUrl       = opts.pagedQueryUrl || '/api/PagedQuery/PagedQuery';

    this.lastQueryFilters    = [];
    this.getSelectedId       = opts.getSelectedId || (() => window.selectedPaperNum || null);
     this.queryRedirectUrl    = opts.queryRedirectUrl || null;
     this.paperAction         = opts.paperAction || null; // { url, paperId?, userId?, eoc, aftFinished }

     this.init();
   }

  init() {
    // 查詢面板
    document.getElementById(this.searchBtnId).onclick = () => {
      const modal = new bootstrap.Modal(document.getElementById(this.modalId));
      modal.show();

      setTimeout(() => {
        if (typeof this.restoreSearchForm === 'function') {
          const formEl = document.getElementById(this.formId);
          const saved = (this.lastQueryFilters && this.lastQueryFilters.length)
            ? this.lastQueryFilters
            : (() => { try { return JSON.parse(localStorage.getItem("orderListQueryFilters") || "[]"); } catch { return []; } })();
          this.restoreSearchForm(formEl, saved);
        }
      }, 100);
    };

    // 新增
    document.getElementById(this.addBtnId).onclick = async () => {
      // ========== Hook: beforeAdd ==========
      if (typeof window.executeGlobalTemplateHook === 'function') {
        const hookResult = window.executeGlobalTemplateHook('beforeAdd');
        if (hookResult === false) {
          console.log('[ToolbarHandler] beforeAdd Hook 中止新增');
          return;
        }
      }

      const ask = await Swal.fire({
        title: '確定要新增一張新單據？',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消'
      });
      if (!ask.isConfirmed) return;

     const resp = await fetchWithBusy(this.addApiUrl, { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({}) }, '建立中…');

      if (!resp.ok) return Swal.fire({ icon: 'error', title: '建立失敗！' });

      const data = await resp.json();
      const paperNum = data.paperNum || data.PaperNum || '';

      // ========== Hook: afterAdd ==========
      if (typeof window.executeGlobalTemplateHook === 'function') {
        window.executeGlobalTemplateHook('afterAdd', { paperNum, data });
      }

      if (paperNum) {
        // 支援兩種格式的佔位符：{PaperNum} 和 {0}
        console.log('[ToolbarHandler] detailRouteTemplate:', this.detailRouteTemplate);
        console.log('[ToolbarHandler] paperNum:', paperNum);
        let url = this.detailRouteTemplate.replace(/\{PaperNum\}/gi, paperNum);
        url = url.replace(/\{0\}/g, paperNum);
        console.log('[ToolbarHandler] final URL:', url);
        window.location = url;
      } else {
        await Swal.fire({ icon: 'success', title: '新增成功，但未取得單號！' });
        location.reload();
      }
    };

    // 作廢
    document.getElementById(this.deleteBtnId).onclick = async () => {
      const selectedId = this.getSelectedId();
      if (!selectedId) {
        await Swal.fire({ icon: 'warning', title: '請先點選一筆資料', confirmButtonText: '確定' });
        return;
      }

      // ========== Hook: beforeDelete ==========
      if (typeof window.executeGlobalTemplateHook === 'function') {
        const hookResult = window.executeGlobalTemplateHook('beforeDelete', { selectedId });
        if (hookResult === false) {
          console.log('[ToolbarHandler] beforeDelete Hook 中止作廢');
          return;
        }
      }

      const ask = await Swal.fire({
        title: '確定要作廢這筆單據？',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消'
      });
      if (!ask.isConfirmed) return;

      try {
        // 動態單據：走 PaperAction（CURdPaperAction）
        if (this.paperAction?.url) {
          const payload = {
            paperId: this.paperAction.paperId || this.tableName,
            paperNum: selectedId,
            userId: this.paperAction.userId || window._userId || 'admin',
            eoc: Number(this.paperAction.eoc ?? 0),
            aftFinished: Number(this.paperAction.aftFinished ?? 2)
          };
          const resp = await fetchWithBusy(this.paperAction.url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
          }, '作廢中');

          if (resp.ok) {
            const data = await resp.json().catch(() => ({}));
            await Swal.fire({ icon: 'success', title: data.message || '作廢成功！' });

            // ========== Hook: afterDelete ==========
            if (typeof window.executeGlobalTemplateHook === 'function') {
              window.executeGlobalTemplateHook('afterDelete', { selectedId });
            }

            if (this.queryRedirectUrl) window.location.href = this.queryRedirectUrl;
            else location.reload();
            return;
          }

          const msg = await resp.text().catch(() => '');
          await Swal.fire({ icon: 'error', title: msg || '作廢失敗！' });
          return;
        }

        // 舊行為：DELETE /api/{Table}/{Id}
        const resp = await fetchWithBusy(this.deleteApiUrlFn(selectedId), { method: 'DELETE' }, '作廢中');
        if (resp.ok) {
          await Swal.fire({ icon: 'success', title: '作廢成功！' });

          // ========== Hook: afterDelete ==========
          if (typeof window.executeGlobalTemplateHook === 'function') {
            window.executeGlobalTemplateHook('afterDelete', { selectedId });
          }

          if (this.queryRedirectUrl) window.location.href = this.queryRedirectUrl;
          else location.reload();
          return;
        }
        let msg = '作廢失敗！';
        try { const r = await resp.json(); if (r?.error) msg = r.error; } catch {}
        await Swal.fire({ icon: 'error', title: msg });
      } catch (e) {
        await Swal.fire({ icon: 'error', title: '作廢失敗', text: String(e) });
      }
    };

    // 表單 submit（建立 filters）
    document.getElementById(this.formId).onsubmit = async (e) => {
      e.preventDefault();

      const formSel = `#${this.formId}`;
      const filters = [];

      const valueEls = Array.from(
        document.querySelectorAll(`${formSel} [name]:not([name^="Cond_"]):not([type="hidden"])`)
      );
      const namePos = Object.create(null);

      valueEls.forEach((el) => {
        const name = el.name;
        if (!name || name === '__RequestVerificationToken') return;

        const idx = namePos[name] = (namePos[name] ?? 0);
        namePos[name]++;

        let val = el.value;

        // 交給全域的 normalizeForDateInput (若有)
        if (typeof window.normalizeForDateInput === 'function') {
          val = window.normalizeForDateInput(el, val);
        }
        if (val == null || val === '') return;

        const condList = document.querySelectorAll(`${formSel} [name="${CSS.escape('Cond_' + name)}"]`);
        const op = (condList && condList.length > idx) ? (condList[idx].value || '') : '';

        filters.push({
          Key: `${name}#${idx}`,
          Field: el.dataset.field || name,
          Op: op,
          Value: val
        });
      });

      filters.push({ Field: 'page', Op: '', Value: '1' });
      filters.push({ Field: 'pageSize', Op: '', Value: String(this.pageSize) });

      this.lastQueryFilters = filters.slice();
      try { localStorage.setItem('orderListQueryFilters', JSON.stringify(filters)); } catch {}
      try { localStorage.setItem('orderListPageNumber', '1'); } catch {}

      if (typeof this.restoreSearchForm === 'function') {
        this.restoreSearchForm(document.getElementById(this.formId), filters);
      }

      // 轉跳查詢（List 頁用）
      if (this.queryRedirectUrl) {
        const params = new URLSearchParams();
        filters.forEach(f => {
          if (['page','pageSize'].includes(f.Field)) return;
          if (f.Value != null && f.Value !== '') params.append(f.Field, f.Value);
        });
        window.location.href = this.queryRedirectUrl + '?' + params.toString();
        return;
      }

      // AJAX 查詢（單頁查詢用）
      const resp = await fetchWithBusy(this.pagedQueryUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ table: this.tableName, filters })
      }, '查詢中…');

      if (!resp.ok) { Swal.fire({ icon:'error', title:'查詢失敗' }); return; }

      const result = await resp.json();
      window._lookupMapData = result.lookupMapData;
      this.renderTable?.(result.data);
      this.renderPagination?.(result.totalCount, this.pageSize, 1);
      this.renderOrderCount?.(result.totalCount, this.pageSize, 1);

      bootstrap.Modal.getOrCreateInstance(document.getElementById(this.modalId)).hide();
    };
  }
}

window.ToolbarHandler = ToolbarHandler;
