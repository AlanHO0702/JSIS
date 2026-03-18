// CFW00005 部門主檔
// 功能：一進入自動載入全部資料 + 右側部門組織架構樹狀結構
// 隱藏 Toolbar 按鈕，只保留修改

(function () {
  'use strict';

  let departmentTreeData = [];
  let expandedNodes = new Set();

  // ==========================================
  // 隱藏不需要的 Toolbar 按鈕（只隱藏查詢和 Excel）
  // ==========================================
  function hideToolbarButtons() {
    const hideIds = ['btnOpenQuery', 'btnExportExcel'];
    hideIds.forEach(id => {
      const el = document.getElementById(id);
      if (el) el.classList.add('d-none');
    });
  }

  // ==========================================
  // 建立樹狀面板
  // ==========================================
  function buildTreePanel() {
    const tableWrapper = document.querySelector('.table-wrapper');
    if (!tableWrapper) return;

    // 建立外層容器（左右分割），高度固定與 table-wrapper 一致
    const container = document.createElement('div');
    container.className = 'depart-container';
    container.style.cssText = 'display:flex; flex:1; min-height:0; gap:5px; height:calc(100vh - var(--app-toolbar-height, 56px)); overflow:hidden;';

    // 將 table-wrapper 包進左側 grid-panel
    const gridPanel = document.createElement('div');
    gridPanel.className = 'grid-panel';
    gridPanel.style.cssText = 'flex:1; display:flex; flex-direction:column; min-height:0; overflow:hidden;';

    tableWrapper.parentNode.insertBefore(container, tableWrapper);
    // 清除 table-wrapper 原本的固定高度，改由外層 depart-container 控制
    tableWrapper.style.height = '100%';
    gridPanel.appendChild(tableWrapper);
    container.appendChild(gridPanel);

    // 右側樹狀面板
    const treePanel = document.createElement('div');
    treePanel.className = 'tree-panel';
    treePanel.style.cssText = `
      flex: 0 0 450px;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      padding: 2px 15px 15px 15px;
      background: #fff;
      overflow: auto;
      box-shadow: 0 2px 12px 0 #c3d4e6;
      min-height: 0;
    `;
    treePanel.innerHTML = `
      <div style="font-weight:600; color:#2564b3; margin-bottom:2px; padding-bottom:2px; border-bottom:2px solid #e9ecef;">
        <i class="bi bi-diagram-3"></i> 部門組織架構
      </div>
      <div id="departmentTree"></div>
    `;
    container.appendChild(treePanel);
  }

  // ==========================================
  // 載入部門樹狀結構
  // ==========================================
  async function loadDepartmentTree() {
    try {
      const response = await fetch('/api/AJNdDepart/tree');
      if (!response.ok) throw new Error('載入部門資料失敗');
      departmentTreeData = await response.json();
      renderTree();
    } catch (error) {
      console.error('載入部門樹狀結構錯誤:', error);
    }
  }

  // ==========================================
  // 渲染樹狀結構
  // ==========================================
  function renderTree() {
    const container = document.getElementById('departmentTree');
    if (!container) return;
    container.innerHTML = '';

    if (!departmentTreeData || departmentTreeData.length === 0) {
      container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">尚無部門資料</div>';
      return;
    }

    departmentTreeData.forEach(node => {
      const levelNo = node.LEVelNo;
      if (levelNo === 0 || levelNo === null || levelNo === undefined) {
        container.appendChild(createTreeNode(node, 0));
      }
    });
  }

  // ==========================================
  // 建立樹狀節點
  // ==========================================
  function createTreeNode(node, level) {
    const div = document.createElement('div');
    const departId = node.DepartId;

    const nodeDiv = document.createElement('div');
    nodeDiv.style.cssText = 'padding:0 12px; cursor:pointer; border-radius:6px; transition:all 0.2s; user-select:none;';
    nodeDiv.onmouseenter = () => { nodeDiv.style.background = '#f0f8ff'; };
    nodeDiv.onmouseleave = () => { nodeDiv.style.background = ''; };

    if (node.IsStop === 1) {
      nodeDiv.style.color = '#999';
      nodeDiv.style.textDecoration = 'line-through';
    }

    // 展開/收合按鈕
    if (node.HasChildren && node.Children && node.Children.length > 0) {
      const toggle = document.createElement('span');
      toggle.style.cssText = 'display:inline-block; width:16px; height:16px; line-height:14px; text-align:center; margin-right:4px; cursor:pointer; border-radius:3px; font-size:12px; background:#e9ecef;';
      const isExpanded = expandedNodes.has(departId);
      toggle.innerHTML = isExpanded ? '−' : '+';
      toggle.onclick = function (e) {
        e.stopPropagation();
        toggleNode(departId);
      };
      nodeDiv.appendChild(toggle);
    } else {
      const spacer = document.createElement('span');
      spacer.style.cssText = 'display:inline-block; width:20px;';
      nodeDiv.appendChild(spacer);
    }

    // 圖示
    const icon = document.createElement('span');
    icon.style.cssText = 'display:inline-block; width:20px; text-align:center; margin-right:5px;';
    const levelNo = node.LEVelNo;
    if (levelNo === 0 || levelNo === null || levelNo === undefined) {
      icon.textContent = '\u{1F3E2}';
    } else if (node.HasChildren) {
      icon.textContent = '\u{1F4C1}';
    } else {
      icon.textContent = '\u{1F4C4}';
    }
    nodeDiv.appendChild(icon);

    // 文字
    const text = document.createElement('span');
    text.textContent = `${departId} - ${node.DepartName || ''}`;
    if (node.IsStop === 1) {
      text.textContent += ' (停用)';
    }
    nodeDiv.appendChild(text);

    div.appendChild(nodeDiv);

    // 子節點
    if (node.Children && node.Children.length > 0) {
      const childrenDiv = document.createElement('div');
      childrenDiv.style.cssText = 'margin-left:24px;';
      const isExpanded = expandedNodes.has(departId);
      childrenDiv.style.display = isExpanded ? 'block' : 'none';

      node.Children.forEach(child => {
        childrenDiv.appendChild(createTreeNode(child, level + 1));
      });
      div.appendChild(childrenDiv);
    }

    return div;
  }

  // ==========================================
  // 切換節點展開/收合
  // ==========================================
  function toggleNode(departId) {
    if (expandedNodes.has(departId)) {
      expandedNodes.delete(departId);
    } else {
      expandedNodes.add(departId);
    }
    renderTree();
  }

  // ==========================================
  // 初始化
  // ==========================================
  function init() {
    hideToolbarButtons();
    buildTreePanel();
    loadDepartmentTree();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();