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

function ensurePaperTypeModal() {
  const id = 'paperTypeSelectModal';
  let el = document.getElementById(id);
  if (el) return el;

  el = document.createElement('div');
  el.id = id;
  el.className = 'modal fade';
  el.tabIndex = -1;
  el.innerHTML = `
  <div class="modal-dialog modal-lg modal-dialog-scrollable modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">單據類別選定</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body p-0">
        <div class="table-responsive">
          <table class="table table-sm table-bordered mb-0">
            <thead class="table-light">
              <tr>
                <th class="text-nowrap" style="width:44px">類別</th>
                <th class="text-nowrap" style="min-width:140px">名稱</th>
                <th class="text-nowrap" style="width:60px">單頭</th>
                <th class="text-nowrap" style="width:80px">功能分類代碼</th>
                <th class="text-nowrap" style="min-width:140px">功能分類名稱</th>
                <th class="text-nowrap" style="width:90px">更新欄位</th>
                <th class="text-nowrap" style="width:90px">更新值</th>
              </tr>
            </thead>
            <tbody></tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-success" data-role="ok">確定</button>
        <button type="button" class="btn btn-outline-danger" data-bs-dismiss="modal">取消</button>
      </div>
    </div>
  </div>`;
  document.body.appendChild(el);
  return el;
}

function openPaperTypeModal(types) {
  return new Promise((resolve) => {
    const modalEl = ensurePaperTypeModal();
    const tbody = modalEl.querySelector('tbody');
    const okBtn = modalEl.querySelector('[data-role="ok"]');
    const bsModal = bootstrap.Modal.getOrCreateInstance(modalEl);

    tbody.innerHTML = '';
    let selectedIndex = -1;
    let confirmed = false;

    function setSelected(idx) {
      selectedIndex = idx;
      Array.from(tbody.querySelectorAll('tr')).forEach((tr, i) => {
        tr.classList.toggle('table-primary', i === idx);
      });
    }

    types.forEach((t, i) => {
      const funcName = t.PowerTypeName ?? t.powerTypeName ?? '';
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td class="text-center">${t.PaperType ?? ''}</td>
        <td>${t.PaperTypeName ?? ''}</td>
        <td class="text-center">${t.HeadFirst ?? ''}</td>
        <td class="text-center">${t.PowerType ?? ''}</td>
        <td>${funcName}</td>
        <td class="text-center">${t.UpdateFieldName ?? ''}</td>
        <td class="text-center">${t.UpdateValue ?? ''}</td>`;
      tr.addEventListener('click', () => setSelected(i));
      tr.addEventListener('dblclick', () => {
        setSelected(i);
        bsModal.hide();
      });
      tbody.appendChild(tr);
    });
    if (types.length > 0) setSelected(0);

    const onOk = () => {
      confirmed = true;
      bsModal.hide();
    };

    const onHidden = () => {
      okBtn.removeEventListener('click', onOk);
      modalEl.removeEventListener('hidden.bs.modal', onHidden);
      if (!confirmed) {
        resolve(null);
        return;
      }
      resolve(selectedIndex >= 0 ? types[selectedIndex] : null);
    };

    okBtn.addEventListener('click', onOk);
    modalEl.addEventListener('hidden.bs.modal', onHidden, { once: true });

    bsModal.show();
  });
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
     this.forceStoredQueryOnRedirect = opts.forceStoredQueryOnRedirect === true;
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
      // ========== Hook: beforeAdd (Inherited) ==========
      const itemId = window._itemId || window._singleItemId || '';
      if (itemId) {
        const hooks = window.InheritedActionHooks || {};
        const beforeAddFn = hooks[itemId]?.beforeAdd || hooks.beforeAdd;
        if (typeof beforeAddFn === 'function') {
          try {
            const result = await beforeAddFn({ tableName: this.tableName, userId: window._userId || 'admin' });
            if (result === false) {
              return;
            }
          } catch (err) {
            console.error('[Inherited] beforeAdd 執行失敗:', err);
            await Swal.fire({ icon: 'error', title: 'Hook 執行失敗', text: err?.message || String(err) });
            return;
          }
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

      const userId = (window._userId || 'admin').toString().trim();
      const useId = (window._useId || window.DEFAULT_USEID || 'A001').toString().trim();
      let selectedType = null;

      try {
        const typeTable = (this.tableName || '').toString().trim();
        if (!typeTable) {
          await Swal.fire({ icon: 'error', title: '找不到資料表名稱' });
          return;
        }
        const typeUrl = `/api/DynamicTable/PaperTypes/${encodeURIComponent(typeTable)}?itemId=${encodeURIComponent(itemId || '')}`;
        const typeResp = await fetchWithBusy(typeUrl, { method:'GET' }, '載入類別…');
        if (!typeResp.ok) {
          await Swal.fire({ icon: 'error', title: '載入類別失敗' });
          return;
        }
        const typeData = await typeResp.json().catch(() => ({}));
        const selectType = Number(typeData?.selectType ?? typeData?.SelectType ?? 0);
        const types = Array.isArray(typeData?.types) ? typeData.types : [];

        if (selectType === 1) {
          if (types.length === 0) {
            await Swal.fire({ icon: 'warning', title: '找不到單據類別' });
            return;
          }
          selectedType = await openPaperTypeModal(types);
          if (!selectedType) return;
        }
      } catch (err) {
        console.error('[新增] 讀取類別失敗:', err);
        await Swal.fire({ icon: 'error', title: '載入類別失敗' });
        return;
      }

      const payload = { itemId, userId, useId };
      if (selectedType) {
        payload.paperType = selectedType.PaperType ?? selectedType.paperType;
        payload.paperTypeName = selectedType.PaperTypeName ?? selectedType.paperTypeName;
        payload.headFirst = selectedType.HeadFirst ?? selectedType.headFirst;
        payload.powerType = selectedType.PowerType ?? selectedType.powerType;
        payload.updateFieldName = selectedType.UpdateFieldName ?? selectedType.updateFieldName;
        payload.updateValue = selectedType.UpdateValue ?? selectedType.updateValue;
        payload.tradeId = selectedType.TradeId ?? selectedType.tradeId;
      }

     const resp = await fetchWithBusy(this.addApiUrl, { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify(payload) }, '建立中…');

      if (!resp.ok) return Swal.fire({ icon: 'error', title: '建立失敗！' });

      const data = await resp.json();
      const paperNum = data.paperNum || data.PaperNum || '';

      // ========== Hook: afterAdd (Inherited) ==========
      if (itemId) {
        const hooks = window.InheritedActionHooks || {};
        const afterAddFn = hooks[itemId]?.afterAdd || hooks.afterAdd;
        if (typeof afterAddFn === 'function') {
          try {
            await afterAddFn({ tableName: this.tableName, paperNum, userId: window._userId || 'admin', result: data });
          } catch (err) {
            console.error('[Inherited] afterAdd 執行失敗:', err);
          }
        }
      }

      if (paperNum) {
        // 支援兩種格式的佔位符：{PaperNum} 和 {0}
        try { localStorage.setItem("afterSave", "1"); } catch {}
        let url = this.detailRouteTemplate.replace(/\{PaperNum\}/gi, paperNum);
        url = url.replace(/\{0\}/g, paperNum);
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

      const ask = await Swal.fire({
        title: '確定要作廢這筆單據？',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '確定',
        cancelButtonText: '取消'
      });
      if (!ask.isConfirmed) return;

      try {
        // ========== Hook: beforeDelete (Inherited) ==========
        const itemId = window._itemId || window._singleItemId || '';
        if (itemId) {
          const hooks = window.InheritedActionHooks || {};
          const beforeDeleteFn = hooks[itemId]?.beforeDelete || hooks.beforeDelete;
          if (typeof beforeDeleteFn === 'function') {
            try {
              const result = await beforeDeleteFn({ tableName: this.tableName, paperNum: selectedId, userId: window._userId || 'admin' });
              if (result === false) {
                return;
              }
            } catch (err) {
              console.error('[Inherited] beforeDelete 執行失敗:', err);
              await Swal.fire({ icon: 'error', title: 'Hook 執行失敗', text: err?.message || String(err) });
              return;
            }
          }
        }

        // 動態單據：走 PaperAction（CURdPaperAction）
        if (this.paperAction?.url) {
          const buildPayload = (reason) => ({
            paperId: this.paperAction.paperId || this.tableName,
            paperNum: selectedId,
            userId: this.paperAction.userId || window._userId || 'admin',
            eoc: Number(this.paperAction.eoc ?? 0),
            aftFinished: Number(this.paperAction.aftFinished ?? 2),
            itemId,
            useId: (window._useId || window.DEFAULT_USEID || 'A001'),
            voidReason: (reason || '').toString().trim()
          });

          const sendVoid = async (reason) => {
            const resp = await fetchWithBusy(this.paperAction.url, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify(buildPayload(reason))
            }, '作廢中');

            if (resp.ok) {
              const data = await resp.json().catch(() => ({}));
              return { ok: true, data };
            }

            let err = null;
            try { err = await resp.json(); }
            catch {
              const text = await resp.text().catch(() => '');
              err = { message: text };
            }
            return { ok: false, err, status: resp.status };
          };

          let result = await sendVoid('');
          if (!result.ok && result.err?.code === 'NEED_VOID_REASON') {
            const ask = await Swal.fire({
              title: '請輸入作廢原因',
              input: 'textarea',
              inputPlaceholder: '作廢原因',
              showCancelButton: true,
              confirmButtonText: '確定',
              cancelButtonText: '取消',
              inputValidator: (value) => (!value || !value.trim()) ? '請輸入作廢原因' : null
            });
            if (!ask.isConfirmed) return;
            result = await sendVoid(ask.value || '');
          }

          if (result.ok) {
            const data = result.data || {};

            // ========== Hook: afterDelete (Inherited) ==========
            if (itemId) {
              const hooks = window.InheritedActionHooks || {};
              const afterDeleteFn = hooks[itemId]?.afterDelete || hooks.afterDelete;
              if (typeof afterDeleteFn === 'function') {
                try {
                  await afterDeleteFn({ tableName: this.tableName, paperNum: selectedId, userId: window._userId || 'admin', result: data });
                } catch (err) {
                  console.error('[Inherited] afterDelete 執行失敗:', err);
                }
              }
            }

            await Swal.fire({ icon: 'success', title: data.message || '作廢成功！' });

            if (this.queryRedirectUrl) window.location.href = this.queryRedirectUrl;
            else location.reload();
            return;
          }

          const msg = result.err?.message || '作廢失敗！';
          await Swal.fire({ icon: 'error', title: msg });
          return;
        }

        // 舊行為：DELETE /api/{Table}/{Id}
        const resp = await fetchWithBusy(this.deleteApiUrlFn(selectedId), { method: 'DELETE' }, '作廢中');
        if (resp.ok) {
          // ========== Hook: afterDelete (Inherited) ==========
          if (itemId) {
            const hooks = window.InheritedActionHooks || {};
            const afterDeleteFn = hooks[itemId]?.afterDelete || hooks.afterDelete;
            if (typeof afterDeleteFn === 'function') {
              try {
                await afterDeleteFn({ tableName: this.tableName, paperNum: selectedId, userId: window._userId || 'admin' });
              } catch (err) {
                console.error('[Inherited] afterDelete 執行失敗:', err);
              }
            }
          }

          await Swal.fire({ icon: 'success', title: '作廢成功！' });

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

      // 保存查詢條件（帶表名的鍵，與導航功能保持一致）
      const tn = (this.tableName || '').toString().trim().toLowerCase();
      const filterKey = tn ? `orderListQueryFilters:${tn}` : 'orderListQueryFilters';
      try { localStorage.setItem(filterKey, JSON.stringify(filters)); } catch {}
      try { localStorage.setItem('orderListQueryFilters', JSON.stringify(filters)); } catch {}  // 保留舊鍵作為備用
      try { localStorage.setItem('orderListPageNumber', '1'); } catch {}

      if (typeof this.restoreSearchForm === 'function') {
        this.restoreSearchForm(document.getElementById(this.formId), filters);
      }

      // 轉跳查詢（統一處理：列表頁或單身頁都跳轉到列表頁）
      if (this.queryRedirectUrl) {
        if (this.forceStoredQueryOnRedirect) {
          const params = new URLSearchParams();
          params.set('useStoredQuery', '1');
          params.set('page', '1');
          window.location.href = this.queryRedirectUrl + '?' + params.toString();
        } else {
          const params = new URLSearchParams();
          filters.forEach(f => {
            if (['page','pageSize'].includes(f.Field)) return;
            if (f.Value != null && f.Value !== '') params.append(f.Field, f.Value);
          });
          window.location.href = this.queryRedirectUrl + '?' + params.toString();
        }
        return;
      }

      // 如果沒有 queryRedirectUrl 但沒有渲染函數（單身頁面），提示用戶
      if (!this.renderTable) {
        await Swal.fire({
          icon: 'warning',
          title: '無法執行查詢',
          text: '頁面配置錯誤，請聯繫系統管理員'
        });
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
