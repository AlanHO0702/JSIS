// AJ000005 損益表設定
// 將預設 Tabs 佈局轉換為 BalanceSheet 佈局
// 結構：損益項目(頂部) → 損益表項目設定(左側) → 損益表科目設定/匯總排除項目(右側 Tabs)

(function () {
  'use strict';

  const cfgs = window._mmdConfigs || {};
  const domId = Object.keys(cfgs).find(k => (cfgs[k].ItemId || '').toUpperCase() === 'AJ000005');
  if (!domId) return;

  const cfg = cfgs[domId];

  // ==========================================
  // 1. 覆寫 config 屬性
  // ==========================================
  cfg.Layout = 3; // BalanceSheet
  cfg.MasterHeight = 150;
  cfg.EnableDetailFocusCascade = true;
  cfg.EnableTopToolbar = true;
  cfg.EnableSplitters = true;
  cfg.EnableGridCounts = true;
  cfg.DetailCascadeMode = 2; // 扇出式：Detail[1]+ ← Detail[0]

  // ==========================================
  // 2. 從 Tabs 佈局搬移元素到 BalanceSheet 佈局
  // ==========================================
  const root = document.getElementById(domId);
  if (!root) return;

  // 切換 layout class
  root.classList.remove('mmd-layout-tabs');
  root.classList.add('mmd-layout-balancesheet');
  // 整體容器不超出頁面
  root.style.height = 'calc(100vh - var(--app-toolbar-height, 40px))';
  root.style.maxHeight = root.style.height;
  root.style.display = 'flex';
  root.style.flexDirection = 'column';
  root.style.paddingTop = '0';
  root.style.paddingBottom = '0';
  root.style.overflow = 'hidden';

  // --- 取出現有的 DOM 元素 ---
  const masterWrapper = root.querySelector(`#${domId}-masterWrapper`);
  const masterGrid = root.querySelector(`#${domId}-masterGrid`);
  const masterIndicator = root.querySelector(`#${domId}-masterIndicator`);
  const masterCountEl = root.querySelector(`#${domId}-count-master`);
  const masterTitleText = cfg.MasterTitle || cfg.MasterTable || 'Master';
  const details = cfg.Details || [];

  const detailEls = [];
  for (let i = 0; i < details.length; i++) {
    const dId = `${domId}-detail-${i}`;
    detailEls.push({
      wrapper: root.querySelector(`#${dId}-wrapper`),
      table: root.querySelector(`#${dId}-grid`),
      indicator: root.querySelector(`#${domId}-detailIndicator-${i}`),
      countEl: root.querySelector(`#${domId}-count-detail-${i}`),
      title: details[i].DetailTitle || details[i].DetailTable || `Detail ${i}`,
      id: dId
    });
  }

  // --- 先把要搬的元素脫離 DOM ---
  if (masterWrapper) masterWrapper.remove();
  if (masterIndicator) masterIndicator.remove();
  if (masterCountEl) masterCountEl.remove();
  for (const dt of detailEls) {
    if (dt.wrapper) dt.wrapper.remove();
    if (dt.indicator) dt.indicator.remove();
    if (dt.countEl) dt.countEl.remove();
  }

  // 移除 Tabs 結構
  const masterCard = root.querySelector('.mmd-master-card');
  const navTabs = root.querySelector('.nav-tabs');
  const tabContent = root.querySelector('.tab-content');
  if (masterCard) masterCard.remove();
  if (navTabs) navTabs.remove();
  if (tabContent) tabContent.remove();

  // --- Class 轉換：Tabs → BalanceSheet ---
  if (masterWrapper) {
    masterWrapper.classList.remove('table-responsive', 'mmd-master-scroll');
    masterWrapper.className = 'mmd-section-body';
  }
  if (masterGrid) {
    masterGrid.classList.remove('table', 'table-sm', 'table-hover', 'align-middle', 'mb-0');
    masterGrid.classList.add('mmd-grid');
    const masterThead = masterGrid.querySelector('thead');
    if (masterThead) masterThead.classList.remove('table-primary', 'sticky-top');
  }
  for (const dt of detailEls) {
    if (dt.wrapper) {
      dt.wrapper.classList.remove('mmd-detail-scroll');
      dt.wrapper.className = 'mmd-section-body';
    }
    if (dt.table) {
      dt.table.classList.remove('table', 'table-sm', 'table-striped', 'align-middle', 'mb-0');
      dt.table.classList.add('mmd-grid');
      const thead = dt.table.querySelector('thead');
      if (thead) thead.classList.remove('table-secondary', 'sticky-top');
    }
  }

  // --- 輔助：建立 section header ---
  function buildSectionHeader(indicator, titleText, countEl) {
    const header = document.createElement('div');
    header.className = 'mmd-section-header';
    const left = document.createElement('div');
    left.className = 'd-flex align-items-center gap-2';
    if (indicator) left.appendChild(indicator);
    const label = document.createElement('span');
    label.textContent = titleText;
    left.appendChild(label);
    header.appendChild(left);
    if (countEl) header.appendChild(countEl);
    return header;
  }

  // --- 建立 BalanceSheet 容器 ---
  const container = document.createElement('div');
  container.className = 'mmd-balancesheet-container';

  // ===== 頂部區域：Master + 項目新增工具列 =====
  const topSelector = document.createElement('div');
  topSelector.className = 'mmd-panel mmd-panel-top-selector';
  topSelector.style.height = `${cfg.MasterHeight || 150}px`;
  topSelector.setAttribute('data-min-height', '80');

  const topFlex = document.createElement('div');
  topFlex.className = 'd-flex h-100';

  // 左側：Master Grid (3:1 比例)
  const topGrid = document.createElement('div');
  topGrid.className = 'mmd-top-selector-grid';
  topGrid.style.cssText = 'flex: 3; min-width: 0;';
  topGrid.appendChild(buildSectionHeader(masterIndicator, masterTitleText, masterCountEl));
  if (masterWrapper) topGrid.appendChild(masterWrapper);
  topFlex.appendChild(topGrid);

  // 右側：項目新增工具列
  if (cfg.EnableTopToolbar) {
    const topToolbar = document.createElement('div');
    topToolbar.className = 'mmd-top-toolbar border-start';
    topToolbar.style.cssText = 'flex: 1; min-width: 0;';
    topToolbar.innerHTML = `
      <div class="mmd-section-header"><span>項目新增</span></div>
      <div class="mmd-section-body p-3">
        <div class="row g-2">
          <div class="col-auto">
            <label class="form-label mb-1">編碼</label>
            <input type="text" id="${domId}-newItemCode" class="form-control form-control-sm" style="width: 80px;" />
          </div>
          <div class="col">
            <label class="form-label mb-1">項目名稱</label>
            <input type="text" id="${domId}-newItemName" class="form-control form-control-sm" />
          </div>
          <div class="col-auto d-flex align-items-end">
            <button type="button" id="${domId}-btnAddItem" class="btn btn-primary btn-sm">加入</button>
          </div>
        </div>
      </div>
    `;
    topFlex.appendChild(topToolbar);
  }

  topSelector.appendChild(topFlex);
  container.appendChild(topSelector);

  // 水平分隔器
  const hSplitter = document.createElement('div');
  hSplitter.className = 'mmd-splitter mmd-splitter-h';
  hSplitter.setAttribute('data-target', 'top-selector');
  container.appendChild(hSplitter);

  // ===== 主要區域：Detail[0](左側) + Detail[1]...(右側 Tabs) =====
  const mainArea = document.createElement('div');
  mainArea.className = 'mmd-panel mmd-panel-main-area d-flex';
  mainArea.style.cssText = 'flex: 1; overflow: hidden;';

  // 左側：Detail[0]
  if (detailEls.length > 0) {
    const dt0 = detailEls[0];
    const leftPanel = document.createElement('div');
    leftPanel.className = 'mmd-panel mmd-panel-master';
    leftPanel.style.width = '300px';
    leftPanel.setAttribute('data-min-width', '200');
    leftPanel.appendChild(buildSectionHeader(dt0.indicator, dt0.title, dt0.countEl));
    if (dt0.wrapper) leftPanel.appendChild(dt0.wrapper);
    mainArea.appendChild(leftPanel);

    // 垂直分隔器
    const vSplitter = document.createElement('div');
    vSplitter.className = 'mmd-splitter mmd-splitter-v';
    vSplitter.setAttribute('data-target', 'master');
    mainArea.appendChild(vSplitter);
  }

  // 右側：Detail[1]... Tabs
  if (detailEls.length > 1) {
    const rightPanel = document.createElement('div');
    rightPanel.className = 'mmd-panel mmd-panel-detail flex-fill';

    // 頁籤導航
    const tabNav = document.createElement('ul');
    tabNav.className = 'nav nav-tabs mmd-detail-tabs';
    tabNav.id = `${domId}-detail-tabs`;

    // 頁籤內容
    const tabContentEl = document.createElement('div');
    tabContentEl.className = 'tab-content mmd-detail-tab-content';
    tabContentEl.id = `${domId}-detail-tabContent`;

    for (let i = 1; i < detailEls.length; i++) {
      const dt = detailEls[i];
      const active = (i === 1);

      // Tab button
      const li = document.createElement('li');
      li.className = 'nav-item';
      const btn = document.createElement('button');
      btn.className = `nav-link${active ? ' active' : ''}`;
      btn.id = `${domId}-detail-tab-${i}`;
      btn.setAttribute('data-bs-toggle', 'tab');
      btn.setAttribute('data-bs-target', `#${domId}-detail-tabpane-${i}`);
      btn.type = 'button';
      btn.textContent = dt.title;
      if (dt.countEl) {
        dt.countEl.classList.add('ms-2');
        btn.appendChild(dt.countEl);
      }
      li.appendChild(btn);
      tabNav.appendChild(li);

      // Tab pane
      const pane = document.createElement('div');
      pane.className = `tab-pane fade${active ? ' show active' : ''}`;
      pane.id = `${domId}-detail-tabpane-${i}`;
      if (dt.wrapper) {
        dt.wrapper.classList.add('mmd-tabpane-body');
        pane.appendChild(dt.wrapper);
      }
      tabContentEl.appendChild(pane);
    }

    rightPanel.appendChild(tabNav);
    rightPanel.appendChild(tabContentEl);
    mainArea.appendChild(rightPanel);
  }

  container.appendChild(mainArea);

  // 插入到 toolbar 之後
  const toolbar = root.querySelector('.top-toolbar');
  if (toolbar) {
    toolbar.after(container);
  } else {
    root.appendChild(container);
  }
})();