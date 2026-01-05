using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

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

        [HttpGet("OcxTempleteOptions")]
        public async Task<IActionResult> GetOcxTempleteOptions()
        {
            await using var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "select OCXTemplete, OCXTempleteName from CURdOCXTemplete";

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
    }
}
