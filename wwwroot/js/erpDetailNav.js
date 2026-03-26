/**
 * erpDetailNav.js — Detail 頁面 AJAX 導覽共用模組
 *
 * 用法：
 *   const nav = new DetailNavigator({
 *     tableName:      'EMOdProdECNMain',
 *     keyFields:      ['PaperNum'],
 *     detailPath:     '/EMOdProdECNMain/Detail',
 *     dataHandlerUrl: '/EMOdProdECNMain?handler=Data',
 *     extraParams:    { itemId: 'EMO00019' },
 *     currentKeys:    { PaperNum: 'ED26030001' },
 *     onNavigate:     async (keys) => { await loadMasterData(keys.PaperNum); }
 *   });
 *   nav.init();
 */
class DetailNavigator {
  /**
   * @param {Object} opts
   * @param {string}   opts.tableName      - 表名（用於 localStorage key）
   * @param {string[]} opts.keyFields      - 主鍵欄位名稱陣列
   * @param {string}   opts.detailPath     - Detail 頁面路徑（不含 query）
   * @param {string}   opts.dataHandlerUrl - Index handler=Data 的 URL（不含 pageIndex/pageSize）
   * @param {Object}   opts.extraParams    - URL 額外參數 (如 { itemId: 'EMO00019' })
   * @param {Object}   opts.currentKeys    - 目前這筆的主鍵值 (如 { PaperNum: 'ED26030001' })
   * @param {Function} opts.onNavigate     - async (keys) => {} — 切換記錄時的回呼
   */
  constructor(opts) {
    this.tableName      = opts.tableName;
    this.keyFields      = opts.keyFields || [];
    this.detailPath     = opts.detailPath;
    this.dataHandlerUrl = opts.dataHandlerUrl;
    this.extraParams    = opts.extraParams || {};
    this.currentKeys    = { ...opts.currentKeys };
    this.onNavigate     = opts.onNavigate;

    this._tn       = (this.tableName || '').toLowerCase();
    this._navKeys  = [];
    this._navIndex = -1;

    // 按鈕元素
    this._btnFirst = document.getElementById('btnFirst');
    this._btnPrev  = document.getElementById('btnPrev');
    this._btnNext  = document.getElementById('btnNext');
    this._btnLast  = document.getElementById('btnLast');
  }

  // ── 初始化：載入 key 清單 + 找出目前位置 ──────────────────────
  async init() {
    // 先用 localStorage 立即顯示計數
    const el = document.getElementById('toolbarHeaderCount');
    const si = localStorage.getItem(`orderListQueryIndex:${this._tn}`);
    const st = localStorage.getItem(`orderListQueryTotal:${this._tn}`);
    if (el) el.textContent = (si && st) ? `${si} / ${st}` : '載入中...';

    try {
      const sep = this.dataHandlerUrl.includes('?') ? '&' : '?';
      const url = `${this.dataHandlerUrl}${sep}pageIndex=1&pageSize=999999`;
      const res = await fetch(url);
      if (!res.ok) throw new Error('fetch failed');
      const json = await res.json();
      const rows = Array.isArray(json.items) ? json.items
                 : (Array.isArray(json) ? json : []);

      // 建立 key 清單
      this._navKeys = rows.map(r => {
        const keyObj = {};
        this.keyFields.forEach(kf => {
          keyObj[kf] = (r[kf] || r[kf.toLowerCase()] || r[kf.charAt(0).toLowerCase() + kf.slice(1)] || '').toString().trim();
        });
        return keyObj;
      }).filter(k => {
        // 至少一個 key 有值
        return this.keyFields.some(kf => k[kf]);
      });

      // 找出目前這筆的位置
      this._navIndex = this._navKeys.findIndex(k =>
        this.keyFields.every(kf =>
          (k[kf] || '').toLowerCase() === (this.currentKeys[kf] || '').toLowerCase()
        )
      );

      this._updateNavButtons();
    } catch (e) {
      console.error('DetailNavigator init failed:', e);
      if (this._btnPrev) this._btnPrev.disabled = true;
      if (this._btnNext) this._btnNext.disabled = true;
      this._updateHeaderCount();
    }
  }

  // ── AJAX 切換記錄 ─────────────────────────────────────────
  async gotoRecord(key, targetIndex) {
    if (!key) return;

    // 更新 localStorage
    if (targetIndex !== undefined && this._navKeys.length > 0) {
      localStorage.setItem(`orderListQueryIndex:${this._tn}`, targetIndex + 1);
      localStorage.setItem(`orderListQueryTotal:${this._tn}`, this._navKeys.length);
    }

    // 組裝新 URL
    const params = new URLSearchParams();
    this.keyFields.forEach(kf => {
      if (key[kf]) params.append(kf, key[kf]);
    });
    Object.entries(this.extraParams).forEach(([k, v]) => {
      params.append(k, v);
    });
    const newUrl = `${this.detailPath}?${params.toString()}`;
    history.pushState(null, '', newUrl);

    // 更新內部狀態
    this.keyFields.forEach(kf => {
      this.currentKeys[kf] = key[kf] || '';
    });
    this._navIndex = targetIndex;

    // 呼叫頁面的 onNavigate callback
    if (typeof this.onNavigate === 'function') {
      await this.onNavigate(key);
    }

    this._updateNavButtons();
  }

  // ── 更新按鈕狀態 ─────────────────────────────────────────
  _updateNavButtons() {
    const hasPrev = this._navIndex > 0;
    const hasNext = this._navIndex >= 0 && this._navIndex < this._navKeys.length - 1;

    if (this._btnFirst) {
      this._btnFirst.disabled = !hasPrev;
      this._btnFirst.onclick = () => hasPrev && this.gotoRecord(this._navKeys[0], 0);
    }
    if (this._btnPrev) {
      this._btnPrev.disabled = !hasPrev;
      this._btnPrev.onclick = () => hasPrev && this.gotoRecord(this._navKeys[this._navIndex - 1], this._navIndex - 1);
    }
    if (this._btnNext) {
      this._btnNext.disabled = !hasNext;
      this._btnNext.onclick = () => hasNext && this.gotoRecord(this._navKeys[this._navIndex + 1], this._navIndex + 1);
    }
    if (this._btnLast) {
      this._btnLast.disabled = !hasNext;
      this._btnLast.onclick = () => hasNext && this.gotoRecord(this._navKeys[this._navKeys.length - 1], this._navKeys.length - 1);
    }

    this._updateHeaderCount();
  }

  // ── 更新計數器 ───────────────────────────────────────────
  _updateHeaderCount() {
    const el = document.getElementById('toolbarHeaderCount');
    if (!el) return;
    if (this._navKeys.length > 0 && this._navIndex >= 0) {
      el.textContent = `${this._navIndex + 1} / ${this._navKeys.length}`;
    } else {
      const si = parseInt(localStorage.getItem(`orderListQueryIndex:${this._tn}`) || '0', 10);
      const st = parseInt(localStorage.getItem(`orderListQueryTotal:${this._tn}`) || '0', 10);
      el.textContent = st > 0 ? `${si || '--'} / ${st}` : '-- / --';
    }
  }

  // ── 編輯模式：禁用/啟用導覽按鈕 ─────────────────────────────
  setEditMode(editing) {
    [this._btnFirst, this._btnPrev, this._btnNext, this._btnLast].forEach(btn => {
      if (btn) btn.disabled = editing;
    });
  }

  // ── 取得目前 navIndex（供外部使用） ─────────────────────────
  get index() { return this._navIndex; }
  set index(val) { this._navIndex = val; }

  get keys() { return this._navKeys; }

  // ── 新增記錄後重新初始化 ─────────────────────────────────
  async reinit() {
    await this.init();
  }
}