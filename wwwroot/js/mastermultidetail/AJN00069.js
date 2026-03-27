// AJN00069 系統預設摘要
// 將預設 Tabs 佈局轉換為 VerticalStack（垂直堆疊）佈局
// 結構：系統作業 → 摘要規則 → 摘要對應科目

(function () {
  'use strict';

  const cfgs = window._mmdConfigs || {};
  const domId = Object.keys(cfgs).find(k => (cfgs[k].ItemId || '').toUpperCase() === 'AJN00069');
  if (!domId) return;

  const cfg = cfgs[domId];

  // ==========================================
  // 1. 覆寫 config 屬性
  // ==========================================
  cfg.Layout = 2; // VerticalStack
  cfg.EnableSplitters = true;
  cfg.EnableGridCounts = true;
  cfg.EnableDetailFocusCascade = true;
  cfg.DetailCascadeMode = 1; // 串聯式：Detail[N] ← Detail[N-1]

  // ==========================================
  // 2. 從 Tabs 佈局搬移元素到 VerticalStack 佈局
  // ==========================================
  const root = document.getElementById(domId);
  if (!root) return;

  // 切換 layout class
  root.classList.remove('mmd-layout-tabs');
  root.classList.add('mmd-layout-verticalstack');
  // 整體容器不超出頁面
  root.style.height = 'calc(100vh - var(--app-toolbar-height, 40px))';
  root.style.minHeight = root.style.height;
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

  // 取出各 detail 元素
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

  // --- Class 轉換：Tabs → VerticalStack ---
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

  // --- 建立 VerticalStack 容器 ---
  const container = document.createElement('div');
  container.className = 'mmd-vertical-stack-container';

  // ===== Master 區域 =====
  const masterPanel = document.createElement('div');
  masterPanel.className = 'mmd-panel mmd-panel-stack-master';
  masterPanel.setAttribute('data-min-height', '120');
  masterPanel.appendChild(buildSectionHeader(masterIndicator, masterTitleText, masterCountEl));
  if (masterWrapper) masterPanel.appendChild(masterWrapper);
  container.appendChild(masterPanel);

  // 水平分隔器
  const hSplitter1 = document.createElement('div');
  hSplitter1.className = 'mmd-splitter mmd-splitter-h';
  hSplitter1.setAttribute('data-target', 'stack-master');
  container.appendChild(hSplitter1);

  // ===== Detail[0] 區域 =====
  if (detailEls.length > 0) {
    const dt0 = detailEls[0];
    const detailPanel = document.createElement('div');
    detailPanel.className = 'mmd-panel mmd-panel-stack-detail';
    detailPanel.setAttribute('data-min-height', '150');
    detailPanel.appendChild(buildSectionHeader(dt0.indicator, dt0.title, dt0.countEl));
    if (dt0.wrapper) detailPanel.appendChild(dt0.wrapper);
    container.appendChild(detailPanel);

    // 水平分隔器
    const hSplitter2 = document.createElement('div');
    hSplitter2.className = 'mmd-splitter mmd-splitter-h';
    hSplitter2.setAttribute('data-target', 'stack-detail');
    container.appendChild(hSplitter2);
  }

  // ===== Detail[1] (SubDetail) 區域 =====
  if (detailEls.length > 1) {
    const dt1 = detailEls[1];
    const subDetailPanel = document.createElement('div');
    subDetailPanel.className = 'mmd-panel mmd-panel-stack-subdetail';
    subDetailPanel.setAttribute('data-min-height', '100');
    subDetailPanel.appendChild(buildSectionHeader(dt1.indicator, dt1.title, dt1.countEl));
    if (dt1.wrapper) subDetailPanel.appendChild(dt1.wrapper);
    container.appendChild(subDetailPanel);

    // 水平分隔器（底部）
    const hSplitter3 = document.createElement('div');
    hSplitter3.className = 'mmd-splitter mmd-splitter-h';
    hSplitter3.setAttribute('data-target', 'stack-subdetail');
    container.appendChild(hSplitter3);
  }

  // 底部填充區域
  const filler = document.createElement('div');
  filler.className = 'mmd-panel-filler';
  container.appendChild(filler);

  // 插入到 toolbar 之後
  const toolbar = root.querySelector('.top-toolbar');
  if (toolbar) {
    toolbar.after(container);
  } else {
    root.appendChild(container);
  }
})();
