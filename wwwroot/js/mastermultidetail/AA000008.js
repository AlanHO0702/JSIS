// AA000008 會計科目主檔
// 將預設 Tabs 佈局轉換為 ThreeColumn（三欄式）佈局
// 結構：總類 → 分類 → 總帳科目 → 明細科目

(function () {
  'use strict';

  const cfgs = window._mmdConfigs || {};
  const domId = Object.keys(cfgs).find(k => (cfgs[k].ItemId || '').toUpperCase() === 'AA000008');
  if (!domId) return;

  const cfg = cfgs[domId];

  // ==========================================
  // 1. 覆寫 config 屬性
  // ==========================================
  cfg.Layout = 1; // ThreeColumn
  cfg.EnableSplitters = true;
  cfg.EnableGridCounts = true;
  cfg.EnableDetailFocusCascade = true;
  cfg.DetailCascadeMode = 1; // 串聯式：Detail[N] ← Detail[N-1]

  // ==========================================
  // 2. 從 Tabs 佈局搬移元素到 ThreeColumn 佈局
  // ==========================================
  const root = document.getElementById(domId);
  if (!root) return;

  // 切換 layout class
  root.classList.remove('mmd-layout-tabs');
  root.classList.add('mmd-layout-threecolumn');
  // 整體容器不超出頁面：扣掉固定 toolbar 高度，不出現頁面卷軸
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

  // --- Class 轉換：Tabs → ThreeColumn ---
  // masterWrapper: table-responsive mmd-master-scroll → mmd-section-body
  if (masterWrapper) {
    masterWrapper.classList.remove('table-responsive', 'mmd-master-scroll');
    masterWrapper.className = 'mmd-section-body';
  }
  // masterGrid: table table-sm table-hover align-middle mb-0 → mmd-grid
  if (masterGrid) {
    masterGrid.classList.remove('table', 'table-sm', 'table-hover', 'align-middle', 'mb-0');
    masterGrid.classList.add('mmd-grid');
    // thead: 移除 table-primary sticky-top
    const masterThead = masterGrid.querySelector('thead');
    if (masterThead) {
      masterThead.classList.remove('table-primary', 'sticky-top');
    }
  }
  // detail wrappers & tables
  for (const dt of detailEls) {
    if (dt.wrapper) {
      dt.wrapper.classList.remove('mmd-detail-scroll');
      dt.wrapper.className = 'mmd-section-body';
    }
    if (dt.table) {
      dt.table.classList.remove('table', 'table-sm', 'table-striped', 'align-middle', 'mb-0');
      dt.table.classList.add('mmd-grid');
      const thead = dt.table.querySelector('thead');
      if (thead) {
        thead.classList.remove('table-secondary', 'sticky-top');
      }
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

  // --- 建立 ThreeColumn 容器 ---
  const container = document.createElement('div');
  container.className = 'mmd-three-column-container';

  // ===== 左側面板：Master + Detail[0] =====
  const leftPanel = document.createElement('div');
  leftPanel.className = 'mmd-panel mmd-panel-left';
  leftPanel.setAttribute('data-min-width', '200');

  // Master 區域
  const masterSection = document.createElement('div');
  masterSection.className = 'mmd-section mmd-section-master';
  masterSection.setAttribute('data-min-height', '150');
  masterSection.appendChild(buildSectionHeader(masterIndicator, masterTitleText, masterCountEl));
  if (masterWrapper) masterSection.appendChild(masterWrapper);
  leftPanel.appendChild(masterSection);

  // 水平分隔器
  const hSplitter = document.createElement('div');
  hSplitter.className = 'mmd-splitter mmd-splitter-h';
  hSplitter.setAttribute('data-target', 'master');
  leftPanel.appendChild(hSplitter);

  // Detail[0] 區域
  if (detailEls.length > 0) {
    const dt0 = detailEls[0];
    const d0Section = document.createElement('div');
    d0Section.className = 'mmd-section mmd-section-detail0';
    d0Section.setAttribute('data-min-height', '100');
    d0Section.appendChild(buildSectionHeader(dt0.indicator, dt0.title, dt0.countEl));
    if (dt0.wrapper) d0Section.appendChild(dt0.wrapper);
    leftPanel.appendChild(d0Section);
  }

  container.appendChild(leftPanel);

  // 輔助：建立 detail panel
  function buildDetailPanel(panelClass, dt) {
    const panel = document.createElement('div');
    panel.className = `mmd-panel ${panelClass}`;
    panel.setAttribute('data-min-width', '200');
    const section = document.createElement('div');
    section.className = 'mmd-section';
    section.appendChild(buildSectionHeader(dt.indicator, dt.title, dt.countEl));
    if (dt.wrapper) section.appendChild(dt.wrapper);
    panel.appendChild(section);
    return panel;
  }

  // ===== 垂直分隔器 + 中間面板：Detail[1] =====
  if (detailEls.length > 1) {
    const vSplitter1 = document.createElement('div');
    vSplitter1.className = 'mmd-splitter mmd-splitter-v';
    container.appendChild(vSplitter1);
    container.appendChild(buildDetailPanel('mmd-panel-mid', detailEls[1]));
  }

  // ===== 垂直分隔器 + 右側面板：Detail[2] =====
  if (detailEls.length > 2) {
    const vSplitter2 = document.createElement('div');
    vSplitter2.className = 'mmd-splitter mmd-splitter-v';
    container.appendChild(vSplitter2);
    container.appendChild(buildDetailPanel('mmd-panel-right', detailEls[2]));
  }

  // 插入到 toolbar 之後
  const toolbar = root.querySelector('.top-toolbar');
  if (toolbar) {
    toolbar.after(container);
  } else {
    root.appendChild(container);
  }
})();
