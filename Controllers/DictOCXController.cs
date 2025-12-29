using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpApi.Models;

[Route("api/[controller]/[action]")]
public class DictOCXController : Controller
{
    private readonly PcbErpContext _ctx;

    public DictOCXController(PcbErpContext ctx)
    {
        _ctx = ctx;
    }

    private string ResolveUseId()
    {
        // 優先從 Claims 取 UseId；沒有登入資訊時用系統預設值，避免 DB NOT NULL 失敗
        var claim =
            User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "UseId", StringComparison.OrdinalIgnoreCase))
            ?? User?.Claims?.FirstOrDefault(c => string.Equals(c.Type, "useid", StringComparison.OrdinalIgnoreCase));

        var useId = claim?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(useId)) return useId;

        var name = User?.Identity?.Name?.Trim();
        if (!string.IsNullOrWhiteSpace(name)) return name;

        return "A001";
    }

    // 取得 OCX 設定
    [HttpGet]
    public IActionResult GetOCX(string table, string field)
    {
        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        var tname = Clean(table);
        var fname = Clean(field);

        var f = _ctx.CURdTableFields.FirstOrDefault(x =>
            x.TableName != null
            && (x.TableName.ToLower() == tname || x.TableName.ToLower().Replace("dbo.", "") == tname)
            && x.FieldName != null
            && x.FieldName.ToLower() == fname);

        var maps = _ctx.CURdOCXTableFieldLK
            .Where(x =>
                x.TableName != null
                && (x.TableName.ToLower() == tname || x.TableName.ToLower().Replace("dbo.", "") == tname)
                && x.FieldName != null
                && x.FieldName.ToLower() == fname)
            .OrderBy(x => x.KeyFieldName)
            .ThenBy(x => x.KeySelfName)
            .Select(x => new { x.KeyFieldName, x.KeySelfName })
            .ToList();

        var lk = maps.FirstOrDefault();

        return Ok(new
        {
            success = true,
            ocxTable = f?.OCXLKTableName ?? "",
            ocxResult = f?.OCXLKResultName ?? "",
            keyField = lk?.KeyFieldName ?? "",
            keySelf = lk?.KeySelfName ?? field,
            maps
        });
    }

    // 儲存 OCX 設定（獨立、不動原本 Save）
    [HttpPost]
    public IActionResult SaveOCX([FromQuery] string table, [FromQuery] string field,
                                 [FromBody] OCXDto dto)
    {
        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        var tname = Clean(table);
        var fname = Clean(field);

        var f = _ctx.CURdTableFields.First(x =>
            x.TableName != null
            && (x.TableName.ToLower() == tname || x.TableName.ToLower().Replace("dbo.", "") == tname)
            && x.FieldName != null
            && x.FieldName.ToLower() == fname);
        f.OCXLKTableName = dto.OCXTable;
        f.OCXLKResultName = dto.OCXResult;

        // 兼容舊版：僅保留一筆 key mapping
        var exist = _ctx.CURdOCXTableFieldLK
            .Where(x =>
                x.TableName != null
                && (x.TableName.ToLower() == tname || x.TableName.ToLower().Replace("dbo.", "") == tname)
                && x.FieldName != null
                && x.FieldName.ToLower() == fname)
            .ToList();
        if (exist.Count > 0) _ctx.CURdOCXTableFieldLK.RemoveRange(exist);

        var kf = (dto.KeyField ?? "").Trim();
        var ks = (dto.KeySelf ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(kf))
        {
            if (string.IsNullOrWhiteSpace(ks)) ks = field;
            var useId = ResolveUseId();
            _ctx.CURdOCXTableFieldLK.Add(new CURdOCXTableFieldLK
            {
                TableName = f.TableName,
                FieldName = f.FieldName,
                KeyFieldName = kf,
                KeySelfName = ks,
                UseId = useId
            });
        }

        _ctx.SaveChanges();

        return Ok(new { success = true });
    }

    [HttpPost]
    public IActionResult SaveOCXMapsBatch([FromBody] List<SaveOCXMapsInput> list)
    {
        if (list == null) return BadRequest(new { success = false, message = "Invalid request" });

        static string Clean(string s) => (s ?? "")
            .Trim().Trim('[', ']')
            .Replace("dbo.", "", StringComparison.OrdinalIgnoreCase)
            .ToLowerInvariant();

        var useId = ResolveUseId();

        foreach (var item in list)
        {
            var tname = Clean(item.TableName);
            var fname = Clean(item.FieldName);
            if (string.IsNullOrWhiteSpace(tname) || string.IsNullOrWhiteSpace(fname)) continue;

            // 找到實際 TableName/FieldName（保留原大小寫/格式）
            var tf = _ctx.CURdTableFields.FirstOrDefault(x =>
                x.TableName != null
                && (x.TableName.ToLower() == tname || x.TableName.ToLower().Replace("dbo.", "") == tname)
                && x.FieldName != null
                && x.FieldName.ToLower() == fname);
            if (tf == null) continue;

            var existing = _ctx.CURdOCXTableFieldLK
                .Where(x => x.TableName == tf.TableName && x.FieldName == tf.FieldName)
                .ToList();
            if (existing.Count > 0) _ctx.CURdOCXTableFieldLK.RemoveRange(existing);

            var maps = item.Maps ?? new();
            foreach (var m in maps)
            {
                var kf = (m.KeyFieldName ?? "").Trim();
                var ks = (m.KeySelfName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(kf) && string.IsNullOrWhiteSpace(ks)) continue;
                if (string.IsNullOrWhiteSpace(kf) || string.IsNullOrWhiteSpace(ks)) continue;

                _ctx.CURdOCXTableFieldLK.Add(new CURdOCXTableFieldLK
                {
                    TableName = tf.TableName,
                    FieldName = tf.FieldName,
                    KeyFieldName = kf,
                    KeySelfName = ks,
                    UseId = useId
                });
            }
        }

        _ctx.SaveChanges();
        return Ok(new { success = true });
    }

    public class OCXDto
    {
        public string OCXTable { get; set; } = "";
        public string OCXResult { get; set; } = "";
        public string KeyField { get; set; } = "";
        public string KeySelf { get; set; } = "";
        
    }

    public class SaveOCXMapsInput
    {
        public string TableName { get; set; } = "";
        public string FieldName { get; set; } = "";
        public List<SaveOCXMapsPair> Maps { get; set; } = new();
    }

    public class SaveOCXMapsPair
    {
        public string KeyFieldName { get; set; } = "";
        public string KeySelfName { get; set; } = "";
    }
}
