// BT000011 申報結算(401)
// 主表：ATXdTable401 (PK: UseId, HisId)
// 功能：結轉設定、查核、詳細資料頁籤（基本資料/銷項/特種銷項/進項/稅額計算/其它項目）、稅額自動計算、403模式
// SP：ATXdTaxHistorySet, ATXdTaxCheck, ATXdTaxUpdate403
// 版面：上半 SingleGrid、下半 MultiGrid 風格頁籤 + CustomButton

(function () {
  'use strict';

  // ==========================================
  // 工具函式
  // ==========================================
  const getUserId = () =>
    (window._userId || localStorage.getItem('erpLoginUserId') || 'admin').toString().trim();

  const getUseId = () =>
    (localStorage.getItem('erpLoginBUId') || window.DEFAULT_USEID || window._useId || 'A001').toString().trim();

  let is403 = false;
  let isEditMode = false;

  // Delphi DoEditLock 中可在編輯模式解鎖的欄位
  const editableFields = new Set([
    'Col22', 'Col107', 'Col108', 'Col110', 'Col111', 'Col112', 'Col113', 'Col114', 'Col115',
    'InvDtlCount', 'IsInCerVolume', 'IsInCerCount', 'DiscountCount', 'TaxReportCount', 'NoTaxedCount', 'CustomsCount',
    'PlusCol62', 'PlusCol75', 'PlusCol16'
  ]);

  // ==========================================
  // API 呼叫
  // ==========================================
  async function queryDirect(tableName, whereClause, columns) {
    const body = { TableName: tableName, WhereClause: whereClause || '1=1' };
    if (columns) body.Columns = columns;
    const resp = await fetch('/api/StoredProc/queryDirect', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });
    if (!resp.ok) throw new Error('HTTP ' + resp.status);
    const result = await resp.json();
    return result.data || result;
  }

  async function callSP(key, args) {
    const resp = await fetch('/api/StoredProc/exec', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Key: key, Args: args })
    });
    const result = await resp.json();
    if (!resp.ok || !result.ok) throw new Error(result.error || '執行失敗');
    return result;
  }

  async function querySP(key, args) {
    const resp = await fetch('/api/StoredProc/query', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Key: key, Args: args })
    });
    const result = await resp.json();
    if (!resp.ok || !result.ok) throw new Error(result.error || '執行失敗');
    return result;
  }

  // ==========================================
  // Grid 欄位讀寫
  // ==========================================
  function readSelectedRow() {
    const tr = window.SELECTED_ROW || document.querySelector('tr.selected-row');
    if (!tr) return null;
    const data = {};
    tr.querySelectorAll('td[data-field]').forEach(td => {
      const field = td.dataset.field;
      if (!field) return;
      const inp = td.querySelector('.cell-edit');
      const span = td.querySelector('.cell-view');
      data[field] = inp?.value ?? span?.textContent?.trim() ?? '';
    });
    return data;
  }

  function updateGridCell(fieldName, value) {
    const tr = window.SELECTED_ROW || document.querySelector('tr.selected-row');
    if (!tr) return;
    const td = tr.querySelector(`td[data-field="${fieldName}"]`);
    if (!td) return;
    const inp = td.querySelector('.cell-edit');
    const span = td.querySelector('.cell-view');
    if (inp) { inp.value = value; inp.dataset.raw = value; }
    if (span) { span.textContent = value; span.dataset.raw = value; }
  }

  function getVal(data, field) {
    if (!data) return '';
    return (data[field] ?? '').toString().trim();
  }

  function getNum(data, field) {
    return parseInt(getVal(data, field), 10) || 0;
  }

  function fmtNum(n) {
    return (n || 0).toString();
  }

  // ==========================================
  // 上下拖曳分割器
  // ==========================================
  function bindVerticalSplitter(splitter, topEl, bottomEl) {
    if (!splitter || !topEl || !bottomEl) return;
    let startY, topH0, botH0;
    splitter.addEventListener('mousedown', e => {
      e.preventDefault();
      startY = e.clientY;
      topH0 = topEl.getBoundingClientRect().height;
      botH0 = bottomEl.getBoundingClientRect().height;
      splitter.classList.add('dragging');
      const onMove = ev => {
        const dy = ev.clientY - startY;
        const newTop = Math.max(80, topH0 + dy);
        const newBot = Math.max(80, botH0 - dy);
        topEl.style.flex = 'none';
        bottomEl.style.flex = 'none';
        topEl.style.height = newTop + 'px';
        bottomEl.style.height = newBot + 'px';
      };
      const onUp = () => {
        splitter.classList.remove('dragging');
        document.removeEventListener('mousemove', onMove);
        document.removeEventListener('mouseup', onUp);
      };
      document.addEventListener('mousemove', onMove);
      document.addEventListener('mouseup', onUp);
    });
  }

  // ==========================================
  // 版面：50/50 上下分割 + MultiGrid 風格頁籤
  // ==========================================
  function injectSplitLayout() {
    const cardBody = document.querySelector('.card-body');
    if (!cardBody) return;

    // 注入 CSS
    const style = document.createElement('style');
    style.textContent = `
      /* 上下分割 */
      .card-body { gap: 0 !important; }
      .table-fix-wrapper { flex: 1 1 0 !important; min-height: 80px !important; max-height: none !important; }
      #tax401DetailPanel { flex: 1 1 0; min-height: 80px; display: flex; flex-direction: column; overflow: hidden; }

      /* 上下拖曳器 */
      #tax401Splitter { height: 6px; background: #d0d0d0; cursor: row-resize; flex-shrink: 0; user-select: none; }
      #tax401Splitter:hover, #tax401Splitter.dragging { background: #4a90e2; }
      #tax401DetailPanel .nav-tabs { flex-shrink: 0; }
      #tax401DetailPanel .tab-content { flex: 1; overflow-y: auto; overflow-x: auto; }

      /* 上半 grid 緊密化 */
      #mainGrid.table { font-size: .95rem !important; }
      .cell-view, .cell-edit { height: 18px !important; min-height: 18px !important; max-height: 18px !important; line-height: 14px !important; padding: 1px 3px !important; font-size: .95rem !important; }
      #mainGrid th { padding: 1px 4px !important; font-size: .95rem !important; }
      #mainGrid td { padding: 0 2px !important; }

      /* MultiGrid 風格頁籤樣式 */
      #tax401DetailPanel .nav-tabs { border-bottom: 2px solid #dee2e6; background: #f6f7f9; padding: 0 4px; }
      #tax401DetailPanel .nav-link { font-size: 15px; padding: 4px 14px; border-radius: 6px 6px 0 0; color: #495057; }
      #tax401DetailPanel .nav-link.active { background: #fff; border-color: #dee2e6 #dee2e6 #fff; font-weight: 600; color: #1a56db; }
      #tax401DetailPanel .tab-content { background: #fff; border: 1px solid #dee2e6; border-top: none; font-size: 15px; }
      #tax401DetailPanel .tab-pane { padding: 6px 8px; white-space: nowrap; }

      /* 下半表格不撐滿、自適應內容寬度 */
      #tax401DetailPanel .table { width: auto; }
      #tax401DetailPanel .tab-pane > .row { width: max-content; min-width: 100%; }

      /* 欄位拖曳調寬 */
      #tax401DetailPanel .table th { position: relative; user-select: none; }
      #tax401DetailPanel .table th .dtl-resizer { position: absolute; top: 0; right: -3px; width: 6px; height: 100%; cursor: col-resize; user-select: none; z-index: 1; }
      #tax401DetailPanel .table th .dtl-resizer:hover { background: rgba(59,130,246,.3); }

      /* 查核 modal 表格欄寬 */
      #taxCheckModal .modal-header { padding: 2px 1rem; border-bottom: none; margin-bottom: 0; }
      #taxCheckModal .modal-body { overflow: auto; padding: 0; }
      #taxCheckTable { margin-top: 0; border-collapse: collapse; }
      #taxCheckModal .table-responsive { overflow: visible; }
      #taxCheckTable thead th { position: sticky; top: 0; z-index: 2; background: #e9ecef; user-select: none; white-space: nowrap; }
      #taxCheckTable td { white-space: nowrap; }
      #taxCheckTable th .chk-resizer { position: absolute; top: 0; right: -3px; width: 6px; height: 100%; cursor: col-resize; user-select: none; z-index: 1; }
      #taxCheckTable th .chk-resizer:hover { background: rgba(59,130,246,.3); }

      /* 表格內 input 填滿 cell 寬度 */
      #tax401DetailPanel .table td .form-control-sm,
      #tax401DetailPanel .table td .dtl-field { width: 100% !important; box-sizing: border-box; }
      #tax401DetailPanel .table { table-layout: fixed; }

      /* 下半頁籤內緊密化 */
      #tax401DetailPanel .dtl-field { height: 22px !important; min-height: 22px !important; padding: 1px 4px !important; font-size: 15px !important; }
      #tax401DetailPanel .mb-1 { margin-bottom: 2px !important; }
      #tax401DetailPanel label { font-size: 15px; line-height: 1.2; }
      #tax401DetailPanel .form-check-label { font-size: 15px; }
      #tax401DetailPanel .table th, #tax401DetailPanel .table td { padding: 1px 4px !important; font-size: 15px; }
      #tax401DetailPanel .table .form-control-sm { height: 22px !important; min-height: 22px !important; padding: 1px 4px !important; font-size: 15px !important; }
      #tax401DetailPanel fieldset { padding: 4px 8px !important; margin-bottom: 4px !important; }
      #tax401DetailPanel legend { font-size: 15px !important; margin-bottom: 2px; }
      #tax401DetailPanel .form-check { margin-bottom: 0; min-height: auto; padding-top: 0; padding-bottom: 0; }
      #tax401DetailPanel h6 { font-size: 15px; margin-bottom: 4px !important; padding-bottom: 2px !important; }
      #tax401DetailPanel p.small { font-size: 13px; margin-top: 2px !important; margin-bottom: 4px !important; }
    `;
    document.head.appendChild(style);

    // 結轉按鈕放到 toolbar
    const toolbarMain = document.querySelector('.toolbar-main .d-flex');
    if (toolbarMain) {
      toolbarMain.insertAdjacentHTML('beforeend',
        `<button type="button" id="btnTaxTune" class="btn toolbar-btn" style="color:#0d6efd; border-color:#abc4f8;">
          <i class="bi bi-arrow-repeat"></i> 結轉
        </button>
        <button type="button" id="btnTaxCheck" class="btn toolbar-btn" style="color:#0d6efd; border-color:#abc4f8;">
          <i class="bi bi-list-check"></i> 明細
        </button>`
      );
    }

    // 建立下半部面板
    const panel = document.createElement('div');
    panel.id = 'tax401DetailPanel';
    panel.innerHTML = `
      <ul class="nav nav-tabs flex-wrap mb-0" id="tax401Tabs" role="tablist">
        <li class="nav-item" role="presentation"><button class="nav-link active" data-bs-toggle="tab" data-bs-target="#tab401Basic" type="button" role="tab">基本資料</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab401Sales" type="button" role="tab">銷項</button></li>
        <li class="nav-item tax403-nav" role="presentation" style="display:none;"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab401SpSale" type="button" role="tab">特種銷項</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab401Purchase" type="button" role="tab">進項</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab401TaxCalc" type="button" role="tab">稅額計算</button></li>
        <li class="nav-item" role="presentation"><button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab401Other" type="button" role="tab">其它項目</button></li>
      </ul>
      <div class="tab-content pt-0">
        ${buildBasicTab()}
        ${buildSalesTab()}
        ${buildSpSaleTab()}
        ${buildPurchaseTab()}
        ${buildTaxCalcTab()}
        ${buildOtherTab()}
      </div>
    `;

    // 建立��曳器
    const splitter = document.createElement('div');
    splitter.id = 'tax401Splitter';
    splitter.title = '拖曳調整上下區域高度';

    // 插入到 table-fix-wrapper 之後
    const wrapper = cardBody.querySelector('.table-fix-wrapper');
    if (wrapper) {
      wrapper.insertAdjacentElement('afterend', splitter);
      splitter.insertAdjacentElement('afterend', panel);
    } else {
      cardBody.appendChild(splitter);
      cardBody.appendChild(panel);
    }

    // 拖曳邏輯
    bindVerticalSplitter(splitter, wrapper, panel);

  }

  // ---- 輸入欄位 helper ----
  function inp(field, w, readonly) {
    const wStyle = w ? `width:${w}px;` : '';
    const ro = readonly ? `readonly style="background:#eee;${wStyle}"` : (wStyle ? `style="${wStyle}"` : '');
    return `<input type="text" class="form-control form-control-sm dtl-field" id="dtl_${field}" data-field="${field}" ${ro} />`;
  }

  function formRow(label, field, opts = {}) {
    const w = opts.width || 0;
    const ro = opts.readonly || false;
    const cls = opts.cls403 ? 'tax403-field' : '';
    const hide = opts.cls403 ? 'style="display:none;"' : '';
    const suffix = opts.suffix || '';
    return `<div class="d-flex align-items-center mb-1 ${cls}" ${hide}>
      <label class="text-end me-1" style="min-width:${opts.labelWidth || 100}px;">${label}</label>
      ${inp(field, w, ro)}${suffix ? `<span class="ms-1">${suffix}</span>` : ''}
    </div>`;
  }

  // ---- Tab 1: 基本資料 ----
  function buildBasicTab() {
    return `<div class="tab-pane fade show active" id="tab401Basic" role="tabpanel">
      <div class="row">
        <div class="col-md-6">
          ${formRow('統一編號', 'UniformId', { readonly: true, width: 150 })}
          ${formRow('營業人名稱', 'BusinessName', { readonly: true, width: 300 })}
          ${formRow('稅籍編號', 'TaxNum', { readonly: true, width: 150 })}
          ${formRow('負責人姓名', 'BossName', { readonly: true, width: 150 })}
          ${formRow('營業地址', 'Address', { readonly: true })}
          <div class="d-flex align-items-center mb-1">
            <label class="text-end me-1" style="min-width:100px;">期別狀態</label>
            <span id="dtl_CompletedCaption" class="ms-1 fw-bold"></span>
          </div>
          ${formRow('申報期別', 'HisId', { readonly: true, width: 65 })}
          ${formRow('使用發票份數', 'InvCount', { readonly: true, width: 65, suffix: '份' })}
        </div>
        <div class="col-md-2">
          <fieldset class="border rounded p-2 mb-2">
            <legend class="w-auto px-1" style="font-size:13px;">註記欄</legend>
            <div id="dtl_Notes_group">
              <div class="form-check"><input class="form-check-input" type="radio" name="dtl_Notes" id="dtl_Notes_0" value="0" disabled><label class="form-check-label" for="dtl_Notes_0">核准按月申報</label></div>
              <div class="form-check"><input class="form-check-input" type="radio" name="dtl_Notes" id="dtl_Notes_1" value="1" disabled><label class="form-check-label" for="dtl_Notes_1">總機構彙總申報</label></div>
              <div class="form-check"><input class="form-check-input" type="radio" name="dtl_Notes" id="dtl_Notes_2" value="2" disabled><label class="form-check-label" for="dtl_Notes_2">各單位分別申報</label></div>
            </div>
          </fieldset>
          <fieldset class="border rounded p-2">
            <legend class="w-auto px-1" style="font-size:13px;">本期(月)應退稅處理方式</legend>
            <div id="dtl_TaxBackType_group">
              <div class="form-check"><input class="form-check-input" type="radio" name="dtl_TaxBackType" id="dtl_TaxBackType_0" value="0" disabled><label class="form-check-label" for="dtl_TaxBackType_0">利用存款帳戶劃撥</label></div>
              <div class="form-check"><input class="form-check-input" type="radio" name="dtl_TaxBackType" id="dtl_TaxBackType_1" value="1" disabled><label class="form-check-label" for="dtl_TaxBackType_1">領取退稅支票</label></div>
            </div>
          </fieldset>
        </div>
      </div>
    </div>`;
  }

  // ---- 帶代號的 cell：代號 + input ----
  function ci(code, field, readonly) {
    if (!field) return '';
    return `<div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">${code}</span>${inp(field, 0, readonly)}</div>`;
  }

  // ---- Tab 2: 銷項 ----
  function buildSalesTab() {
    return `<div class="tab-pane fade" id="tab401Sales" role="tabpanel">
      <table class="table table-sm table-bordered mb-0">
        <thead><tr>
          <th style="width:200px;"></th>
          <th class="text-center" style="width:150px;">應稅銷售額</th>
          <th class="text-center" style="width:150px;">應稅稅額</th>
          <th class="text-center" style="width:150px;">零稅率銷售額</th>
          <th class="text-center tax403-col" style="width:150px; display:none;">免稅銷售額</th>
        </tr></thead>
        <tbody>
          <tr><td class="text-nowrap">三聯式發票、電子計算機發票</td>
            <td>${ci('1','Col1')}</td><td>${ci('2','Col2')}</td><td></td>
            <td class="tax403-col" style="display:none;">${ci('4','PlusCol4')}</td></tr>
          <tr><td class="text-nowrap">收銀機(三聯式)及電子發票</td>
            <td>${ci('5','Col5')}</td><td>${ci('6','Col6')}</td><td>${ci('7','Col7')}</td>
            <td class="tax403-col" style="display:none;">${ci('8','PlusCol8')}</td></tr>
          <tr><td class="text-nowrap">二聯式、收銀機發票(二聯式)</td>
            <td>${ci('9','Col9')}</td><td>${ci('10','Col10')}</td><td></td>
            <td class="tax403-col" style="display:none;">${ci('12','PlusCol12')}</td></tr>
          <tr><td class="text-nowrap">免用發票</td>
            <td>${ci('13','Col13')}</td><td>${ci('14','Col14')}</td><td>${ci('15','Col15')}</td>
            <td class="tax403-col" style="display:none;">${ci('16','PlusCol16')}</td></tr>
          <tr><td class="text-nowrap">減：退回及折讓</td>
            <td>${ci('17','Col17')}</td><td>${ci('18','Col18')}</td><td>${ci('19','Col19')}</td>
            <td class="tax403-col" style="display:none;">${ci('20','PlusCol20')}</td></tr>
          <tr><td class="text-nowrap">合　　計</td>
            <td>${ci('21','Col21',true)}</td><td>${ci('22','Col22',true)}</td><td>${ci('23','Col23',true)}</td>
            <td class="tax403-col" style="display:none;">${ci('24','PlusCol24',true)}</td></tr>
          <tr><td class="text-nowrap">銷售額總計</td>
            <td>${ci('25','Col25',true)}</td><td></td><td>${ci('27','Col27')}</td>
            <td class="tax403-col" style="display:none;">${ci('26','PlusCol26')}</td></tr>
        </tbody>
      </table>
    </div>`;
  }

  // ---- Tab 3: 特種銷項 (403 only) ----
  function buildSpSaleTab() {
    function spCi(codeAmt, idAmt, codeTax, idTax) {
      const roStyle = 'style="background:#eee;"';
      const amtInp = idAmt ? `<div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">${codeAmt}</span><input type="text" class="form-control form-control-sm" id="dtl_${idAmt}" ${roStyle} readonly /></div>` : '';
      const taxInp = idTax ? `<div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">${codeTax}</span><input type="text" class="form-control form-control-sm" id="dtl_${idTax}" ${roStyle} readonly /></div>` : '';
      return { amt: amtInp, tax: taxInp };
    }
    return `<div class="tab-pane fade" id="tab401SpSale" role="tabpanel">
      <table class="table table-sm table-bordered mb-0">
        <thead><tr>
          <th style="width:220px;"></th>
          <th class="text-center" style="width:150px;">應稅銷售額</th>
          <th class="text-center" style="width:150px;">應稅稅額</th>
        </tr></thead>
        <tbody>
          <tr><td class="text-nowrap">特種飲食業 25%</td>
            <td>${spCi('52','SpCalc52','53','SpCalc53').amt}</td>
            <td>${spCi('52','SpCalc52','53','SpCalc53').tax}</td></tr>
          <tr><td class="text-nowrap">　　　　　 15%</td>
            <td>${spCi('54','SpCalc54','55','SpCalc55').amt}</td>
            <td>${spCi('54','SpCalc54','55','SpCalc55').tax}</td></tr>
          <tr><td class="text-nowrap">銀行保險及專屬本業收入 2%</td>
            <td>${spCi('56','SpCalc56','57','SpCalc57').amt}</td>
            <td>${spCi('56','SpCalc56','57','SpCalc57').tax}</td></tr>
          <tr><td class="text-nowrap">信託投資業</td>
            <td>${spCi('58','SpCalc58','59','SpCalc59').amt}</td>
            <td>${spCi('58','SpCalc58','59','SpCalc59').tax}</td></tr>
          <tr><td class="text-nowrap">再保收入 1%</td>
            <td>${spCi('60','SpCalc60','61','SpCalc61').amt}</td>
            <td>${spCi('60','SpCalc60','61','SpCalc61').tax}</td></tr>
          <tr><td class="text-nowrap">免稅收入</td>
            <td>${ci('62','PlusCol62')}</td><td></td></tr>
          <tr><td class="text-nowrap">減：退回及折讓</td>
            <td>${ci('63','PlusCol63',true)}</td><td>${ci('64','PlusCol64',true)}</td></tr>
          <tr><td class="text-nowrap">合　　計</td>
            <td>${ci('65','PlusCol65',true)}</td><td>${ci('66','PlusCol66',true)}</td></tr>
        </tbody>
      </table>
    </div>`;
  }

  // ---- Tab 4: 進項 ----
  function buildPurchaseTab() {
    return `<div class="tab-pane fade" id="tab401Purchase" role="tabpanel">
      <table class="table table-sm table-bordered mb-0">
        <thead><tr>
          <th style="width:200px;"></th>
          <th style="width:80px;"></th>
          <th class="text-center" style="width:140px;">得扣抵金額</th>
          <th class="text-center" style="width:140px;">得扣抵稅額</th>
        </tr></thead>
        <tbody>
          <tr><td rowspan="2" class="text-nowrap align-middle">統一發票扣抵聯<br><small>(含電子計算機發票)</small></td>
            <td class="small">進費及費用</td><td>${ci('28','Col28')}</td><td>${ci('29','Col29')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('30','Col30')}</td><td>${ci('31','Col31')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">三聯式收銀機<br><small>發票扣抵聯</small></td>
            <td class="small">進費及費用</td><td>${ci('32','Col32')}</td><td>${ci('33','Col33')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('34','COl34')}</td><td>${ci('35','Col35')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">載有稅額之其他憑證<br><small>含二聯式收銀機發票</small></td>
            <td class="small">進費及費用</td><td>${ci('36','Col36')}</td><td>${ci('37','COl37')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('38','Col38')}</td><td>${ci('39','Col39')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">海關代徵營業稅<br><small>繳納證扣抵聯</small></td>
            <td class="small">進費及費用</td><td>${ci('78','Col78')}</td><td>${ci('79','COl79')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('80','Col80')}</td><td>${ci('81','Col81')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">減：退出、折讓及海關<br><small>退還溢繳稅款</small></td>
            <td class="small">進費及費用</td><td>${ci('40','Col40')}</td><td>${ci('41','Col41')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('42','Col42')}</td><td>${ci('43','Col43')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">合　　計</td>
            <td class="small">進費及費用</td><td>${ci('44','Col44')}</td><td>${ci('45','Col45')}</td></tr>
          <tr><td class="small">固定資產</td><td>${ci('46','Col46')}</td><td>${ci('47','Col47')}</td></tr>

          <tr><td rowspan="2" class="text-nowrap align-middle">進項總金額</td>
            <td class="small text-nowrap">進費及費用</td>
              <td colspan="2">
                <div class="d-flex align-items-center">
                  ${ci('48','Col48',true)}
                  <span class="ms-1">元</span>
                </div>
              </td>
            </tr>
          <tr><td class="small text-nowrap">固定資產</td>
              <td colspan="2">
                <div class="d-flex align-items-center">
                  ${ci('49','Col49',true)}
                  <span class="ms-1">元</span>
                </div>
              </td>
            </tr>
        </tbody>
      </table>
    </div>`;
  }

  // ---- Tab 5: 稅額計算 ----
  function buildTaxCalcTab() {
    function calcRow(code, label, formula, field, readonly) {
      return `<tr>
        <td class="text-nowrap">${label}</td>
        <td class="text-nowrap small text-muted">${formula}</td>
        <td>${ci(code, field, readonly)}</td>
      </tr>`;
    }
    return `<div class="tab-pane fade" id="tab401TaxCalc" role="tabpanel">
      <div class="row">
        <div class="col-lg-7">
          <table class="table table-sm table-bordered mb-0">
            <thead><tr>
              <th class="text-center">代　　號　　項　　目</th>
              <th style="width:110px;">公式</th>
              <th class="text-center" style="width:160px;">稅　額</th>
            </tr></thead>
            <tbody>
              ${calcRow('101', '1 本(期)銷項稅額合計', '(22)', 'Col22', true)}
              <tr class="tax403-field" style="display:none;">
                <td>3 購買國外勞務應納稅額</td><td></td><td>${ci('103', 'PlusCol76', true)}</td></tr>
              <tr class="tax403-field" style="display:none;">
                <td>4 特種稅額計算之應納稅額</td><td></td>
                <td><div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">104</span><input type="text" class="form-control form-control-sm" id="dtl_SpCalc104" style="background:#eee;" readonly /></div></td></tr>
              <tr class="tax403-field" style="display:none;">
                <td>5 中途歇業年底調整補徵</td><td></td><td>${ci('105', 'PlusCol105')}</td></tr>
              <tr class="tax403-field" style="display:none;">
                <td>6 小計</td><td class="text-nowrap small text-muted">(1+2+3+4+5)</td><td>${ci('106', 'PlusCol106', true)}</td></tr>
              ${calcRow('107', '7 得扣抵進項稅額合計', '<span id="lblFormula107">(45)+(47)</span>', 'Col107', false)}
              ${calcRow('108', '8 上期(月)累積留抵稅額', '', 'Col108', false)}
              <tr class="tax403-field" style="display:none;">
                <td>9 中途歇業年底調整應退稅額</td><td></td><td>${ci('109', 'PlusCol109')}</td></tr>
              ${calcRow('110', '10 小計', '<span id="lblTail1">(7+8)</span>', 'Col110', true)}
              ${calcRow('111', '11 本期(月)應實繳稅額', '<span id="lblTail2">(1-10)</span>', 'Col111', true)}
              ${calcRow('112', '12 本期(月)申報留抵稅額', '<span id="lblTail3">(10-1)</span>', 'Col112', true)}
              ${calcRow('113', '13 得退稅限額合計', '(23)×5%+(47)', 'Col113', true)}
              ${calcRow('114', '14 本期(月)應退稅額', '', 'Col114', false)}
              ${calcRow('115', '15 本期(月)累積留抵稅額', '(12-14)', 'Col115', true)}
            </tbody>
          </table>
        </div>
        <div class="col-lg-5 tax403-field" style="display:none;">
          <div class="border rounded p-2">
            <h6>403 調整計算</h6>
            <button type="button" class="btn btn-sm btn-outline-primary mb-1" id="btnPreviewRPT">預覽調整計算表</button>
            <button type="button" class="btn btn-sm btn-outline-success mb-1" id="btnUpdateRPTData">轉入全年調整資料</button>
          </div>
        </div>
      </div>
    </div>`;
  }

  // ---- Tab 6: 其它項目 ----
  function buildOtherTab() {
    return `<div class="tab-pane fade" id="tab401Other" role="tabpanel">
      <div class="row">
        <div class="col-md-6">
          <h6 class="border-bottom pb-1 mb-2">進口免稅貨物及購買國外勞務</h6>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:150px;">　進口免稅貨物 73</label>
            ${inp('Col73', 120)}
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:150px;">　購買國外勞務 74</label>
            ${inp('Col74', 120)}
          </div>
          <p class="small text-muted ms-2 mt-1">免稅出口區內之區內事業、科學工業園區內之園區事業及<br>
            海關管理之保稅工廠、保稅倉庫或物流中心按進口報關程<br>
            序銷售貨物至我國境內其他地區之免開立統一發票銷售額</p>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:150px;">　上述銷售額 82</label>
            ${inp('Col82', 120)}
          </div>

          <div class="tax403-field mb-3" style="display:none;">
            <div class="border rounded p-2">
              <h6>403 其它項目</h6>
              <table class="table table-sm table-bordered mb-0">
                <thead><tr>
                  <th style="width:220px;">項　目</th>
                  <th class="text-center" style="width:120px;">公式</th>
                  <th class="text-center" style="width:160px;">金額</th>
                </tr></thead>
                <tbody>
                  <tr><td class="text-nowrap">購買國外勞務 應比例計算之進項稅額</td>
                    <td class="text-nowrap small text-muted">74 x 稅率</td>
                    <td>${ci('75', 'PlusCol75')}</td></tr>
                  <tr><td class="text-nowrap">　　　　　　 應納稅額</td>
                    <td class="text-nowrap small text-muted">75 x 50</td>
                    <td><div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">76</span><input type="text" class="form-control form-control-sm dtl-field" id="dtl_PlusCol76_oth" data-field="PlusCol76" style="background:#eee;" readonly /></div></td></tr>
                  <tr><td class="text-nowrap">不得扣抵比例</td>
                    <td class="text-nowrap small text-muted">[24+65-26]/[25-26]</td>
                    <td><div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">50</span>${inp('PlusCol50', 0)}<span class="ms-1">%</span></div></td></tr>
                  <tr><td class="text-nowrap">得扣抵之進項稅額</td>
                    <td class="text-nowrap small text-muted">[45+47]x[1-50]</td>
                    <td><div class="d-flex align-items-center"><span class="text-muted me-1" style="font-size:11px;min-width:16px;">51</span>${inp('PlusCol51', 0)}<span class="ms-1">元</span></div></td></tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
        <div class="col-md-6">
          <h6 class="border-bottom pb-1 mb-2">附件份數</h6>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">1. 統一發票明細表</label>
            ${inp('InvDtlCount', 60)}<span class="ms-1">冊</span>
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">2. 進項憑證</label>
            ${inp('IsInCerVolume', 60)}
            <span class="ms-1 me-2">冊</span>
            ${inp('IsInCerCount', 60)}
            <span class="ms-1">份</span>
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">3. 海關代徵營業稅繳納證</label>
            ${inp('CustomsCount', 60)}<span class="ms-1">冊</span>
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">4. 退回(出)及折讓證明單</label>
            ${inp('DiscountCount', 60)}<span class="ms-1">冊</span>
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">5. 營業稅繳款書申報聯</label>
            ${inp('TaxReportCount', 60)}<span class="ms-1">冊</span>
          </div>
          <div class="d-flex align-items-center mb-1">
            <label class="me-1" style="min-width:170px;">6. 零稅率銷售額清單</label>
            ${inp('NoTaxedCount', 60)}<span class="ms-1">冊</span>
          </div>
        </div>
      </div>
    </div>`;
  }

  // ==========================================
  // 頁籤欄位鎖定/解鎖 (對應 Delphi DoEditLock)
  // ==========================================
  function setDetailEditLock(locked) {
    isEditMode = !locked;

    // 所有 dtl-field input
    document.querySelectorAll('#tax401DetailPanel .dtl-field').forEach(el => {
      const field = el.dataset.field;
      if (!locked && editableFields.has(field)) {
        el.removeAttribute('readonly');
        el.style.background = '';
      } else {
        el.setAttribute('readonly', 'readonly');
        el.style.background = '#eee';
      }
    });

    // Radio buttons: Notes, TaxBackType
    document.querySelectorAll('input[name="dtl_Notes"], input[name="dtl_TaxBackType"]').forEach(r => {
      r.disabled = locked;
    });
  }

  // ==========================================
  // 從 DB 取得完整單筆資料並填入頁籤
  // ==========================================
  async function fetchAndPopulateTabs() {
    const gridData = readSelectedRow();
    if (!gridData) { populateTabs(null); return; }

    const hisId = getVal(gridData, 'HisId');
    const useId = getVal(gridData, 'UseId');
    if (!hisId || !useId) { populateTabs(gridData); return; }

    try {
      const safeUseId = useId.replace(/'/g, "''");
      const safeHisId = hisId.replace(/'/g, "''");
      const rows = await queryDirect('ATXdTable401', `UseId='${safeUseId}' AND HisId='${safeHisId}'`);
      const fullData = (rows && rows.length > 0) ? rows[0] : gridData;
      populateTabs(fullData);
    } catch (e) {
      console.error('取得完整資料失敗:', e);
      populateTabs(gridData);
    }
  }

  // ==========================================
  // 填入頁籤資料
  // ==========================================
  function populateTabs(data) {
    if (!data) {
      document.querySelectorAll('#tax401DetailPanel .dtl-field').forEach(el => { el.value = ''; });
      const caption = document.getElementById('dtl_CompletedCaption');
      if (caption) caption.textContent = '';
      return;
    }

    // 填入所有 dtl-field
    document.querySelectorAll('#tax401DetailPanel .dtl-field').forEach(el => {
      const field = el.dataset.field;
      if (field && data[field] !== undefined) {
        el.value = data[field] ?? '';
      }
    });

    // 403 其它項目中的 PlusCol76 複製欄
    const plc76Oth = document.getElementById('dtl_PlusCol76_oth');
    if (plc76Oth) plc76Oth.value = getVal(data, 'PlusCol76');

    // 期別狀態
    const caption = document.getElementById('dtl_CompletedCaption');
    if (caption) {
      const completedCaption = getVal(data, 'CompletedCaption');
      if (completedCaption) {
        caption.textContent = completedCaption;
      } else {
        const completed = getVal(data, 'Completed');
        caption.textContent = (completed === '1' || completed === 'True' || completed === 'true') ? '已完成' : '未完成';
      }
    }

    // Radio: Notes (預設值 '0')
    const notesVal = getVal(data, 'Notes') || '0';
    document.querySelectorAll('input[name="dtl_Notes"]').forEach(r => {
      r.checked = (r.value === notesVal);
    });

    // Radio: TaxBackType (預設值 '0')
    const taxBackVal = getVal(data, 'TaxBackType') || '0';
    document.querySelectorAll('input[name="dtl_TaxBackType"]').forEach(r => {
      r.checked = (r.value === taxBackVal);
    });

    check403Mode(data);
  }

  // ==========================================
  // 403 模式切換
  // ==========================================
  function check403Mode(data) {
    is403 = getNum(data, 'Is403') === 1;

    document.querySelectorAll('.tax403-nav').forEach(el => {
      el.style.display = is403 ? '' : 'none';
    });
    document.querySelectorAll('.tax403-col').forEach(el => {
      el.style.display = is403 ? '' : 'none';
    });
    document.querySelectorAll('.tax403-field').forEach(el => {
      el.style.display = is403 ? '' : 'none';
    });

    const lbl1 = document.getElementById('lblTail1');
    const lbl2 = document.getElementById('lblTail2');
    const lbl3 = document.getElementById('lblTail3');
    if (lbl1) lbl1.textContent = is403 ? '(7+8+9)' : '(7+8)';
    if (lbl2) lbl2.textContent = is403 ? '(6-10)' : '(1-10)';
    if (lbl3) lbl3.textContent = is403 ? '(10-6)' : '(10-1)';
  }

  // ==========================================
  // 稅額自動計算
  // ==========================================
  function setupCalculations() {
    ['dtl_Col107', 'dtl_Col108'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.addEventListener('change', () => {
        const v107 = parseInt(document.getElementById('dtl_Col107')?.value, 10) || 0;
        const v108 = parseInt(document.getElementById('dtl_Col108')?.value, 10) || 0;
        const v110 = v107 + v108;
        setDtlValue('Col110', fmtNum(v110));
        calcFromCol110(v110);
      });
    });

    const el114 = document.getElementById('dtl_Col114');
    if (el114) el114.addEventListener('change', () => {
      const v112 = parseInt(document.getElementById('dtl_Col112')?.value, 10) || 0;
      const v114 = parseInt(el114.value, 10) || 0;
      setDtlValue('Col115', fmtNum(v112 - v114));
    });
  }

  function calcFromCol110(v110) {
    const v22 = parseInt(document.getElementById('dtl_Col22')?.value, 10) || 0;
    const baseVal = is403
      ? (parseInt(document.getElementById('dtl_PlusCol106')?.value, 10) || v22)
      : v22;

    const v111 = Math.max(0, baseVal - v110);
    setDtlValue('Col111', fmtNum(v111));

    const v112 = Math.max(0, v110 - baseVal);
    setDtlValue('Col112', fmtNum(v112));

    const v113 = parseInt(document.getElementById('dtl_Col113')?.value, 10) || 0;
    const v114 = Math.min(v112, v113);
    setDtlValue('Col114', fmtNum(v114));

    setDtlValue('Col115', fmtNum(v112 - v114));
  }

  function setDtlValue(field, value) {
    const el = document.getElementById('dtl_' + field);
    if (el) el.value = value;
    updateGridCell(field, value);
  }

  // ==========================================
  // 頁籤欄位變更 → 同步到 Grid
  // ==========================================
  function bindTabFieldSync() {
    document.querySelectorAll('#tax401DetailPanel .dtl-field').forEach(el => {
      el.addEventListener('change', () => {
        const field = el.dataset.field;
        if (field) updateGridCell(field, el.value);
      });
    });

    document.querySelectorAll('input[name="dtl_Notes"]').forEach(r => {
      r.addEventListener('change', () => {
        if (r.checked) updateGridCell('Notes', r.value);
      });
    });
    document.querySelectorAll('input[name="dtl_TaxBackType"]').forEach(r => {
      r.addEventListener('change', () => {
        if (r.checked) updateGridCell('TaxBackType', r.value);
      });
    });
  }

  // ==========================================
  // 結轉設定 (btTune)
  // ==========================================
  function ensureTuneModal() {
    let modal = document.getElementById('taxTuneModal');
    if (modal) return modal;

    const wrapper = document.createElement('div');
    wrapper.innerHTML = `
    <div class="modal fade" id="taxTuneModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered" style="max-width:600px;">
        <div class="modal-content" style="width:500px; height:250px; display:flex; flex-direction:column;">
          <div class="modal-header py-2">
            <h6 class="modal-title mb-0">營業稅結轉</h6>
            <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body py-2" style="flex:1; overflow-y:auto;">
            <div class="row mb-2 align-items-center">
              <label class="col-3 col-form-label col-form-label-sm text-end">期別</label>
              <div class="col-5"><input type="text" class="form-control form-control-sm" id="tuneHisId" placeholder="例: 115.01" /></div>
            </div>
            <div class="row mb-2 align-items-center">
              <label class="col-3 col-form-label col-form-label-sm text-end">公司別</label>
              <div class="col-7"><select class="form-select form-select-sm" id="tuneUseId"><option value="">-- 請選擇 --</option></select></div>
            </div>
            <div class="row mb-2 align-items-center">
              <label class="col-3 col-form-label col-form-label-sm text-end">申報別</label>
              <div class="col-5">
                <select class="form-select form-select-sm" id="tuneType">
                  <option value="401">401</option>
                  <option value="403">403</option>
                </select>
              </div>
            </div>
          </div>
          <div class="modal-footer py-2">
            <button type="button" class="btn btn-sm btn-secondary" data-bs-dismiss="modal">取消</button>
            <button type="button" class="btn btn-sm btn-primary" id="btnTuneConfirm">確定結轉</button>
          </div>
        </div>
      </div>
    </div>`;
    document.body.appendChild(wrapper.firstElementChild);
    modal = document.getElementById('taxTuneModal');

    document.getElementById('btnTuneConfirm')?.addEventListener('click', handleTuneConfirm);
    return modal;
  }

  async function handleTune() {
    const modal = ensureTuneModal();

    try {
      const sel = document.getElementById('tuneUseId');
      if (sel && sel.options.length <= 1) {
        const list = await fetch('/api/TableFieldLayout/LookupData?table=CURdBU&key=BUId&result=BUName');
        if (list.ok) {
          const data = await list.json();
          (data || []).forEach(item => {
            const opt = document.createElement('option');
            opt.value = item.key ?? '';
            opt.textContent = `${item.key ?? ''} ${item.result0 ?? ''}`.trim();
            sel.appendChild(opt);
          });
        }
      }
    } catch (e) {
      console.error('載入公司別失敗:', e);
    }

    // 預帶公司別 (對應 Delphi cboUseId.text := sUseId)
    const useEl = document.getElementById('tuneUseId');
    if (useEl) {
      const targetUseId = getUseId();
      useEl.value = targetUseId;
      // 若 select 中沒有匹配的 option，選第一個非空選項
      if (!useEl.value && useEl.options.length > 1) {
        useEl.selectedIndex = 1;
      }
    }

    // 從 CURdSysParams 取 CurrCostMonth 作為預設期別 (對應 Delphi btTuneClick)
    try {
      const paramRows = await queryDirect('CURdSysParams',
        "SystemId='ATX' and ParamId='CurrCostMonth'", ['Value']);
      const hisEl = document.getElementById('tuneHisId');
      if (hisEl && paramRows && paramRows.length > 0) {
        hisEl.value = (paramRows[0].Value ?? '').toString().trim();
      }
    } catch (e) {
      console.error('取得 CurrCostMonth 失敗:', e);
    }

    try {
      const rows = await queryDirect('CURdSysParams',
        "SystemId='ATX' and ParamId='Use403' and Value='1'");
      const typeRow = document.getElementById('tuneType');
      if (typeRow) {
        typeRow.closest('.mb-3').style.display = (rows && rows.length > 0) ? '' : 'none';
      }
    } catch (e) { /* ignore */ }

    if (window.bootstrap) {
      window.bootstrap.Modal.getOrCreateInstance(modal).show();
    }
  }

  async function handleTuneConfirm() {
    const hisId = document.getElementById('tuneHisId')?.value?.trim();
    const useId = document.getElementById('tuneUseId')?.value?.trim();
    const type = document.getElementById('tuneType')?.value || '401';

    if (!hisId) { Swal.fire({ icon: 'error', title: '請輸入期別！' }); return; }
    if (!useId) { Swal.fire({ icon: 'error', title: '請選擇公司別！' }); return; }

    try {
      await callSP('ATXdTaxHistorySet', {
        HisId: hisId,
        UseId: useId,
        Is403: type === '403' ? 1 : 0
      });

      await fetch('/api/SysParamsApi/delete-rows', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify([{ SystemId: 'ATX', ParamId: 'CurrCostMonth' }])
      });
      await fetch('/api/SysParamsApi/create', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ SystemId: 'ATX', ParamId: 'CurrCostMonth', Value: hisId })
      });

      const modal = document.getElementById('taxTuneModal');
      if (modal && window.bootstrap) {
        window.bootstrap.Modal.getInstance(modal)?.hide();
      }

      await Swal.fire({ icon: 'success', title: '結轉作業完成！' });
      await reloadGridAndLocate();
    } catch (e) {
      Swal.fire({ icon: 'error', title: '結轉失敗', text: e.message });
    }
  }

  // ==========================================
  // 查核 (btCheck)
  // ==========================================
  function ensureCheckModal() {
    let modal = document.getElementById('taxCheckModal');
    if (modal) return modal;

    const wrapper = document.createElement('div');
    wrapper.innerHTML = `
    <div class="modal" id="taxCheckModal" tabindex="-1">
      <div class="modal-dialog modal-xl modal-dialog-scrollable">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">歷史查詢</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="table-responsive">
              <table class="table table-sm table-bordered table-hover" id="taxCheckTable">
                <thead class="table-light">
                  <tr>
                    <th>作業人</th><th>格式代碼</th><th>檢查</th>
                    <th>稅籍編號</th><th>檢查</th>
                    <th>流水號</th><th>檢查</th>
                    <th>期別</th><th>檢查</th>
                    <th>買受人統編</th><th>檢查</th>
                    <th>銷售人統編</th><th>檢查</th>
                    <th>發票號碼</th><th>檢查</th>
                    <th>銷售金額</th><th>檢查</th>
                    <th>課稅別</th><th>檢查</th>
                    <th>營業稅額</th><th>檢查</th>
                    <th>扣抵代碼</th><th>檢查</th>
                    <th>彙加備記</th><th>檢查</th>
                    <th>詳細備記</th><th>檢查</th>
                    <th>原始單號</th><th>公司別</th>
                  </tr>
                </thead>
                <tbody id="taxCheckBody"></tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>`;
    document.body.appendChild(wrapper.firstElementChild);
    return document.getElementById('taxCheckModal');
  }

  async function handleCheck() {
    const row = readSelectedRow();
    if (!row) {
      Swal.fire({ icon: 'warning', title: '請先選擇一筆資料！' });
      return;
    }

    const hisId = getVal(row, 'HisId');
    const useId = getVal(row, 'UseId');
    if (!hisId) {
      Swal.fire({ icon: 'warning', title: '該筆資料無期別！' });
      return;
    }

    try {
      const resp = await fetch('/api/StoredProc/query', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          Key: 'ATXdTaxCheck',
          Args: { HisId: hisId, UseId: useId, UserId: getUserId(), IsInCond: '2', IsCurrectCond: '2' }
        })
      });
      if (!resp.ok) throw new Error('HTTP ' + resp.status);
      const items = await resp.json();

      const modal = ensureCheckModal();
      const tbody = document.getElementById('taxCheckBody');
      if (tbody) {
        if (items.length === 0) {
          tbody.innerHTML = '<tr><td colspan="29" class="text-center text-muted">查無資料</td></tr>';
        } else {
          tbody.innerHTML = items.map(r => `<tr>
            <td>${r.UserId || ''}</td>
            <td>${r.TaxTypeId ?? ''}</td><td>${r.ChkTaxTypeId ?? ''}</td>
            <td>${r.CompanyTaxNum || ''}</td><td>${r.ChkCompanyTaxNum ?? ''}</td>
            <td>${r.SerialNum ?? ''}</td><td>${r.ChkSerialNum ?? ''}</td>
            <td>${r.HisId || ''}</td><td>${r.ChkHisId ?? ''}</td>
            <td>${r.BUniformId || ''}</td><td>${r.ChkBUniformId ?? ''}</td>
            <td>${r.SUniformId || ''}</td><td>${r.ChkSUniformId ?? ''}</td>
            <td>${r.InvoiceNum || ''}</td><td>${r.ChkInvoiceNum ?? ''}</td>
            <td>${r.Amount ?? ''}</td><td>${r.ChkAmount ?? ''}</td>
            <td>${r.TaxActionType || ''}</td><td>${r.ChkTaxActionType ?? ''}</td>
            <td>${r.TaxAmount ?? ''}</td><td>${r.ChkTaxAmount ?? ''}</td>
            <td>${r.TaxCutTypeId ?? ''}</td><td>${r.ChkTaxCutTypeId ?? ''}</td>
            <td>${r.SumNotes || ''}</td><td>${r.ChkSumNotes ?? ''}</td>
            <td>${r.SpecialNotes || ''}</td><td>${r.ChkSpecialNotes ?? ''}</td>
            <td>${r.OrigSourceId || ''}</td><td>${r.OrgUseId || ''}</td>
          </tr>`).join('');
        }
      }

      if (window.bootstrap) {
        window.bootstrap.Modal.getOrCreateInstance(modal).show();
        setupCheckTableResizers();
      }
    } catch (e) {
      Swal.fire({ icon: 'error', title: '查核失敗', text: e.message });
    }
  }

  function setupCheckTableResizers() {
    const table = document.getElementById('taxCheckTable');
    if (!table) return;
    // 避免重複加 handle
    if (table.dataset.resizersReady) return;
    table.dataset.resizersReady = '1';

    table.querySelectorAll('thead th').forEach(th => {
      const handle = document.createElement('span');
      handle.className = 'chk-resizer';
      th.appendChild(handle);

      let startX, startW;
      handle.addEventListener('mousedown', e => {
        e.preventDefault();
        startX = e.clientX;
        startW = th.offsetWidth;
        const onMove = ev => {
          const diff = ev.clientX - startX;
          th.style.width = Math.max(30, startW + diff) + 'px';
          th.style.minWidth = th.style.width;
        };
        const onUp = () => {
          document.removeEventListener('mousemove', onMove);
          document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
      });
    });
  }

  // ==========================================
  // 重新載入 Grid 並定位到指定 HisId (對應 Delphi tblEasyDB.Close/Open + Locate)
  // ==========================================
  async function reloadGridAndLocate(hisId) {
    if (!hisId) {
      const row = readSelectedRow();
      if (row) hisId = getVal(row, 'HisId');
    }
    if (hisId) {
      sessionStorage.setItem('_bt000011_locateHisId', hisId);
    }
    location.reload();
  }

  function tryLocateRow() {
    const hisId = sessionStorage.getItem('_bt000011_locateHisId');
    if (!hisId) return;
    sessionStorage.removeItem('_bt000011_locateHisId');

    const grid = document.getElementById('mainGrid');
    if (!grid) return;
    const rows = grid.querySelectorAll('tbody tr');
    for (const tr of rows) {
      const td = tr.querySelector('td[data-field="HisId"]');
      const val = td?.querySelector('.cell-edit')?.value
        ?? td?.querySelector('.cell-view')?.textContent?.trim();
      if (val === hisId) {
        tr.click();
        tr.scrollIntoView({ block: 'nearest' });
        return;
      }
    }
  }

  // ==========================================
  // 轉入全年調整資料 (403)
  // ==========================================
  async function handleUpdateRPTData() {
    const row = readSelectedRow();
    if (!row) { Swal.fire({ icon: 'warning', title: '請先選擇一筆資料！' }); return; }

    const hisId = getVal(row, 'HisId');
    const useId = getVal(row, 'UseId');

    const confirm = await Swal.fire({
      icon: 'question',
      title: '確定要轉入全年調整資料？',
      showCancelButton: true,
      confirmButtonText: '確定',
      cancelButtonText: '取消'
    });
    if (!confirm.isConfirmed) return;

    try {
      await callSP('ATXdTaxUpdate403', { HisId: hisId, UseId: useId });
      await Swal.fire({ icon: 'success', title: '轉入完成！' });
      await reloadGridAndLocate(hisId);
    } catch (e) {
      Swal.fire({ icon: 'error', title: '執行失敗', text: e.message });
    }
  }

  // ==========================================
  // 預覽調整計算表 (403)
  // ==========================================
  async function handlePreviewRPT() {
    const row = readSelectedRow();
    if (!row) { Swal.fire({ icon: 'warning', title: '請先選擇一筆資料！' }); return; }
    const hisId = getVal(row, 'HisId');
    const useId = getVal(row, 'UseId');

    const btn = document.getElementById('btnPreviewRPT');
    const orig = btn?.innerHTML;
    if (btn) { btn.disabled = true; btn.textContent = '列印中...'; }
    try {
      const payload = {
        spName: 'ATXdTaxCalc',
        reportName: 'ATXdTaxCalc',
        params: { HisId: hisId, UseId: useId }
      };
      const res = await fetch('/api/report/generate-url', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      });
      if (!res.ok) throw new Error(await res.text());
      const blob = await res.blob();
      const blobUrl = URL.createObjectURL(blob);
      const win = window.open('about:blank', '_blank');
      if (win) {
        win.document.write(
          '<!DOCTYPE html><html><head><meta charset="utf-8"><title>調整計算表<\/title><\/head>'
          + '<body style="margin:0;overflow:hidden;">'
          + '<embed src="' + blobUrl + '" type="application\/pdf" style="position:fixed;top:0;left:0;width:100%;height:100%;" \/>'
          + '<\/body><\/html>'
        );
        win.document.close();
      } else {
        const a = document.createElement('a');
        a.href = blobUrl; a.target = '_blank';
        document.body.appendChild(a); a.click(); document.body.removeChild(a);
      }
      setTimeout(() => URL.revokeObjectURL(blobUrl), 60000);
    } catch (err) {
      Swal.fire({ icon: 'error', title: '列印失敗', text: err.message || '請稍後再試' });
    } finally {
      if (btn) { btn.disabled = false; btn.innerHTML = orig; }
    }
  }

  // ==========================================
  // 監聽 Grid 列選取
  // ==========================================
  function bindRowSelection() {
    const mainGrid = document.getElementById('mainGrid');
    if (!mainGrid) return;

    mainGrid.addEventListener('click', (ev) => {
      const tr = ev.target.closest('tbody tr');
      if (!tr) return;
      setTimeout(() => fetchAndPopulateTabs(), 50);
    });

    ['sgFirst', 'sgPrev', 'sgNext', 'sgLast'].forEach(btnId => {
      const btn = document.getElementById(btnId);
      if (btn) {
        btn.addEventListener('click', () => {
          setTimeout(() => fetchAndPopulateTabs(), 100);
        });
      }
    });

    setTimeout(() => {
      const data = readSelectedRow();
      if (data) fetchAndPopulateTabs();
    }, 300);
  }

  // ==========================================
  // 下半表格欄位寬度可拖曳調整
  // ==========================================
  function setupDetailTableResizers() {
    const panel = document.getElementById('tax401DetailPanel');
    if (!panel) return;

    panel.querySelectorAll('table.table thead th').forEach(th => {
      const handle = document.createElement('span');
      handle.className = 'dtl-resizer';
      th.appendChild(handle);

      let startX, startW;
      handle.addEventListener('mousedown', e => {
        e.preventDefault();
        startX = e.clientX;
        startW = th.offsetWidth;
        const onMove = ev => {
          const diff = ev.clientX - startX;
          th.style.width = Math.max(30, startW + diff) + 'px';
          th.style.minWidth = th.style.width;
        };
        const onUp = () => {
          document.removeEventListener('mousemove', onMove);
          document.removeEventListener('mouseup', onUp);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
      });
    });
  }

  // ==========================================
  // 初始化
  // ==========================================
  function init() {
    // 隱藏 Excel 按鈕
    document.getElementById('btnExportExcel')?.classList.add('d-none');

    injectSplitLayout();
    setupCalculations();
    bindTabFieldSync();
    bindRowSelection();
    setupDetailTableResizers();

    // 重載後定位到先前選取的列 (對應 Delphi Locate)
    setTimeout(() => tryLocateRow(), 500);

    // 初始鎖定所有頁籤欄位 (對應 Delphi btnGetParamsClick 中將所有欄位設為 ReadOnly)
    setDetailEditLock(true);

    // 攔截修改按鈕：加入確認提示 + 頁籤欄位鎖定/解鎖
    const editBtn = document.getElementById('btnEditToggle');
    if (editBtn) {
      let _skipConfirm = false;

      editBtn.addEventListener('click', (e) => {
        if (_skipConfirm) {
          _skipConfirm = false;
          // 進入編輯模式後解鎖頁籤欄位
          setTimeout(() => setDetailEditLock(false), 0);
          return;
        }

        const wrapper = document.querySelector('.table-wrapper');
        const wasEditing = wrapper?.classList.contains('edit-mode');

        if (!wasEditing) {
          // 進入編輯模式前：顯示確認對話框 (對應 Delphi btnUpdateClick)
          e.stopImmediatePropagation();
          e.preventDefault();

          Swal.fire({
            icon: 'question',
            title: '確定要編修內容？',
            showCancelButton: true,
            confirmButtonText: '是',
            cancelButtonText: '否'
          }).then(result => {
            if (result.isConfirmed) {
              _skipConfirm = true;
              editBtn.click();
            }
          });
        } else {
          // 離開編輯模式：鎖定頁籤欄位 (對應 Delphi btnBrowseClick → DoEditLock(1))
          setTimeout(() => setDetailEditLock(true), 50);
        }
      }, true); // capture phase，確保在原始 handler 之前執行
    }

    // 綁定 toolbar 結轉、明細按鈕
    document.getElementById('btnTaxTune')?.addEventListener('click', handleTune);
    document.getElementById('btnTaxCheck')?.addEventListener('click', handleCheck);

    // 綁定 403 按鈕
    document.getElementById('btnPreviewRPT')?.addEventListener('click', handlePreviewRPT);
    document.getElementById('btnUpdateRPTData')?.addEventListener('click', handleUpdateRPTData);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();