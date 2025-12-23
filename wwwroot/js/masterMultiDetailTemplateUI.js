// ========================================================
// Master Multi Detail Template - UI 初始化
// 負責處理計數器、拖曳器等 UI 相關功能
// ========================================================

(() => {
    // ========================================================
    // 初始化：根據佈局模式執行不同的初始化邏輯
    // ========================================================
    document.addEventListener('DOMContentLoaded', function() {
        // 等待配置載入
        setTimeout(() => {
            const configs = window._mmdConfigs || {};

            Object.keys(configs).forEach(domId => {
                const cfg = configs[domId];
                initializeUI(domId, cfg);
            });
        }, 300);
    });

    // ========================================================
    // 初始化單一 MMD UI
    // ========================================================
    function initializeUI(domId, cfg) {
        const root = document.getElementById(domId);
        if (!root) {
            console.error('找不到根容器:', domId);
            return;
        }

        const layoutMode = cfg.Layout;
        const layoutModeStr = getLayoutModeString(layoutMode);

        // ★ 根據配置啟用計數器（cfg.EnableGridCounts）
        if (cfg.EnableGridCounts) {
            initGridCounts(domId);
        }

        // VerticalStack 佈局：計算並記錄 SubDetail 的動態最小高度
        if (layoutModeStr === 'VerticalStack') {
            const container = root.querySelector('.mmd-vertical-stack-container');
            const subdetailPanel = root.querySelector('.mmd-panel-stack-subdetail');

            if (container && subdetailPanel) {
                // 計算 SubDetail 的初始高度
                // SubDetail高度 = 容器總高度 - Master(120px) - Detail(150px) - 3個分隔器(3x5px=15px)
                const containerHeight = container.offsetHeight;
                const calculatedSubDetailHeight = containerHeight - 120 - 150 - 15;

                // 保存計算出的初始高度作為最小高度限制，用於拖曳限制
                subdetailPanel.setAttribute('data-min-height', calculatedSubDetailHeight);
            }
        }

        // ★ 根據配置啟用拖曳器（cfg.EnableSplitters）
        if (cfg.EnableSplitters) {
            bindSplitters();
        }
    }

    // ========================================================
    // 將數字模式轉換為字串
    // ========================================================
    function getLayoutModeString(layout) {
        const modes = {
            0: 'Tabs',
            1: 'ThreeColumn',
            2: 'VerticalStack',
            3: 'BalanceSheet'
        };
        return modes[layout] || 'Tabs';
    }

    // ========================================================
    // 初始化表格計數（通用版，支援所有佈局）
    // ========================================================
    function initGridCounts(domId) {
        // 選擇所有表格：支援 Tabs、ThreeColumn、VerticalStack、BalanceSheet 佈局
        const tables = document.querySelectorAll(
            `#${domId} .mmd-grid, ` +
            `#${domId} .mmd-master-table, ` +
            `#${domId} .mmd-detail-table, ` +
            `#${domId} .mmd-topselector-table`
        );

        tables.forEach(table => {
            // 支援大小寫的 wrapper：-wrapper（小寫）和 Wrapper（大寫）
            const wrapper = table.closest('[id$="-wrapper"], [id$="Wrapper"]') || table.closest('.mmd-section-body');
            if (!wrapper) return;

            // 解析 wrapper ID 以確定計數器 ID
            const wrapperId = wrapper.id.replace('-wrapper', '').replace('Wrapper', '').replace(`${domId}-`, '');
            let countId = '';

            // Master 表格計數器
            if (wrapperId === 'master') {
                countId = `${domId}-count-master`;
            }
            // TopSelector 表格計數器（BalanceSheet 專用）
            else if (wrapperId === 'topSelector') {
                countId = `${domId}-count-topselector`;
            }
            // Detail 表格計數器
            else if (wrapperId.startsWith('detail-')) {
                const detailIndex = wrapperId.replace('detail-', '');
                countId = `${domId}-count-detail-${detailIndex}`;
            }

            if (countId) {
                // 初始更新計數
                updateGridCount(table, countId);

                // 監聽表格變化，自動更新計數
                const tbody = table.querySelector('tbody');
                if (tbody) {
                    const observer = new MutationObserver(() => {
                        updateGridCount(table, countId);
                    });
                    observer.observe(tbody, {
                        childList: true,
                        subtree: true
                    });
                }
            }
        });
    }

    // ========================================================
    // 更新表格計數
    // ========================================================
    function updateGridCount(table, countId) {
        const tbody = table.querySelector('tbody');
        if (!tbody) return;

        const rows = tbody.querySelectorAll('tr');
        let count = 0;
        let selectedIndex = 0;

        rows.forEach((row, index) => {
            // 排除提示訊息行
            if (!row.textContent.includes('請點選') && !row.textContent.includes('載入中')) {
                count++;
                if (row.classList.contains('selected')) {
                    selectedIndex = count;
                }
            }
        });

        const countElem = document.getElementById(countId);
        if (countElem) {
            // ★ 統一設定計數器格式（只需在此處修改）
            countElem.className = 'badge bg-secondary';
            countElem.textContent = `${selectedIndex > 0 ? selectedIndex : count} / ${count}`;
        }
    }

    // ========================================================
    // 綁定分隔器拖曳
    // ========================================================
    function bindSplitters() {
        const splitters = document.querySelectorAll('.mmd-splitter-v, .mmd-splitter-h');
        splitters.forEach(splitter => {
            splitter.addEventListener('mousedown', function(e) {
                e.preventDefault();
                const isVertical = splitter.classList.contains('mmd-splitter-v');
                const startPos = isVertical ? e.clientX : e.clientY;
                const prevPanel = splitter.previousElementSibling;
                const nextPanel = splitter.nextElementSibling;
                const startSize = isVertical ? prevPanel.offsetWidth : prevPanel.offsetHeight;
                const startNextSize = nextPanel ? (isVertical ? nextPanel.offsetWidth : nextPanel.offsetHeight) : 0;

                // 檢查是否為最後一個水平分隔器（垂直堆疊佈局的底部拖曳器）
                const isLastHSplitter = !isVertical &&
                                        prevPanel &&
                                        prevPanel.classList.contains('mmd-panel-stack-subdetail');

                // 從 data 屬性讀取最小高度/寬度限制
                const minHeightLimit = parseInt(prevPanel.getAttribute('data-min-height')) || 100;
                const minWidthLimit = parseInt(prevPanel.getAttribute('data-min-width')) || 200;

                if (isLastHSplitter) {
                    prevPanel.style.flex = 'none';
                    prevPanel.style.height = startSize + 'px';
                    if (nextPanel) {
                        nextPanel.style.flex = '1';
                    }
                }

                function onMouseMove(e) {
                    const delta = (isVertical ? e.clientX : e.clientY) - startPos;
                    const newSize = startSize + delta;
                    const newNextSize = startNextSize - delta;

                    // 使用對應的最小限制
                    const minLimit = isVertical ? minWidthLimit : minHeightLimit;

                    if (newSize >= minLimit) {
                        if (isVertical) {
                            prevPanel.style.width = newSize + 'px';
                            if (nextPanel && !isLastHSplitter && newNextSize > minWidthLimit) {
                                nextPanel.style.width = newNextSize + 'px';
                            }
                        } else {
                            prevPanel.style.height = newSize + 'px';
                            // 最後一個水平分隔器允許無限增長，不限制 nextPanel 大小
                            if (!isLastHSplitter && nextPanel && newNextSize > minHeightLimit) {
                                nextPanel.style.height = newNextSize + 'px';
                            }
                            // 對於最後一個分隔器，讓 filler 自動調整（可以為 0）
                            if (isLastHSplitter && nextPanel) {
                                nextPanel.style.minHeight = '0';
                            }
                        }
                    }
                }

                function onMouseUp() {
                    document.removeEventListener('mousemove', onMouseMove);
                    document.removeEventListener('mouseup', onMouseUp);
                }

                document.addEventListener('mousemove', onMouseMove);
                document.addEventListener('mouseup', onMouseUp);
            });
        });
    }

    // ========================================================
    // 監聽選中行變化，更新計數顯示（通用版，支援所有佈局）
    // ========================================================
    document.addEventListener('click', function(e) {
        const tr = e.target.closest('tr');
        if (tr && tr.parentElement.tagName === 'TBODY') {
            const table = tr.closest('table');
            // 支援所有表格類型
            const isValidTable = table && (
                table.classList.contains('mmd-grid') ||
                table.classList.contains('mmd-master-table') ||
                table.classList.contains('mmd-detail-table') ||
                table.classList.contains('mmd-topselector-table')
            );

            if (isValidTable) {
                // 支援大小寫的 wrapper：-wrapper（小寫）和 Wrapper（大寫）
                const wrapper = table.closest('[id$="-wrapper"], [id$="Wrapper"]') || table.closest('.mmd-section-body');
                if (!wrapper) return;

                const domId = table.id.split('-')[0];
                const wrapperId = wrapper.id.replace('-wrapper', '').replace('Wrapper', '').replace(`${domId}-`, '');
                let countId = '';

                // Master 表格計數器
                if (wrapperId === 'master') {
                    countId = `${domId}-count-master`;
                }
                // TopSelector 表格計數器（BalanceSheet 專用）
                else if (wrapperId === 'topSelector') {
                    countId = `${domId}-count-topselector`;
                }
                // Detail 表格計數器
                else if (wrapperId.startsWith('detail-')) {
                    const detailIndex = wrapperId.replace('detail-', '');
                    countId = `${domId}-count-detail-${detailIndex}`;
                }

                if (countId) {
                    setTimeout(() => updateGridCount(table, countId), 100);
                }
            }
        }
    });

})();
