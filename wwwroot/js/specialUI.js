// specialUI.js - 特殊介面元件庫
// 用於動態載入作業特定的介面元件

(function() {
    'use strict';

    // ==========================================
    // 共用設定檔 - 只需指定 moduleType，後端會自動對應資料表
    // config 可設定：
    //   moduleType: 模組類型（必填）- advance / advancerev
    //   masterTabLabel: 主檔頁籤標籤（預設：沖帳明細）
    //   foreignTabLabel: 外幣頁籤標籤（預設：(外幣)預收付）
    //   localTabLabel: 本位幣頁籤標籤（預設：(本位幣)預收付）
    //   bankTabLabel: 銀存頁籤標籤（預設：銀存預收付）
    //   billTabLabel: 票據頁籤標籤（預設：票據預收付）
    //   otherTabLabel: 其他頁籤標籤（預設：其他預收付）
    // 可用的 moduleType:
    //   - advance: 預收付款 (APRdAdvanceMain, APRdAdvanceSource, APRdAdvanceBill...)
    //   - advancerev: 預收付沖銷 (APRdAdvanceRevMain, APRdAdvanceRevSource...)
    // ==========================================
    const SharedConfigs = {
        // 預收付帳款單設定
        AdvanceDtl: {
            debitLabel: '借方金額',
            creditLabel: '貸方金額',
            masterTabLabel: '沖帳明細',
            foreignTabLabel: '(外幣)預收付',
            localTabLabel: '(本位幣)預收付',
            bankTabLabel: '銀存預收付',
            billTabLabel: '票據預收付',
            otherTabLabel: '其他預收付',
            moduleType: 'advance'  // 後端自動對應: APRdAdvanceMain, APRdAdvanceSource, APRdAdvanceBill...
        },
        // 預收付退款單設定
        AdvanceRevDtl: {
            debitLabel: '借方金額',
            creditLabel: '貸方金額',
            masterTabLabel: '退款明細',
            foreignTabLabel: '(外幣)退款金額',
            localTabLabel: '(本位幣)退款金額',
            bankTabLabel: '銀存退款',
            billTabLabel: '票據退款',
            otherTabLabel: '其他退款',
            moduleType: 'advancerev'  // 後端自動對應: APRdAdvanceRevMain, APRdAdvanceRevSource...
        }
        // === 在此新增共用設定 ===
    };

    // ==========================================
    // 特殊介面設定檔（根據 ItemId 對應設定）
    // 可直接寫設定，或引用 SharedConfigs
    // ==========================================
    window.SpecialUIConfigs = {
        // 方式一：多個 ItemId 引用同一個共用設定
        'AP000004': SharedConfigs.AdvanceDtl,      // 預收付帳款單
        'AP000010': SharedConfigs.AdvanceDtl,      // 預收付帳款單
        'ARM00010': SharedConfigs.AdvanceRevDtl,   // 預收付退款單
        'APR00065': SharedConfigs.AdvanceRevDtl    // 預收付退款單

        // 方式二：引用共用設定並覆寫部分屬性
        // 'APR00068': Object.assign({}, SharedConfigs.apAdvance, {
        //     debitLabel: '自訂借方',
        //     creditLabel: '自訂貸方'
        // }),

        // 方式三：完全獨立的設定
        // 'AP000010': {
        //     debitLabel: '借方金額',
        //     creditLabel: '貸方金額'
        // },

        // === 在此新增您的設定 ===

    };

    // ==========================================
    // 取得指定 ItemId 的設定（合併預設值）
    // ==========================================
    window.getSpecialUIConfig = function(itemId, baseConfig) {
        const itemConfig = window.SpecialUIConfigs[itemId] || {};
        // 合併：baseConfig (資料庫設定) + itemConfig (JS設定)，JS設定優先
        return Object.assign({}, baseConfig || {}, itemConfig);
    };

    // ==========================================
    // 特殊介面元件庫
    // ==========================================
    window.SpecialUIComponents = {

        // ==========================================
        // 借貸總額元件（用於收付傳票等作業）
        // config 可設定：
        //   debitLabel: 借方標籤名稱（預設：借方金額）
        //   creditLabel: 貸方標籤名稱（預設：貸方金額）
        //   apiUrl: 自訂 API 路徑（預設：/api/SpecialUI/GetPayRecvAmount）
        //   debitField: API 回傳的借方欄位名稱（預設：amountD）
        //   creditField: API 回傳的貸方欄位名稱（預設：amountC）
        // ==========================================
        PayRecvSummary: {
            _config: {},

            render: function(config) {
                this._config = config || {};
                const debitLabel = this._config.debitLabel || '借方金額';
                const creditLabel = this._config.creditLabel || '貸方金額';

                const html = `
                    <div class="special-ui-panel pay-recv-summary" style="
                        border-top: 1px solid #ddd;
                        padding: 10px 20px;
                        background-color: #f5f5f5;
                        margin-top: 10px;
                    ">
                        <div style="text-align: right;">
                            <label style="font-weight: bold;">${debitLabel}：</label>
                            <span id="lbDAmount" class="amount-display" style="
                                font-size: 16px;
                                color: #000000;
                                font-weight: 600;
                                display: inline-block;
                                text-align: left;
                            ">0.00</span>
                            <label style="font-weight: bold; margin-left: 10px;">${creditLabel}：</label>
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
                this._config = config || {};
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

                // 取得自訂設定
                const apiUrl = this._config.apiUrl || '/api/SpecialUI/GetPayRecvAmount';
                const debitField = this._config.debitField || 'amountD';
                const creditField = this._config.creditField || 'amountC';

                // 呼叫 API 計算借貸總額
                $.ajax({
                    url: apiUrl,
                    method: 'POST',
                    data: {
                        paperNum: paperNum,
                        tableName: detailTable
                    },
                    success: function(data) {
                        const debitAmount = (data[debitField] || 0).toLocaleString('en-US', {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        });
                        const creditAmount = (data[creditField] || 0).toLocaleString('en-US', {
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
        },

        StrikeDetail: {
            _config: {},

            // 不渲染獨立容器，改為整合到模板頁籤
            render: function(config) {
                this._config = config || {};
                return '';
            },

            init: function(itemId, config) {
                this._config = config || {};
                // 注入頁籤到現有模板結構
                this.injectIntoExistingTabs();
                // 綁定事件
                this.bindEvents();
                // 初始載入資料
                this.loadAllData();
            },

            // ==========================================
            // 注入頁籤到現有的主檔/明細頁籤結構
            // ==========================================
            injectIntoExistingTabs: function() {
                const cfg = this._config;
                // 取得標籤設定（可自訂）
                const masterTabLabel = cfg.masterTabLabel || '沖帳明細';
                const foreignTabLabel = cfg.foreignTabLabel || '(外幣)預收付';
                const localTabLabel = cfg.localTabLabel || '(本位幣)預收付';

                // === 1. 注入主檔頁籤（沖帳明細 - 預收付源頭）到 #headerTabs ===
                const headerTabs = $('#headerTabs');
                const headerTabContent = headerTabs.closest('form').find('.tab-content').first();

                if (headerTabs.length && headerTabContent.length) {
                    // 新增主檔頁籤按鈕
                    const masterTabBtn = `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" id="tab-strike-master-tab" data-bs-toggle="tab"
                                data-bs-target="#tab-strike-master-content" type="button" role="tab">
                                ${masterTabLabel}
                            </button>
                        </li>
                    `;
                    headerTabs.append(masterTabBtn);

                    // 新增主檔頁籤內容（包含外幣/本位幣子頁籤）
                    const masterTabContent = `
                        <div class="tab-pane fade" id="tab-strike-master-content" role="tabpanel">
                            <div class="strike-master-content" style="padding: 10px;">
                                <!-- 預收付子頁籤 -->
                                <ul class="nav nav-tabs" id="strikeSourceTabs" role="tablist" style="border-bottom: 1px solid #dee2e6;">
                                    <li class="nav-item" role="presentation">
                                        <button class="nav-link active" id="source-foreign-tab" data-bs-toggle="tab"
                                            data-bs-target="#source-foreign-pane" type="button" role="tab">
                                            ${foreignTabLabel}
                                        </button>
                                    </li>
                                    <li class="nav-item" role="presentation">
                                        <button class="nav-link" id="source-local-tab" data-bs-toggle="tab"
                                            data-bs-target="#source-local-pane" type="button" role="tab">
                                            ${localTabLabel}
                                        </button>
                                    </li>
                                </ul>
                                <div class="tab-content" id="strikeSourceTabContent" style="padding: 10px;">
                                    <!-- 外幣預收付 -->
                                    <div class="tab-pane fade show active" id="source-foreign-pane" role="tabpanel">
                                        <div class="table-responsive strike-table-wrapper">
                                            <table class="table table-sm table-bordered table-striped" id="gridSourceForeign">
                                                <thead class="table-light">
                                                    <tr>
                                                        <th style="width: 60px;">項目</th>
                                                        <th>現金預收付</th>
                                                        <th>銀行預收付</th>
                                                        <th>票據預收付</th>
                                                        <th>其它預收付</th>
                                                    </tr>
                                                </thead>
                                                <tbody></tbody>
                                            </table>
                                        </div>
                                    </div>
                                    <!-- 本位幣預收付 -->
                                    <div class="tab-pane fade" id="source-local-pane" role="tabpanel">
                                        <div class="table-responsive strike-table-wrapper">
                                            <table class="table table-sm table-bordered table-striped" id="gridSourceLocal">
                                                <thead class="table-light">
                                                    <tr>
                                                        <th style="width: 60px;">項目</th>
                                                        <th>現金預收付</th>
                                                        <th>銀行預收付</th>
                                                        <th>票據預收付</th>
                                                        <th>其它預收付</th>
                                                    </tr>
                                                </thead>
                                                <tbody></tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    headerTabContent.append(masterTabContent);
                }

                // === 2. 注入明細頁籤（銀存/票據/其他預收付）到 .multi-tab-detail ===
                const detailTabs = $('.multi-tab-detail .nav-tabs').first();
                const detailTabContent = $('.multi-tab-detail .tab-content').first();

                // 取得明細頁籤標籤設定
                const detailTabLabel = cfg.detailTabLabel || '沖帳明細';
                const bankTabLabel = cfg.bankTabLabel || '銀存預收付';
                const billTabLabel = cfg.billTabLabel || '票據預收付';
                const otherTabLabel = cfg.otherTabLabel || '其他預收付';

                if (detailTabs.length && detailTabContent.length) {
                    /* ===== 主頁籤：沖帳明細 ===== */
                    detailTabs.append(`
                        <li class="nav-item" role="presentation">
                            <button class="nav-link"
                                id="tab-strike-main"
                                data-bs-toggle="tab"
                                data-bs-target="#content-strike-main"
                                type="button"
                                role="tab">
                                ${detailTabLabel}
                            </button>
                        </li>
                    `);

                    detailTabContent.append(`
                        <div class="tab-pane fade" id="content-strike-main" role="tabpanel">
                            <div class="p-2">

                                <!-- 次頁籤 -->
                                <ul class="nav nav-tabs mb-2 strike-sub-tabs" role="tablist"></ul>

                                <!-- 次頁籤內容 -->
                                <div class="tab-content strike-sub-content"></div>

                            </div>
                        </div>
                    `);

                    const subTabs = $('#content-strike-main .strike-sub-tabs');
                    const subContent = $('#content-strike-main .strike-sub-content');

                    // 新增明細頁籤按鈕 - 銀存預收付
                    const bankTabBtn = `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" id="tab-strike-bank"
                                data-bs-toggle="tab" data-bs-target="#content-strike-bank"
                                data-tab-id="strike-bank" type="button" role="tab">
                                ${bankTabLabel}
                            </button>
                        </li>
                    `;
                    subTabs.append(bankTabBtn);

                    // 新增明細頁籤按鈕 - 票據預收付
                    const billTabBtn = `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" id="tab-strike-bill"
                                data-bs-toggle="tab" data-bs-target="#content-strike-bill"
                                data-tab-id="strike-bill" type="button" role="tab">
                                ${billTabLabel}
                            </button>
                        </li>
                    `;
                    subTabs.append(billTabBtn);

                    // 新增明細頁籤按鈕 - 其他預收付
                    const otherTabBtn = `
                        <li class="nav-item" role="presentation">
                            <button class="nav-link" id="tab-strike-other"
                                data-bs-toggle="tab" data-bs-target="#content-strike-other"
                                data-tab-id="strike-other" type="button" role="tab">
                                ${otherTabLabel}
                            </button>
                        </li>
                    `;
                    subTabs.append(otherTabBtn);

                    // 新增明細頁籤內容 - 銀存預收付
                    const bankTabContent = `
                        <div class="tab-pane fade" id="content-strike-bank" role="tabpanel">
                            <div class="strike-detail-content" style="padding: 10px;">
                                <div class="row mb-2">
                                    <div class="col-md-4">
                                        <label class="form-label">銀行名稱</label>
                                        <input type="text" class="form-control form-control-sm" id="txtBankId" readonly>
                                    </div>
                                    <div class="col-md-4">
                                        <label class="form-label">帳戶編號</label>
                                        <input type="text" class="form-control form-control-sm" id="txtAccountId" readonly>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    subContent.append(bankTabContent);

                    // 新增明細頁籤內容 - 票據預收付
                    const billTabContent = `
                        <div class="tab-pane fade" id="content-strike-bill" role="tabpanel">
                            <div class="strike-detail-content" style="padding: 10px;">
                                <div class="table-responsive strike-table-wrapper">
                                    <table class="table table-sm table-bordered table-striped" id="gridBill">
                                        <thead class="table-light">
                                            <tr>
                                                <th style="width: 50px;">項目</th>
                                                <th>票據號碼</th>
                                                <th>票據金額</th>
                                                <th>開票銀行</th>
                                                <th>開票帳戶</th>
                                                <th>收票銀行</th>
                                                <th>收票帳戶</th>
                                                <th>票據種類</th>
                                                <th>票別</th>
                                                <th>到期日</th>
                                                <th>禁背</th>
                                                <th>劃線</th>
                                                <th>抬頭</th>
                                            </tr>
                                        </thead>
                                        <tbody></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    `;
                    subContent.append(billTabContent);

                    // 新增明細頁籤內容 - 其他預收付
                    const otherTabContent = `
                        <div class="tab-pane fade" id="content-strike-other" role="tabpanel">
                            <div class="strike-detail-content" style="padding: 10px;">
                                <div class="row">
                                    <div class="col-md-7">
                                        <div class="table-responsive strike-table-wrapper">
                                            <table class="table table-sm table-bordered table-striped" id="gridOtherAccDtl" data-dict-table="APRdAdvanceOtherAccDtl">
                                                <thead class="table-light">
                                                    <tr>
                                                        <th style="width: 40px;">項目</th>
                                                        <th>科目</th>
                                                        <th>科目名稱</th>
                                                        <th>子科目</th>
                                                        <th>子科目名稱</th>
                                                        <th>金額</th>
                                                        <th>外幣金額</th>
                                                    </tr>
                                                </thead>
                                                <tbody></tbody>
                                            </table>
                                        </div>
                                    </div>
                                    <div class="col-md-5">
                                        <div class="card">
                                            <div class="card-header py-1">手續費(台幣)</div>
                                            <div class="card-body p-2">
                                                <div class="table-responsive strike-table-wrapper">
                                                    <table class="table table-sm table-bordered table-striped" id="gridStrikePost" data-dict-table="APRdStrikePost">
                                                        <thead class="table-light">
                                                            <tr>
                                                                <th style="width: 40px;">項目</th>
                                                                <th>金額</th>
                                                                <th>銀行</th>
                                                                <th>銀行名稱</th>
                                                                <th>帳號</th>
                                                                <th>科目</th>
                                                                <th>科目名稱</th>
                                                                <th>明細科目</th>
                                                                <th>明細科目名稱</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody></tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `;
                    subContent.append(otherTabContent);
                }

                // 注入樣式
                this.injectStyles();
            },

            // 注入沖帳明細專用樣式
            injectStyles: function() {
                if ($('#strike-detail-styles').length) return;
                const styles = `
                    <style id="strike-detail-styles">
                        /* 沖帳明細頁籤樣式 */
                        .strike-master-content .nav-tabs .nav-link,
                        .strike-detail-content .nav-tabs .nav-link {
                            font-size: var(--field-label-size, 13px);
                            padding: 6px 12px;
                        }
                        .strike-table-wrapper {
                            max-height: 250px;
                            overflow-y: auto;
                        }
                        .strike-table-wrapper table {
                            font-size: var(--field-value-size, 13px);
                        }
                        .strike-table-wrapper th {
                            position: sticky;
                            top: 0;
                            z-index: 1;
                            background: #f8f9fa !important;
                        }
                        .strike-detail-content {
                            background: #fff;
                        }
                        #tab-strike-master-content .strike-master-content {
                            background: #fafbfc;
                            border-radius: 0 0 8px 8px;
                        }
                    </style>
                `;
                $('head').append(styles);
            },

            bindEvents: function() {
                const self = this;

                // 監聽單頭切換
                $(document).on('masterRowChanged', function() {
                    self.loadAllData();
                });

                // 監聽資料變更
                $(document).on('detailDataChanged', function() {
                    self.loadAllData();
                });

                // 監聽儲存完成
                $(document).on('saveCompleted', function() {
                    self.loadAllData();
                });
            },

            loadAllData: function() {
                const paperNum = this.getPaperNum();
                if (!paperNum) {
                    this.clearAllGrids();
                    return;
                }

                const cfg = this._config;
                // 取得模組類型設定（後端會自動對應正確的資料表）
                const moduleType = cfg.moduleType || 'advance';

                // 統一使用 AdvanceDtl API 載入所有資料（透過 moduleType 自動對應資料表）
                this.loadAdvanceDtlData('GetSource', moduleType, paperNum, this.renderSourceGrids.bind(this));
                this.loadAdvanceDtlData('GetBill', moduleType, paperNum, this.renderBillGrid.bind(this));
                this.loadAdvanceDtlData('GetOtherAcc', moduleType, paperNum, this.renderOtherAccDtlGrid.bind(this));
                this.loadAdvanceDtlData('GetStrikePost', moduleType, paperNum, this.renderStrikePostGrid.bind(this));
                this.loadAdvanceDtlData('GetMaster', moduleType, paperNum, this.renderBankInfo.bind(this));
            },

            // 使用 AdvanceDtl API 載入資料（透過 moduleType 自動對應資料表）
            loadAdvanceDtlData: function(apiMethod, moduleType, paperNum, callback) {
                $.ajax({
                    url: '/api/AdvanceDtl/' + apiMethod,
                    method: 'GET',
                    data: {
                        moduleType: moduleType,
                        paperNum: paperNum
                    },
                    success: function(resp) {
                        if (resp && resp.ok) {
                            // GetMaster 回傳 data（單筆物件），其他回傳 rows（陣列）
                            if (resp.data !== undefined) {
                                callback(resp.data);
                            } else if (resp.rows) {
                                callback(resp.rows);
                            } else {
                                callback([]);
                            }
                        } else {
                            callback([]);
                        }
                    },
                    error: function(xhr, status, error) {
                        console.error('載入 ' + apiMethod + ' 資料失敗:', error);
                        callback([]);
                    }
                });
            },

            clearAllGrids: function() {
                $('#gridSourceForeign tbody').empty();
                $('#gridSourceLocal tbody').empty();
                $('#gridBill tbody').empty();
                $('#gridOtherAccDtl tbody').empty();
                $('#gridStrikePost tbody').empty();
                $('#txtBankId').val('');
                $('#txtAccountId').val('');
            },

            getPaperNum: function() {
                let paperNum = $('#PaperNum').val() || $('input[name="PaperNum"]').val();
                if (!paperNum && window._specialUIConfig) {
                    paperNum = window._specialUIConfig.paperNum;
                }
                if (!paperNum) {
                    const pathParts = window.location.pathname.split('/');
                    if (pathParts.length >= 5 && pathParts[1] === 'DynamicTemplate' && pathParts[2] === 'Paper') {
                        paperNum = pathParts[4];
                    }
                }
                if (!paperNum) {
                    paperNum = window._currentPaperNum;
                }
                return paperNum || '';
            },

            // 通用的資料載入方法，使用 DynamicTable API
            loadTableData: function(tableName, paperNum, callback) {
                $.ajax({
                    url: '/api/DynamicTable/ByPaperNum',
                    method: 'GET',
                    data: {
                        table: tableName,
                        paperNum: paperNum
                    },
                    success: function(data) {
                        callback(data);
                    },
                    error: function(xhr, status, error) {
                        console.error('載入 ' + tableName + ' 資料失敗:', error);
                        callback([]);
                    }
                });
            },

            // 渲染預收付 Grid（外幣 + 本位幣）
            renderSourceGrids: function(data) {
                this.renderSourceForeignGrid(data);
                this.renderSourceLocalGrid(data);
            },

            // 通用的欄位取值方法（不區分大小寫）
            getField: function(row, fieldName) {
                if (!row) return '';
                // 先嘗試原始名稱
                if (row[fieldName] !== undefined) return row[fieldName];
                // 再嘗試不區分大小寫
                const lowerName = fieldName.toLowerCase();
                for (const key in row) {
                    if (key.toLowerCase() === lowerName) {
                        return row[key];
                    }
                }
                return '';
            },

            renderSourceForeignGrid: function(data) {
                const self = this;
                const tbody = $('#gridSourceForeign tbody');
                tbody.empty();
                if (!data || data.length === 0) {
                    tbody.append('<tr><td colspan="5" class="text-center text-muted">無資料</td></tr>');
                    return;
                }
                data.forEach(function(row) {
                    const tr = $('<tr>');
                    tr.append($('<td>').text(self.getField(row, 'Item') || ''));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'CashAmountOg'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'BankAmountOg'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'BillAmountOg'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'OtherAmountOg'))));
                    tbody.append(tr);
                });
            },

            renderSourceLocalGrid: function(data) {
                const self = this;
                const tbody = $('#gridSourceLocal tbody');
                tbody.empty();
                if (!data || data.length === 0) {
                    tbody.append('<tr><td colspan="5" class="text-center text-muted">無資料</td></tr>');
                    return;
                }
                data.forEach(function(row) {
                    const tr = $('<tr>');
                    tr.append($('<td>').text(self.getField(row, 'Item') || ''));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'CashAmount'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'BankAmount'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'BillAmount'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'OtherAmount'))));
                    tbody.append(tr);
                });
            },

            renderBillGrid: function(data) {
                const self = this;
                const tbody = $('#gridBill tbody');
                tbody.empty();
                if (!data || data.length === 0) {
                    tbody.append('<tr><td colspan="13" class="text-center text-muted">無資料</td></tr>');
                    return;
                }
                data.forEach(function(row) {
                    const tr = $('<tr>');
                    tr.append($('<td>').text(self.getField(row, 'Item') || ''));
                    tr.append($('<td>').text(self.getField(row, 'BillId') || ''));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'Amount'))));
                    tr.append($('<td>').text(self.getField(row, 'PayBankId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'PayAccountId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'RecvBankId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'RecvAccountId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'BillAccId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'BillTypeId') || ''));
                    tr.append($('<td>').text(self.formatDate(self.getField(row, 'DueDate'))));
                    tr.append($('<td>').text(self.getField(row, 'Inhibit') == 1 ? '是' : ''));
                    tr.append($('<td>').text(self.getField(row, 'ParaLine') == 1 ? '是' : ''));
                    tr.append($('<td>').text(self.getField(row, 'Title') == 1 ? '是' : ''));
                    tbody.append(tr);
                });
            },

            renderOtherAccDtlGrid: function(data) {
                const self = this;
                const tbody = $('#gridOtherAccDtl tbody');
                tbody.empty();
                if (!data || data.length === 0) {
                    tbody.append('<tr><td colspan="7" class="text-center text-muted">無資料</td></tr>');
                    return;
                }
                data.forEach(function(row) {
                    const tr = $('<tr>');
                    tr.append($('<td>').text(self.getField(row, 'Item') || ''));
                    tr.append($('<td>').text(self.getField(row, 'AccId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'AccIdName') || ''));
                    tr.append($('<td>').text(self.getField(row, 'SubAccId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'SubAccName') || ''));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'Amount'))));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'AmountOg'))));
                    tbody.append(tr);
                });
            },

            renderStrikePostGrid: function(data) {
                const self = this;
                const tbody = $('#gridStrikePost tbody');
                tbody.empty();
                if (!data || data.length === 0) {
                    tbody.append('<tr><td colspan="9" class="text-center text-muted">無資料</td></tr>');
                    return;
                }
                data.forEach(function(row) {
                    const tr = $('<tr>');
                    tr.append($('<td>').text(self.getField(row, 'Item') || ''));
                    tr.append($('<td>').text(self.formatNumber(self.getField(row, 'PostAmount'))));
                    tr.append($('<td>').text(self.getField(row, 'PostBankId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'PostBankName') || ''));
                    tr.append($('<td>').text(self.getField(row, 'PostAccountId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'PostAccId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'AccIdName') || ''));
                    tr.append($('<td>').text(self.getField(row, 'PostSubAccId') || ''));
                    tr.append($('<td>').text(self.getField(row, 'SubAccName') || ''));
                    tbody.append(tr);
                });
            },

            // 渲染銀行資訊（從 GetMaster API 回傳的資料）
            renderBankInfo: function(data) {
                // GetMaster 回傳的是單筆物件，不是陣列
                if (data && typeof data === 'object') {
                    $('#txtBankId').val(this.getField(data, 'BankId') || '');
                    $('#txtAccountId').val(this.getField(data, 'AccountId') || '');
                } else {
                    $('#txtBankId').val('');
                    $('#txtAccountId').val('');
                }
            },

            formatNumber: function(value) {
                if (value === null || value === undefined || value === '') return '';
                const num = parseFloat(value);
                if (isNaN(num)) return value;
                return num.toLocaleString('en-US', {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                });
            },

            formatDate: function(value) {
                if (!value) return '';
                try {
                    const date = new Date(value);
                    if (isNaN(date.getTime())) return value;
                    return date.toLocaleDateString('zh-TW');
                } catch (e) {
                    return value;
                }
            }
        }
    };

    // ==========================================
    // 動態載入特殊介面的主函式
    // 支援多個特殊介面，用逗號分隔，例如: 'PayRecvSummary,StrikeDetail'
    // ==========================================
    window.loadSpecialUI = function(itemId, specialUIType, specialUIConfig, detailTableName, masterTableName, paperNum) {
        // 檢查是否有指定特殊介面類型
        if (!specialUIType) {
            return;
        }

        // 尋找容器
        const container = $('#specialUIContainer');
        if (!container.length) {
            console.warn('找不到 specialUIContainer，無法載入特殊介面');
            return;
        }

        // 解析資料庫傳入的設定
        let dbConfig = {};
        if (specialUIConfig) {
            try {
                dbConfig = typeof specialUIConfig === 'string'
                    ? JSON.parse(specialUIConfig)
                    : specialUIConfig;
            } catch (e) {
                console.warn('無法解析 SpecialUIConfig:', e);
            }
        }

        // 根據 ItemId 取得 JS 設定並合併（JS 設定優先於資料庫設定）
        const config = window.getSpecialUIConfig(itemId, dbConfig);

        // 加入表名稱到設定中（將 Main 替換為 Sub）
        config.detailTableName = masterTableName.replace('Main', 'Sub');

        // 支援多個特殊介面（逗號分隔）
        const types = specialUIType.split(',').map(function(t) { return t.trim(); }).filter(function(t) { return t; });

        // 清空容器
        container.empty();

        // 依序載入每個特殊介面
        types.forEach(function(type) {
            const component = window.SpecialUIComponents[type];
            if (!component) {
                console.warn('找不到特殊介面元件: ' + type);
                return;
            }

            // 渲染 HTML 並附加到容器
            const html = component.render(config);
            container.append(html);

            // 初始化元件
            if (component.init) {
                try {
                    component.init(itemId, config);
                } catch (e) {
                    console.error('特殊介面初始化失敗: ' + type, e);
                }
            }
        });
    };

    // ==========================================
    // 提供手動觸發計算的全域函式（供其他地方呼叫）
    // ==========================================
    window.triggerSpecialUIUpdate = function() {
        $(document).trigger('detailDataChanged');
    };

})();