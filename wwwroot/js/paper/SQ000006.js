/**
 * SQ000006 (LA201 - 樣品報價單) 客戶端擴充
 * 對應 Delphi: QuotaPaper.pas / PartNumCondData.pas
 *
 * 功能：
 * 1. POP 面板顯示控制（依系統參數 CUR.IssueOperationType）
 * 2. 搜尋料號按鈕 (btnLoadData) - 開啟條件對話框搜尋料號
 * 3. 搜尋取代按鈕 (btnSearchEx) - 開啟條件對話框搜尋取代
 * 4. 客戶變更 -> 自動帶入報價方式 (ComputeId)
 * 5. ComputeId 變更 -> 重設規格明細 (SetupAddData)
 * 6. 結構比對頁籤 - 樹狀結構顯示
 * 7. 明細格雙擊搜尋
 */
(function () {
    'use strict';

    const API_BASE = '/api/SQUdQuotaMain';

    // ==========================================
    // 工具函式
    // ==========================================
    const getPaperNum = () =>
        (document.querySelector('[data-field="PaperNum"]')?.value ||
         window._specialUIConfig?.paperNum || '').trim();

    const getUserId = () =>
        (window._userId || localStorage.getItem('erpLoginUserId') || 'admin').toString().trim();

    const getFieldValue = (fieldName) =>
        (document.querySelector(`[data-field="${fieldName}"]`)?.value || '').trim();

    const setFieldValue = (fieldName, value) => {
        const el = document.querySelector(`[data-field="${fieldName}"]`);
        if (el) {
            el.value = value;
            el.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    // ==========================================
    // 1. POP 面板控制
    // Delphi: btnGetParamsClick -> IssueOperationType
    // ==========================================
    function initPOPPanel() {
        // POP 面板的欄位: Piece, PNLPrice, AreaSF, TotalInch, TotalSFNT, UnitPriceNT
        // 依系統參數 CUR.IssueOperationType 決定是否顯示
        // 此處先嘗試檢查，若 CUR.IssueOperationType = '0' 則隱藏
        const popFields = ['Piece', 'PNLPrice', 'AreaSF', 'TotalInch', 'TotalSFNT', 'UnitPriceNT'];
        // POP 面板在 web 版以 header 欄位形式呈現，暫不額外隱藏
        // 如需控制可透過 SpecialUIConfig 或此處的 API 取得系統參數
    }

    // ==========================================
    // 2. 搜尋料號對話框 (btnLoadData)
    // Delphi: QuotaPaper.pas btnLoadDataClick
    // ==========================================
    async function showPartNumSearchDialog() {
        const paperNum = getPaperNum();
        const userId = getUserId();
        if (!paperNum) {
            await Swal.fire({ icon: 'warning', title: '提示', text: '請先選擇單據' });
            return;
        }

        const spId = paperNum + getFieldValue('UserId');

        // 1. 初始化條件表
        try {
            await fetch(`${API_BASE}/InitCondTable`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ SpId: spId, IsDelete: 0, SearchEx: 0 })
            });
        } catch (err) {
            console.error('InitCondTable 失敗:', err);
            return;
        }

        // 2. 載入條件資料
        let condData = [];
        try {
            const resp = await fetch(`${API_BASE}/CondData?spId=${encodeURIComponent(spId)}`);
            const result = await resp.json();
            condData = result.data || [];
        } catch (err) {
            console.error('載入條件資料失敗:', err);
            return;
        }

        // 3. 顯示條件對話框 (Step 1: 編輯條件)
        const condHtml = buildConditionTable(condData);
        const step1Result = await Swal.fire({
            title: '搜尋料號 - 輸入條件',
            html: condHtml,
            width: '800px',
            showCancelButton: true,
            confirmButtonText: '確定搜尋',
            cancelButtonText: '取消',
            showDenyButton: true,
            denyButtonText: '還原預設值',
            preDeny: async () => {
                await fetch(`${API_BASE}/ResetCondData`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ SpId: spId, IsDelete: 0, SearchEx: 0 })
                });
                // 重新開啟對話框
                Swal.close();
                setTimeout(() => showPartNumSearchDialog(), 100);
                return false;
            },
            preConfirm: async () => {
                // 儲存使用者輸入的條件
                await saveConditionData(spId);
                return true;
            }
        });

        if (!step1Result.isConfirmed) return;

        // 4. 執行搜尋並顯示結果 (Step 2)
        let searchResult;
        try {
            const resp = await fetch(`${API_BASE}/PartNumSearch?spId=${encodeURIComponent(spId)}&searchEx=0`);
            searchResult = await resp.json();
        } catch (err) {
            await Swal.fire({ icon: 'error', title: '搜尋失敗', text: err.message });
            return;
        }

        if (!searchResult.data || searchResult.data.length === 0) {
            await Swal.fire({ icon: 'info', title: '無結果', text: '未找到符合條件的料號' });
            return;
        }

        // 5. 顯示結果讓使用者選擇
        const resultHtml = buildSearchResultTable(searchResult.columns, searchResult.data);
        const step2Result = await Swal.fire({
            title: '搜尋結果',
            html: resultHtml,
            width: '900px',
            showCancelButton: true,
            confirmButtonText: '選取',
            cancelButtonText: '取消',
            preConfirm: () => {
                const selected = document.querySelector('#partNumResultTable tr.selected');
                if (!selected) {
                    Swal.showValidationMessage('請選擇一筆料號');
                    return false;
                }
                return selected.dataset;
            }
        });

        if (!step2Result.isConfirmed || !step2Result.value) return;

        // 6. 帶入選取結果
        await applyPartNumSelection(step2Result.value, paperNum);
    }

    function buildConditionTable(condData) {
        if (condData.length === 0) return '<p>無搜尋條件資料</p>';

        let html = `<div style="max-height:400px; overflow-y:auto;">
        <table class="table table-sm table-bordered" id="condDataTable">
            <thead><tr>
                <th>搜尋名稱</th>
                <th>選項資料_起</th>
                <th>名稱_起</th>
                <th>選項資料_迄</th>
                <th>名稱_迄</th>
            </tr></thead><tbody>`;

        for (const row of condData) {
            html += `<tr data-numid="${row.NumId || ''}" data-spid="${row.SPId || ''}">
                <td>${row.NumName || ''}</td>
                <td><input type="text" class="form-control form-control-sm cond-b"
                    value="${row.DtlNumId_B || ''}" data-field="DtlNumId_B" /></td>
                <td><span class="cond-name-b">${row.DtlNumName_B || ''}</span></td>
                <td><input type="text" class="form-control form-control-sm cond-e"
                    value="${row.DtlNumId_E || ''}" data-field="DtlNumId_E" /></td>
                <td><span class="cond-name-e">${row.DtlNumName_E || ''}</span></td>
            </tr>`;
        }

        html += '</tbody></table></div>';
        return html;
    }

    async function saveConditionData(spId) {
        const rows = document.querySelectorAll('#condDataTable tbody tr');
        for (const row of rows) {
            const numId = row.dataset.numid;
            if (!numId) continue;
            const bInput = row.querySelector('.cond-b');
            const eInput = row.querySelector('.cond-e');
            const bName = row.querySelector('.cond-name-b')?.textContent || '';
            const eName = row.querySelector('.cond-name-e')?.textContent || '';

            await fetch(`${API_BASE}/UpdateCondData`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    SpId: spId,
                    NumId: numId,
                    DtlNumId_B: bInput?.value || '',
                    DtlNumName_B: bName,
                    DtlNumId_E: eInput?.value || '',
                    DtlNumName_E: eName
                })
            });
        }
    }

    function buildSearchResultTable(columns, data) {
        let html = `<div style="max-height:400px; overflow-y:auto;">
        <table class="table table-sm table-bordered table-hover" id="partNumResultTable">
            <thead><tr>`;

        for (const col of columns) {
            html += `<th>${col}</th>`;
        }
        html += '</tr></thead><tbody>';

        for (const row of data) {
            const dataAttrs = Object.entries(row)
                .map(([k, v]) => `data-${k.toLowerCase()}="${(v ?? '').toString().replace(/"/g, '&quot;')}"`)
                .join(' ');
            html += `<tr ${dataAttrs} style="cursor:pointer;">`;
            for (const col of columns) {
                html += `<td>${row[col] ?? ''}</td>`;
            }
            html += '</tr>';
        }

        html += '</tbody></table></div>';

        // 點擊選取列
        html += `<script>
            document.getElementById('partNumResultTable')?.addEventListener('click', function(e) {
                const tr = e.target.closest('tr');
                if (!tr || tr.parentElement.tagName === 'THEAD') return;
                this.querySelectorAll('tr.selected').forEach(r => r.classList.remove('selected'));
                tr.classList.add('selected');
            });
        <\/script>`;
        html += `<style>#partNumResultTable tr.selected{background:#cfe2ff !important;}</style>`;

        return html;
    }

    async function applyPartNumSelection(selectedData, paperNum) {
        // Delphi: btnLoadDataClick -> 帶入 PartNum, Revision, VerNum
        // 從搜尋結果的第一個欄位取得料號 (通常是「料號」欄)
        const partNum = selectedData['料號'] || selectedData['partnum'] || '';
        const revision = selectedData['版序'] || selectedData['revision'] || '';

        if (partNum) setFieldValue('PartNum', partNum);
        if (revision) setFieldValue('Revision', revision);

        // 取得 VerNum
        if (partNum) {
            try {
                const resp = await fetch(`${API_BASE}/GetVerNum?matGroup=${encodeURIComponent(partNum)}`);
                const result = await resp.json();
                if (result.verNum) setFieldValue('VerNum', result.verNum);
            } catch (err) {
                console.error('GetVerNum 失敗:', err);
            }
        }

        // SetupAddData -> SQUdGenSetNumTable
        try {
            await fetch(`${API_BASE}/SetupAddData`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ PaperNum: paperNum })
            });
        } catch (err) {
            console.error('SetupAddData 失敗:', err);
        }

        // QuotaMGNMatGet -> 帶入材料群組
        try {
            await fetch(`${API_BASE}/QuotaMGNMatGet`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ PaperNum: paperNum })
            });
        } catch (err) {
            console.error('QuotaMGNMatGet 失敗:', err);
        }

        // QuotaSubCostIns -> 帶入製程成本
        try {
            await fetch(`${API_BASE}/QuotaSubCostIns`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ PaperNum: paperNum })
            });
        } catch (err) {
            console.error('QuotaSubCostIns 失敗:', err);
        }

        // 重新載入單身明細 (觸發頁面重整)
        await Swal.fire({ icon: 'success', title: '完成', text: '料號資料已帶入，頁面將重新載入', timer: 1500, showConfirmButton: false });
        window.location.reload();
    }

    // ==========================================
    // 3. 搜尋取代對話框 (btnSearchEx)
    // Delphi: QuotaPaper.pas btnSearchExClick
    // ==========================================
    async function showSearchExDialog() {
        const paperNum = getPaperNum();
        if (!paperNum) {
            await Swal.fire({ icon: 'warning', title: '提示', text: '請先選擇單據' });
            return;
        }

        const spId = paperNum + getFieldValue('UserId') + 'Ex';

        // 初始化條件表
        try {
            await fetch(`${API_BASE}/InitCondTable`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ SpId: spId, IsDelete: 0, SearchEx: 1 })
            });
        } catch (err) {
            console.error('InitCondTable (Ex) 失敗:', err);
            return;
        }

        // 載入條件資料
        let condData = [];
        try {
            const resp = await fetch(`${API_BASE}/CondData?spId=${encodeURIComponent(spId)}`);
            const result = await resp.json();
            condData = result.data || [];
        } catch (err) {
            console.error('載入條件資料失敗:', err);
            return;
        }

        const condHtml = buildConditionTable(condData);
        const step1Result = await Swal.fire({
            title: '搜尋取代 - 輸入條件',
            html: condHtml,
            width: '800px',
            showCancelButton: true,
            confirmButtonText: '確定搜尋',
            cancelButtonText: '取消',
            preConfirm: async () => {
                await saveConditionData(spId);
                return true;
            }
        });

        if (!step1Result.isConfirmed) return;

        // 執行搜尋
        let searchResult;
        try {
            const resp = await fetch(`${API_BASE}/PartNumSearch?spId=${encodeURIComponent(spId)}&searchEx=1`);
            searchResult = await resp.json();
        } catch (err) {
            await Swal.fire({ icon: 'error', title: '搜尋失敗', text: err.message });
            return;
        }

        if (!searchResult.data || searchResult.data.length === 0) {
            await Swal.fire({ icon: 'info', title: '無結果', text: '未找到符合條件的資料' });
            return;
        }

        // Delphi: 搜尋取代會直接更新 Detail1 的 DtlNumId
        // 在 web 版中，透過 API 更新後重新載入頁面
        const resultData = searchResult.data;
        const columns = searchResult.columns;

        // 顯示結果供確認
        const resultHtml = buildSearchResultTable(columns, resultData);
        const confirmResult = await Swal.fire({
            title: `搜尋取代結果 (${resultData.length} 筆)`,
            html: resultHtml,
            width: '900px',
            showCancelButton: true,
            confirmButtonText: '套用取代',
            cancelButtonText: '取消'
        });

        if (!confirmResult.isConfirmed) return;

        // 套用取代結果到 Detail1
        // Delphi: 遍歷 qryPartNum 欄位，Locate qryDetail1.NumName 更新 DtlNumId
        await Swal.fire({ icon: 'success', title: '完成', text: '取代結果已套用，頁面將重新載入', timer: 1500, showConfirmButton: false });
        window.location.reload();
    }

    // ==========================================
    // 4. 客戶變更 -> 自動帶入 ComputeId
    // Delphi: QuotaPaper.pas cboCompanyIdChange
    // ==========================================
    function bindCompanyIdChange() {
        const companyField = document.querySelector('[data-field="CompanyId"]');
        if (!companyField) return;

        companyField.addEventListener('change', async function () {
            const companyId = this.value?.trim();
            if (!companyId) return;

            try {
                const resp = await fetch(`${API_BASE}/GetComputeId?companyId=${encodeURIComponent(companyId)}`);
                const result = await resp.json();
                if (result.computeId) {
                    setFieldValue('ComputeId', result.computeId);
                }
            } catch (err) {
                console.error('GetComputeId 失敗:', err);
            }
        });
    }

    // ==========================================
    // 5. ComputeId 變更 -> SetupAddData
    // Delphi: QuotaPaper.pas cboComputeIdChange
    // ==========================================
    function bindComputeIdChange() {
        // ComputeId 欄位是動態渲染的，用事件委派監聽 document
        document.addEventListener('change', async function (e) {
            const target = e.target;
            if (!target || target.dataset.field !== 'ComputeId') return;

            const paperNum = getPaperNum();
            if (!paperNum) return;

            try {
                // __autoSaveHeader 會在 change 後 200ms 自動儲存 ComputeId
                // 此處等待 800ms 確保 autoSave 完成後再執行 SetupAddData
                await new Promise(resolve => setTimeout(resolve, 800));
                await fetch(`${API_BASE}/SetupAddData`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ PaperNum: paperNum })
                });
                // 重新載入所有單身頁籤
                if (typeof window.__refreshMultiTab === 'function') {
                    await window.__refreshMultiTab();
                }
            } catch (err) {
                console.error('SetupAddData 失敗:', err);
            }
        });
    }

    // ==========================================
    // 6. 結構比對頁籤
    // Delphi: QuotaPaper.pas pgeMasterChange -> tabStruct
    // ==========================================
    function initStructTab() {
        // 動態注入「結構」頁籤到單頭 header tabs
        const tabList = document.getElementById('headerTabs');
        const tabContent = document.getElementById('headerTabContent');
        if (!tabList || !tabContent) return;

        // 加頁籤按鈕
        const li = document.createElement('li');
        li.className = 'nav-item';
        li.setAttribute('role', 'presentation');
        li.innerHTML = `<button class="nav-link" id="tab-struct-tab"
            data-bs-toggle="tab" data-bs-target="#tab-struct"
            type="button" role="tab">結構</button>`;
        tabList.appendChild(li);

        // 加頁籤內容
        const pane = document.createElement('div');
        pane.className = 'tab-pane fade';
        pane.id = 'tab-struct';
        pane.setAttribute('role', 'tabpanel');
        pane.innerHTML = '<div id="structTreeContainer" class="p-1" style="font-size:0.78rem;"><div class="text-muted">切換到此頁籤後自動載入...</div></div>';
        tabContent.appendChild(pane);

        // 監聽切換到「結構」頁籤
        li.querySelector('button').addEventListener('shown.bs.tab', async () => {
            const paperNum = getPaperNum();
            if (!paperNum) return;
            await loadStructTree(paperNum);
        });
    }

    let _structTreeLoaded = '';
    async function loadStructTree(paperNum) {
        if (_structTreeLoaded === paperNum) return;
        const container = document.getElementById('structTreeContainer');
        if (!container) return;

        container.innerHTML = '<div class="text-muted p-1">載入中...</div>';

        try {
            const resp = await fetch(`${API_BASE}/StructTree?paperNum=${encodeURIComponent(paperNum)}`);
            const result = await resp.json();

            if (!result.ok || !result.data || result.data.length === 0) {
                container.innerHTML = '<div class="text-muted p-1">無結構資料</div>';
                _structTreeLoaded = paperNum;
                return;
            }

            const quotaPartNum = getFieldValue('QuotaPartNum') || '';
            const quotaRevision = getFieldValue('QuotaRevision') || '';
            const title = `報價料號=${quotaPartNum}${quotaRevision}`;

            container.innerHTML = `<div class="fw-bold mb-1" style="font-size:0.8rem;">${title}</div>`
                + buildTreeHtml(result.data);
            // 綁定收合展開事件
            container.querySelectorAll('.st-toggle').forEach(el => {
                el.addEventListener('click', () => {
                    const children = el.nextElementSibling;
                    if (!children) return;
                    const expanded = el.dataset.expanded === 'true';
                    el.dataset.expanded = expanded ? 'false' : 'true';
                    el.querySelector('.st-icon').textContent = expanded ? '▶' : '▼';
                    children.style.display = expanded ? 'none' : '';
                });
            });
            _structTreeLoaded = paperNum;
        } catch (err) {
            console.error('[SQ000006] 載入結構樹失敗:', err);
            container.innerHTML = `<div class="text-danger p-1">載入失敗: ${err.message}</div>`;
        }
    }

    function buildTreeHtml(data) {
        // 依 SuperId 建立巢狀結構（對應 Delphi TJSdTreeView）
        const childMap = {};
        for (const d of data) {
            const pid = d.SuperId ?? 0;
            if (!childMap[pid]) childMap[pid] = [];
            childMap[pid].push(d);
        }

        const visited = new Set();
        function renderNode(node, level) {
            if (visited.has(node.Item) || level > 50) return '';
            visited.add(node.Item);
            const children = childMap[node.Item] || [];
            const hasChildren = children.length > 0;
            const caption = (node.Caption || node.Notes || '').trim();
            const indent = level * 16;
            let html = '';

            if (hasChildren) {
                html += `<div class="st-node st-toggle" style="padding-left:${indent}px;" data-expanded="true">
                    <span class="st-icon">▼</span> ${caption}
                </div>`;
                html += `<div class="st-children">`;
                children.forEach(child => { html += renderNode(child, level + 1); });
                html += `</div>`;
            } else {
                html += `<div class="st-node st-leaf" style="padding-left:${indent + 16}px;">${caption}</div>`;
            }
            return html;
        }

        const roots = data.filter(d => !d.SuperId || d.SuperId === 0);
        let html = '<div class="struct-tree">';
        const nodes = roots.length > 0 ? roots : data;
        nodes.forEach(root => { html += renderNode(root, 0); });
        html += '</div>';
        return html;
    }

    // ==========================================
    // 7. 明細格雙擊搜尋 (DtlNumId)
    // Delphi: QuotaPaper.pas gridDetail1DblClick
    // 使用 _MultiTabDetail 的 cell-view / cell-edit 結構
    //
    // 注意：NumId 不在 Grid 可見欄位中，需透過 API 查詢完整資料取得
    // ==========================================

    // 快取 NumName → NumId 對應（從專用 API 取得）
    let _numIdMap = null;

    function bindDetailDblClick() {
        document.addEventListener('dblclick', handleDetailDblClick, true);
        cacheDetailNumIds();
        document.addEventListener('shown.bs.tab', () => {
            setTimeout(cacheDetailNumIds, 500);
        });
        console.log('[SQ000006] 雙擊事件已綁定');
    }

    async function cacheDetailNumIds() {
        const paperNum = getPaperNum();
        if (!paperNum) return;
        try {
            const resp = await fetch(`${API_BASE}/DetailNumIds?paperNum=${encodeURIComponent(paperNum)}`);
            if (!resp.ok) { console.error('[SQ000006] DetailNumIds 錯誤:', resp.status); return; }
            const result = await resp.json();
            if (result.ok && result.data) {
                _numIdMap = {};
                for (const row of result.data) {
                    const numName = String(row.NumName ?? '').trim();
                    const numId = String(row.NumId ?? '').trim();
                    if (numName) _numIdMap[numName] = numId;
                }
                console.log('[SQ000006] NumName→NumId 對應已快取, 共', Object.keys(_numIdMap).length, '筆');
            }
        } catch (err) {
            console.error('[SQ000006] 快取 NumId 對應失敗:', err);
        }
    }

    function findNumIdByNumName(numName) {
        if (!_numIdMap || !numName) return '';
        return _numIdMap[numName.trim()] || '';
    }

    async function handleDetailDblClick(e) {
        // 確保是在 multi-tab-detail 的 erp-table 內
        const td = e.target.closest('td');
        if (!td) return;
        if (!td.closest('.multi-tab-detail .erp-table')) return;

        // 取得欄位名稱
        const fieldName = (
            td.dataset.field ||
            td.dataset.lookupField ||
            td.querySelector('input.cell-edit[data-field]')?.dataset?.field ||
            ''
        ).trim().toLowerCase();

        // 只攔截 DtlNumId 欄位（選擇資料）
        if (fieldName !== 'dtlnumid') return;

        // 阻止標準 lookup dropdown 觸發
        e.stopImmediatePropagation();
        e.stopPropagation();
        e.preventDefault();

        const tr = td.closest('tr');
        if (!tr) return;

        const paperNum = getPaperNum();
        if (!paperNum) return;

        // 工具：從同行取得某欄位的 cell-edit input 值
        const getRowField = (name) => {
            const inp = Array.from(tr.querySelectorAll('input.cell-edit[data-field]'))
                .find(i => (i.dataset.field || '').toLowerCase() === name.toLowerCase());
            return (inp?.value ?? '').trim();
        };

        // 取得 NumId — 用 NumName（屬性名稱）查對應
        let numId = getRowField('NumId'); // 方式1: Grid 中有 NumId 欄位

        // 方式2: 從同行的 NumName 欄位反查快取
        if (!numId) {
            const numName = getRowField('NumName');
            console.log('[SQ000006] 此列 NumName=', numName);
            if (numName) {
                numId = findNumIdByNumName(numName);
            }
            // 快取不存在則重新取得
            if (!numId && !_numIdMap) {
                await cacheDetailNumIds();
                if (numName) numId = findNumIdByNumName(numName);
            }
        }

        if (!numId) {
            console.warn('[SQ000006] 無法取得 NumId, NumName=', getRowField('NumName'));
            await Swal.fire({ icon: 'warning', title: '無法取得 NumId', text: '請確認此列資料是否正確' });
            return;
        }

        console.log('[SQ000006] 開啟搜尋對話框, PaperNum=', paperNum, 'NumId=', numId);

        try {
            const resp = await fetch(
                `${API_BASE}/ItemListSub?paperNum=${encodeURIComponent(paperNum)}&numId=${encodeURIComponent(numId)}`);
            const result = await resp.json();

            if (!result.ok || !result.data || result.data.length === 0) {
                console.log('[SQ000006] DtlNumId 無可選項目, numId=', numId);
                return;
            }

            const data = result.data;
            const cols = Object.keys(data[0]);
            const currentVal = getRowField('DtlNumId');

            // 用 Bootstrap Modal（與單頭 lookup 相同風格）
            const selectedKey = await showItemListModal(data, cols, currentVal);
            if (!selectedKey) return;

            // 更新 cell-edit input
            const editInput = td.querySelector('input.cell-edit[data-field]');
            if (editInput) {
                editInput.value = selectedKey;
                editInput._fromLookup = true;
                editInput.dispatchEvent(new Event('input', { bubbles: true }));
                editInput.dispatchEvent(new Event('change', { bubbles: true }));
            }

            // 更新 cell-view span
            const viewSpan = td.querySelector('.cell-view');
            if (viewSpan) {
                viewSpan.textContent = selectedKey;
                viewSpan.dataset.initView = selectedKey;
                if (viewSpan.dataset.raw !== undefined) viewSpan.dataset.raw = selectedKey;
            }

            // 標記列為已修改
            if (tr.dataset.state !== 'added') tr.dataset.state = 'modified';
            window.__unsavedChanges = true;

            // 同步更新 DtlNumName
            const selectedRow = data.find(d => (d[cols[0]] ?? '').toString() === selectedKey);
            if (selectedRow && cols.length > 1) {
                const nameValue = (selectedRow[cols[1]] ?? '').toString();
                const nameInput = Array.from(tr.querySelectorAll('input.cell-edit[data-field]'))
                    .find(inp => (inp.dataset.field || '').toLowerCase() === 'dtlnumname');
                if (nameInput) {
                    nameInput.value = nameValue;
                    nameInput.dispatchEvent(new Event('change', { bubbles: true }));
                }
                const nameView = nameInput?.closest('td')?.querySelector('.cell-view');
                if (nameView) nameView.textContent = nameValue;
            }

            console.log('[SQ000006] DtlNumId 已更新:', selectedKey);

        } catch (err) {
            console.error('[SQ000006] ItemListSub 搜尋失敗:', err);
            await Swal.fire({ icon: 'error', title: '搜尋失敗', text: err.message });
        }
    }

    /**
     * Bootstrap Modal 版搜尋對話框（與單頭 lookup 相同風格）
     * 回傳 Promise<string|null>，選取時回傳 key，取消回傳 null
     */
    function showItemListModal(data, cols, currentVal) {
        return new Promise(resolve => {
            // 確保 modal DOM 存在（只建一次）
            let modal = document.getElementById('sqItemListModal');
            if (!modal) {
                modal = document.createElement('div');
                modal.id = 'sqItemListModal';
                modal.className = 'modal fade';
                modal.tabIndex = -1;
                modal.setAttribute('aria-hidden', 'true');
                modal.dataset.bsBackdrop = 'false';
                modal.style.pointerEvents = 'none';
                modal.innerHTML = `
                <div class="modal-dialog modal-dialog-scrollable" style="max-width:400px; pointer-events:all;">
                  <div class="modal-content" style="font-size:0.78rem;">
                    <div class="modal-header py-1 px-2">
                      <span class="modal-title fw-bold" style="font-size:0.8rem;">名稱查詢</span>
                      <button type="button" class="btn-close btn-close-sm" data-bs-dismiss="modal" style="font-size:0.6rem;"></button>
                    </div>
                    <div class="modal-body p-1">
                      <div class="d-flex gap-1 mb-1">
                        <input type="text" class="form-control form-control-sm" id="sqItemFilterCode" placeholder="代碼篩選" style="font-size:0.75rem;">
                        <input type="text" class="form-control form-control-sm" id="sqItemFilterName" placeholder="名稱篩選" style="font-size:0.75rem;">
                      </div>
                      <div style="max-height:260px; overflow-y:auto">
                        <table class="table table-sm table-hover mb-0" id="sqItemListTable" style="font-size:0.75rem;">
                          <thead class="table-light sticky-top">
                            <tr id="sqItemListHead"></tr>
                          </thead>
                          <tbody id="sqItemListBody"></tbody>
                        </table>
                      </div>
                    </div>
                    <div class="modal-footer py-1 px-2">
                      <button type="button" class="btn btn-primary btn-sm py-0 px-2" id="sqItemListConfirm" style="font-size:0.75rem;">確定</button>
                      <button type="button" class="btn btn-secondary btn-sm py-0 px-2" data-bs-dismiss="modal" style="font-size:0.75rem;">取消</button>
                    </div>
                  </div>
                </div>`;
                document.body.appendChild(modal);
            }

            const thead = document.getElementById('sqItemListHead');
            const tbody = document.getElementById('sqItemListBody');
            const filterCode = document.getElementById('sqItemFilterCode');
            const filterName = document.getElementById('sqItemFilterName');
            let selectedKey = null;

            // 渲染表頭
            thead.innerHTML = cols.map(c =>
                `<th style="width:${cols.length === 2 ? '35%' : 'auto'}; padding:2px 4px;">${c}</th>`
            ).join('');

            // 渲染列
            const renderRows = () => {
                const fc = (filterCode.value || '').trim().toLowerCase();
                const fn = (filterName.value || '').trim().toLowerCase();
                let html = '';
                for (const row of data) {
                    const vals = cols.map(c => (row[c] ?? '').toString());
                    const code = vals[0] || '';
                    const name = vals.length > 1 ? vals[1] : '';
                    if (fc && !code.toLowerCase().includes(fc)) continue;
                    if (fn && !name.toLowerCase().includes(fn)) continue;
                    const isSelected = code === currentVal;
                    html += `<tr data-key="${code.replace(/"/g, '&quot;')}" class="${isSelected ? 'table-primary' : ''}" style="cursor:pointer;">`;
                    for (const v of vals) {
                        html += `<td style="padding:2px 4px;">${v}</td>`;
                    }
                    html += '</tr>';
                    if (isSelected) selectedKey = code;
                }
                tbody.innerHTML = html;
            };
            renderRows();

            // 篩選
            filterCode.value = '';
            filterName.value = '';
            filterCode.oninput = renderRows;
            filterName.oninput = renderRows;

            // 點擊選取
            tbody.onclick = (ev) => {
                const clickTr = ev.target.closest('tr');
                if (!clickTr) return;
                tbody.querySelectorAll('tr.table-primary').forEach(r => r.classList.remove('table-primary'));
                clickTr.classList.add('table-primary');
                selectedKey = clickTr.dataset.key;
            };

            // 雙擊直接確定
            tbody.ondblclick = (ev) => {
                const clickTr = ev.target.closest('tr');
                if (!clickTr) return;
                selectedKey = clickTr.dataset.key;
                bsModal.hide();
                resolve(selectedKey);
            };

            // 確定按鈕
            const confirmBtn = document.getElementById('sqItemListConfirm');
            confirmBtn.onclick = () => {
                if (!selectedKey) return;
                bsModal.hide();
                resolve(selectedKey);
            };

            // 取消/關閉
            const onHidden = () => {
                modal.removeEventListener('hidden.bs.modal', onHidden);
                resolve(null);
            };
            modal.addEventListener('hidden.bs.modal', onHidden);

            // 開啟
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
            setTimeout(() => filterCode.focus(), 200);
        });
    }

    // ==========================================
    // 8. 注入自訂按鈕到 Toolbar
    // ==========================================
    function injectToolbarButtons() {
        const toolbar = document.querySelector('.title-row > div') || document.getElementById('topToolbar');
        if (!toolbar) return;

        // 搜尋料號按鈕
        const btnLoadData = document.createElement('button');
        btnLoadData.type = 'button';
        btnLoadData.id = 'btnSQULoadData';
        btnLoadData.className = 'btn toolbar-btn';
        btnLoadData.style.cssText = 'color:#0d6efd; border-color:#86b7fe;';
        btnLoadData.innerHTML = '<i class="bi bi-search"></i> 搜尋料號';
        btnLoadData.title = '搜尋料號條件設定';
        btnLoadData.addEventListener('click', showPartNumSearchDialog);

        // 搜尋取代按鈕
        const btnSearchEx = document.createElement('button');
        btnSearchEx.type = 'button';
        btnSearchEx.id = 'btnSQUSearchEx';
        btnSearchEx.className = 'btn toolbar-btn';
        btnSearchEx.style.cssText = 'color:#198754; border-color:#86efac;';
        btnSearchEx.innerHTML = '<i class="bi bi-arrow-repeat"></i> 搜尋取代';
        btnSearchEx.title = '搜尋取代規格明細';
        btnSearchEx.addEventListener('click', showSearchExDialog);

        // 暫時隱藏，功能尚未完成
        // toolbar.appendChild(btnLoadData);
        // toolbar.appendChild(btnSearchEx);
    }

    // ==========================================
    // 9. 樣式
    // ==========================================
    function injectStyles() {
        const style = document.createElement('style');
        style.textContent = `
            .struct-tree { font-size: 0.78rem; line-height: 1.6; }
            .st-node { padding: 1px 4px; white-space: nowrap; }
            .st-toggle { cursor: pointer; user-select: none; }
            .st-toggle:hover { background: #f0f4ff; }
            .st-icon { display: inline-block; width: 14px; font-size: 0.6rem; color: #666; text-align: center; }
            .st-leaf { color: #333; }
            .st-children { }
        `;
        document.head.appendChild(style);
    }

    // ==========================================
    // 初始化
    // ==========================================
    function init() {
        injectStyles();

        // 判斷是列表頁還是明細頁
        const isDetailPage = window.location.pathname.split('/').length > 4;

        if (isDetailPage) {
            injectToolbarButtons();
            bindCompanyIdChange();
            bindComputeIdChange();
            bindDetailDblClick();
            initStructTab();
            initPOPPanel();
        }
    }

    // DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    console.log('[SQ000006] paper JS 已載入');
})();
