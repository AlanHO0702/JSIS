window.CcsCompanyActions = (function () {
  function withJwtHeaders(init) {
    const req = init || {};
    const jwt = localStorage.getItem("jwtId");
    const headers = Object.assign({}, req.headers || {});
    if (jwt) headers["X-JWTID"] = jwt;
    return Object.assign({}, req, { headers });
  }

  function getCompanyIdFromRow(row) {
    const data = row || {};
    const keys = Object.keys(data);
    const idKey = keys.find(k => String(k).toLowerCase() === "companyid");
    return idKey ? String(data[idKey] || "").trim() : "";
  }

  async function postJson(url, payload) {
    const resp = await fetch(url, withJwtHeaders({
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload || {})
    }));
    const json = await resp.json().catch(() => ({}));
    if (!resp.ok || json?.success === false) {
      throw new Error(json?.message || resp.statusText || "request failed");
    }
    return json;
  }

  async function deleteSystemLink(options) {
    const opt = options || {};
    const companyId = String(opt.companyId || "").trim();
    if (!companyId) throw new Error("尚未選取公司代碼");
    const payload = {
      companyId,
      itemId: String(opt.itemId || "").trim(),
      systemId: Number(opt.systemId || 0) || null
    };
    return await postJson("/api/CCSCompanyAdd/delete-system-link", payload);
  }

  async function backReview(options) {
    const opt = options || {};
    const companyId = String(opt.companyId || "").trim();
    if (!companyId) throw new Error("尚未選取公司代碼");
    const payload = {
      companyId,
      itemId: String(opt.itemId || "").trim(),
      systemId: Number(opt.systemId || 0) || null,
      userId: String(opt.userId || "").trim(),
      rejectNotes: String(opt.rejectNotes || "")
    };
    return await postJson("/api/CCSCompanyAdd/back-review", payload);
  }

  return {
    withJwtHeaders,
    getCompanyIdFromRow,
    postJson,
    deleteSystemLink,
    backReview
  };
})();

