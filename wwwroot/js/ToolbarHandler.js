// 共用封鎖狀態 (全域一次就好)
window.BLOCK_STATUSES = ['已確認', '已結案', '已作廢'];
class ToolbarHandler {
    constructor(opts) {
        this.searchBtnId = opts.searchBtnId;
        this.addBtnId = opts.addBtnId;
        this.deleteBtnId = opts.deleteBtnId;

        this.modalId = opts.modalId;
        this.formId = opts.formId;
        this.addApiUrl = opts.addApiUrl;
        this.deleteApiUrlFn = opts.deleteApiUrlFn; // 用 function 傳 id
        this.detailRouteTemplate = opts.detailRouteTemplate;
        this.tableName = opts.tableName;
        this.pageSize = opts.pageSize || 50;
        this.renderTable = opts.renderTable;
        this.renderPagination = opts.renderPagination;
        this.renderOrderCount = opts.renderOrderCount;
        this.restoreSearchForm = opts.restoreSearchForm;
        this.lastQueryFilters = [];
        this.getSelectedId = opts.getSelectedId || (() => window.selectedPaperNum || null);
        this.queryRedirectUrl = opts.queryRedirectUrl || null;
    
        this.init();
    }

    init() {
        document.getElementById(this.searchBtnId).onclick = () => {
            var modal = new bootstrap.Modal(document.getElementById(this.modalId));
            modal.show();
            setTimeout(() => {
                this.restoreSearchForm(this.lastQueryFilters);
            }, 100);
        };

        document.getElementById(this.addBtnId).onclick = async () => {
            const result = await Swal.fire({
                title: '確定要新增一張新單據？',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            });
            if (!result.isConfirmed) return;

            const resp = await fetch(this.addApiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            });

            if (resp.ok) {
                const data = await resp.json();
                const paperNum = data.paperNum || data.PaperNum || "";
                if (paperNum) {
                    var jumpUrl = this.detailRouteTemplate.replace("{PaperNum}", paperNum);
                    window.location = jumpUrl;
                } else {
                    await Swal.fire({ icon: 'success', title: '新增成功，但未取得單號！' });
                    location.reload();
                }
            } else {
                await Swal.fire({ icon: 'error', title: '建立失敗！' });
            }
        };

        document.getElementById(this.deleteBtnId).onclick = async () => {
            const selectedId = this.getSelectedId();
            if (!selectedId) {
                await Swal.fire({
                    icon: 'warning',
                    title: '請先點選一筆資料',
                    confirmButtonText: '確定'
                });
                return;
            }
            const result = await Swal.fire({
                title: '確定要作廢這筆單據？',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            });
            if (!result.isConfirmed) return;

            let resp = await fetch(this.deleteApiUrlFn(selectedId), { method: "DELETE" });
            if (resp.ok) {
                await Swal.fire({ icon: 'success', title: '作廢成功！' });
                window.location.href = this.queryRedirectUrl;
            } else {
                let errorMsg = "作廢失敗！";
                try {
                    let result = await resp.json();
                    if (result.error) errorMsg = result.error;
                } catch (e) { }
                await Swal.fire({ icon: 'error', title: errorMsg });
            }
        };

        // 綁定查詢 submit
        document.getElementById(this.formId).onsubmit = async (e) => {
            e.preventDefault();
            let filters = [];
            document.querySelectorAll(`#${this.formId} [name]`).forEach((el) => {
                let name = el.name;
                if (name.startsWith('Cond_') || name === "__RequestVerificationToken") return;
                let condName = 'Cond_' + name;
                let cond = document.querySelector(`#${this.formId} [name='${condName}']`);
                let op = cond ? cond.value : '';
                let value = el.value;
                if (value) {
                    filters.push({
                        Field: name.replace(/\d+$/, ""),
                        Op: op,
                        Value: value
                    });
                }
            });
            filters.push({ Field: "page", Op: "", Value: String(1) });
            filters.push({ Field: "pageSize", Op: "", Value: String(this.pageSize) });

            // **這裡最重要：查詢送出就更新狀態！**
            this.lastQueryFilters = filters.slice();   // <-- 每次查詢都即時更新

            // 2. 立刻寫入 localStorage
            localStorage.setItem("orderListQueryFilters", JSON.stringify(filters));
            localStorage.setItem("orderListPageNumber", "1");

            // ⏩ 如果有 queryRedirectUrl 就跳頁！**
            if (this.queryRedirectUrl) {
                // 將 filters 轉成 query string
                const params = new URLSearchParams();
                filters.forEach(f => {
                    // 避免 page/pageSize、空值
                    if (["page", "pageSize"].includes(f.Field)) return;
                    if (f.Value) params.append(f.Field, f.Value);
                });
                window.location.href = this.queryRedirectUrl + '?' + params.toString();
                return;
            }

            const postBody = {
                table: this.tableName,
                filters: filters
            };
            // 如果 API 真的要求要包一層 request，就：
            // const postBody = { request: { table: ..., filters: ... } };

            const resp = await fetch('/api/PagedQuery/PagedQuery', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(postBody)
            });

            if (!resp.ok) {
                Swal.fire({ icon: 'error', title: '查詢失敗' });
                return;
            }
            const result = await resp.json();
            window._lookupMapData = result.lookupMapData;   // <--- 必加這行
            if (this.renderTable) this.renderTable(result.data);
            if (this.renderPagination) this.renderPagination(result.totalCount, this.pageSize, 1);
            if (this.renderOrderCount) this.renderOrderCount(result.totalCount, this.pageSize, 1);
            bootstrap.Modal.getOrCreateInstance(document.getElementById(this.modalId)).hide();
        };

    };

}
