/**
 * EMOdTmpPress 壓合疊構 - 主要 JavaScript
 * 功能: Master Grid + TreeView (BOM疊構) + Detail Grid (壓合明細)
 *       + 壓合材料設定 Dialog + 組合模型選擇 Dialog + 查詢
 */
(function () {
    'use strict';

    // ==================== State ====================
    let masterRows = [];
    let selectedMasterRow = null;
    let selectedMasterIdx = -1;
    let bomDtlRows = [];         // BOM 明細 (TreeView 用)
    let detailRows = [];         // 壓合明細
    let isEditMode = false;
    let sysParams = {};
    let masterDict = [];         // 主檔欄位辭典
    let detailDict = [];         // 明細欄位辭典
    let matClassNameMap = {};    // MatClass -> ClassName
    let bomPressNameMap = {};    // LayerId -> LayerName
    let currLayer = 'L0~0';     // 目前選取的層別
    let queryParams = {};        // 查詢參數
    let currentPage = 1;
    let pageSize = 200;
    let totalCount = 0;

    // 壓合材料設定 Dialog state
    let msSourceItems = [];      // 可用物料
    let msTargetItems = [];      // 已選物料
    let msSelectedSource = null;
    let msSelectedTarget = null;

    // BOM 選擇 Dialog state
    let bomMasList = [];
    let bomSelectedRow = null;
    let bomDtlTreeRows = [];

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
            const [activeType, changeLayerName] = await Promise.all([
                apiGet('/api/EMOdTmpPress/SysParam?systemId=EMO&paramId=TmpRouteActiveType'),
                apiGet('/api/EMOdTmpPress/SysParam?systemId=EMO&paramId=ChangeLayerName'),
            ]);
            sysParams.activeType = activeType.value;
            sysParams.changeLayerName = changeLayerName.value;
        } catch (e) {
            console.warn('Failed to load sys params:', e);
        }
    }

    // ==================== Field Dictionary ====================
    async function loadFieldDicts() {
        try {
            const [masDict, dtlDict] = await Promise.all([
                apiGet('/api/EMOdTmpPress/DictFields?table=EMOdTmpPressMas'),
                apiGet('/api/EMOdTmpPress/DictFields?table=EMOdTmpPressDtl'),
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

    function calcColWidth(col) {
        if (col.isCheckbox) return 60;
        if (col.width) return Math.max(col.width * 8 + 20, 50);
        return 120;
    }

    // ==================== Lookup Data ====================
    async function loadLookupData() {
        try {
            const [matClassRes, bomNameRes] = await Promise.all([
                apiGet('/api/EMOdTmpPress/GetMatClassName'),
                apiGet('/api/EMOdTmpPress/GetBOMPressName'),
            ]);
            matClassNameMap = {};
            (matClassRes.data || []).forEach(r => { matClassNameMap[String(r.MatClass || '').trim()] = String(r.ClassName || '').trim(); });
            bomPressNameMap = {};
            (bomNameRes.data || []).forEach(r => { bomPressNameMap[String(r.LayerId || '').trim()] = String(r.LayerName || '').trim(); });
        } catch (e) {
            console.warn('Failed to load lookup data:', e);
        }
    }

    // ==================== Master Grid ====================
    async function loadMasterGrid() {
        try {
            let url = `/api/EMOdTmpPressMas/paged?page=${currentPage}&pageSize=${pageSize}`;
            if (queryParams.TmpId) url += `&TmpId=${encodeURIComponent(queryParams.TmpId)}`;
            if (queryParams.Notes) url += `&Notes=${encodeURIComponent(queryParams.Notes)}`;
            if (queryParams.Status !== undefined && queryParams.Status !== '') url += `&Status=${queryParams.Status}`;

            const res = await apiGet(url);
            masterRows = res.data || [];
            totalCount = res.totalCount || 0;
            renderMasterGrid();

            if (masterRows.length > 0) {
                selectMasterRow(0);
            } else {
                selectedMasterRow = null;
                selectedMasterIdx = -1;
                clearTree();
                clearDetail();
            }
            updateMasterRecordCount();
        } catch (e) {
            console.error('Failed to load master grid:', e);
            alert('載入主檔失敗: ' + e.message);
        }
    }

    function renderMasterGrid() {
        const cols = getVisibleFields(masterDict, ['TmpId', 'TmpBOMId', 'Notes', 'Status']);
        const thead = document.getElementById('masterHead');
        const tbody = document.getElementById('masterBody');
        if (!thead || !tbody) return;

        // Header
        thead.innerHTML = '';
        cols.forEach(col => {
            const th = document.createElement('th');
            th.textContent = col.label;
            th.style.width = calcColWidth(col) + 'px';
            th.innerHTML += '<div class="th-resize"></div>';
            thead.appendChild(th);
        });
        enableThResize(thead);

        // Body
        tbody.innerHTML = '';
        masterRows.forEach((row, idx) => {
            const tr = document.createElement('tr');
            cols.forEach(col => {
                const td = document.createElement('td');
                let val = row[col.field];
                if (col.isCheckbox) {
                    td.textContent = val == 1 ? '\u2611' : '\u2610';
                    td.style.textAlign = 'center';
                } else if (col.field === 'Status') {
                    td.textContent = val == 1 ? (sysParams.activeType === '1' ? '已核准' : '使用中') : '設定中';
                } else {
                    td.textContent = val != null ? String(val).trim() : '';
                }
                td.style.width = calcColWidth(col) + 'px';
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => selectMasterRow(idx));
            tbody.appendChild(tr);
        });
    }

    function selectMasterRow(idx) {
        if (idx < 0 || idx >= masterRows.length) return;
        selectedMasterIdx = idx;
        selectedMasterRow = masterRows[idx];

        // Highlight
        const tbody = document.getElementById('masterBody');
        if (tbody) {
            Array.from(tbody.querySelectorAll('tr')).forEach((tr, i) => {
                tr.classList.toggle('selected', i === idx);
            });
        }

        updateMasterRecordCount();
        loadBOMTree();
    }

    function updateMasterRecordCount() {
        const el = document.getElementById('masterRecordCount');
        const el2 = document.getElementById('recordCountMas');
        const idx = selectedMasterIdx >= 0 ? selectedMasterIdx + 1 : 0;
        const total = masterRows.length;
        if (el) el.textContent = `${idx} / ${total}`;
        if (el2) el2.textContent = `${idx} / ${total}`;
    }

    // ==================== TreeView (BOM 疊構) ====================
    async function loadBOMTree() {
        if (!selectedMasterRow) { clearTree(); return; }
        const tmpBOMId = String(selectedMasterRow.TmpBOMId || '').trim();
        if (!tmpBOMId || tmpBOMId === '----') {
            clearTree();
            return;
        }

        try {
            const res = await apiGet(`/api/EMOdTmpPress/GetBOMDtl?tmpBOMId=${encodeURIComponent(tmpBOMId)}`);
            bomDtlRows = res.data || [];
            renderTree();
            // 預設選第一個節點
            if (bomDtlRows.length > 0) {
                const firstId = String(bomDtlRows[0].LayerId || '').trim();
                selectTreeNode(firstId);
            } else {
                currLayer = 'L0~0';
                clearDetail();
            }
        } catch (e) {
            console.warn('Failed to load BOM tree:', e);
            clearTree();
        }
    }

    function renderTree() {
        const container = document.getElementById('treeContainer');
        if (!container) return;
        container.innerHTML = '';

        if (bomDtlRows.length === 0) {
            container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">無疊構資料</div>';
            return;
        }

        // 建立階層結構
        const nodeMap = {};
        bomDtlRows.forEach(r => {
            const id = String(r.LayerId || '').trim();
            const parentId = String(r.AftLayerId || '').trim();
            const name = String(r.LayerName || r.LayerId || '').trim();
            nodeMap[id] = { id, parentId, name, degree: r.Degree, children: [], data: r };
        });

        const roots = [];
        Object.values(nodeMap).forEach(node => {
            if (node.parentId && nodeMap[node.parentId]) {
                nodeMap[node.parentId].children.push(node);
            } else {
                roots.push(node);
            }
        });

        // 排序
        const sortNodes = (nodes) => {
            nodes.sort((a, b) => (a.data.Sort || 0) - (b.data.Sort || 0));
            nodes.forEach(n => sortNodes(n.children));
        };
        sortNodes(roots);

        // 渲染
        const renderNode = (node, level) => {
            const div = document.createElement('div');

            const item = document.createElement('div');
            item.className = 'tree-node-item';
            item.dataset.layerId = node.id;
            item.style.paddingLeft = (level * 20 + 8) + 'px';

            if (node.children.length > 0) {
                const toggle = document.createElement('span');
                toggle.className = 'tree-toggle expanded';
                toggle.addEventListener('click', (e) => {
                    e.stopPropagation();
                    toggle.classList.toggle('expanded');
                    const childDiv = div.querySelector('.tree-children');
                    if (childDiv) childDiv.style.display = toggle.classList.contains('expanded') ? '' : 'none';
                });
                item.appendChild(toggle);
            } else {
                const spacer = document.createElement('span');
                spacer.style.width = '16px';
                spacer.style.display = 'inline-block';
                item.appendChild(spacer);
            }

            const icon = document.createElement('span');
            icon.className = 'tree-node-icon';
            icon.innerHTML = node.children.length > 0 ? '<i class="bi bi-folder2-open" style="color:#e6a817;"></i>' : '<i class="bi bi-file-earmark" style="color:#6c757d;"></i>';
            item.appendChild(icon);

            const label = document.createElement('span');
            label.textContent = node.name;
            item.appendChild(label);

            item.addEventListener('click', () => selectTreeNode(node.id));
            div.appendChild(item);

            if (node.children.length > 0) {
                const childDiv = document.createElement('div');
                childDiv.className = 'tree-children';
                node.children.forEach(child => childDiv.appendChild(renderNode(child, level + 1)));
                div.appendChild(childDiv);
            }

            return div;
        };

        roots.forEach(root => container.appendChild(renderNode(root, 0)));
    }

    function selectTreeNode(layerId) {
        currLayer = layerId;

        // Highlight
        const container = document.getElementById('treeContainer');
        if (container) {
            container.querySelectorAll('.tree-node-item').forEach(el => {
                el.classList.toggle('selected', el.dataset.layerId === layerId);
            });
        }

        const label = document.getElementById('detailLayerLabel');
        if (label) label.textContent = layerId;

        loadPressDtl();
    }

    function clearTree() {
        const container = document.getElementById('treeContainer');
        if (container) container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">請選擇左側範本</div>';
        bomDtlRows = [];
        currLayer = 'L0~0';
        clearDetail();
    }

    // ==================== Detail Grid (壓合明細) ====================
    async function loadPressDtl() {
        if (!selectedMasterRow || !currLayer) { clearDetail(); return; }
        const tmpId = String(selectedMasterRow.TmpId || '').trim();

        try {
            const res = await apiGet(`/api/EMOdTmpPress/GetPressDtl?tmpId=${encodeURIComponent(tmpId)}&layerId=${encodeURIComponent(currLayer)}`);
            detailRows = res.data || [];
            renderDetailGrid();
        } catch (e) {
            console.warn('Failed to load press detail:', e);
            clearDetail();
        }
    }

    function renderDetailGrid() {
        const cols = getVisibleFields(detailDict, ['SerialNum', 'BefLayer', 'MatClass', 'matcode', 'MatName', 'Notes', 'ClassName', 'LayerName']);
        const thead = document.getElementById('detailHead');
        const tbody = document.getElementById('detailBody');
        if (!thead || !tbody) return;

        // Header
        thead.innerHTML = '';
        cols.forEach(col => {
            const th = document.createElement('th');
            th.textContent = col.label;
            th.style.width = calcColWidth(col) + 'px';
            th.innerHTML += '<div class="th-resize"></div>';
            thead.appendChild(th);
        });
        enableThResize(thead);

        // Body
        tbody.innerHTML = '';
        detailRows.forEach((row, idx) => {
            const tr = document.createElement('tr');
            cols.forEach(col => {
                const td = document.createElement('td');
                let val = row[col.field];
                // Lookup: ClassName from MatClass
                if (col.field === 'ClassName' && !val) {
                    val = matClassNameMap[String(row.MatClass || '').trim()] || '';
                }
                // Lookup: LayerName from FullLayerId
                if (col.field === 'LayerName' && !val) {
                    val = bomPressNameMap[String(row.FullLayerId || '').trim()] || '';
                }
                td.textContent = val != null ? String(val).trim() : '';
                td.style.width = calcColWidth(col) + 'px';
                tr.appendChild(td);
            });
            tbody.appendChild(tr);
        });

        updateDetailRecordCount();
    }

    function clearDetail() {
        detailRows = [];
        const tbody = document.getElementById('detailBody');
        if (tbody) tbody.innerHTML = '';
        updateDetailRecordCount();
    }

    function updateDetailRecordCount() {
        const el = document.getElementById('detailRecordCount');
        if (el) el.textContent = `${detailRows.length} 筆`;
    }

    // ==================== Column Resize ====================
    function enableThResize(thead) {
        thead.querySelectorAll('.th-resize').forEach(handle => {
            handle.addEventListener('mousedown', function (e) {
                e.preventDefault();
                const th = handle.parentElement;
                const startX = e.clientX;
                const startW = th.offsetWidth;
                const colIdx = Array.from(th.parentElement.children).indexOf(th);
                const table = thead.closest('table');

                const onMove = (ev) => {
                    const w = Math.max(startW + ev.clientX - startX, 30);
                    th.style.width = w + 'px';
                    if (table) {
                        table.querySelectorAll('tbody tr').forEach(tr => {
                            const td = tr.children[colIdx];
                            if (td) td.style.width = w + 'px';
                        });
                    }
                };
                const onUp = () => {
                    document.removeEventListener('mousemove', onMove);
                    document.removeEventListener('mouseup', onUp);
                };
                document.addEventListener('mousemove', onMove);
                document.addEventListener('mouseup', onUp);
            });
        });
    }

    // ==================== Splitter ====================
    function initSplitter(splitterId, leftPanelId, direction) {
        const splitter = document.getElementById(splitterId);
        const leftPanel = document.getElementById(leftPanelId);
        if (!splitter || !leftPanel) return;

        splitter.addEventListener('mousedown', function (e) {
            e.preventDefault();
            const startX = e.clientX;
            const startW = leftPanel.offsetWidth;

            const onMove = (ev) => {
                const w = Math.max(startW + ev.clientX - startX, 100);
                leftPanel.style.flex = `0 0 ${w}px`;
            };
            const onUp = () => {
                document.removeEventListener('mousemove', onMove);
                document.removeEventListener('mouseup', onUp);
            };
            document.addEventListener('mousemove', onMove);
            document.addEventListener('mouseup', onUp);
        });
    }

    // ==================== 壓合材料設定 Dialog ====================
    async function openPressSetDialog() {
        if (!selectedMasterRow || !currLayer) {
            alert('請先選擇範本和層別!');
            return;
        }

        const tmpId = String(selectedMasterRow.TmpId || '').trim();
        const status = selectedMasterRow.Status;
        if (status == 1) {
            alert(sysParams.activeType === '1' ? '此範本已核准，不可修改!' : '此範本已使用，不可修改!');
            return;
        }
        if (!isEditMode) {
            alert('請先切換到編輯模式!');
            return;
        }

        // 載入資料
        try {
            const [matRes, defaultRes] = await Promise.all([
                apiGet('/api/EMOdTmpPress/GetMatClass'),
                apiGet(`/api/EMOdTmpPress/GetPressDefault?tmpId=${encodeURIComponent(tmpId)}&layerId=${encodeURIComponent(currLayer)}`),
            ]);

            msSourceItems = (matRes.data || []).map(r => ({
                caption: String(r.ClassName || r.MatClass || '').trim(),
                matClass: String(r.MatClass || '').trim(),
                matName: String(r.MatName || '').trim(),
                className: String(r.ClassName || '').trim()
            }));

            msTargetItems = (defaultRes.data || []).map(r => ({
                caption: String(r.MatName || r.ClassName || '').trim(),
                matClass: String(r.MatClass || '').trim(),
                matName: String(r.MatName || '').trim(),
                className: String(r.ClassName || '').trim()
            }));

            msSelectedSource = null;
            msSelectedTarget = null;
        } catch (e) {
            alert('載入壓合材料資料失敗: ' + e.message);
            return;
        }

        // 設定層別顯示
        const layerLabel = document.getElementById('pressSetLayerLabel');
        if (layerLabel) layerLabel.textContent = currLayer;

        // 隱藏替換名稱功能 (依系統參數)
        // sysParams.changeLayerName != '1' 時隱藏

        renderMsList();
        const modal = new bootstrap.Modal(document.getElementById('pressSetModal'));
        modal.show();
    }

    function renderMsList() {
        renderMsSourceList();
        renderMsTargetList();
    }

    function renderMsSourceList() {
        const tbody = document.getElementById('msSourceList');
        if (!tbody) return;
        tbody.innerHTML = '';
        msSourceItems.forEach((item, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${esc(item.caption)}</td><td>${esc(item.matClass)}</td><td>${esc(item.matClass)}</td><td>${esc(item.className)}</td>`;
            tr.addEventListener('click', () => {
                msSelectedSource = idx;
                tbody.querySelectorAll('tr').forEach((r, i) => r.classList.toggle('selected', i === idx));
            });
            tbody.appendChild(tr);
        });
    }

    function renderMsTargetList() {
        const tbody = document.getElementById('msTargetList');
        if (!tbody) return;
        tbody.innerHTML = '';
        msTargetItems.forEach((item, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${esc(item.caption)}</td><td>${esc(item.matClass)}</td><td>${esc(item.matClass)}</td><td>${esc(item.className)}</td>`;
            tr.addEventListener('click', () => {
                msSelectedTarget = idx;
                tbody.querySelectorAll('tr').forEach((r, i) => r.classList.toggle('selected', i === idx));
            });
            tbody.appendChild(tr);
        });
    }

    function msAdd() {
        if (msSelectedSource === null || msSelectedSource < 0) return;
        const item = { ...msSourceItems[msSelectedSource] };
        msTargetItems.push(item);
        renderMsTargetList();
    }

    function msRemove() {
        if (msSelectedTarget === null || msSelectedTarget < 0) return;
        msTargetItems.splice(msSelectedTarget, 1);
        msSelectedTarget = null;
        renderMsTargetList();
    }

    function msAddAll() {
        msSourceItems.forEach(item => msTargetItems.push({ ...item }));
        renderMsTargetList();
    }

    function msRemoveAll() {
        msTargetItems = [];
        msSelectedTarget = null;
        renderMsTargetList();
    }

    function msMoveUp() {
        if (msSelectedTarget === null || msSelectedTarget <= 0) return;
        const idx = msSelectedTarget;
        [msTargetItems[idx - 1], msTargetItems[idx]] = [msTargetItems[idx], msTargetItems[idx - 1]];
        msSelectedTarget = idx - 1;
        renderMsTargetList();
        // Re-select
        const tbody = document.getElementById('msTargetList');
        if (tbody) tbody.children[msSelectedTarget]?.classList.add('selected');
    }

    function msMoveDown() {
        if (msSelectedTarget === null || msSelectedTarget >= msTargetItems.length - 1) return;
        const idx = msSelectedTarget;
        [msTargetItems[idx], msTargetItems[idx + 1]] = [msTargetItems[idx + 1], msTargetItems[idx]];
        msSelectedTarget = idx + 1;
        renderMsTargetList();
        const tbody = document.getElementById('msTargetList');
        if (tbody) tbody.children[msSelectedTarget]?.classList.add('selected');
    }

    function msChgName() {
        if (msSelectedSource === null || msSelectedTarget === null) {
            alert('請分別選擇左側和右側的項目!');
            return;
        }
        const src = msSourceItems[msSelectedSource];
        const tgt = msTargetItems[msSelectedTarget];
        tgt.caption = src.caption;
        tgt.matName = src.matName;
        renderMsTargetList();
    }

    async function savePressSet() {
        if (!selectedMasterRow || !currLayer) return;
        const tmpId = String(selectedMasterRow.TmpId || '').trim();

        try {
            await apiPost('/api/EMOdTmpPress/SavePressDtl', {
                TmpId: tmpId,
                LayerId: currLayer,
                Items: msTargetItems.map(item => ({
                    MatClass: item.matClass,
                    MatName: item.matName || item.caption
                }))
            });

            // 關閉 modal
            const modalEl = document.getElementById('pressSetModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();

            // 重新載入明細
            loadPressDtl();
        } catch (e) {
            alert('儲存壓合材料失敗: ' + e.message);
        }
    }

    // ==================== BOM 選擇 Dialog ====================
    async function openBOMSelectDialog() {
        if (!selectedMasterRow) {
            alert('請先選擇一筆範本!');
            return;
        }
        if (!isEditMode) {
            alert('請先切換到編輯模式!');
            return;
        }
        const status = selectedMasterRow.Status;
        if (status == 1) {
            alert(sysParams.activeType === '1' ? '此範本已核准，不可修改!' : '此範本已使用，不可修改!');
            return;
        }

        try {
            const activeOnly = sysParams.activeType === '1' ? 1 : 0;
            const res = await apiGet(`/api/EMOdTmpPress/GetBOMMasList?activeOnly=${activeOnly}`);
            bomMasList = res.data || [];
            bomSelectedRow = null;
            renderBOMSelectGrid();
            clearBOMSelectTree();

            const modal = new bootstrap.Modal(document.getElementById('bomSelectModal'));
            modal.show();
        } catch (e) {
            alert('載入組合模型清單失敗: ' + e.message);
        }
    }

    function renderBOMSelectGrid() {
        const thead = document.getElementById('bomSelectHead');
        const tbody = document.getElementById('bomSelectBody');
        if (!thead || !tbody) return;

        const cols = [
            { field: 'TmpId', label: '模板代碼', width: 100 },
            { field: 'Notes', label: '備註', width: 150 }
        ];

        thead.innerHTML = '';
        cols.forEach(col => {
            const th = document.createElement('th');
            th.textContent = col.label;
            th.style.width = col.width + 'px';
            thead.appendChild(th);
        });

        tbody.innerHTML = '';
        bomMasList.forEach((row, idx) => {
            const tr = document.createElement('tr');
            cols.forEach(col => {
                const td = document.createElement('td');
                td.textContent = row[col.field] != null ? String(row[col.field]).trim() : '';
                td.style.width = col.width + 'px';
                tr.appendChild(td);
            });
            tr.addEventListener('click', () => selectBOMRow(idx));
            tbody.appendChild(tr);
        });
    }

    async function selectBOMRow(idx) {
        bomSelectedRow = bomMasList[idx];
        const tbody = document.getElementById('bomSelectBody');
        if (tbody) {
            Array.from(tbody.querySelectorAll('tr')).forEach((tr, i) => tr.classList.toggle('selected', i === idx));
        }

        // 載入 BOM 明細 TreeView
        const tmpId = String(bomSelectedRow.TmpId || '').trim();
        try {
            const res = await apiGet(`/api/EMOdTmpPress/GetBOMMasDtl?tmpId=${encodeURIComponent(tmpId)}`);
            bomDtlTreeRows = res.data || [];
            renderBOMSelectTree();
        } catch (e) {
            console.warn('Failed to load BOM detail:', e);
            clearBOMSelectTree();
        }
    }

    function renderBOMSelectTree() {
        const container = document.getElementById('bomTreeContainer');
        if (!container) return;
        container.innerHTML = '';

        if (bomDtlTreeRows.length === 0) {
            container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">無疊構資料</div>';
            return;
        }

        const nodeMap = {};
        bomDtlTreeRows.forEach(r => {
            const id = String(r.LayerId || '').trim();
            const parentId = String(r.AftLayerId || '').trim();
            const name = String(r.LayerName || r.LayerId || '').trim();
            nodeMap[id] = { id, parentId, name, data: r, children: [] };
        });

        const roots = [];
        Object.values(nodeMap).forEach(node => {
            if (node.parentId && nodeMap[node.parentId]) {
                nodeMap[node.parentId].children.push(node);
            } else {
                roots.push(node);
            }
        });

        const sortNodes = (nodes) => {
            nodes.sort((a, b) => (a.data.Sort || 0) - (b.data.Sort || 0));
            nodes.forEach(n => sortNodes(n.children));
        };
        sortNodes(roots);

        const renderNode = (node, level) => {
            const div = document.createElement('div');
            const item = document.createElement('div');
            item.className = 'tree-node-item';
            item.style.paddingLeft = (level * 20 + 8) + 'px';

            if (node.children.length > 0) {
                const toggle = document.createElement('span');
                toggle.className = 'tree-toggle expanded';
                toggle.addEventListener('click', (e) => {
                    e.stopPropagation();
                    toggle.classList.toggle('expanded');
                    const childDiv = div.querySelector('.tree-children');
                    if (childDiv) childDiv.style.display = toggle.classList.contains('expanded') ? '' : 'none';
                });
                item.appendChild(toggle);
            } else {
                const spacer = document.createElement('span');
                spacer.style.cssText = 'width:16px; display:inline-block;';
                item.appendChild(spacer);
            }

            const label = document.createElement('span');
            label.textContent = node.name;
            item.appendChild(label);
            div.appendChild(item);

            if (node.children.length > 0) {
                const childDiv = document.createElement('div');
                childDiv.className = 'tree-children';
                node.children.forEach(child => childDiv.appendChild(renderNode(child, level + 1)));
                div.appendChild(childDiv);
            }
            return div;
        };

        roots.forEach(root => container.appendChild(renderNode(root, 0)));
    }

    function clearBOMSelectTree() {
        const container = document.getElementById('bomTreeContainer');
        if (container) container.innerHTML = '<div style="color:#999; padding:20px; text-align:center;">請選擇左側組合模型</div>';
    }

    async function confirmBOMSelect() {
        if (!bomSelectedRow) {
            alert('請選擇一個組合模型!');
            return;
        }
        if (!selectedMasterRow) return;

        if (!confirm('確定變更結構?')) return;

        const tmpId = String(selectedMasterRow.TmpId || '').trim();
        const newBOMId = String(bomSelectedRow.TmpId || '').trim();

        try {
            await apiPost('/api/EMOdTmpPress/ChangeRoute', {
                TmpId: tmpId,
                TmpBOMId: newBOMId
            });

            // 關閉 modal
            const modalEl = document.getElementById('bomSelectModal');
            const modal = bootstrap.Modal.getInstance(modalEl);
            if (modal) modal.hide();

            // 重新載入
            await loadMasterGrid();
            // 定位到原來的記錄
            const newIdx = masterRows.findIndex(r => String(r.TmpId || '').trim() === tmpId);
            if (newIdx >= 0) selectMasterRow(newIdx);
        } catch (e) {
            alert('變更組合模型失敗: ' + e.message);
        }
    }

    // ==================== BOM 查詢 Tab 切換 ====================
    function initBOMSelectTabs() {
        document.getElementById('tabBomList')?.addEventListener('click', (e) => {
            e.preventDefault();
            document.getElementById('bomListTab').style.display = '';
            document.getElementById('bomQueryTab').classList.remove('active');
            document.getElementById('tabBomList').classList.add('active');
            document.getElementById('tabBomQuery').classList.remove('active');
        });
        document.getElementById('tabBomQuery')?.addEventListener('click', (e) => {
            e.preventDefault();
            document.getElementById('bomListTab').style.display = 'none';
            document.getElementById('bomQueryTab').classList.add('active');
            document.getElementById('tabBomQuery').classList.add('active');
            document.getElementById('tabBomList').classList.remove('active');
        });
        document.getElementById('btnBomSearch')?.addEventListener('click', async () => {
            const tmpId = document.getElementById('bomQueryTmpId')?.value || '';
            const notes = document.getElementById('bomQueryNotes')?.value || '';
            const status = document.getElementById('bomQueryStatus')?.value || '';
            const activeOnly = sysParams.activeType === '1' ? 1 : 0;

            let url = `/api/EMOdTmpPress/GetBOMMasList?activeOnly=${activeOnly}`;
            if (tmpId) url += `&tmpId=${encodeURIComponent(tmpId)}`;
            if (notes) url += `&notes=${encodeURIComponent(notes)}`;
            if (status !== '') url += `&status=${status}`;

            try {
                const res = await apiGet(url);
                bomMasList = res.data || [];
                renderBOMSelectGrid();
                clearBOMSelectTree();
                // 切回列表 tab
                document.getElementById('tabBomList')?.click();
            } catch (e) {
                alert('查詢失敗: ' + e.message);
            }
        });
    }

    // ==================== 工具列操作 ====================
    function toggleEditMode() {
        isEditMode = !isEditMode;
        const btn = document.getElementById('btnModeToggle');
        if (btn) {
            btn.innerHTML = isEditMode
                ? '<i class="bi bi-pencil"></i> 編輯模式'
                : '<i class="bi bi-eye"></i> 瀏覽模式';
            btn.classList.toggle('active', isEditMode);
        }

        // 顯示/隱藏編輯按鈕
        ['btnAddRow', 'btnDelRow', 'sepAddDel', 'btnApprove', 'btnReject'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.style.display = isEditMode ? '' : 'none';
        });
    }

    async function addMasterRow() {
        const tmpId = prompt('請輸入壓合代碼:');
        if (!tmpId || !tmpId.trim()) return;

        try {
            const res = await apiPost('/api/EMOdTmpPress/InsertMaster', { TmpId: tmpId.trim(), Notes: '' });
            if (!res.ok) { alert(res.error || '新增失敗'); return; }
            await loadMasterGrid();
            const newIdx = masterRows.findIndex(r => String(r.TmpId || '').trim() === tmpId.trim());
            if (newIdx >= 0) selectMasterRow(newIdx);
        } catch (e) {
            alert('新增失敗: ' + e.message);
        }
    }

    async function deleteMasterRow() {
        if (!selectedMasterRow) { alert('請先選擇一筆範本!'); return; }
        const tmpId = String(selectedMasterRow.TmpId || '').trim();

        if (!confirm(`確定要刪除 [${tmpId}] ?`)) return;

        try {
            const res = await apiPost('/api/EMOdTmpPress/DeleteMaster', { TmpId: tmpId });
            if (!res.ok) { alert(res.error || '刪除失敗'); return; }
            await loadMasterGrid();
        } catch (e) {
            alert('刪除失敗: ' + e.message);
        }
    }

    async function saveAsMaster() {
        if (!selectedMasterRow) { alert('請先選擇一筆參考範本!'); return; }
        if (!isEditMode) { alert('請先切換到編輯模式!'); return; }

        const newTmpId = document.getElementById('edtTmpIdNew')?.value?.trim();
        if (!newTmpId) { alert('請輸入新代碼!'); return; }

        const sourceTmpId = String(selectedMasterRow.TmpId || '').trim();

        try {
            const res = await apiPost('/api/EMOdTmpPress/SaveAs', {
                SourceTmpId: sourceTmpId,
                NewTmpId: newTmpId,
                LayerId: currLayer,
                Items: msTargetItems.map(item => ({
                    MatClass: item.matClass,
                    MatName: item.matName || item.caption
                }))
            });
            if (!res.ok) { alert(res.error || '另存失敗'); return; }

            await loadMasterGrid();
            const newIdx = masterRows.findIndex(r => String(r.TmpId || '').trim() === newTmpId);
            if (newIdx >= 0) selectMasterRow(newIdx);
        } catch (e) {
            alert('另存失敗: ' + e.message);
        }
    }

    async function approvalPost(isPost) {
        if (!selectedMasterRow) { alert('請先選擇一筆範本!'); return; }
        const tmpId = String(selectedMasterRow.TmpId || '').trim();
        const action = isPost === 1 ? '送審' : '退審';

        if (!confirm(`確定要${action} [${tmpId}] ?`)) return;

        try {
            const res = await apiPost('/api/EMOdTmpPress/ApprovalPost', { TmpId: tmpId, IsPost: isPost });
            if (!res.ok) { alert(res.error || `${action}失敗`); return; }
            alert(`${action}成功`);
            await loadMasterGrid();
            const newIdx = masterRows.findIndex(r => String(r.TmpId || '').trim() === tmpId);
            if (newIdx >= 0) selectMasterRow(newIdx);
        } catch (e) {
            alert(`${action}失敗: ` + e.message);
        }
    }

    // ==================== 查詢 ====================
    function openQueryDialog() {
        const modal = new bootstrap.Modal(document.getElementById('queryModal'));
        modal.show();
    }

    async function executeQuery() {
        queryParams.TmpId = document.getElementById('qryTmpId')?.value?.trim() || '';
        queryParams.Notes = document.getElementById('qryNotes')?.value?.trim() || '';
        queryParams.Status = document.getElementById('qryStatus')?.value ?? '';

        currentPage = 1;

        const modalEl = document.getElementById('queryModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();

        await loadMasterGrid();
    }

    // ==================== Utility ====================
    function esc(str) {
        const div = document.createElement('div');
        div.textContent = str || '';
        return div.innerHTML;
    }

    // ==================== Initialize ====================
    async function init() {
        await Promise.all([loadSysParams(), loadFieldDicts(), loadLookupData()]);
        await loadMasterGrid();

        // Splitters
        initSplitter('vSplitter1', 'masterPanel');
        initSplitter('vSplitter2', 'treePanel');

        // Toolbar events
        document.getElementById('btnModeToggle')?.addEventListener('click', toggleEditMode);
        document.getElementById('btnAddRow')?.addEventListener('click', addMasterRow);
        document.getElementById('btnDelRow')?.addEventListener('click', deleteMasterRow);
        document.getElementById('btnChange')?.addEventListener('click', openPressSetDialog);
        document.getElementById('btnChangeRoute')?.addEventListener('click', openBOMSelectDialog);
        document.getElementById('btnSaveAs')?.addEventListener('click', saveAsMaster);
        document.getElementById('btnQuery')?.addEventListener('click', openQueryDialog);
        document.getElementById('btnApprove')?.addEventListener('click', () => approvalPost(1));
        document.getElementById('btnReject')?.addEventListener('click', () => approvalPost(0));

        // 壓合材料設定 Dialog events
        document.getElementById('btnMsAdd')?.addEventListener('click', msAdd);
        document.getElementById('btnMsRemove')?.addEventListener('click', msRemove);
        document.getElementById('btnMsAddAll')?.addEventListener('click', msAddAll);
        document.getElementById('btnMsRemoveAll')?.addEventListener('click', msRemoveAll);
        document.getElementById('btnMsUp')?.addEventListener('click', msMoveUp);
        document.getElementById('btnMsDown')?.addEventListener('click', msMoveDown);
        document.getElementById('btnMsChgName')?.addEventListener('click', msChgName);
        document.getElementById('btnPressSetOk')?.addEventListener('click', savePressSet);

        // BOM 選擇 Dialog events
        document.getElementById('btnBomSelectOk')?.addEventListener('click', confirmBOMSelect);
        initBOMSelectTabs();

        // 查詢 Dialog events
        document.getElementById('btnQueryOk')?.addEventListener('click', executeQuery);
    }

    // DOM Ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
