// specialUI.js - 特殊介面元件庫
// 用於動態載入作業特定的介面元件

(function() {
    'use strict';

    // ==========================================
    // 特殊介面元件庫
    // ==========================================
    window.SpecialUIComponents = {

        // ==========================================
        // 借貸總額元件（用於收付傳票等作業）
        // ==========================================
        PayRecvSummary: {
            render: function(config) {
                const html = `
                    <div class="special-ui-panel pay-recv-summary" style="
                        border-top: 1px solid #ddd;
                        padding: 10px 20px;
                        background-color: #f5f5f5;
                        margin-top: 10px;
                    ">
                        <div style="text-align: right;">
                            <label style="font-weight: bold;">借方金額：</label>
                            <span id="lbDAmount" class="amount-display" style="
                                font-size: 16px;
                                color: #000000;
                                font-weight: 600;
                                display: inline-block;
                                text-align: left;
                            ">0.00</span>
                            <label style="font-weight: bold; margin-left: 10px;">貸方金額：</label>
                            <span id="lbCAmount" class="amount-display" style="
                                font-size: 16px;
                                color: #000000;
                                font-weight: 600;
                                display: inline-block;
                                text-align: left;
                            ">0.00</span>
                        </div>
                    </div>
                `;
                return html;
            },

            init: function(itemId, config) {
                const detailTable = config.detailTableName || '';

                // 綁定事件 - 監聽單身資料變更
                this.bindEvents(detailTable);

                // 初始計算
                this.countSum(detailTable);
            },

            bindEvents: function(detailTable) {
                const self = this;

                // 監聽表格變更事件（新增、刪除、修改後）
                $(document).on('detailDataChanged', function() {
                    self.countSum(detailTable);
                });

                // 監聽單頭切換
                $(document).on('masterRowChanged', function() {
                    self.countSum(detailTable);
                });

                // 監聽儲存完成
                $(document).on('saveCompleted', function() {
                    self.countSum(detailTable);
                });
            },

            countSum: function(detailTable) {
                // 取得當前單號
                const paperNum = this.getPaperNum();

                if (!paperNum) {
                    $('#lbDAmount').text('0.00');
                    $('#lbCAmount').text('0.00');
                    return;
                }

                // 呼叫 API 計算借貸總額
                $.ajax({
                    url: '/api/SpecialUI/GetPayRecvAmount',
                    method: 'POST',
                    data: {
                        paperNum: paperNum,
                        tableName: detailTable
                    },
                    success: function(data) {
                        const debitAmount = (data.amountD || 0).toLocaleString('en-US', {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        });
                        const creditAmount = (data.amountC || 0).toLocaleString('en-US', {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        });

                        $('#lbDAmount').text(debitAmount);
                        $('#lbCAmount').text(creditAmount);
                    },
                    error: function(xhr, status, error) {
                        console.error('計算借貸總額失敗:', error);
                        $('#lbDAmount').text('ERROR');
                        $('#lbCAmount').text('ERROR');
                    }
                });
            },

            // 取得當前單號的方法
            getPaperNum: function() {
                // 方法 1: 從 input 欄位取得
                let paperNum = $('#PaperNum').val() || $('input[name="PaperNum"]').val();

                // 方法 2: 從 ViewData 取得
                if (!paperNum && window._specialUIConfig) {
                    paperNum = window._specialUIConfig.paperNum;
                }

                // 方法 3: 從 URL 路徑取得（/DynamicTemplate/Paper/{itemId}/{paperNum}）
                if (!paperNum) {
                    const pathParts = window.location.pathname.split('/');
                    if (pathParts.length >= 4 && pathParts[1] === 'DynamicTemplate' && pathParts[2] === 'Paper') {
                        paperNum = pathParts[4];
                    }
                }

                // 方法 4: 從全域變數取得
                if (!paperNum) {
                    paperNum = window._currentPaperNum;
                }

                return paperNum || '';
            }
        },

        // ==========================================
        // 未來可以新增其他特殊介面元件
        // ==========================================

        // 專案統計元件（範例）
        ProjectSummary: {
            render: function(config) {
                return '<div class="special-ui-panel project-summary">專案統計功能開發中...</div>';
            },
            init: function(itemId, config) {
                // 初始化邏輯
            }
        },

        // 庫存統計元件（範例）
        StockSummary: {
            render: function(config) {
                return '<div class="special-ui-panel stock-summary">庫存統計功能開發中...</div>';
            },
            init: function(itemId, config) {
                // 初始化邏輯
            }
        }
    };

    // ==========================================
    // 動態載入特殊介面的主函式
    // ==========================================
    window.loadSpecialUI = function(itemId, specialUIType, specialUIConfig, detailTableName) {
        // 檢查是否有指定特殊介面類型
        if (!specialUIType) {
            return;
        }

        // 檢查元件是否存在
        const component = window.SpecialUIComponents[specialUIType];
        if (!component) {
            console.warn(`找不到特殊介面元件: ${specialUIType}`);
            return;
        }

        // 解析設定
        let config = {};
        if (specialUIConfig) {
            try {
                config = typeof specialUIConfig === 'string'
                    ? JSON.parse(specialUIConfig)
                    : specialUIConfig;
            } catch (e) {
                console.warn('無法解析 SpecialUIConfig:', e);
            }
        }

        // 加入單身表名稱到設定中
        config.detailTableName = detailTableName;

        // 尋找容器
        const container = $('#specialUIContainer');
        if (!container.length) {
            console.warn('找不到 specialUIContainer，無法載入特殊介面');
            return;
        }

        // 渲染 HTML
        const html = component.render(config);
        container.html(html);

        // 初始化元件
        if (component.init) {
            try {
                component.init(itemId, config);
            } catch (e) {
                console.error(`特殊介面初始化失敗: ${specialUIType}`, e);
            }
        }
    };

    // ==========================================
    // 提供手動觸發計算的全域函式（供其他地方呼叫）
    // ==========================================
    window.triggerSpecialUIUpdate = function() {
        $(document).trigger('detailDataChanged');
    };

})();