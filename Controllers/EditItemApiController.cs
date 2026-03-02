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
