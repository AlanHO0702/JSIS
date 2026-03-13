/**
 * EMOdTmpRoute 途程主檔 - 主要 JavaScript
 * 功能: Master Grid (途程主檔) + Detail Grid (途程明細) + 備註欄
 *       + 途程設定 Dialog (Multi-Select) + 查詢
 */
(function () {
    'use strict';

    // ==================== State ====================
    let masterRows = [];
    let selectedMasterRow = null;
    let selectedMasterIdx = -1;
    let detailRows = [];
    let selectedDetailIdx = -1;
    let isEditMode = false;
    let sysParams = {};
    let masterDict = [];
    let detailDict = [];
    let queryParams = {};
    let currentPage = 1;
    let pageSize = 200;
    let totalCount = 0;

    // 途程設定 Dialog state
    let msSourceItems = [];   // 可用製程 { ProcCode, ProcName }
    let msTargetItems = [];   // 已選製程 { ProcCode, ProcName }
    let msSelectedSourceIdx = -1;
    let msSelectedTargetIdx = -1;

    // ==================== API Helpers ====================
    async function apiFetch(url, options) {
        const jwtId = localStorage.getItem('jwtId') || '';
        const headers = { 'Content-Type': 'application/json' };
        if (jwtId) headers['X-JWTID'] = jwtId;
        const resp = await fetch(url, { ...options, headers: { ...headers, ...(options?.headers || {}) } });
        if (!resp.ok) {
            let msg = `API Error: ${resp.status}`;
            try { const body = await resp.json(); msg = body.error || body.title || body.message || msg; } catch (_) { }
            throw new Error(msg);
        }
        return resp.json();
    }
    async function apiGet(url) { return apiFetch(url); }
    async function apiPost(url, body) { return apiFetch(url, { method: 'POST', body: JSON.stringify(body) }); }

    // ==================== System Parameters ====================
    async function loadSysParams() {
        try {
            const [activeType] = await Promise.all([
                apiGet('/api/EMOdTmpRoute/SysParam?systemId=EMO&paramId=TmpRouteActiveType'),
            ]);
            sysParams.activeType = activeType.value;
        } catch (e) {
            console.warn('Failed to load sys params:', e);
        }
    }

    // ==================== Field Dictionary ====================
    async function loadFieldDicts() {
        try {
            const [masDict, dtlDict] = await Promise.all([
                apiGet('/api/EMOdTmpRoute/DictFields?table=EMOdTmpRouteMas'),
                apiGet('/api/EMOdTmpRoute/DictFields?table=EMOdTmpRouteDtl'),
            ]);
            masterDict = (masDict || []).filter(d => d.Visible);
            detailDict = (dtlDict || []).filter(d => d.Visible);
        } catch (e) {
            console.warn('Failed to load field dicts:', e);
            masterDict = [];
            detailDict = [];
        }
    }

    function getVisibleFields(dict, fallbackKeys) {
        if (dict && dict.length > 0) {
            return dict.map(d => ({
                field: d.FieldName,
                label: d.DisplayLabel || d.FieldName,
                isCheckbox: Number(d.ComboStyle ?? 0) === 1,
                width: d.FieldWidth || d.DisplaySize || null
            }));
        }
        return fallbackKeys.map(k => ({ field: k, label: k, isCheckbox: false, width: null }));
    }

    function calcColWidth(col) {
        if (col.isCheckbox) return 60;
        if (col.width) return Math.max(col.width * 8 + 20, 50);
        return 120;
    }

    // ==================== Master Grid ====================
    function buildMasterHead() {
        const tr = document.getElementById('masterHead');
        tr.innerHTML = '';
        const cols = getVisibleFields(masterDict, ['TmpId', 'Notes', 'StatusName']);
        let totalW = 0;
        cols.forEach(col => {
            const w = calcColWidth(col);
            totalW += w;
            const th = document.createElement('th');
            th.style.width = w + 'px';
            if (col.isCheckbox) th.style.textAlign = 'center';
            th.textContent = col.label;
            const resizer = document.createElement('div');
            resizer.className = 'th-resize';
            th.appendChild(resizer);
            tr.appendChild(th);
        });
        const table = document.getElementById('masterGrid');
        if (table) table.style.width = totalW + 'px';
    }

    function renderMasterBody() {
        const tbody = document.getElementById('masterBody');
        tbody.innerHTML = '';
        const cols = getVisibleFields(masterDict, ['TmpId', 'Notes', 'StatusName']);
        masterRows.forEach((row, idx) => {
            const tr = document.createElement('tr');
            if (idx === selectedMasterIdx) tr.classList.add('selected');
            cols.forEach(col => {
                const td = document.createElement('td');
                td.style.width = calcColWidth(col) + 'px';
                const val = row[col.field] ?? '';
                td.title = String(val);
                if (col.isCheckbox) {
                    td.style.textAlign = 'center';
                    const chk = document.createElement('input');
                    chk.type = 'checkbox';
                    chk.className = 'form-check-input';
                    chk.disabled = true;
                    chk.checked = val === true || val === 1 || val === '1' || val === 'Y';
                    td.appendChild(chk);
                } else {
                    td.textContent = String(val);
                }
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => selectMasterRow(idx));
            tr.addEventListener('dblclick', () => {
                selectMasterRow(idx);
                if (isEditMode) startInlineEdit(tr, row, idx);
            });
            tbody.appendChild(tr);
        });
        const countText = `${masterRows.length > 0 ? (selectedMasterIdx + 1) : 0} / ${masterRows.length}`;
        document.getElementById('masterRecordCount').textContent = countText;
        const statusCount = document.getElementById('modeStatusCount');
        if (statusCount) statusCount.textContent = countText;
    }

    async function selectMasterRow(idx) {
        selectedMasterIdx = idx;
        selectedMasterRow = masterRows[idx] || null;
        selectedDetailIdx = -1;

        // 清空備註
        const txtNote = document.getElementById('txtDtlNote');
        txtNote.value = '';
        txtNote.readOnly = true;

        renderMasterBody();

        const label = document.getElementById('detailTmpIdLabel');
        label.textContent = selectedMasterRow ? selectedMasterRow.TmpId || '' : '';

        if (selectedMasterRow) {
            await loadDetail(selectedMasterRow.TmpId);
        } else {
            detailRows = [];
            renderDetailBody();
        }
    }

    // ==================== Detail Grid ====================
    function buildDetailHead() {
        const tr = document.getElementById('detailHead');
        tr.innerHTML = '';
        // 固定顯示 SerialNum, ProcCode, ProcName, FinishRate + dict 中的其他欄位
        const fallback = ['SerialNum', 'ProcCode', 'ProcName', 'FinishRate', 'Notes'];
        const cols = getVisibleFields(detailDict.length > 0 ? detailDict : [], fallback);
        // 確保 ProcName 一定存在（join 欄位）
        const hasProcName = cols.some(c => c.field === 'ProcName');
        if (!hasProcName) {
            const afterProcCode = cols.findIndex(c => c.field === 'ProcCode');
            cols.splice(afterProcCode + 1, 0, { field: 'ProcName', label: '製程名稱', isCheckbox: false, width: 15 });
        }
        let totalW = 0;
        cols.forEach(col => {
            const w = calcColWidth(col);
            totalW += w;
            const th = document.createElement('th');
            th.style.width = w + 'px';
            if (col.isCheckbox) th.style.textAlign = 'center';
            th.textContent = col.label;
            const resizer = document.createElement('div');
            resizer.className = 'th-resize';
            th.appendChild(resizer);
            tr.appendChild(th);
        });
        const table = document.getElementById('detailGrid');
        if (table) table.style.width = totalW + 'px';
    }

    function getDtlCols() {
        const fallback = ['SerialNum', 'ProcCode', 'ProcName', 'FinishRate', 'Notes'];
        const cols = getVisibleFields(detailDict.length > 0 ? detailDict : [], fallback);
        const hasProcName = cols.some(c => c.field === 'ProcName');
        if (!hasProcName) {
            const afterProcCode = cols.findIndex(c => c.field === 'ProcCode');
            cols.splice(afterProcCode + 1, 0, { field: 'ProcName', label: '製程名稱', isCheckbox: false, width: 15 });
        }
        return cols;
    }

    function renderDetailBody() {
        const tbody = document.getElementById('detailBody');
        tbody.innerHTML = '';
        const cols = getDtlCols();
        detailRows.forEach((row, idx) => {
            const tr = document.createElement('tr');
            if (idx === selectedDetailIdx) tr.classList.add('selected');
            cols.forEach(col => {
                const td = document.createElement('td');
                td.style.width = calcColWidth(col) + 'px';
                const val = row[col.field] ?? '';
                td.title = String(val);
                if (col.isCheckbox) {
                    td.style.textAlign = 'center';
                    const chk = document.createElement('input');
                    chk.type = 'checkbox';
                    chk.className = 'form-check-input';
                    chk.disabled = true;
                    chk.checked = val === true || val === 1 || val === '1' || val === 'Y';
                    td.appendChild(chk);
                } else if (col.field === 'Notes') {
                    td.textContent = String(val).length > 20 ? String(val).substring(0, 20) + '…' : String(val);
                } else {
                    td.textContent = String(val);
                }
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => selectDetailRow(idx));
            tbody.appendChild(tr);
        });
        document.getElementById('detailRecordCount').textContent =
            `${detailRows.length > 0 ? (selectedDetailIdx + 1) : 0} / ${detailRows.length}`;
    }

    function selectDetailRow(idx) {
        selectedDetailIdx = idx;
        renderDetailBody();

        const row = detailRows[idx];
        const txtNote = document.getElementById('txtDtlNote');
        if (row) {
            txtNote.value = row.Notes ?? '';
            txtNote.readOnly = !isEditMode || (selectedMasterRow?.Status === 1);
        } else {
            txtNote.value = '';
            txtNote.readOnly = true;
        }
    }

    async function loadDetail(tmpId) {
        try {
            const res = await apiGet(`/api/EMOdTmpRoute/GetRouteDtl?tmpId=${encodeURIComponent(tmpId)}`);
            detailRows = res.data || [];
            selectedDetailIdx = -1;
            buildDetailHead();
            renderDetailBody();
        } catch (e) {
            console.error('loadDetail error:', e);
            detailRows = [];
            renderDetailBody();
        }
    }

    // ==================== Load Master ====================
    async function loadMaster(resetPage = true) {
        if (resetPage) currentPage = 1;

        const params = new URLSearchParams({ page: currentPage, pageSize });
        if (queryParams.TmpId) params.set('TmpId', queryParams.TmpId);
        if (queryParams.Notes) params.set('Notes', queryParams.Notes);
        if (queryParams.Status !== undefined && queryParams.Status !== '') params.set('Status', queryParams.Status);

        try {
            const res = await apiGet(`/api/EMOdTmpRouteMas/paged?${params}`);
            masterRows = res.data || [];
            totalCount = res.totalCount || 0;
            selectedMasterIdx = -1;
            selectedMasterRow = null;
            detailRows = [];
            selectedDetailIdx = -1;
            buildMasterHead();
            renderMasterBody();
            buildDetailHead();
            renderDetailBody();
            document.getElementById('detailTmpIdLabel').textContent = '';
            document.getElementById('txtDtlNote').value = '';
            document.getElementById('txtDtlNote').readOnly = true;
            document.getElementById('recordCountMas').textContent = `共 ${totalCount} 筆`;
        } catch (e) {
            alert('載入資料失敗: ' + e.message);
        }
    }

    // ==================== Edit Mode ====================
    function setEditMode(editOn) {
        isEditMode = editOn;
        const box = document.getElementById('modeStatusBox');
        const label = document.getElementById('modeStatusLabel');
        const btn = document.getElementById('btnModeToggle');
        const btnAdd = document.getElementById('btnAddRow');
        const btnDel = document.getElementById('btnDelRow');

        if (editOn) {
            if (box) box.classList.add('edit-mode');
            if (label) label.textContent = '編輯模式';
            if (btn) { btn.textContent = '瀏覽'; btn.title = '切換至瀏覽模式'; }
            if (btnAdd) btnAdd.disabled = false;
            if (btnDel) btnDel.disabled = false;
        } else {
            if (box) box.classList.remove('edit-mode');
            if (label) label.textContent = '瀏覽模式';
            if (btn) { btn.textContent = '修改'; btn.title = '切換至編輯模式'; }
            if (btnAdd) btnAdd.disabled = true;
            if (btnDel) btnDel.disabled = true;
        }

        // 備註區塊 readonly 狀態
        const txtNote = document.getElementById('txtDtlNote');
        if (!editOn || !selectedMasterRow || selectedMasterRow.Status === 1) {
            txtNote.readOnly = true;
        } else if (selectedDetailIdx >= 0) {
            txtNote.readOnly = false;
        }
    }

    // ==================== Inline Edit (Master Notes) ====================
    function startInlineEdit(tr, row, idx) {
        if (row.Status === 1) {
            alert('此途程範本已使用中，不可修改!');
            return;
        }
        const cols = getVisibleFields(masterDict, ['TmpId', 'Notes', 'StatusName']);
        const notesColIdx = cols.findIndex(c => c.field === 'Notes');
        if (notesColIdx < 0) return;
        const td = tr.cells[notesColIdx];
        const orig = td.textContent;
        td.innerHTML = '';
        const inp = document.createElement('input');
        inp.type = 'text';
        inp.value = orig;
        inp.style.cssText = 'width:100%;box-sizing:border-box;font-size:13px;padding:1px 4px;';
        td.appendChild(inp);
        inp.focus();
        inp.select();
        inp.addEventListener('blur', async () => {
            const newVal = inp.value;
            td.textContent = newVal;
            if (newVal !== orig) {
                try {
                    const res = await apiPost('/api/EMOdTmpRoute/UpdateMasterField', {
                        TmpId: row.TmpId, FieldName: 'Notes', Value: newVal
                    });
                    if (!res.ok) { alert(res.error || '更新失敗'); td.textContent = orig; }
                    else { masterRows[idx].Notes = newVal; }
                } catch (e) {
                    alert('更新失敗: ' + e.message);
                    td.textContent = orig;
                }
            }
        });
        inp.addEventListener('keydown', e => {
            if (e.key === 'Enter') inp.blur();
            if (e.key === 'Escape') { td.textContent = orig; }
        });
    }

    // ==================== 途程設定 Dialog (Multi-Select) ====================
    let routeSetModal = null;

    function renderMsSource() {
        const tbody = document.getElementById('msSourceList');
        tbody.innerHTML = '';
        msSourceItems.forEach((item, idx) => {
            const tr = document.createElement('tr');
            if (idx === msSelectedSourceIdx) tr.classList.add('selected');
            const td1 = document.createElement('td'); td1.textContent = item.ProcCode;
            const td2 = document.createElement('td'); td2.textContent = item.ProcName;
            tr.appendChild(td1); tr.appendChild(td2);
            tr.addEventListener('click', () => { msSelectedSourceIdx = idx; msSelectedTargetIdx = -1; renderMsSource(); renderMsTarget(); });
            tr.addEventListener('dblclick', () => { msSelectedSourceIdx = idx; msMoveToTarget(); });
            tbody.appendChild(tr);
        });
    }

    function renderMsTarget() {
        const tbody = document.getElementById('msTargetList');
        tbody.innerHTML = '';
        msTargetItems.forEach((item, idx) => {
            const tr = document.createElement('tr');
            if (idx === msSelectedTargetIdx) tr.classList.add('selected');
            const tdNum = document.createElement('td'); tdNum.textContent = idx + 1;
            const td1 = document.createElement('td'); td1.textContent = item.ProcCode;
            const td2 = document.createElement('td'); td2.textContent = item.ProcName;
            tr.appendChild(tdNum); tr.appendChild(td1); tr.appendChild(td2);
            tr.addEventListener('click', () => { msSelectedTargetIdx = idx; msSelectedSourceIdx = -1; renderMsSource(); renderMsTarget(); });
            tr.addEventListener('dblclick', () => { msSelectedTargetIdx = idx; msMoveToSource(); });
            tbody.appendChild(tr);
        });
    }

    function msMoveToTarget() {
        if (msSelectedSourceIdx < 0 || msSelectedSourceIdx >= msSourceItems.length) return;
        const item = msSourceItems.splice(msSelectedSourceIdx, 1)[0];
        msTargetItems.push(item);
        msSelectedSourceIdx = Math.min(msSelectedSourceIdx, msSourceItems.length - 1);
        renderMsSource(); renderMsTarget();
    }

    function msMoveToSource() {
        if (msSelectedTargetIdx < 0 || msSelectedTargetIdx >= msTargetItems.length) return;
        const item = msTargetItems.splice(msSelectedTargetIdx, 1)[0];
        msSourceItems.push(item);
        msSourceItems.sort((a, b) => a.ProcCode.localeCompare(b.ProcCode));
        msSelectedTargetIdx = Math.min(msSelectedTargetIdx, msTargetItems.length - 1);
        renderMsSource(); renderMsTarget();
    }

    function msMoveAllToTarget() {
        msTargetItems = [...msTargetItems, ...msSourceItems];
        msSourceItems = [];
        msSelectedSourceIdx = -1;
        renderMsSource(); renderMsTarget();
    }

    function msMoveAllToSource() {
        msSourceItems = [...msSourceItems, ...msTargetItems];
        msSourceItems.sort((a, b) => a.ProcCode.localeCompare(b.ProcCode));
        msTargetItems = [];
        msSelectedTargetIdx = -1;
        renderMsSource(); renderMsTarget();
    }

    function msMoveUp() {
        if (msSelectedTargetIdx <= 0) return;
        const tmp = msTargetItems[msSelectedTargetIdx];
        msTargetItems[msSelectedTargetIdx] = msTargetItems[msSelectedTargetIdx - 1];
        msTargetItems[msSelectedTargetIdx - 1] = tmp;
        msSelectedTargetIdx--;
        renderMsTarget();
    }

    function msMoveDown() {
        if (msSelectedTargetIdx < 0 || msSelectedTargetIdx >= msTargetItems.length - 1) return;
        const tmp = msTargetItems[msSelectedTargetIdx];
        msTargetItems[msSelectedTargetIdx] = msTargetItems[msSelectedTargetIdx + 1];
        msTargetItems[msSelectedTargetIdx + 1] = tmp;
        msSelectedTargetIdx++;
        renderMsTarget();
    }

    async function openRouteSetModal() {
        if (!selectedMasterRow) { alert('請先選取一筆途程範本!'); return; }
        if (selectedMasterRow.Status === 1) {
            const activeLabel = sysParams.activeType === '1' ? '已發行' : '已使用';
            alert(`此途程範本${activeLabel}，不可修改!`);
            return;
        }

        // 載入可用製程
        try {
            const res = await apiGet('/api/EMOdTmpRoute/GetProcList');
            const allProcs = res.data || [];

            // 已選製程 (現有明細)
            const existCodes = new Set(detailRows.map(r => r.ProcCode));
            msTargetItems = detailRows.map(r => ({ ProcCode: r.ProcCode, ProcName: r.ProcName || '' }));
            msSourceItems = allProcs.filter(p => !existCodes.has(p.ProcCode)).map(p => ({ ProcCode: p.ProcCode, ProcName: p.ProcName || '' }));
        } catch (e) {
            alert('載入製程資料失敗: ' + e.message);
            return;
        }

        msSelectedSourceIdx = -1;
        msSelectedTargetIdx = -1;
        renderMsSource();
        renderMsTarget();

        if (!routeSetModal) {
            routeSetModal = new bootstrap.Modal(document.getElementById('routeSetModal'));
        }
        routeSetModal.show();
    }

    async function searchProcInModal() {
        const bProc = document.getElementById('rsEdtBProc').value.trim();
        const eProc = document.getElementById('rsEdtEProc').value.trim();
        const procLike = document.getElementById('rsEdtProcLike').value.trim();
        const procNameLike = document.getElementById('rsEdtProcNameLike').value.trim();

        const params = new URLSearchParams();
        if (bProc) params.set('bProc', bProc);
        if (eProc) params.set('eProc', eProc);
        if (procLike) params.set('procLike', procLike);
        if (procNameLike) params.set('procNameLike', procNameLike);

        try {
            const res = await apiGet(`/api/EMOdTmpRoute/GetProcList?${params}`);
            const allProcs = res.data || [];
            const targetCodes = new Set(msTargetItems.map(t => t.ProcCode));
            msSourceItems = allProcs.filter(p => !targetCodes.has(p.ProcCode)).map(p => ({ ProcCode: p.ProcCode, ProcName: p.ProcName || '' }));
            msSelectedSourceIdx = -1;
            renderMsSource();
        } catch (e) {
            alert('查詢製程失敗: ' + e.message);
        }
    }

    async function confirmRouteSet() {
        if (!selectedMasterRow) return;

        const procCodes = msTargetItems.map(t => t.ProcCode);

        try {
            const res = await apiPost('/api/EMOdTmpRoute/ChangeRoute', {
                TmpId: selectedMasterRow.TmpId,
                ProcCodes: procCodes
            });
            if (res.ok) {
                routeSetModal.hide();
                await loadDetail(selectedMasterRow.TmpId);
            } else {
                alert(res.error || '儲存失敗');
            }
        } catch (e) {
            alert('儲存失敗: ' + e.message);
        }
    }

    // ==================== 另存 ====================
    async function doSaveAs() {
        const newTmpId = document.getElementById('edtTmpIdNew').value.trim();
        if (!newTmpId) { alert('請輸入新代碼!'); return; }
        if (!selectedMasterRow) { alert('請先選取一筆參考範本!'); return; }

        const procCodes = detailRows.map(r => r.ProcCode);

        try {
            const res = await apiPost('/api/EMOdTmpRoute/SaveAs', {
                SourceTmpId: selectedMasterRow.TmpId,
                NewTmpId: newTmpId,
                ProcCodes: procCodes
            });
            if (res.ok) {
                document.getElementById('edtTmpIdNew').value = '';
                await loadMaster(true);
                // 定位到新建立的資料
                const newIdx = masterRows.findIndex(r => r.TmpId === newTmpId);
                if (newIdx >= 0) await selectMasterRow(newIdx);
            } else {
                alert(res.error || '另存失敗');
            }
        } catch (e) {
            alert('另存失敗: ' + e.message);
        }
    }

    // ==================== 新增主檔 ====================
    let insertModal = null;

    function openInsertModal() {
        document.getElementById('insNewTmpId').value = '';
        document.getElementById('insNotes').value = '';
        if (!insertModal) insertModal = new bootstrap.Modal(document.getElementById('insertModal'));
        insertModal.show();
        setTimeout(() => document.getElementById('insNewTmpId').focus(), 300);
    }

    async function confirmInsert() {
        const newTmpId = document.getElementById('insNewTmpId').value.trim();
        const notes = document.getElementById('insNotes').value.trim();
        if (!newTmpId) { alert('請輸入途程代碼!'); return; }

        try {
            const res = await apiPost('/api/EMOdTmpRoute/InsertMaster', { TmpId: newTmpId, Notes: notes });
            if (res.ok) {
                insertModal.hide();
                await loadMaster(true);
                const newIdx = masterRows.findIndex(r => r.TmpId === newTmpId);
                if (newIdx >= 0) await selectMasterRow(newIdx);
            } else {
                alert(res.error || '新增失敗');
            }
        } catch (e) {
            alert('新增失敗: ' + e.message);
        }
    }

    // ==================== 刪除主檔 ====================
    async function doDeleteMaster() {
        if (!selectedMasterRow) { alert('請先選取一筆!'); return; }
        if (selectedMasterRow.Status === 1) { alert('此途程範本已使用中，不可刪除!'); return; }
        if (!confirm(`確定刪除途程範本 [${selectedMasterRow.TmpId}]?`)) return;

        try {
            const res = await apiPost('/api/EMOdTmpRoute/DeleteMaster', { TmpId: selectedMasterRow.TmpId });
            if (res.ok) {
                await loadMaster(true);
            } else {
                alert(res.error || '刪除失敗');
            }
        } catch (e) {
            alert('刪除失敗: ' + e.message);
        }
    }

    // ==================== 送審 / 退審 ====================
    async function doApproval(isPost) {
        if (!selectedMasterRow) { alert('請先選取一筆!'); return; }
        const action = isPost ? '送審' : '退審';
        if (!confirm(`確定執行 [${selectedMasterRow.TmpId}] ${action}?`)) return;

        try {
            const res = await apiPost('/api/EMOdTmpRoute/ApprovalPost', {
                TmpId: selectedMasterRow.TmpId,
                IsPost: isPost ? 1 : 0,
                Source: 'EMOdTmpRouteMas'
            });
            if (res.ok) {
                const oriTmpId = selectedMasterRow.TmpId;
                await loadMaster(true);
                const newIdx = masterRows.findIndex(r => r.TmpId === oriTmpId);
                if (newIdx >= 0) await selectMasterRow(newIdx);
            } else {
                alert(res.error || `${action}失敗`);
            }
        } catch (e) {
            alert(`${action}失敗: ` + e.message);
        }
    }

    // ==================== 備註儲存 ====================
    async function saveDtlNote() {
        if (!isEditMode) return;
        if (!selectedMasterRow || selectedDetailIdx < 0) return;
        if (selectedMasterRow.Status === 1) return;

        const row = detailRows[selectedDetailIdx];
        if (!row) return;
        const newNote = document.getElementById('txtDtlNote').value;
        if (newNote === (row.Notes ?? '')) return;

        try {
            const res = await apiPost('/api/EMOdTmpRoute/UpdateDtlNote', {
                TmpId: row.TmpId,
                SerialNum: row.SerialNum,
                Notes: newNote
            });
            if (res.ok) {
                detailRows[selectedDetailIdx].Notes = newNote;
                renderDetailBody();
            } else {
                alert(res.error || '儲存備註失敗');
            }
        } catch (e) {
            alert('儲存備註失敗: ' + e.message);
        }
    }

    // ==================== 查詢 (自訂 overlay) ====================
    function openQueryModal() {
        document.getElementById('qryTmpId').value = queryParams.TmpId || '';
        document.getElementById('qryNotes').value = queryParams.Notes || '';
        document.getElementById('qryStatus').value = queryParams.Status !== undefined ? String(queryParams.Status) : '';
        const dialog = document.getElementById('queryDialog');
        dialog.style.position = 'absolute';
        dialog.style.top = '50%';
        dialog.style.left = '50%';
        dialog.style.transform = 'translate(-50%,-50%)';
        document.getElementById('queryOverlay').style.display = 'block';
        document.getElementById('qryTmpId')?.focus();
    }

    function closeQueryDialog() {
        document.getElementById('queryOverlay').style.display = 'none';
    }

    function initQueryDrag() {
        const dialog = document.getElementById('queryDialog');
        const header = dialog?.querySelector('div:first-child');
        if (!header) return;
        header.style.cursor = 'move';
        header.addEventListener('pointerdown', (e) => {
            if (e.target?.closest('button')) return;
            const rect = dialog.getBoundingClientRect();
            dialog.style.transform = 'none';
            dialog.style.position = 'fixed';
            dialog.style.left = rect.left + 'px';
            dialog.style.top = rect.top + 'px';
            const startX = e.clientX - rect.left;
            const startY = e.clientY - rect.top;
            function onMove(ev) {
                dialog.style.left = (ev.clientX - startX) + 'px';
                dialog.style.top  = (ev.clientY - startY) + 'px';
            }
            function onUp() {
                document.removeEventListener('pointermove', onMove);
                document.removeEventListener('pointerup', onUp);
            }
            document.addEventListener('pointermove', onMove);
            document.addEventListener('pointerup', onUp);
            header.setPointerCapture(e.pointerId);
            e.preventDefault();
        });
    }

    function applyQuery() {
        queryParams.TmpId = document.getElementById('qryTmpId').value.trim();
        queryParams.Notes = document.getElementById('qryNotes').value.trim();
        const s = document.getElementById('qryStatus').value;
        queryParams.Status = s === '' ? '' : parseInt(s, 10);
        closeQueryDialog();
        loadMaster(true);
    }

    // ==================== Column Resize ====================
    function initColResize(tableId) {
        const table = document.getElementById(tableId);
        if (!table) return;
        table.querySelectorAll('.th-resize').forEach(resizer => {
            let startX, startW, startTableW, th;
            resizer.addEventListener('mousedown', e => {
                th = resizer.parentElement;
                startX = e.clientX;
                startW = th.offsetWidth;
                startTableW = table.offsetWidth;
                e.preventDefault();
                const onMove = ev => {
                    const delta = ev.clientX - startX;
                    const newW = Math.max(40, startW + delta);
                    th.style.width = newW + 'px';
                    table.style.width = (startTableW + (newW - startW)) + 'px';
                };
                const onUp = () => { document.removeEventListener('mousemove', onMove); document.removeEventListener('mouseup', onUp); };
                document.addEventListener('mousemove', onMove);
                document.addEventListener('mouseup', onUp);
            });
        });
    }

    // ==================== Layout Save / Restore ====================
    const LAYOUT_KEY = 'EMOdTmpRoute.layout';

    function saveLayout() {
        const masterPanel = document.getElementById('masterPanel');
        const notePanel = document.getElementById('notePanel');
        const layout = {
            masterWidth: masterPanel ? masterPanel.offsetWidth : null,
            noteHeight: notePanel ? notePanel.offsetHeight : null
        };
        localStorage.setItem(LAYOUT_KEY, JSON.stringify(layout));
    }

    function restoreLayout() {
        try {
            const raw = localStorage.getItem(LAYOUT_KEY);
            if (!raw) return;
            const layout = JSON.parse(raw);
            if (layout.masterWidth) {
                const masterPanel = document.getElementById('masterPanel');
                if (masterPanel) masterPanel.style.flex = '0 0 ' + layout.masterWidth + 'px';
            }
            if (layout.noteHeight) {
                const notePanel = document.getElementById('notePanel');
                if (notePanel) notePanel.style.height = layout.noteHeight + 'px';
            }
        } catch (_) { }
    }

    // ==================== Vertical Splitter ====================
    function initVSplitter(splitterId, leftPanelId) {
        const splitter = document.getElementById(splitterId);
        const leftPanel = document.getElementById(leftPanelId);
        if (!splitter || !leftPanel) return;
        let startX, startW;
        splitter.addEventListener('mousedown', e => {
            startX = e.clientX;
            startW = leftPanel.offsetWidth;
            e.preventDefault();
            const onMove = ev => { leftPanel.style.flex = '0 0 ' + Math.max(150, startW + ev.clientX - startX) + 'px'; };
            const onUp = () => { document.removeEventListener('mousemove', onMove); document.removeEventListener('mouseup', onUp); };
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    }

    // ==================== Horizontal Splitter (Detail / Note) ====================
    function initHSplitter(splitterId, topPanelId) {
        const splitter = document.getElementById(splitterId);
        const detailWrapper = document.getElementById(topPanelId);
        const notePanel = document.getElementById('notePanel');
        if (!splitter || !detailWrapper || !notePanel) return;
        let startY, startH;
        splitter.addEventListener('mousedown', e => {
            startY = e.clientY;
            startH = notePanel.offsetHeight;
            e.preventDefault();
            const onMove = ev => {
                const delta = startY - ev.clientY;
                notePanel.style.height = Math.max(60, Math.min(400, startH + delta)) + 'px';
            };
            const onUp = () => { document.removeEventListener('mousemove', onMove); document.removeEventListener('mouseup', onUp); };
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    }

    // ==================== Init ====================
    async function init() {
        restoreLayout();
        await Promise.all([loadSysParams(), loadFieldDicts()]);
        buildMasterHead();
        buildDetailHead();
        await loadMaster(true);
        initVSplitter('vSplitter1', 'masterPanel');
        initHSplitter('hSplitter1', 'detailTableWrapper');

        // Toolbar buttons
        document.getElementById('btnModeToggle').addEventListener('click', () => setEditMode(!isEditMode));
        document.getElementById('btnChange').addEventListener('click', openRouteSetModal);
        document.getElementById('btnSaveAs').addEventListener('click', doSaveAs);
        document.getElementById('btnQuery').addEventListener('click', openQueryModal);
        document.getElementById('btnAddRow').addEventListener('click', openInsertModal);
        document.getElementById('btnDelRow').addEventListener('click', doDeleteMaster);
        document.getElementById('btnApprove').addEventListener('click', () => doApproval(true));
        document.getElementById('btnReject').addEventListener('click', () => doApproval(false));
        document.getElementById('btnSaveLayout').addEventListener('click', saveLayout);

        // Query overlay events
        document.getElementById('btnQueryOk').addEventListener('click', applyQuery);
        document.getElementById('btnQueryCancel')?.addEventListener('click', closeQueryDialog);
        document.getElementById('btnQueryClose')?.addEventListener('click', closeQueryDialog);
        document.getElementById('queryOverlay')?.addEventListener('click', (e) => {
            if (e.target === document.getElementById('queryOverlay')) closeQueryDialog();
        });
        document.getElementById('queryOverlay')?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') applyQuery();
            if (e.key === 'Escape') closeQueryDialog();
        });
        initQueryDrag();

        // Insert modal
        document.getElementById('btnInsertOk').addEventListener('click', confirmInsert);
        document.getElementById('insNewTmpId').addEventListener('keydown', e => { if (e.key === 'Enter') confirmInsert(); });

        // RouteSet Modal buttons
        document.getElementById('btnRsSearch').addEventListener('click', searchProcInModal);
        document.getElementById('btnMsAdd').addEventListener('click', msMoveToTarget);
        document.getElementById('btnMsRemove').addEventListener('click', msMoveToSource);
        document.getElementById('btnMsAddAll').addEventListener('click', msMoveAllToTarget);
        document.getElementById('btnMsRemoveAll').addEventListener('click', msMoveAllToSource);
        document.getElementById('btnMsUp').addEventListener('click', msMoveUp);
        document.getElementById('btnMsDown').addEventListener('click', msMoveDown);
        document.getElementById('btnRouteSetOk').addEventListener('click', confirmRouteSet);

        ['rsEdtBProc', 'rsEdtEProc', 'rsEdtProcLike', 'rsEdtProcNameLike'].forEach(id => {
            document.getElementById(id).addEventListener('keydown', e => { if (e.key === 'Enter') searchProcInModal(); });
        });

        // 備註欄 blur 儲存
        document.getElementById('txtDtlNote').addEventListener('blur', saveDtlNote);

        // Column resize (after first render)
        setTimeout(() => {
            initColResize('masterGrid');
            initColResize('detailGrid');
        }, 500);
    }

    document.addEventListener('DOMContentLoaded', init);
})();