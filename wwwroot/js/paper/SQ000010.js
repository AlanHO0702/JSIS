/**
 * SQ000010 (LA204 - 客戶連絡單) 客戶端擴充
 * 對應 Delphi: MeetingPaper.pas
 *
 * 功能：
 * 1. 修正標籤名稱（連絡日期、起迄時間）
 * 2. 檔案載入 / 檢視檔案按鈕
 */
(function () {
    'use strict';

    // ==========================================
    // 工具函式
    // ==========================================
    function renameLabel(fieldName, newCaption) {
        const label = document.querySelector(`.header-label-item[data-field="${CSS.escape(fieldName)}"]`);
        if (label) label.textContent = newCaption;
    }

    function getPaperNum() {
        return (document.querySelector('[data-field="PaperNum"] input, input[name="PaperNum"]')?.value ||
            window._specialUIConfig?.paperNum || '').trim();
    }

    // ==========================================
    // 1. 修正標籤名稱
    // ==========================================
    function fixLabels() {
        renameLabel('PaperDate', '連絡日期');
        renameLabel('InDateF', '起迄時間');
    }

    // ==========================================
    // 2. 檔案載入 / 檢視檔案按鈕
    // ==========================================
    function addFileButtons() {
        // 找到第一個 tab 的欄位容器（position:relative 的 ul）
        const fieldsList = document.querySelector('.header-fields-tab[data-tab-index]');
        if (!fieldsList) return;
        if (document.getElementById('sq10-file-buttons')) return;

        // 找到所有欄位中最右邊的位置，把按鈕放在其右側
        const allFields = fieldsList.querySelectorAll('.draggable-field');
        let maxRight = 0, rightFieldTop = 0;
        allFields.forEach(f => {
            const r = (parseInt(f.style.left) || 0) + (parseInt(f.style.width) || 0);
            if (r > maxRight) {
                maxRight = r;
                rightFieldTop = parseInt(f.style.top) || 0;
            }
        });
        const btnTop = rightFieldTop;
        const btnLeft = maxRight + 20;

        const btnContainer = document.createElement('li');
        btnContainer.id = 'sq10-file-buttons';
        btnContainer.style.cssText = `position:absolute; top:${btnTop}px; left:${btnLeft}px; display:flex; flex-direction:column; gap:6px; z-index:1;`;

        btnContainer.innerHTML =
            '<button type="button" class="btn btn-outline-secondary btn-sm" id="btnFileUpload" style="white-space:nowrap;">📂 檔案載入</button>' +
            '<button type="button" class="btn btn-outline-secondary btn-sm" id="btnFileView" style="white-space:nowrap;">🔍 檢視檔案</button>';

        fieldsList.appendChild(btnContainer);

        document.getElementById('btnFileUpload')?.addEventListener('click', handleFileUpload);
        document.getElementById('btnFileView')?.addEventListener('click', handleFileView);
    }

    function getDetailTableName() {
        // 從明細 tab 的 data-dict-table 取得（不是 window._dictTableName，那個可能是主檔）
        const container = document.querySelector('.multi-tab-detail .table-container[data-dict-table]');
        return (container?.dataset?.dictTable || window._dictTableName || '').trim();
    }

    async function handleFileUpload() {
        const paperNum = getPaperNum();
        const detailTable = getDetailTableName();
        if (!paperNum) {
            await Swal.fire({ icon: 'warning', title: '提示', text: '請先選擇單據' });
            return;
        }
        if (!detailTable) {
            await Swal.fire({ icon: 'warning', title: '提示', text: '無法取得明細表名稱' });
            return;
        }

        const fileInput = document.createElement('input');
        fileInput.type = 'file';
        fileInput.style.display = 'none';
        document.body.appendChild(fileInput);

        fileInput.addEventListener('change', async () => {
            if (!fileInput.files || fileInput.files.length === 0) return;

            const formData = new FormData();
            formData.append('File', fileInput.files[0]);
            formData.append('DetailTable', detailTable);
            formData.append('PaperNum', paperNum);

            try {
                const resp = await fetch('/api/CompanyAttachFile/DetailFileUpload', {
                    method: 'POST',
                    body: formData
                });
                const text = await resp.text();
                let result;
                try { result = JSON.parse(text); } catch { result = null; }

                if (resp.ok) {
                    await Swal.fire({ icon: 'success', title: '上傳成功', timer: 1500, showConfirmButton: false });
                    location.reload();
                } else {
                    await Swal.fire({ icon: 'error', title: '上傳失敗', text: result?.message || text || '未知錯誤' });
                }
            } catch (err) {
                await Swal.fire({ icon: 'error', title: '上傳失敗', text: err.message });
            } finally {
                if (fileInput.parentNode) document.body.removeChild(fileInput);
            }
        });

        fileInput.click();
    }

    async function handleFileView() {
        const paperNum = getPaperNum();
        const detailTable = getDetailTableName();
        if (!paperNum) {
            await Swal.fire({ icon: 'warning', title: '提示', text: '請先選擇單據' });
            return;
        }

        const activeRow = document.querySelector('.erp-table-wrapper tbody tr.row-selected');
        if (!activeRow) {
            await Swal.fire({ icon: 'info', title: '提示', text: '請先在明細檔中選擇一筆檔案' });
            return;
        }

        const getVal = (field) => {
            const cell = activeRow.querySelector(`td[data-field="${CSS.escape(field)}"]`);
            if (!cell) return '';
            const inp = cell.querySelector('input');
            return (inp?.value || cell.textContent || '').trim();
        };

        const fileName = getVal('FileName');
        if (!fileName) {
            await Swal.fire({ icon: 'info', title: '提示', text: '該筆資料無檔案名稱' });
            return;
        }

        // 用 detailTable + paperNum + fileName 開啟檔案
        window.open(`/api/CompanyAttachFile/DetailFileView?detailTable=${encodeURIComponent(detailTable)}&paperNum=${encodeURIComponent(paperNum)}&fileName=${encodeURIComponent(fileName)}`, '_blank');
    }

    // ==========================================
    // 初始化
    // ==========================================
    function init() {
        const form = document.querySelector('form.erp-header-form');
        if (!form) {
            setTimeout(init, 200);
            return;
        }

        fixLabels();
        addFileButtons();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => setTimeout(init, 100));
    } else {
        setTimeout(init, 100);
    }
})();
