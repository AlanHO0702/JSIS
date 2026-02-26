/**
 * EMOdTmpBOM_SC 組合模型 - 主要 JavaScript
 * 功能: Master Grid + TreeView + Detail Grid + BOM Set Dialog
 */
(function () {
    'use strict';

    // ==================== State ====================
    let masterRows = [];
    let selectedMasterRow = null;
    let detailRows = [];
    let isEditMode = false;
    let sysParams = {};
    let masterDict = [];  // 主檔欄位辭典 [{FieldName, DisplayLabel, Visible, ...}]
    let detailDict = [];  // 明細欄位辭典
    let queryParams = {}; // 查詢參數

    // BOM Set Dialog state
    let bomSetReadOnly = false;
    let layerCount = 12;
    let pressCount = 6;
    let layerButtons = [];       // visual layer buttons in the editor
    let selectedLayerBtn = null; // currently selected layer button reference

    // Layout constants for visual editor
    const COL_WIDTH = 90;
    const ROW_HEIGHT = 28;
    const MARGIN_LEFT = 8;
    const MARGIN_TOP = 8;
    const GAP = 2;

    // ==================== API Helpers ====================
    async function apiFetch(url, options) {
        const jwtId = localStorage.getItem('jwtId') || '';
        const headers = { 'Content-Type': 'application/json' };
        if (jwtId) headers['X-JWTID'] = jwtId;
        const resp = await fetch(url, { ...options, headers: { ...headers, ...(options?.headers || {}) } });
        if (!resp.ok) {
            let msg = `API Error: ${resp.status}`;
            try {
                const body = await resp.json();
                msg = body.error || body.title || body.message || msg;
            } catch (_) { /* body not JSON */ }
            throw new Error(msg);
        }
        return resp.json();
    }

    async function apiGet(url) { return apiFetch(url); }
    async function apiPost(url, body) { return apiFetch(url, { method: 'POST', body: JSON.stringify(body) }); }

    // ==================== System Parameters ====================
    async function loadSysParams() {
        try {
            const [activeType, updateBOMNum, changeLayerName] = await Promise.all([
                apiGet('/api/EMOdTmpBOM_SC/SysParam?systemId=EMO&paramId=TmpRouteActiveType'),
                apiGet('/api/EMOdTmpBOM_SC/SysParam?systemId=EMO&paramId=UpdateBOMNum'),
                apiGet('/api/EMOdTmpBOM_SC/SysParam?systemId=EMO&paramId=ChangeLayerName'),
            ]);
            sysParams.activeType = activeType.value;
            sysParams.updateBOMNum = updateBOMNum.value;
            sysParams.changeLayerName = changeLayerName.value;
        } catch (e) {
            console.warn('Failed to load sys params:', e);
        }
    }

    // ==================== Field Dictionary ====================
    async function loadFieldDicts() {
        try {
            const [masDict, dtlDict] = await Promise.all([
                apiGet('/api/EMOdTmpBOM_SC/DictFields?table=EMOdTmpBOMMas'),
                apiGet('/api/EMOdTmpBOM_SC/DictFields?table=EMOdTmpBOMDtl'),
            ]);
            masterDict = (masDict || []).filter(d => d.Visible);
            detailDict = (dtlDict || []).filter(d => d.Visible);
        } catch (e) {
            console.warn('Failed to load field dicts, using defaults:', e);
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

    /** 計算欄位寬度 (px)：DisplaySize 是字元數，乘以 8 再加 padding */
    function calcColWidth(col) {
        if (col.isCheckbox) return 60;
        if (col.width) return Math.max(col.width * 8 + 20, 50);
        return 120; // default
    }

    /** 為表頭加上拖曳調整欄寬功能 */
    function enableThResize(table) {
        const ths = table.querySelectorAll('thead th');
        ths.forEach(th => {
            const handle = th.querySelector('.th-resize');
            if (!handle) return;
            handle.addEventListener('mousedown', (e) => {
                e.stopPropagation();
                e.preventDefault();
                const startX = e.clientX;
                const startW = th.offsetWidth;
                const startTableW = table.offsetWidth;
                function onMove(ev) {
                    const delta = ev.clientX - startX;
                    const newW = Math.max(30, startW + delta);
                    th.style.width = newW + 'px';
                    table.style.width = (startTableW + (newW - startW)) + 'px';
                }
                function onUp() {
                    document.removeEventListener('mousemove', onMove);
                    document.removeEventListener('mouseup', onUp);
                }
                document.addEventListener('mousemove', onMove);
                document.addEventListener('mouseup', onUp);
            });
        });
    }

    function isCheckedValue(v) {
        if (v === true || v === 1) return true;
        const s = String(v ?? '').trim().toLowerCase();
        return s === '1' || s === 'true' || s === 'yes';
    }

    function formatCellValue(value, dict) {
        if (value === null || value === undefined) return '';
        if (dict && dict.FormatStr === 'yyyy/mm/dd' && value) {
            try {
                const d = new Date(value);
                if (!isNaN(d)) return d.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' });
            } catch (_) { }
        }
        return String(value);
    }

    function renderCell(td, value, col, dictEntry) {
        if (col.isCheckbox) {
            const chk = document.createElement('input');
            chk.type = 'checkbox';
            chk.checked = isCheckedValue(value);
            chk.disabled = true;
            chk.className = 'form-check-input';
            chk.style.cssText = 'margin:0; pointer-events:none;';
            td.style.textAlign = 'center';
            td.appendChild(chk);
        } else {
            const text = formatCellValue(value, dictEntry);
            td.textContent = text;
            if (text) td.title = text;
        }
    }

    // ==================== Master Grid ====================
    async function loadMasterGrid() {
        try {
            let url = '/api/EMOdTmpBOMMas/paged?page=1&pageSize=500';
            if (queryParams.TmpId) url += `&TmpId=${encodeURIComponent(queryParams.TmpId)}`;
            if (queryParams.Notes) url += `&Notes=${encodeURIComponent(queryParams.Notes)}`;
            if (queryParams.Status !== undefined && queryParams.Status !== '') url += `&Status=${queryParams.Status}`;
            const result = await apiGet(url);
            masterRows = result.data || [];
            renderMasterGrid();
            updateRecordCount();
        } catch (e) {
            console.error('Load master failed:', e);
        }
    }

    function renderMasterGrid() {
        const thead = document.getElementById('masterHead');
        const tbody = document.getElementById('masterBody');

        const cols = getVisibleFields(masterDict,
            masterRows.length > 0 ? Object.keys(masterRows[0]) : ['TmpId']);

        // Build header with fixed widths + resize handles
        thead.innerHTML = '';
        const masterTable = document.getElementById('masterGrid');
        let totalW = 0;
        cols.forEach(c => {
            const th = document.createElement('th');
            const w = calcColWidth(c);
            th.style.width = w + 'px';
            if (c.isCheckbox) th.style.textAlign = 'center';
            totalW += w;
            th.textContent = c.label;
            const handle = document.createElement('div');
            handle.className = 'th-resize';
            th.appendChild(handle);
            thead.appendChild(th);
        });
        masterTable.style.width = totalW + 'px';

        // Build body
        tbody.innerHTML = '';
        masterRows.forEach((row) => {
            const tr = document.createElement('tr');
            cols.forEach(c => {
                const td = document.createElement('td');
                const dictEntry = masterDict.find(d => d.FieldName === c.field);
                renderCell(td, row[c.field], c, dictEntry);
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => selectMasterRow(row, tr));
            if (selectedMasterRow && row.TmpId === selectedMasterRow.TmpId) {
                tr.classList.add('selected');
            }
            tbody.appendChild(tr);
        });
        enableThResize(masterTable);
    }

    function selectMasterRow(row, tr) {
        // Deselect previous
        document.querySelectorAll('#masterBody tr.selected').forEach(r => r.classList.remove('selected'));
        tr.classList.add('selected');
        selectedMasterRow = row;
        updateRecordCount();
        loadDetailForMaster(row.TmpId);
    }

    function updateRecordCount() {
        const idx = selectedMasterRow ? masterRows.findIndex(r => r.TmpId === selectedMasterRow.TmpId) + 1 : 0;
        const text = `${idx} / ${masterRows.length}`;
        const el1 = document.getElementById('modeStatusCount');
        const el2 = document.getElementById('recordCountMas');
        if (el1) el1.textContent = text;
        if (el2) el2.textContent = text;
    }

    // ==================== Detail / TreeView ====================
    async function loadDetailForMaster(tmpId) {
        try {
            // TreeView 使用 SP (含階層計算), Detail Grid 直接查表 (含所有欄位)
            const [treeResult, dtlResult] = await Promise.all([
                apiGet(`/api/EMOdTmpBOM_SC/GetBOMData?tmpId=${encodeURIComponent(tmpId)}`),
                apiGet(`/api/EMOdTmpBOM_SC/GetBOMDtl?tmpId=${encodeURIComponent(tmpId)}`),
            ]);
            renderTreeView(treeResult.data || []);
            detailRows = dtlResult.data || [];
            renderDetailGrid(detailRows);
        } catch (e) {
            console.error('Load detail failed:', e);
            detailRows = [];
            renderTreeView([]);
            renderDetailGrid([]);
        }
    }

    function renderTreeView(rows) {
        const container = document.getElementById('treeContainer');
        if (!rows || rows.length === 0) {
            container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">無 BOM 組合資料</div>';
            return;
        }

        // Build tree structure from flat data using Degree, FL, LayerName
        // LayerId = node id, AftLayerId = parent id, Degree = level, FL = sort
        const nodeMap = {};
        const roots = [];

        // Sort by FL for consistent ordering
        const sorted = [...rows].sort((a, b) => (a.FL || 0) - (b.FL || 0));

        sorted.forEach(r => {
            const node = {
                id: r.LayerId,
                degree: r.Degree,
                fl: r.FL,
                el: r.EL,
                name: r.LayerName || `L${r.FL}~${r.EL}`,
                parentId: r.AftLayerId,
                children: [],
                raw: r
            };
            nodeMap[node.id] = node;
        });

        // Link children
        Object.values(nodeMap).forEach(node => {
            if (node.parentId && nodeMap[node.parentId]) {
                nodeMap[node.parentId].children.push(node);
            } else {
                roots.push(node);
            }
        });

        // Sort children by FL
        const sortChildren = (nodes) => {
            nodes.sort((a, b) => a.fl - b.fl);
            nodes.forEach(n => sortChildren(n.children));
        };
        sortChildren(roots);

        container.innerHTML = '';
        roots.forEach(node => {
            container.appendChild(createTreeNodeEl(node, 0));
        });
    }

    function createTreeNodeEl(node, level) {
        const div = document.createElement('div');

        const nodeDiv = document.createElement('div');
        nodeDiv.className = 'tree-node-item';
        nodeDiv.setAttribute('data-layer-id', node.id);

        if (node.children.length > 0) {
            const toggle = document.createElement('span');
            toggle.className = 'tree-toggle expanded';
            toggle.addEventListener('click', (e) => {
                e.stopPropagation();
                const childDiv = div.querySelector('.tree-children');
                if (childDiv) {
                    const hidden = childDiv.style.display === 'none';
                    childDiv.style.display = hidden ? 'block' : 'none';
                    toggle.classList.toggle('expanded', hidden);
                }
            });
            nodeDiv.appendChild(toggle);
        } else {
            const spacer = document.createElement('span');
            spacer.style.cssText = 'display:inline-block;width:18px;';
            nodeDiv.appendChild(spacer);
        }

        const text = document.createElement('span');
        text.textContent = node.name;
        nodeDiv.appendChild(text);

        nodeDiv.addEventListener('click', () => {
            document.querySelectorAll('.tree-node-item.selected').forEach(n => n.classList.remove('selected'));
            nodeDiv.classList.add('selected');
            // Highlight corresponding row in detail grid
            highlightDetailRow(node.id);
        });

        div.appendChild(nodeDiv);

        if (node.children.length > 0) {
            const childrenDiv = document.createElement('div');
            childrenDiv.className = 'tree-children';
            node.children.forEach(child => {
                childrenDiv.appendChild(createTreeNodeEl(child, level + 1));
            });
            div.appendChild(childrenDiv);
        }

        return div;
    }

    function highlightDetailRow(layerId) {
        document.querySelectorAll('#detailBody tr').forEach(tr => {
            tr.classList.toggle('selected', tr.getAttribute('data-layer-id') === String(layerId));
        });
    }

    function renderDetailGrid(rows) {
        const thead = document.querySelector('#detailGrid thead tr');
        const tbody = document.getElementById('detailBody');

        const cols = getVisibleFields(detailDict,
            ['LayerId', 'LayerName', 'DefRoute', 'DefRouteNotes']);

        // Rebuild header from dictionary with fixed widths + resize handles
        thead.innerHTML = '';
        const detailTable = document.getElementById('detailGrid');
        let dtlTotalW = 0;
        cols.forEach(c => {
            const th = document.createElement('th');
            const w = calcColWidth(c);
            th.style.width = w + 'px';
            dtlTotalW += w;
            th.textContent = c.label;
            const handle = document.createElement('div');
            handle.className = 'th-resize';
            th.appendChild(handle);
            thead.appendChild(th);
        });
        detailTable.style.width = dtlTotalW + 'px';

        // Build body
        tbody.innerHTML = '';
        if (!rows || rows.length === 0) return;

        rows.forEach(r => {
            const tr = document.createElement('tr');
            tr.setAttribute('data-layer-id', r.LayerId);
            cols.forEach(c => {
                const td = document.createElement('td');
                const dictEntry = detailDict.find(d => d.FieldName === c.field);
                renderCell(td, r[c.field], c, dictEntry);
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => {
                document.querySelectorAll('#detailBody tr.selected').forEach(r => r.classList.remove('selected'));
                tr.classList.add('selected');
            });
            tbody.appendChild(tr);
        });
        enableThResize(detailTable);
    }

    // ==================== Mode Toggle ====================
    function setMode(edit) {
        isEditMode = edit;
        const box = document.getElementById('modeStatusBox');
        const label = document.getElementById('modeStatusLabel');
        const btn = document.getElementById('btnModeToggle');
        const btnAdd = document.getElementById('btnAddRow');
        const btnDel = document.getElementById('btnDelRow');
        if (edit) {
            box.classList.add('edit-mode');
            label.textContent = '編輯模式';
            btn.textContent = '瀏覽';
            btn.title = '切換至瀏覽模式';
            btnAdd.disabled = false;
            btnDel.disabled = false;
        } else {
            box.classList.remove('edit-mode');
            label.textContent = '瀏覽模式';
            btn.textContent = '修改';
            btn.title = '切換至編輯模式';
            btnAdd.disabled = true;
            btnDel.disabled = true;
        }
    }

    // ==================== Toolbar Button Handlers ====================
    function initToolbar() {
        document.getElementById('btnModeToggle').addEventListener('click', () => {
            setMode(!isEditMode);
        });

        document.getElementById('btnAddRow').addEventListener('click', handleAddRow);
        document.getElementById('btnDelRow').addEventListener('click', handleDelRow);
        document.getElementById('btnAddMasterOk').addEventListener('click', doAddMaster);
        // Enter 鍵觸發確定
        document.getElementById('addMasterModal')?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') doAddMaster();
        });
        document.getElementById('btnQuery')?.addEventListener('click', openQueryDialog);
        document.getElementById('btnQueryOk')?.addEventListener('click', executeQuery);
        document.getElementById('btnQueryCancel')?.addEventListener('click', closeQueryDialog);
        document.getElementById('btnQueryClose')?.addEventListener('click', closeQueryDialog);
        document.getElementById('queryOverlay')?.addEventListener('click', (e) => {
            if (e.target === document.getElementById('queryOverlay')) closeQueryDialog();
        });
        document.getElementById('queryOverlay')?.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') executeQuery();
            if (e.key === 'Escape') closeQueryDialog();
        });
        document.getElementById('btnChange').addEventListener('click', handleChange);
        document.getElementById('btnFormView').addEventListener('click', handleFormView);
        document.getElementById('btnSaveAs').addEventListener('click', handleSaveAs);
        document.getElementById('btnApprove').addEventListener('click', () => handleApproval(1));
        document.getElementById('btnReject').addEventListener('click', () => handleApproval(0));
    }

    // ==================== Add / Delete Master Row ====================
    function handleAddRow() {
        if (!isEditMode) return;
        // 清空欄位
        const idInput = document.getElementById('addTmpId');
        const notesInput = document.getElementById('addTmpNotes');
        if (idInput) idInput.value = '';
        if (notesInput) notesInput.value = '';
        const modal = new bootstrap.Modal(document.getElementById('addMasterModal'));
        modal.show();
        // 開啟後自動 focus 代碼欄位
        document.getElementById('addMasterModal').addEventListener('shown.bs.modal', () => {
            idInput?.focus();
        }, { once: true });
    }

    async function doAddMaster() {
        const idInput = document.getElementById('addTmpId');
        const notesInput = document.getElementById('addTmpNotes');
        const id = (idInput?.value || '').trim();
        const notes = (notesInput?.value || '').trim();

        if (!id) { idInput?.focus(); alert('請輸入模型代碼'); return; }
        if (id.length > 12) { idInput?.focus(); alert('代碼長度不可超過12碼'); return; }
        if (masterRows.some(r => r.TmpId === id)) { idInput?.focus(); alert('此代碼已存在'); return; }

        try {
            const result = await apiPost('/api/EMOdTmpBOM_SC/InsertMaster', { tmpId: id, notes });
            if (!result.ok) { alert('新增失敗: ' + (result.error || '未知錯誤')); return; }

            // 關閉 modal
            const modalEl = document.getElementById('addMasterModal');
            bootstrap.Modal.getInstance(modalEl)?.hide();

            await loadMasterGrid();
            const newRow = masterRows.find(r => r.TmpId === id);
            if (newRow) {
                const idx = masterRows.indexOf(newRow);
                const tr = document.querySelectorAll('#masterBody tr')[idx];
                if (tr) selectMasterRow(newRow, tr);
            }
        } catch (e) {
            alert('新增失敗: ' + e.message);
        }
    }

    async function handleDelRow() {
        if (!isEditMode) return;
        if (!selectedMasterRow) { alert('請先選擇一筆資料'); return; }

        const tmpId = selectedMasterRow.TmpId;
        if (!confirm(`確定要刪除 [${tmpId}] 嗎？`)) return;

        try {
            await apiPost('/api/EMOdTmpBOM_SC/DeleteMaster', { tmpId });
            selectedMasterRow = null;
            await loadMasterGrid();
        } catch (e) {
            alert('刪除失敗: ' + e.message);
        }
    }

    async function handleChange() {
        if (!isEditMode) {
            alert('請先切換至編輯模式');
            return;
        }
        if (!selectedMasterRow) {
            alert('請先選擇一筆資料');
            return;
        }

        const tmpId = selectedMasterRow.TmpId;

        // Check if editable
        try {
            const chk = await apiGet(`/api/EMOdTmpBOM_SC/CheckUpdate?tmpId=${encodeURIComponent(tmpId)}`);
            if (!chk.canEdit) {
                const msg = sysParams.activeType === '1' ? '該組合模型已送審，無法異動！' : '該組合模型已使用，無法異動！';
                alert(msg);
                return;
            }
        } catch (e) {
            alert('檢查失敗: ' + e.message);
            return;
        }

        bomSetReadOnly = false;
        openBomSetDialog(tmpId);
    }

    function handleFormView() {
        if (!selectedMasterRow) {
            alert('請先選擇一筆資料');
            return;
        }
        bomSetReadOnly = true;
        openBomSetDialog(selectedMasterRow.TmpId);
    }

    async function handleSaveAs() {
        if (!isEditMode) {
            alert('請先切換至編輯模式');
            return;
        }
        if (!selectedMasterRow) {
            alert('請先選擇一筆資料');
            return;
        }

        const newId = document.getElementById('edtTmpIdNew').value.trim();
        if (!newId) {
            alert('請輸入另存代碼');
            return;
        }

        try {
            const result = await apiPost('/api/EMOdTmpBOM_SC/CopyBOM', {
                sourceTmpId: selectedMasterRow.TmpId,
                newTmpId: newId
            });

            if (!result.ok) {
                alert(result.error || '複製失敗');
                return;
            }

            alert('複製成功');
            await loadMasterGrid();
            // Navigate to new record
            const newRow = masterRows.find(r => r.TmpId === newId);
            if (newRow) {
                const trs = document.querySelectorAll('#masterBody tr');
                const idx = masterRows.indexOf(newRow);
                if (trs[idx]) selectMasterRow(newRow, trs[idx]);
            }
        } catch (e) {
            alert('複製失敗: ' + e.message);
        }
    }

    // ==================== 送審 / 退審 ====================
    async function handleApproval(isPost) {
        if (!selectedMasterRow) {
            alert('請先選擇一筆資料');
            return;
        }

        const tmpId = selectedMasterRow.TmpId;
        const action = isPost === 1 ? '送審' : '退審';

        if (!confirm(`確定要將 [${tmpId}] ${action} 嗎？`)) return;

        try {
            const result = await apiPost('/api/EMOdTmpBOM_SC/ApprovalPost', {
                tmpId: tmpId,
                isPost: isPost
            });

            if (!result.ok) {
                alert(`${action}失敗: ${result.error || '未知錯誤'}`);
                return;
            }

            alert(`${action}成功`);
            // Refresh master grid to update status
            await loadMasterGrid();
            // Re-select the same row
            const row = masterRows.find(r => r.TmpId === tmpId);
            if (row) {
                const trs = document.querySelectorAll('#masterBody tr');
                const idx = masterRows.indexOf(row);
                if (trs[idx]) selectMasterRow(row, trs[idx]);
            }
        } catch (e) {
            alert(`${action}失敗: ${e.message}`);
        }
    }

    // ==================== 查詢 (自訂 overlay，不用 Bootstrap Modal) ====================
    function openQueryDialog() {
        const dialog = document.getElementById('queryDialog');
        // 每次開啟重設置中位置
        dialog.style.position = 'absolute';
        dialog.style.top = '50%';
        dialog.style.left = '50%';
        dialog.style.transform = 'translate(-50%,-50%)';
        document.getElementById('queryOverlay').style.display = 'block';
        document.getElementById('qryTmpId')?.focus();
    }

    function initQueryDrag() {
        const overlay = document.getElementById('queryOverlay');
        const dialog = document.getElementById('queryDialog');
        const header = dialog?.querySelector('div:first-child');
        if (!header) return;
        header.style.cursor = 'move';
        header.addEventListener('pointerdown', (e) => {
            if (e.target?.closest('button')) return;
            // 切換成 fixed 定位，避免 transform 座標系干擾
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

    function closeQueryDialog() {
        document.getElementById('queryOverlay').style.display = 'none';
    }

    async function executeQuery() {
        queryParams.TmpId = document.getElementById('qryTmpId')?.value?.trim() || '';
        queryParams.Notes = document.getElementById('qryNotes')?.value?.trim() || '';
        queryParams.Status = document.getElementById('qryStatus')?.value ?? '';

        closeQueryDialog();
        await loadMasterGrid();

        if (masterRows.length > 0) {
            const firstTr = document.querySelector('#masterBody tr');
            if (firstTr) selectMasterRow(masterRows[0], firstTr);
        } else {
            selectedMasterRow = null;
            detailRows = [];
            renderTreeView([]);
            renderDetailGrid([]);
            updateRecordCount();
        }
    }

    // ==================== BOM Set Dialog ====================
    async function openBomSetDialog(tmpId) {
        const spnLayerEl = document.getElementById('spnLayer');
        const spnPressEl = document.getElementById('spnPress');
        const footerEl = document.getElementById('bomSetFooter');
        const lblNameEl = document.getElementById('lblLayerName');
        const edtNameEl = document.getElementById('edtOriName');
        const btnRenameEl = document.getElementById('btnRenameLayer');

        // Read-only adjustments
        if (bomSetReadOnly) {
            spnLayerEl.disabled = true;
            spnPressEl.disabled = true;
            footerEl.style.display = 'none';
            lblNameEl.style.display = 'none';
            edtNameEl.style.display = 'none';
            btnRenameEl.style.display = 'none';
        } else {
            const canUpdateNum = sysParams.updateBOMNum === '1';
            spnLayerEl.disabled = !canUpdateNum;
            spnPressEl.disabled = !canUpdateNum;
            footerEl.style.display = '';

            const canChangeName = sysParams.changeLayerName === '1';
            lblNameEl.style.display = canChangeName ? '' : 'none';
            edtNameEl.style.display = canChangeName ? '' : 'none';
            btnRenameEl.style.display = canChangeName ? '' : 'none';
        }

        // Load current BOM data
        try {
            const maxInfo = await apiGet(`/api/EMOdTmpBOM_SC/GetMaxLayerDegree?tmpId=${encodeURIComponent(tmpId)}`);
            const bomData = await apiGet(`/api/EMOdTmpBOM_SC/GetBOMData?tmpId=${encodeURIComponent(tmpId)}`);

            // Set layer/press counts
            layerCount = Math.max(maxInfo.maxLayer || 12, 4);
            pressCount = Math.max(maxInfo.maxDegree || 6, 2);
            spnLayerEl.value = layerCount;
            spnPressEl.value = pressCount;

            // Clear and rebuild visual editor
            layerButtons = [];
            selectedLayerBtn = null;
            buildPressButtons();
            buildLayerGrid();

            // Load existing data into visual editor
            if (bomData.data && bomData.data.length > 0) {
                loadBomDataToEditor(bomData.data);
            }
        } catch (e) {
            console.error('Load BOM data failed:', e);
            layerCount = 12;
            pressCount = 6;
            spnLayerEl.value = layerCount;
            spnPressEl.value = pressCount;
            layerButtons = [];
            selectedLayerBtn = null;
            buildPressButtons();
            buildLayerGrid();
        }

        // Show modal
        const modal = new bootstrap.Modal(document.getElementById('bomSetModal'));
        modal.show();
    }

    function buildPressButtons() {
        const container = document.getElementById('pressButtonsRow');
        container.innerHTML = '';

        // Use same left formula as createLayerButton: left = MARGIN_LEFT + col * (COL_WIDTH + GAP)
        for (let col = 1; col <= pressCount; col++) {
            const btn = document.createElement('button');
            btn.className = 'press-btn';
            btn.style.width = COL_WIDTH + 'px';
            btn.style.left = (MARGIN_LEFT + col * (COL_WIDTH + GAP)) + 'px';

            if (col === 1) {
                btn.textContent = '發料';
            } else {
                btn.textContent = `壓合${col - 1}`;
            }

            btn.setAttribute('data-press-index', col);
            btn.addEventListener('click', () => handlePressClick(col));
            container.appendChild(btn);
        }
    }

    function buildLayerGrid() {
        const area = document.getElementById('layerEditorArea');
        area.innerHTML = '';
        layerButtons = [];

        // Create base layer buttons (degree 0, first column)
        for (let i = 0; i < layerCount; i++) {
            const layerNum = i + 1;
            createLayerButton({
                fl: layerNum,
                el: layerNum,
                degree: 0,
                col: 0,
                name: `L${layerNum}`,
                isBase: true
            });
        }

        updateLayerEditorHeight();
    }

    function createLayerButton(opts) {
        const area = document.getElementById('layerEditorArea');

        const btn = document.createElement('div');
        btn.className = 'layer-btn' + (opts.isBase ? ' base-layer' : ' combined');

        // Position: column = degree/press stage, row = FL-1
        const left = MARGIN_LEFT + opts.col * (COL_WIDTH + GAP);
        const top = MARGIN_TOP + (opts.fl - 1) * (ROW_HEIGHT + GAP);
        const height = (opts.el - opts.fl + 1) * (ROW_HEIGHT + GAP) - GAP;

        btn.style.left = left + 'px';
        btn.style.top = top + 'px';
        btn.style.width = COL_WIDTH + 'px';
        btn.style.height = height + 'px';
        btn.textContent = opts.name;

        const layerData = {
            fl: opts.fl,
            el: opts.el,
            degree: opts.degree,
            col: opts.col,
            name: opts.name,
            isBase: opts.isBase || false,
            element: btn
        };

        btn.addEventListener('click', (e) => {
            e.stopPropagation();
            toggleLayerSelection(layerData);
        });

        btn.addEventListener('contextmenu', (e) => {
            e.preventDefault();
            if (bomSetReadOnly) return;
            if (layerData.isBase) return; // base layers cannot be removed
            showContextMenu(e, layerData);
        });

        area.appendChild(btn);
        layerButtons.push(layerData);

        return layerData;
    }

    function updateLayerEditorHeight() {
        const area = document.getElementById('layerEditorArea');
        const totalHeight = MARGIN_TOP + layerCount * (ROW_HEIGHT + GAP) + 20;
        const totalWidth = MARGIN_LEFT + (pressCount + 1) * (COL_WIDTH + GAP) + 20;
        area.style.minHeight = Math.max(totalHeight, 400) + 'px';
        area.style.minWidth = Math.max(totalWidth, 300) + 'px';
    }

    function toggleLayerSelection(layerData) {
        if (layerData.element.classList.contains('selected')) {
            layerData.element.classList.remove('selected');
            if (selectedLayerBtn === layerData) selectedLayerBtn = null;
            document.getElementById('edtOriName').value = '';
        } else {
            // Deselect other buttons in the same column
            // (allow multi-select within the same press stage for combining)
            layerData.element.classList.add('selected');
            selectedLayerBtn = layerData;
            document.getElementById('edtOriName').value = layerData.name;
        }
    }

    function handlePressClick(pressIndex) {
        if (bomSetReadOnly) return;
        if (pressIndex === 0) return; // col 0 is base layers, not clickable

        // Find all selected buttons from previous columns (col < pressIndex)
        const selected = layerButtons.filter(b =>
            b.element.classList.contains('selected') && b.col < pressIndex
        );

        if (selected.length === 0) {
            alert('請先選擇要組合的層別');
            return;
        }

        // Calculate FL and EL
        const fl = Math.min(...selected.map(b => b.fl));
        const el = Math.max(...selected.map(b => b.el));

        // Validate: check no overlap with existing buttons in this column
        const existing = layerButtons.filter(b => b.col === pressIndex && !b.isBase);
        for (const ex of existing) {
            if (fl <= ex.el && el >= ex.fl) {
                alert('與現有組合重疊，請先移除後再重新組合');
                return;
            }
        }

        // Validate: check continuity - the selected layers should cover FL to EL completely
        if (!checkContinuity(selected, fl, el)) {
            alert('選擇的層別不連續，無法組合');
            return;
        }

        // Create combined button
        const name = `L${fl}~${el}`;
        createLayerButton({
            fl: fl,
            el: el,
            degree: pressIndex,
            col: pressIndex,
            name: name,
            isBase: false
        });

        // Deselect all
        deselectAll();
    }

    function checkContinuity(selected, fl, el) {
        // Sort by fl
        const sorted = [...selected].sort((a, b) => a.fl - b.fl);
        // Check if they cover the entire range
        let expected = fl;
        for (const s of sorted) {
            if (s.fl !== expected) return false;
            expected = s.el + 1;
        }
        return expected === el + 1;
    }

    function deselectAll() {
        layerButtons.forEach(b => b.element.classList.remove('selected'));
        selectedLayerBtn = null;
        document.getElementById('edtOriName').value = '';
    }

    // Load BOM data from DB into visual editor
    function loadBomDataToEditor(data) {
        // data comes from EMOdTmpBomGetData: FL, EL, Degree, MaxDegree, LayerName
        if (!data || data.length === 0) return;

        const maxDegree = Math.max(...data.map(d => d.MaxDegree || d.Degree || 0));

        data.forEach(row => {
            const fl = row.FL;
            const el = row.EL;
            const rawDegree = row.Degree;
            // Reverse the degree: in DB, higher degree = more base; in visual, col 0 = base
            const col = maxDegree - rawDegree + 1;
            const name = row.LayerName || `L${fl}~${el}`;

            if (col === 0) return; // Skip base layers (already drawn)

            // Update layer/press counts if needed
            if (el > layerCount) {
                layerCount = el;
                document.getElementById('spnLayer').value = layerCount;
            }
            if (col + 1 > pressCount) {
                pressCount = col + 1;
                document.getElementById('spnPress').value = pressCount;
                buildPressButtons();
            }

            createLayerButton({
                fl: fl,
                el: el,
                degree: col,
                col: col,
                name: name,
                isBase: false
            });
        });
    }

    // Context Menu
    function showContextMenu(e, layerData) {
        const menu = document.getElementById('ctxMenu');
        menu.style.left = e.clientX + 'px';
        menu.style.top = e.clientY + 'px';
        menu.style.display = 'block';

        menu._targetLayer = layerData;
    }

    function hideContextMenu() {
        document.getElementById('ctxMenu').style.display = 'none';
    }

    function initContextMenu() {
        document.getElementById('ctxRemove').addEventListener('click', () => {
            const menu = document.getElementById('ctxMenu');
            const target = menu._targetLayer;
            if (target && !target.isBase) {
                removeLayerButton(target);
            }
            hideContextMenu();
        });

        document.getElementById('ctxClearAll').addEventListener('click', () => {
            if (bomSetReadOnly) return;
            // Remove all non-base layer buttons
            const toRemove = layerButtons.filter(b => !b.isBase);
            toRemove.forEach(b => removeLayerButton(b));
            hideContextMenu();
        });

        document.addEventListener('click', hideContextMenu);
    }

    function removeLayerButton(layerData) {
        if (layerData.element && layerData.element.parentNode) {
            layerData.element.parentNode.removeChild(layerData.element);
        }
        const idx = layerButtons.indexOf(layerData);
        if (idx >= 0) layerButtons.splice(idx, 1);
    }

    // Rename layer
    function initRenameLayer() {
        document.getElementById('btnRenameLayer').addEventListener('click', () => {
            if (!selectedLayerBtn) {
                alert('請先選擇一個層別按鈕');
                return;
            }
            const newName = document.getElementById('edtOriName').value.trim();
            if (!newName) return;

            selectedLayerBtn.name = newName;
            selectedLayerBtn.element.textContent = newName;
            deselectAll();
        });
    }

    // Spinner change handlers
    function initSpinners() {
        document.getElementById('spnLayer').addEventListener('change', (e) => {
            if (bomSetReadOnly) return;
            layerCount = parseInt(e.target.value) || 12;
            rebuildEditorFromScratch();
        });

        document.getElementById('spnPress').addEventListener('change', (e) => {
            if (bomSetReadOnly) return;
            pressCount = parseInt(e.target.value) || 6;
            buildPressButtons();
            updateLayerEditorHeight();
        });
    }

    function rebuildEditorFromScratch() {
        // Remove all buttons and rebuild base layers
        const area = document.getElementById('layerEditorArea');
        area.innerHTML = '';
        layerButtons = [];
        selectedLayerBtn = null;
        buildPressButtons();
        buildLayerGrid();
    }

    // Save BOM from visual editor
    function collectBomFromEditor() {
        // Collect all non-base layer buttons as BOM items
        const combined = layerButtons.filter(b => !b.isBase);

        // Sort by col (press index) then FL
        combined.sort((a, b) => a.col - b.col || a.fl - b.fl);

        // For each combined layer, find its parent (higher col that contains it)
        const result = [];
        combined.forEach(b => {
            // Find the parent: next higher col that spans a range containing this one
            let aftFL = 0, aftEL = 0;
            const higher = combined.filter(h => h.col > b.col && h.fl <= b.fl && h.el >= b.el);
            if (higher.length > 0) {
                // Pick closest parent (smallest col > b.col)
                higher.sort((a, c) => a.col - c.col);
                aftFL = higher[0].fl;
                aftEL = higher[0].el;
            }

            result.push({
                issLayer: b.col === 1 ? 1 : 0, // degree 1 = issue layer
                degree: b.col,
                fl: b.fl,
                el: b.el,
                aftFL: aftFL,
                aftEL: aftEL,
                layerName: b.name
            });
        });

        return result;
    }

    function initBomSetOk() {
        document.getElementById('btnBomSetOk').addEventListener('click', async () => {
            if (bomSetReadOnly) return;
            if (!selectedMasterRow) return;

            const layers = collectBomFromEditor();
            if (layers.length === 0) {
                alert('請至少設定一組組合');
                return;
            }

            try {
                const result = await apiPost('/api/EMOdTmpBOM_SC/SaveBOM', {
                    tmpId: selectedMasterRow.TmpId,
                    layers: layers
                });

                if (!result.ok) {
                    alert(result.error || '儲存失敗');
                    return;
                }

                // Close modal and refresh
                bootstrap.Modal.getInstance(document.getElementById('bomSetModal')).hide();
                await loadDetailForMaster(selectedMasterRow.TmpId);
                alert('儲存成功');
            } catch (e) {
                alert('儲存失敗: ' + e.message);
            }
        });
    }

    // ==================== Splitter ====================
    const LAYOUT_KEY = 'EMOdTmpBOM_SC.layout';

    function saveLayout() {
        const masterPanel = document.getElementById('masterPanel');
        const detailGridContainer = document.getElementById('detailGridContainer');
        const layout = {
            masterWidth: masterPanel ? masterPanel.offsetWidth : null,
            detailHeight: detailGridContainer ? detailGridContainer.offsetHeight : null,
        };
        localStorage.setItem(LAYOUT_KEY, JSON.stringify(layout));
        alert('欄寬已保存');
    }

    function restoreLayout() {
        try {
            const raw = localStorage.getItem(LAYOUT_KEY);
            if (!raw) return;
            const layout = JSON.parse(raw);
            if (layout.masterWidth) {
                const masterPanel = document.getElementById('masterPanel');
                if (masterPanel) masterPanel.style.flex = `0 0 ${layout.masterWidth}px`;
            }
            if (layout.detailHeight) {
                const detailGridContainer = document.getElementById('detailGridContainer');
                if (detailGridContainer) {
                    detailGridContainer.style.flex = `0 0 ${layout.detailHeight}px`;
                    detailGridContainer.style.maxHeight = 'none';
                }
            }
        } catch (_) { }
    }

    function initSplitters() {
        // 還原上次保存的位置
        restoreLayout();

        // Vertical splitter (between master panel and detail panel)
        const vSplitter = document.getElementById('vSplitter');
        const masterPanel = document.getElementById('masterPanel');

        let vStartX, vStartWidth;
        vSplitter.addEventListener('mousedown', (e) => {
            vStartX = e.clientX;
            vStartWidth = masterPanel.offsetWidth;
            document.addEventListener('mousemove', onVSplitterMove);
            document.addEventListener('mouseup', onVSplitterUp);
            e.preventDefault();
        });

        function onVSplitterMove(e) {
            const newWidth = vStartWidth + (e.clientX - vStartX);
            if (newWidth >= 300 && newWidth <= 900) {
                masterPanel.style.flex = `0 0 ${newWidth}px`;
            }
        }
        function onVSplitterUp() {
            document.removeEventListener('mousemove', onVSplitterMove);
            document.removeEventListener('mouseup', onVSplitterUp);
        }

        // Horizontal splitter (between tree and detail grid)
        const hSplitter = document.getElementById('hSplitter1');
        const detailGridContainer = document.getElementById('detailGridContainer');

        let startY, startHeight;
        hSplitter.addEventListener('mousedown', (e) => {
            startY = e.clientY;
            startHeight = detailGridContainer.offsetHeight;
            document.addEventListener('mousemove', onHSplitterMove);
            document.addEventListener('mouseup', onHSplitterUp);
            e.preventDefault();
        });

        function onHSplitterMove(e) {
            const newHeight = startHeight - (e.clientY - startY);
            if (newHeight >= 60 && newHeight <= 500) {
                detailGridContainer.style.flex = `0 0 ${newHeight}px`;
                detailGridContainer.style.maxHeight = 'none';
            }
        }
        function onHSplitterUp() {
            document.removeEventListener('mousemove', onHSplitterMove);
            document.removeEventListener('mouseup', onHSplitterUp);
        }

        // 保存欄寬按鈕
        document.getElementById('btnSaveLayout')?.addEventListener('click', saveLayout);
    }

    // ==================== Initialize ====================
    async function init() {
        await Promise.all([loadSysParams(), loadFieldDicts()]);
        initToolbar();
        initQueryDrag();
        initContextMenu();
        initRenameLayer();
        initSpinners();
        initBomSetOk();
        initSplitters();
        await loadMasterGrid();

        // Auto-select first row
        if (masterRows.length > 0) {
            const firstTr = document.querySelector('#masterBody tr');
            if (firstTr) selectMasterRow(masterRows[0], firstTr);
        }
    }

    document.addEventListener('DOMContentLoaded', init);
})();
