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

    // 取得 OCX 設定
    [HttpGet]
    public IActionResult GetOCX(string table, string field)
    {
        var f = _ctx.CURdTableFields.FirstOrDefault(x => x.TableName == table && x.FieldName == field);
        var lk = _ctx.CURdOCXTableFieldLK.FirstOrDefault(x => x.TableName == table && x.FieldName == field);

        return Ok(new
        {
            success = true,
            ocxTable = f?.OCXLKTableName ?? "",
            ocxResult = f?.OCXLKResultName ?? "",
            keyField = lk?.KeyFieldName ?? "",
            keySelf = lk?.KeySelfName ?? field
        });
    }

    // 儲存 OCX 設定（獨立、不動原本 Save）
    [HttpPost]
    public IActionResult SaveOCX([FromQuery] string table, [FromQuery] string field,
                                 [FromBody] OCXDto dto)
    {
        var f = _ctx.CURdTableFields.First(x => x.TableName == table && x.FieldName == field);
        f.OCXLKTableName = dto.OCXTable;
        f.OCXLKResultName = dto.OCXResult;

        var lk = _ctx.CURdOCXTableFieldLK
            .FirstOrDefault(x => x.TableName == table && x.FieldName == field);

        if (lk == null)
        {
            lk = new CURdOCXTableFieldLK
            {
                TableName = table,
                FieldName = field
            };
            _ctx.CURdOCXTableFieldLK.Add(lk);
        }

        lk.KeyFieldName = dto.KeyField;
        lk.KeySelfName = dto.KeySelf;

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
}
