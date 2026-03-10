using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Data;
using System.Data.Common;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => /api/EditItemApi
    public class EditItemApiController : ControllerBase
    {
        private readonly PcbErpContext _context;
        public EditItemApiController(PcbErpContext context) => _context = context;

        public class EditItemDto
        {
            public string ItemId { get; set; } = "";
            public string ItemName { get; set; } = "";
            public string SuperId { get; set; } = "";
            public int? ItemType { get; set; }
            public int? OutputType { get; set; }
            public string? ObjectName { get; set; }
            public string? ClassName { get; set; }
            public int SerialNum { get; set; }
            public int Enabled { get; set; }   // 0=停用, 1=啟用
            public string Notes { get; set; } = "";
            public string Ocxtemplete { get; set; } = "";
            public string SWebMenuId { get; set; } = "";
            public string SWebSuperMenuId { get; set; } = "";
            public long? IWebMenuOrderSeq { get; set; }
            public int? IWebMenuLevel { get; set; }
            public int? IWebEnable { get; set; }
        }

        public class AddonParamDto
        {
            public string ParamName { get; set; } = "";
            public string? DisplayName { get; set; }
            public int? ControlType { get; set; }
            public string? CommandText { get; set; }
            public string? DefaultValue { get; set; }
            public int DefaultType { get; set; }
            public string? EditMask { get; set; }
            public string? SuperId { get; set; }
            public int ParamSn { get; set; }
            public string? DisplayNameCn { get; set; }
            public string? DisplayNameEn { get; set; }
            public string? DisplayNameJp { get; set; }
            public string? DisplayNameTh { get; set; }
        }

        public class AddonParamsSaveDto
        {
            public string ItemId { get; set; } = "";
            public List<AddonParamDto> Params { get; set; } = new();
        }

        public class OcxTempleteOption
        {
            public string OcxTemplete { get; set; } = "";
            public string OcxTempleteName { get; set; } = "";
        }

        public class IntValueTextOption
        {
            public int Value { get; set; }
            public string Text { get; set; } = "";
        }

        [HttpGet("OcxTempleteOptions")]
        public async Task<IActionResult> GetOcxTempleteOptions()
        {
            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select * from CURdOCXTemplete(nolock) order by OCXTemplete";

            var list = new List<OcxTempleteOption>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new OcxTempleteOption
                {
                    OcxTemplete = reader["OCXTemplete"]?.ToString() ?? "",
                    OcxTempleteName = reader["OCXTempleteName"]?.ToString() ?? ""
                });
            }

            return Ok(list);
        }

        [HttpGet("ItemTypeOptions")]
        public async Task<IActionResult> GetItemTypeOptions()
        {
            var list = await LoadIntOptionsAsync(
                "CURdLinkType",
                new[] { "ItemType", "LinkType", "TypeId", "SerialNum", "Id" },
                new[] { "ItemTypeName", "LinkTypeName", "TypeName", "DisplayName", "ItemName", "Name", "Notes" });
            return Ok(list);
        }

        [HttpGet("OutputTypeOptions")]
        public async Task<IActionResult> GetOutputTypeOptions()
        {
            var list = await LoadIntOptionsAsync(
                "CURdOutputType",
                new[] { "OutputType", "TypeId", "SerialNum", "Id" },
                new[] { "OutputTypeName", "TypeName", "DisplayName", "ItemName", "Name", "Notes" });
            return Ok(list);
        }

        public class UpdateOcxTemplateDto
        {
            public string ItemId { get; set; } = "";
            public string OcxTemplete { get; set; } = "";
        }

        [HttpPost("UpdateOcxTemplete")]
        public async Task<IActionResult> UpdateOcxTemplete([FromBody] UpdateOcxTemplateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                return BadRequest(new { success = false, message = "ItemId 不可空白" });

            var item = await _context.CurdSysItems.FirstOrDefaultAsync(x => x.ItemId == dto.ItemId.Trim());
            if (item == null)
                return NotFound(new { success = false, message = "找不到項目" });

            item.Ocxtemplete = dto.OcxTemplete?.Trim() ?? string.Empty;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EditItemDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                return BadRequest(new { success = false, message = "ItemId 不可空白" });

            var item = await _context.CurdSysItems
                .FirstOrDefaultAsync(x => x.ItemId == dto.ItemId);

            if (item == null)
                return NotFound(new { success = false, message = "找不到項目" });

            item.ItemName  = dto.ItemName?.Trim();
            item.SuperId   = dto.SuperId?.Trim();
            if (dto.ItemType.HasValue) item.ItemType = dto.ItemType.Value;
            if (dto.OutputType.HasValue) item.OutputType = dto.OutputType.Value;
            if (dto.ObjectName != null)
                item.ObjectName = string.IsNullOrWhiteSpace(dto.ObjectName) ? null : dto.ObjectName.Trim();
            if (dto.ClassName != null)
                item.ClassName = string.IsNullOrWhiteSpace(dto.ClassName) ? null : dto.ClassName.Trim();
            item.SerialNum = dto.SerialNum;
            item.Enabled   = dto.Enabled;
            item.Notes   = dto.Notes?.Trim();
            item.Ocxtemplete = dto.Ocxtemplete?.Trim();
            item.SWebMenuId = string.IsNullOrWhiteSpace(dto.SWebMenuId) ? null : dto.SWebMenuId.Trim();
            item.SWebSuperMenuId = string.IsNullOrWhiteSpace(dto.SWebSuperMenuId) ? null : dto.SWebSuperMenuId.Trim();
            item.IWebMenuOrderSeq = dto.IWebMenuOrderSeq;
            item.IWebMenuLevel = dto.IWebMenuLevel;
            item.IWebEnable = dto.IWebEnable;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpGet("AddonParams/{itemId}")]
        public async Task<IActionResult> GetAddonParams(string itemId)
        {
            var list = await _context.CurdAddonParams
                .Where(p => p.ItemId == itemId)
                .OrderBy(p => p.ParamSn)
                .ToListAsync();
            return Ok(list);
        }

        public class ExtractParamsDto
        {
            public string ItemId { get; set; } = "";
            public string ClassName { get; set; } = "";
        }

        [HttpPost("AddonParams/Extract")]
        public async Task<IActionResult> ExtractAddonParams([FromBody] ExtractParamsDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                return BadRequest(new { success = false, message = "ItemId 不可空白" });

            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "exec CURdSetAddOnParams @P1, @P2";
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@P1"; p1.Value = dto.ItemId.Trim(); cmd.Parameters.Add(p1);
            // ClassName 去掉 .rpt 副檔名後傳入
            var className = (dto.ClassName ?? "").Trim();
            if (className.EndsWith(".rpt", StringComparison.OrdinalIgnoreCase))
                className = className[..^4];
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@P2"; p2.Value = className; cmd.Parameters.Add(p2);
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { success = true });
        }

        [HttpPost("AddonParams/Save")]
        public async Task<IActionResult> SaveAddonParams([FromBody] AddonParamsSaveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ItemId))
                return BadRequest(new { success = false, message = "ItemId 不可空白" });

            var existing = await _context.CurdAddonParams
                .Where(p => p.ItemId == dto.ItemId)
                .ToListAsync();

            var incomingNames = dto.Params.Select(p => p.ParamName).ToHashSet();

            // 刪除已移除的參數
            var toDelete = existing.Where(e => !incomingNames.Contains(e.ParamName)).ToList();
            _context.CurdAddonParams.RemoveRange(toDelete);

            // 更新或新增
            foreach (var p in dto.Params)
            {
                var ex = existing.FirstOrDefault(e => e.ParamName == p.ParamName);
                if (ex != null)
                {
                    ex.DisplayName = p.DisplayName;
                    ex.ControlType = p.ControlType;
                    ex.CommandText = p.CommandText;
                    ex.DefaultValue = p.DefaultValue;
                    ex.DefaultType = p.DefaultType;
                    ex.EditMask = p.EditMask;
                    ex.SuperId = p.SuperId;
                    ex.ParamSn = p.ParamSn;
                    ex.DisplayNameCn = p.DisplayNameCn;
                    ex.DisplayNameEn = p.DisplayNameEn;
                    ex.DisplayNameJp = p.DisplayNameJp;
                    ex.DisplayNameTh = p.DisplayNameTh;
                }
                else
                {
                    _context.CurdAddonParams.Add(new CurdAddonParam
                    {
                        ItemId = dto.ItemId,
                        ParamName = p.ParamName,
                        DisplayName = p.DisplayName,
                        ControlType = p.ControlType,
                        CommandText = p.CommandText,
                        DefaultValue = p.DefaultValue,
                        DefaultType = p.DefaultType,
                        EditMask = p.EditMask,
                        SuperId = p.SuperId,
                        ParamSn = p.ParamSn,
                        DisplayNameCn = p.DisplayNameCn,
                        DisplayNameEn = p.DisplayNameEn,
                        DisplayNameJp = p.DisplayNameJp,
                        DisplayNameTh = p.DisplayNameTh
                    });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, count = dto.Params.Count });
        }

        private async Task<List<IntValueTextOption>> LoadIntOptionsAsync(
            string tableName,
            string[] valueCandidates,
            string[] textCandidates)
        {
            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from {tableName}(nolock)";

            var list = new List<IntValueTextOption>();
            var seen = new HashSet<int>();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (!reader.HasRows) return list;

            var valueOrdinal = ResolveValueOrdinal(reader, valueCandidates);
            var textOrdinal = ResolveTextOrdinal(reader, textCandidates, valueOrdinal);

            while (await reader.ReadAsync())
            {
                if (!TryGetInt(reader, valueOrdinal, out var value)) continue;

                var text = string.Empty;
                if (textOrdinal >= 0 && textOrdinal < reader.FieldCount && !reader.IsDBNull(textOrdinal))
                    text = reader.GetValue(textOrdinal)?.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(text))
                    text = value.ToString();

                if (seen.Add(value))
                    list.Add(new IntValueTextOption { Value = value, Text = text });
            }

            return list.OrderBy(x => x.Value).ToList();
        }

        private static int ResolveValueOrdinal(DbDataReader reader, IReadOnlyCollection<string> candidates)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (candidates.Any(c => string.Equals(c, reader.GetName(i), StringComparison.OrdinalIgnoreCase)))
                    return i;
            }

            for (var i = 0; i < reader.FieldCount; i++)
            {
                var t = reader.GetFieldType(i);
                if (t == typeof(byte) || t == typeof(short) || t == typeof(int) || t == typeof(long) ||
                    t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                    return i;
            }

            return 0;
        }

        private static int ResolveTextOrdinal(DbDataReader reader, IReadOnlyCollection<string> candidates, int valueOrdinal)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (i == valueOrdinal) continue;
                if (candidates.Any(c => string.Equals(c, reader.GetName(i), StringComparison.OrdinalIgnoreCase)))
                    return i;
            }

            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (i == valueOrdinal) continue;
                var t = reader.GetFieldType(i);
                if (t == typeof(string))
                    return i;
            }

            return -1;
        }

        private static bool TryGetInt(DbDataReader reader, int ordinal, out int value)
        {
            value = 0;
            if (ordinal < 0 || ordinal >= reader.FieldCount || reader.IsDBNull(ordinal))
                return false;

            var raw = reader.GetValue(ordinal);
            try
            {
                switch (raw)
                {
                    case byte b: value = b; return true;
                    case short s: value = s; return true;
                    case int i: value = i; return true;
                    case long l when l >= int.MinValue && l <= int.MaxValue:
                        value = (int)l; return true;
                    case decimal d:
                        value = decimal.ToInt32(d); return true;
                    case double db:
                        value = Convert.ToInt32(db); return true;
                    case float f:
                        value = Convert.ToInt32(f); return true;
                    default:
                        return int.TryParse(raw.ToString(), out value);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
