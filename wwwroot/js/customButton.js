/**
 * customButton.js - 動態按鈕模組
 * 從後台設定讀取按鈕，動態產生並執行 SP
 */

(function (window) {
    'use strict';

    const CustomButton = {
        // 設定
        config: {
            // 使用現有的 CustomButtonApi（已有資料）
            apiBase: '/api/CustomButtonApi',
            containerId: 'customButtonArea',
            buttonClass: 'btn btn-outline-primary btn-sm me-1 mb-1'
        },

        // 快取已載入的按鈕設定
        _buttonsCache: {},

        /**
         * 初始化 - 載入並產生按鈕
         * @param {string} itemId - 功能代碼 (如 FME00020)
         * @param {object} options - 選項
         */
        async init(itemId, options = {}) {
            const config = { ...this.config, ...options };

            try {
                // 1. 從 API 載入按鈕設定
                const buttons = await this.fetchButtons(itemId);
                this._buttonsCache[itemId] = buttons;

                // 2. 產生按鈕 UI
                this.renderButtons(buttons, config);

                return buttons;
            } catch (error) {
                console.error('[CustomButton] 初始化失敗:', error);
                return [];
            }
        },

        /**
         * 從 API 取得按鈕設定
         * 使用現有的 /api/CustomButtonApi/ByItem/{itemId}
         */
        async fetchButtons(itemId) {
            const response = await fetch(`${this.config.apiBase}/ByItem/${encodeURIComponent(itemId)}`);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const data = await response.json();

            // 轉換欄位名稱為小寫駝峰（統一格式）
            return data.map(btn => ({
                itemId: btn.ItemId || btn.itemId,
                buttonName: btn.ButtonName || btn.buttonName,
                custCaption: btn.CustCaption || btn.custCaption,
                custHint: btn.CustHint || btn.custHint,
                spName: btn.SpName || btn.spName,
                bVisible: btn.bVisible ?? btn.BVisible ?? 1,
                serialNum: btn.SerialNum || btn.serialNum,
                designType: btn.DesignType || btn.designType,
                bNeedNum: btn.bNeedNum ?? btn.BNeedNum,
                chkCanUpdate: btn.ChkCanUpdate || btn.chkCanUpdate,
                // 彈窗選單相關欄位
                searchTemplate: btn.SearchTemplate || btn.searchTemplate || '',
                multiSelectDD: btn.MultiSelectDD || btn.multiSelectDD || '',
                execSpName: btn.ExecSpName || btn.execSpName || '',
                // 自訂顏色
                buttonColor: btn.ButtonColor || btn.buttonColor || ''
            }));
        },

        /**
         * 取得按鈕的參數設定
         * 使用現有的 /api/CustomButtonApi/Detail/{itemId}/{buttonName}
         */
        async fetchParams(itemId, buttonName) {
            const response = await fetch(
                `${this.config.apiBase}/Detail/${encodeURIComponent(itemId)}/${encodeURIComponent(buttonName)}`
            );
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const data = await response.json();

            // 回傳 searchParams 陣列
            const params = data.searchParams || data.SearchParams || [];
            return params.map(p => ({
                paramName: p.ParamName || p.paramName,
                displayName: p.DisplayName || p.displayName,
                controlType: p.ControlType || p.controlType,
                commandText: p.CommandText || p.commandText,
                defaultValue: p.DefaultValue || p.defaultValue,
                defaultType: p.DefaultType || p.defaultType,
                paramSN: p.ParamSN || p.paramSN,
                paramValue: p.ParamValue || p.paramValue,
                paramType: p.ParamType ?? p.paramType,  // 參數類型：0=常數, 1=欄位, 5=目前單號
                iReadOnly: p.iReadOnly ?? p.IReadOnly,
                iVisible: p.iVisible ?? p.IVisible ?? 1
            }));
        },

        /**
         * 產生按鈕 UI
         */
        renderButtons(buttons, config) {
            const container = document.getElementById(config.containerId);
            if (!container) {
                return;
            }

            // 清空容器
            container.innerHTML = '';

            // 過濾可見的按鈕（bVisible == 1，支援數字或字串）
            const visibleButtons = buttons.filter(btn => {
                return btn.bVisible == 1 || btn.bVisible === true;
            });

            // 排序
            const sortedButtons = visibleButtons.sort((a, b) => (a.serialNum || 0) - (b.serialNum || 0));

            // 找出「功能」按鈕的索引
            const menuButtonIndex = sortedButtons.findIndex(btn => {
                const caption = (btn.custCaption || btn.buttonName || '').trim();
                return caption === '功能';
            });

            // 如果找到「功能」按鈕，分割按鈕陣列
            let beforeMenuButtons = sortedButtons;
            let menuButton = null;
            let afterMenuButtons = [];

            if (menuButtonIndex !== -1) {
                beforeMenuButtons = sortedButtons.slice(0, menuButtonIndex);
                menuButton = sortedButtons[menuButtonIndex];
                afterMenuButtons = sortedButtons.slice(menuButtonIndex + 1);
            }

            // 1. 渲染「功能」按鈕之前的按鈕
            beforeMenuButtons.forEach(btn => {
                const button = this.createButton(btn, config.buttonClass);
                container.appendChild(button);
            });

            // 2. 如果有「功能」按鈕，創建展開/收合結構
            if (menuButton) {
                // 創建「功能」按鈕
                const menuToggle = document.createElement('button');
                menuToggle.type = 'button';
                menuToggle.className = config.buttonClass + ' fab-menu-toggle';
                menuToggle.innerHTML = `
                    <span class="menu-text">${menuButton.custCaption || menuButton.buttonName}</span>
                    <span class="arrow">▼</span>
                `;
                menuToggle.title = menuButton.custHint || '';

                // 套用自訂顏色（如果有設定）
                if (menuButton.buttonColor && menuButton.buttonColor.trim()) {
                    menuToggle.style.backgroundColor = menuButton.buttonColor;
                }

                // 創建子選單容器
                const submenu = document.createElement('div');
                submenu.className = 'fab-submenu';
                submenu.style.display = 'none';

                // 3. 渲染「功能」按鈕之後的按鈕到子選單中
                afterMenuButtons.forEach(btn => {
                    const subButton = this.createButton(btn, 'fab fab-sub');
                    submenu.appendChild(subButton);
                });

                // 綁定展開/收合事件
                menuToggle.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    if (menuToggle.classList.contains('open')) {
                        menuToggle.classList.remove('open');
                        submenu.style.display = 'none';
                    } else {
                        menuToggle.classList.add('open');
                        submenu.style.display = 'flex';
                    }
                });

                container.appendChild(menuToggle);
                container.appendChild(submenu);
            } else {
                // 如果沒有「功能」按鈕，就把所有按鈕都正常顯示
                // (beforeMenuButtons 已經包含所有按鈕)
            }
        },

        /**
         * 創建單個按鈕元素
         */
        createButton(btn, buttonClass) {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = buttonClass;
            button.textContent = btn.custCaption || btn.buttonName;
            button.title = btn.custHint || '';
            button.dataset.itemId = btn.itemId;
            button.dataset.buttonName = btn.buttonName;
            button.dataset.spName = btn.spName || '';
            button.dataset.designType = btn.designType || '0';
            button.dataset.searchTemplate = btn.searchTemplate || '';
            button.dataset.multiSelectDD = btn.multiSelectDD || '';
            button.dataset.execSpName = btn.execSpName || '';

            // 套用自訂顏色（如果有設定）
            if (btn.buttonColor && btn.buttonColor.trim()) {
                button.style.backgroundColor = btn.buttonColor;
            }

            button.addEventListener('click', () => this.handleButtonClick(btn));

            return button;
        },

        /**
         * 按鈕點擊處理
         */
        async handleButtonClick(btnConfig) {
            console.log('==================== 按鈕點擊 ====================');
            console.log('[CustomButton] 按鈕資訊:', {
                按鈕名稱: btnConfig.custCaption || btnConfig.buttonName,
                ButtonName: btnConfig.buttonName,
                ItemId: btnConfig.itemId,
                SP名稱: btnConfig.spName,
                SearchTemplate: btnConfig.searchTemplate,
                完整設定: btnConfig
            });

            // 判斷是否為彈窗選單按鈕（不區分大小寫）
            const searchTemplate = (btnConfig.searchTemplate || '').toLowerCase();
            const isDialogButton = searchTemplate.includes('jsdpapersearchdll.dll') ||
                                   searchTemplate.includes('jsdpapersearchdll');

            if (isDialogButton) {
                console.log('[CustomButton] 這是彈窗選單按鈕，打開視窗');
                // 彈窗選單按鈕 - 直接打開視窗，不需要確認
                return this.handleDialogButton(btnConfig);
            }

            // 一般按鈕 - 直接執行 SP
            // 如果沒有設定 SP，顯示提示
            if (!btnConfig.spName) {
                console.warn('[CustomButton] 按鈕未設定 SP 名稱');
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        icon: 'info',
                        title: btnConfig.custCaption || btnConfig.buttonName,
                        text: '此按鈕尚未設定 SP 名稱'
                    });
                } else {
                    alert(`${btnConfig.custCaption || btnConfig.buttonName}: 尚未設定 SP 名稱`);
                }
                return;
            }

            try {
                // 1. 嘗試取得參數設定（如果失敗就使用預設參數）
                console.log('[CustomButton] 步驟1: 取得參數設定...');
                let paramConfigs = [];
                try {
                    paramConfigs = await this.fetchParams(btnConfig.itemId, btnConfig.buttonName);
                    console.log('[CustomButton] 參數設定取得成功:', paramConfigs);
                } catch (paramError) {
                    console.error('[CustomButton] 無法取得參數設定，使用預設參數:', paramError);
                    // 繼續執行，使用空的參數設定
                }

                // 2. 收集參數（根據參數設定或使用預設的 PaperNum）
                console.log('[CustomButton] 步驟2: 收集參數...');
                let params = {};

                if (paramConfigs.length > 0) {
                    console.log('[CustomButton] 使用參數設定收集參數（共 ' + paramConfigs.length + ' 個參數）');
                    // 有參數設定，根據參數種類收集
                    params = this.collectParamsByType(paramConfigs);
                } else {
                    console.log('[CustomButton] 無參數設定，使用預設的 PaperNum 參數');
                    // 沒有參數設定，使用基本參數（PaperNum）
                    params = this.collectBasicParameters();
                }
                console.log('[CustomButton] 收集到的參數:', params);

                // 3. 確認執行
                console.log('[CustomButton] 步驟3: 確認執行...');
                const confirmed = await this.confirmExecution(btnConfig);
                if (!confirmed) {
                    console.log('[CustomButton] 使用者取消執行');
                    return;
                }

                // 4. 執行 SP（後端會根據資料庫設定自動處理參數）
                console.log('[CustomButton] 步驟4: 準備執行 SP...');
                // 確保參數名稱不帶 @ 符號（後端會自動加上）
                const cleanParams = {};
                for (const [key, value] of Object.entries(params)) {
                    const cleanKey = key.startsWith('@') ? key.substring(1) : key;
                    cleanParams[cleanKey] = value;
                }
                console.log('[CustomButton] 清理後的參數（移除@符號）:', cleanParams);
                console.log('[CustomButton] 即將執行 SP:', btnConfig.spName);

                const result = await this.executeButton(btnConfig, cleanParams);
                console.log('[CustomButton] SP 執行結果:', result);

                // 5. 顯示結果
                console.log('[CustomButton] 步驟5: 顯示結果');
                this.showResult(result, btnConfig);
                console.log('==================== 執行完成 ====================');

            } catch (error) {
                console.error('[CustomButton] 執行失敗:', error);
                console.error('[CustomButton] 錯誤堆疊:', error.stack);
                this.showError(error.message || '執行失敗');
            }
        },

        /**
         * 根據參數種類 (ParamType) 收集參數
         * CURdOCX_VCusBtnParamTypeCALLSp 定義：
         *   0 = 欄位值
         *   1 = 常數
         *   2 = 操作者
         *   3 = 公司別
         *   4 = 系統別
         *   5 = 目前單號
         */
        collectParamsByType(paramConfigs) {
            console.log('[collectParamsByType] 開始收集參數，參數配置:', paramConfigs);
            const params = {};
            const basicParams = this.collectBasicParameters();
            console.log('[collectParamsByType] 基本參數 (PaperNum等):', basicParams);

            paramConfigs.forEach((p, index) => {
                const paramName = p.paramName;
                // 優先使用 paramType，若無則用 defaultType，預設為 1（常數）
                const paramType = (p.paramType !== null && p.paramType !== undefined)
                    ? p.paramType
                    : (p.defaultType ?? 1);

                console.log(`[collectParamsByType] 參數 ${index + 1}/${paramConfigs.length}:`, {
                    參數名稱: paramName,
                    ParamType: paramType,
                    DefaultValue: p.defaultValue,
                    ParamValue: p.paramValue,
                    DisplayName: p.displayName
                });

                // ParamType = 5: 目前單號
                if (paramType === 5 || paramType === '5') {
                    const paperNumValue = basicParams.PaperNum || basicParams.DLLPaperNum || '';
                    params[paramName] = paperNumValue;
                    console.log(`  → ParamType=5 (目前單號): ${paramName} = "${paperNumValue}"`);
                }
                // ParamType = 0: 欄位值（從畫面取得）
                else if (paramType === 0 || paramType === '0') {
                    const fieldName = p.paramValue || p.defaultValue || paramName.replace('@', '');
                    const el = document.querySelector(
                        `[name="${fieldName}"], [data-field="${fieldName}"], #${fieldName}`
                    );
                    const value = el ? (el.value || el.textContent || '').trim() : '';
                    params[paramName] = value;
                    console.log(`  → ParamType=0 (欄位值): ${paramName} = "${value}" (從欄位 "${fieldName}" 取得, 元素:`, el, ')');
                }
                // ParamType = 1: 常數
                else if (paramType === 1 || paramType === '1') {
                    const value = p.defaultValue || p.paramValue || '';
                    params[paramName] = value;
                    console.log(`  → ParamType=1 (常數): ${paramName} = "${value}"`);
                }
                // ParamType = 2: 操作者
                else if (paramType === 2 || paramType === '2') {
                    // 嘗試從全域變數或 session 取得使用者
                    const value = window.currentUser || window.userId || '';
                    params[paramName] = value;
                    console.log(`  → ParamType=2 (操作者): ${paramName} = "${value}"`);
                }
                // ParamType = 3: 公司別
                else if (paramType === 3 || paramType === '3') {
                    const value = window.companyId || '';
                    params[paramName] = value;
                    console.log(`  → ParamType=3 (公司別): ${paramName} = "${value}"`);
                }
                // ParamType = 4: 系統別
                else if (paramType === 4 || paramType === '4') {
                    const value = window.systemId || '';
                    params[paramName] = value;
                    console.log(`  → ParamType=4 (系統別): ${paramName} = "${value}"`);
                }
                // 其他情況，使用預設值
                else {
                    const value = p.defaultValue || p.paramValue || '';
                    params[paramName] = value;
                    console.log(`  → ParamType=未知(${paramType}), 使用預設值: ${paramName} = "${value}"`);
                }
            });

            console.log('[collectParamsByType] 最終收集到的所有參數:', params);
            return params;
        },

        /**
         * 收集參數 - 從畫面或彈出對話框
         */
        async collectParameters(paramConfigs, btnConfig) {
            // 檢查是否需要彈出對話框
            const visibleParams = paramConfigs.filter(p => p.iVisible !== 0);

            if (visibleParams.length === 0) {
                // 所有參數都隱藏，直接從畫面收集
                return this.collectFromPage(paramConfigs);
            }

            // 有需要顯示的參數，彈出對話框
            return await this.showParamDialog(paramConfigs, btnConfig);
        },

        /**
         * 從畫面收集基本參數 (PaperNum, Item 等)
         */
        collectBasicParameters() {
            const params = {};

            // 1. 嘗試使用全域函式 getPaperNum()（若頁面有定義）
            if (typeof getPaperNum === 'function') {
                try {
                    const pn = getPaperNum();
                    if (pn) {
                        params.PaperNum = pn;
                        params.DLLPaperNum = pn;
                    }
                } catch (e) {
                    // getPaperNum 執行失敗，繼續嘗試其他方法
                }
            }

            // 2. 嘗試從單頭取得 PaperNum（多種選擇器）
            if (!params.PaperNum) {
                const selectors = [
                    '[name="PaperNum"]',
                    '[data-field="PaperNum"]',
                    '#PaperNum',
                    'input[name="main.PaperNum"]',
                    '[data-bind="PaperNum"]',
                    '[name="DLLPaperNum"]',
                    '#DLLPaperNum'
                ];

                const paperNumInput = document.querySelector(selectors.join(', '));
                if (paperNumInput) {
                    const val = (paperNumInput.value || paperNumInput.textContent || '').trim();
                    params.PaperNum = val;
                    params.DLLPaperNum = val;
                }
            }

            // 3. 嘗試從 URL 取得 PaperNum（如 /FME/FmedIssueSub/I250100001）
            if (!params.PaperNum) {
                const pathParts = window.location.pathname.split('/');
                const lastPart = pathParts[pathParts.length - 1];
                if (lastPart && lastPart.match(/^[A-Z]\d+$/i)) {
                    params.PaperNum = lastPart;
                    params.DLLPaperNum = lastPart;
                }
            }

            // 嘗試從選中的列取得 Item
            const selectedRow = document.querySelector('tr.row-selected');
            if (selectedRow) {
                params.Item = selectedRow.dataset.item;
                if (!params.PaperNum) {
                    params.PaperNum = selectedRow.dataset.paperNum;
                }
            }

            return params;
        },

        /**
         * 從畫面收集參數（根據參數定義）
         */
        collectFromPage(paramConfigs) {
            const params = {};
            const basicParams = this.collectBasicParameters();

            paramConfigs.forEach(p => {
                // 先從 basicParams 取
                if (basicParams[p.paramName] !== undefined) {
                    params[p.paramName] = basicParams[p.paramName];
                    return;
                }

                // 嘗試從頁面元素取
                const el = document.querySelector(
                    `[name="${p.paramName}"], [data-field="${p.paramName}"], #${p.paramName}`
                );
                if (el) {
                    params[p.paramName] = el.value || el.textContent;
                    return;
                }

                // 使用預設值
                if (p.defaultValue) {
                    params[p.paramName] = p.defaultValue;
                } else if (p.paramValue) {
                    params[p.paramName] = p.paramValue;
                }
            });

            return params;
        },

        /**
         * 顯示參數輸入對話框
         */
        async showParamDialog(paramConfigs, btnConfig) {
            return new Promise((resolve) => {
                // 使用 SweetAlert2 (如果有的話) 或原生對話框
                if (typeof Swal !== 'undefined') {
                    this._showSwalDialog(paramConfigs, btnConfig, resolve);
                } else {
                    // 簡易版：使用原生 prompt
                    const params = this.collectFromPage(paramConfigs);
                    resolve(params);
                }
            });
        },

        /**
         * 使用 SweetAlert2 顯示對話框
         */
        _showSwalDialog(paramConfigs, btnConfig, resolve) {
            const visibleParams = paramConfigs.filter(p => p.iVisible !== 0);
            const preFilledParams = this.collectFromPage(paramConfigs);

            // 建立表單 HTML
            let formHtml = '<div class="custom-param-form">';
            visibleParams.forEach(p => {
                const value = preFilledParams[p.paramName] || p.defaultValue || '';
                const readonly = p.iReadOnly === 1 ? 'readonly' : '';
                formHtml += `
                    <div class="mb-3 text-start">
                        <label class="form-label">${p.displayName || p.paramName}</label>
                        <input type="text" class="form-control" name="${p.paramName}"
                               value="${value}" ${readonly}>
                    </div>
                `;
            });
            formHtml += '</div>';

            Swal.fire({
                title: btnConfig.custCaption || '參數設定',
                html: formHtml,
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消',
                focusConfirm: false,
                preConfirm: () => {
                    const params = { ...preFilledParams };
                    visibleParams.forEach(p => {
                        const input = Swal.getPopup().querySelector(`[name="${p.paramName}"]`);
                        if (input) {
                            params[p.paramName] = input.value;
                        }
                    });
                    return params;
                }
            }).then((result) => {
                if (result.isConfirmed) {
                    resolve(result.value);
                } else {
                    resolve(null);
                }
            });
        },

        /**
         * 確認執行
         */
        async confirmExecution(btnConfig) {
            if (typeof Swal !== 'undefined') {
                const result = await Swal.fire({
                    title: '確認執行',
                    text: `確定要執行「${btnConfig.custCaption || btnConfig.buttonName}」嗎？`,
                    icon: 'question',
                    showCancelButton: true,
                    confirmButtonText: '確定',
                    cancelButtonText: '取消'
                });
                return result.isConfirmed;
            }
            return confirm(`確定要執行「${btnConfig.custCaption || btnConfig.buttonName}」嗎？`);
        },

        /**
         * 執行按鈕 SP
         * 使用 /api/DynamicSp/exec - 根據資料庫設定動態執行
         */
        async executeButton(btnConfig, params) {
            const spName = btnConfig.spName;

            if (!spName) {
                throw new Error('此按鈕未設定 SP 名稱');
            }

            const requestPayload = {
                ItemId: btnConfig.itemId,
                ButtonName: btnConfig.buttonName,
                Args: params
            };

            console.log('[executeButton] ========== 準備呼叫後端 API ==========');
            console.log('[executeButton] API URL: /api/DynamicSp/exec');
            console.log('[executeButton] Request Payload:', requestPayload);
            console.log('[executeButton] 按鈕設定的 SP 名稱:', spName);

            // 使用動態 SP 執行 API
            // 傳送 ItemId + ButtonName，後端會自動查詢 SP 名稱和參數定義
            const response = await fetch('/api/DynamicSp/exec', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(requestPayload)
            });

            console.log('[executeButton] HTTP Response Status:', response.status, response.statusText);

            const result = await response.json();
            console.log('[executeButton] Response JSON:', result);

            // 檢查是否有錯誤（包括 HTTP 錯誤和 SP 返回的錯誤）
            if (!response.ok || !result.ok) {
                const errorMsg = result.error || '執行失敗';
                console.error('[executeButton] ❌ 執行錯誤:', errorMsg);

                // 記錄額外的錯誤資訊
                if (result.returnValue !== undefined && result.returnValue !== 0) {
                    console.error('[executeButton] SP 返回值:', result.returnValue);
                }
                if (result.resultSets && result.resultSets.length > 0) {
                    console.error('[executeButton] SP 返回的資料:', result.resultSets);
                }
                if (result.detail) {
                    console.error('[executeButton] 錯誤詳情:', result.detail);
                }

                throw new Error(errorMsg);
            }

            console.log('[executeButton] ✅ 執行成功');
            if (result.returnValue !== undefined) {
                console.log('[executeButton] SP 返回值:', result.returnValue);
            }
            if (result.resultSets && result.resultSets.length > 0) {
                console.log('[executeButton] SP 返回的資料集數量:', result.resultSets.length);
            }

            return { success: true, ...result };
        },

        /**
         * 顯示執行結果
         */
        showResult(result, btnConfig) {
            // 直接設定 2 秒後刷新頁面
            if (btnConfig.refreshAfter !== false) {
                setTimeout(() => {
                    window.location.reload();
                }, 2000);
            }

            // 顯示成功訊息（不依賴它來刷新頁面）
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'success',
                    title: '執行成功',
                    text: result.message || `${btnConfig.custCaption} 執行完成`,
                    timer: 2000,
                    showConfirmButton: false
                });
            } else {
                alert('執行成功');
            }
        },

        /**
         * 顯示錯誤
         */
        showError(message) {
            if (typeof Swal !== 'undefined') {
                Swal.fire({
                    icon: 'error',
                    title: '執行失敗',
                    text: message
                });
            } else {
                alert('執行失敗: ' + message);
            }
        },

        /**
         * 處理彈窗選單按鈕（匯入訂單等）
         */
        async handleDialogButton(btnConfig) {
            // 檢查是否有設定必要的 SP
            if (!btnConfig.multiSelectDD) {
                this.showError('此按鈕未設定查詢 SP (MultiSelectDD)');
                return;
            }

            if (!btnConfig.execSpName) {
                this.showError('此按鈕未設定執行 SP (ExecSpName)');
                return;
            }

            // 自動偵測可用的 Picker（支援多種頁面）
            const availablePickers = [
                'issueDetailPicker',      // 製令單
                'detailPicker',           // 銷售訂單
                'purchaseDetailPicker',   // 採購單（預留）
                'productionDetailPicker'  // 生產單（預留）
            ];

            let pickerApi = null;

            for (const id of availablePickers) {
                if (window['OrderDetailPicker_' + id]) {
                    pickerApi = window['OrderDetailPicker_' + id];
                    break;
                }
            }

            if (!pickerApi) {
                this.showError('找不到匯入視窗，請確認頁面已載入 Picker 模組');
                return;
            }

            // 設定辭典表名稱（使用 multiSelectDD 作為辭典表）
            if (pickerApi.setDictTableName) {
                console.log('[CustomButton] 設定辭典表:', btnConfig.multiSelectDD);
                pickerApi.setDictTableName(btnConfig.multiSelectDD);
            } else {
                console.warn('[CustomButton] Picker 沒有 setDictTableName 方法');
            }

            // 取得目前單號
            const basicParams = this.collectBasicParameters();
            const paperNum = basicParams.PaperNum || basicParams.DLLPaperNum || '';

            console.log('[CustomButton] 打開 Picker:', {
                multiSelectDD: btnConfig.multiSelectDD,
                execSpName: btnConfig.execSpName,
                paperNum: paperNum
            });

            // 打開視窗（延遲一點點，確保 setDictTableName 已完成）
            setTimeout(() => {
                pickerApi.open({
                    paperNum: paperNum,
                    partNum: '',    // 可以從選中的列取得料號
                    revision: ''    // 可以從選中的列取得版序
                });
            }, 100);
        }
    };

    // 匯出到全域
    window.CustomButton = CustomButton;

    // 提供簡便函式
    window.loadCustomButtons = function (itemId, options) {
        return CustomButton.init(itemId, options);
    };

})(window);
