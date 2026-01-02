/**
 * ========================================
 * 模板 Hook 擴展機制
 * ========================================
 * 提供類似 Delphi inherited 的功能，允許頁面在套用模板後擴展自定義邏輯
 *
 * 使用方式：
 * 1. 在頁面中呼叫 registerTemplateHooks() 註冊 Hook
 * 2. 可以定義確認前/確認後/新增前/新增後等 Hook
 * 3. Hook 回傳 false 可以中止操作
 *
 * 範例：
 * registerTemplateHooks('apr_certif', {
 *   beforeSave: () => { console.log('確認前'); return true; },
 *   afterSave: () => { console.log('確認後'); },
 *   beforeAdd: () => { return true; },
 *   afterAdd: (row) => { console.log('新增後', row); }
 * });
 */

(function (window) {
    'use strict';

    // 全域 Hook 註冊表
    const hookRegistry = {};

    /**
     * 註冊模板 Hook
     * @param {string} pageId - 頁面識別 ID（對應 config.DomId）
     * @param {object} hooks - Hook 物件
     * @param {function} hooks.beforeSave - 確認前 Hook，return false 中止儲存
     * @param {function} hooks.afterSave - 確認後 Hook
     * @param {function} hooks.beforeAdd - 新增前 Hook，return false 中止新增
     * @param {function} hooks.afterAdd - 新增後 Hook
     * @param {function} hooks.beforeDelete - 刪除前 Hook，return false 中止刪除
     * @param {function} hooks.afterDelete - 刪除後 Hook
     * @param {function} hooks.onValidate - 自訂驗證 Hook
     * @param {function} hooks.onInit - 頁面初始化 Hook
     */
    function registerTemplateHooks(pageId, hooks) {
        if (!pageId) {
            console.error('[TemplateHooks] pageId 不可為空');
            return;
        }

        if (!hooks || typeof hooks !== 'object') {
            console.error('[TemplateHooks] hooks 必須是物件');
            return;
        }

        // 註冊 Hook
        hookRegistry[pageId] = hooks;
        console.log(`[TemplateHooks] 已註冊 Hook: ${pageId}`, Object.keys(hooks));

        // 執行初始化 Hook
        if (typeof hooks.onInit === 'function') {
            try {
                hooks.onInit();
            } catch (error) {
                console.error(`[TemplateHooks] onInit 執行錯誤:`, error);
            }
        }
    }

    /**
     * 執行 Hook
     * @param {string} pageId - 頁面識別 ID
     * @param {string} hookName - Hook 名稱
     * @param {...any} args - Hook 參數
     * @returns {*} Hook 回傳值
     */
    function executeHook(pageId, hookName, ...args) {
        const hooks = hookRegistry[pageId];
        if (!hooks) {
            return undefined;
        }

        const hook = hooks[hookName];
        if (typeof hook !== 'function') {
            return undefined;
        }

        try {
            console.log(`[TemplateHooks] 執行 Hook: ${pageId}.${hookName}`, args);
            return hook(...args);
        } catch (error) {
            console.error(`[TemplateHooks] Hook 執行錯誤 (${pageId}.${hookName}):`, error);
            return undefined;
        }
    }

    /**
     * 自動偵測並執行 Hook（會自動尋找已註冊的 Hook）
     * @param {string} hookName - Hook 名稱
     * @param {...any} args - Hook 參數
     * @returns {*} Hook 回傳值，如果所有 Hook 都回傳 true 或 undefined 則回傳 true
     */
    function executeGlobalHook(hookName, ...args) {
        let result = undefined;
        let hasExecuted = false;

        // 執行所有已註冊的 Hook
        for (const pageId in hookRegistry) {
            const hooks = hookRegistry[pageId];
            const hook = hooks[hookName];

            if (typeof hook === 'function') {
                hasExecuted = true;
                try {
                    console.log(`[TemplateHooks] 執行全域 Hook: ${pageId}.${hookName}`, args);
                    const hookResult = hook(...args);

                    // 如果任何一個 Hook 回傳 false，整體結果就是 false
                    if (hookResult === false) {
                        result = false;
                    } else if (result !== false && hookResult !== undefined) {
                        result = hookResult;
                    }
                } catch (error) {
                    console.error(`[TemplateHooks] Hook 執行錯誤 (${pageId}.${hookName}):`, error);
                }
            }
        }

        // 如果沒有執行任何 Hook，回傳 undefined（允許繼續）
        // 如果有執行 Hook 但沒有回傳 false，回傳 true（允許繼續）
        return hasExecuted ? (result !== false) : undefined;
    }

    /**
     * 檢查是否有註冊指定的 Hook
     * @param {string} pageId - 頁面識別 ID
     * @param {string} hookName - Hook 名稱
     * @returns {boolean}
     */
    function hasHook(pageId, hookName) {
        return hookRegistry[pageId]?.[hookName] !== undefined;
    }

    /**
     * 取得已註冊的 Hook
     * @param {string} pageId - 頁面識別 ID
     * @returns {object|undefined}
     */
    function getHooks(pageId) {
        return hookRegistry[pageId];
    }

    /**
     * 移除已註冊的 Hook
     * @param {string} pageId - 頁面識別 ID
     */
    function unregisterTemplateHooks(pageId) {
        if (hookRegistry[pageId]) {
            delete hookRegistry[pageId];
            console.log(`[TemplateHooks] 已移除 Hook: ${pageId}`);
        }
    }

    /**
     * 包裝 setupGridController，自動整合 Hook
     * @param {string} pageId - 頁面識別 ID
     * @param {object} options - 原始 setupGridController 選項
     * @returns {object} 包裝後的 controller
     */
    function setupGridControllerWithHooks(pageId, options) {
        const hooks = hookRegistry[pageId];
        if (!hooks) {
            console.warn(`[TemplateHooks] 未找到註冊的 Hook: ${pageId}，使用原始選項`);
            return window.setupGridController ? window.setupGridController(options) : null;
        }

        // 合併原始 options 與 Hook
        const wrappedOptions = { ...options };

        // 包裝 beforeSave
        if (hooks.beforeSave) {
            const originalBeforeSave = options.beforeSave;
            wrappedOptions.beforeSave = function () {
                // 先執行原始 beforeSave
                if (originalBeforeSave && originalBeforeSave() === false) {
                    return false;
                }
                // 再執行 Hook
                return hooks.beforeSave();
            };
        }

        // 包裝 afterSave
        if (hooks.afterSave) {
            const originalAfterSave = options.afterSave;
            wrappedOptions.afterSave = function () {
                // 先執行原始 afterSave
                if (originalAfterSave) {
                    originalAfterSave();
                }
                // 再執行 Hook
                hooks.afterSave();
            };
        }

        // 包裝 beforeAdd
        if (hooks.beforeAdd) {
            const originalBeforeAdd = options.beforeAdd;
            wrappedOptions.beforeAdd = function () {
                // 先執行原始 beforeAdd
                if (originalBeforeAdd && originalBeforeAdd() === false) {
                    return false;
                }
                // 再執行 Hook
                return hooks.beforeAdd();
            };
        }

        // 包裝 afterAdd
        if (hooks.afterAdd) {
            const originalAfterAdd = options.afterAdd;
            wrappedOptions.afterAdd = function (row) {
                // 先執行原始 afterAdd
                if (originalAfterAdd) {
                    originalAfterAdd(row);
                }
                // 再執行 Hook
                hooks.afterAdd(row);
            };
        }

        // 包裝 beforeDelete
        if (hooks.beforeDelete) {
            const originalOnDelete = options.onDelete;
            wrappedOptions.onDelete = async function () {
                // 先執行 Hook
                const result = hooks.beforeDelete();
                if (result === false) {
                    console.log('[TemplateHooks] beforeDelete Hook 中止刪除');
                    return;
                }
                // 再執行原始 onDelete
                if (originalOnDelete) {
                    await originalOnDelete();
                }
                // 執行 afterDelete Hook
                if (hooks.afterDelete) {
                    hooks.afterDelete();
                }
            };
        }

        return window.setupGridController ? window.setupGridController(wrappedOptions) : null;
    }

    // ========================================
    // 工具函數
    // ========================================

    /**
     * 顯示確認訊息
     * @param {string} message - 訊息內容
     * @returns {boolean}
     */
    function confirm(message) {
        return window.confirm(message);
    }

    /**
     * 顯示警告訊息
     * @param {string} message - 訊息內容
     */
    function alert(message) {
        window.alert(message);
    }

    /**
     * 驗證必填欄位
     * @param {object} data - 資料物件
     * @param {string[]} requiredFields - 必填欄位清單
     * @returns {boolean}
     */
    function validateRequired(data, requiredFields) {
        for (const field of requiredFields) {
            if (!data[field] || data[field] === '') {
                alert(`欄位 "${field}" 為必填`);
                return false;
            }
        }
        return true;
    }

    /**
     * 驗證數值範圍
     * @param {number} value - 數值
     * @param {number} min - 最小值
     * @param {number} max - 最大值
     * @param {string} fieldName - 欄位名稱
     * @returns {boolean}
     */
    function validateRange(value, min, max, fieldName) {
        if (value < min || value > max) {
            alert(`欄位 "${fieldName}" 必須介於 ${min} 到 ${max} 之間`);
            return false;
        }
        return true;
    }

    // ========================================
    // 暴露到全域
    // ========================================

    window.TemplateHooks = {
        register: registerTemplateHooks,
        execute: executeHook,
        executeGlobal: executeGlobalHook,
        has: hasHook,
        get: getHooks,
        unregister: unregisterTemplateHooks,
        setupGridControllerWithHooks: setupGridControllerWithHooks,
        utils: {
            confirm,
            alert,
            validateRequired,
            validateRange
        }
    };

    // 向後相容的全域函數
    window.registerTemplateHooks = registerTemplateHooks;
    window.executeTemplateHook = executeHook;
    window.executeGlobalTemplateHook = executeGlobalHook;

    console.log('[TemplateHooks] 模板 Hook 系統已載入');

})(window);