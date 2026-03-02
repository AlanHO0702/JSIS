using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using System.Data;
using System.Collections.Generic;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MatInfoUtilityController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly string _connStr;

        public MatInfoUtilityController(PcbErpContext context, IConfiguration config)
        {
            _context = context;
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? _context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("Missing connection string.");
        }

        public sealed class SaveHeightRequest
        {
            public string ItemId { get; set; } = string.Empty;
            public int Height { get; set; }
        }

        public sealed class SpecActionRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string Revision { get; set; } = string.Empty;
            public int? SpecType { get; set; }
        }

        public sealed class UpdateCustPnRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string NewCustPn { get; set; } = string.Empty;
            public string NewCustEg { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        public sealed class CopyPnRequest
        {
            public string PartNum { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }

        public sealed class MatInfoUiConfigResponse
        {
            public string ItemId { get; set; } = string.Empty;
            public int PowerType { get; set; }
            public bool IsProduct { get; set; }
            public bool IsPurchase { get; set; }
            public string SpecTitleCaption { get; set; } = "規格表";
            public string SpecPrintButtonCaption { get; set; } = string.Empty;
            public string EngTabCaption { get; set; } = "工程圖";
            public bool UseSpecTab { get; set; } = true;
            public bool ProductHideUnitReplace { get; set; }
            public bool ProductHideCommonUse { get; set; }
            public bool ProductHidePacking { get; set; }
            public bool ProductHideKitItem { get; set; }
            public bool PurchaseHideSpec { get; set; }
            public bool PurchaseHidePacking { get; set; }
            public bool PurchaseHideKitItem { get; set; }
            public bool MBShowCustPNTabsheet { get; set; }
            public bool UseAVLTab { get; set; }
            public string CustDetailTableName { get; set; } = "MGNdCustPartNum";
            public string CustTabCaption { get; set; } = "客戶料號";
            public List<string> AllowedTabs { get; set; } = new();
        }

        [HttpGet]
        public async Task<IActionResult> BrowseHeight([FromQuery] string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return BadRequest("itemId is required.");

            const string sql = @"
SELECT TOP 1 DLLValue
  FROM CURdOCXItemOtherRule WITH (NOLOCK)
 WHERE ItemId = @ItemId
   AND RuleId = 'PaperMasterHeight';";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemId", itemId);
            await conn.OpenAsync();
            var val = await cmd.ExecuteScalarAsync();
            if (val == null || val == DBNull.Value)
                return Ok(new { height = (int?)null });

            if (int.TryParse(Convert.ToString(val), out var h))
                return Ok(new { height = h });

            return Ok(new { height = (int?)null });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBrowseHeight([FromBody] SaveHeightRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.ItemId))
                return BadRequest("itemId is required.");

            var safeItem = EscapeSqlLiteral(req.ItemId);
            var sql = $"exec CURdLayerHeightSave '{safeItem}', {req.Height}, 0";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> SpecDataSet([FromBody] SpecActionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");
            if (req.SpecType == null)
                return BadRequest("SpecType is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeRev = EscapeSqlLiteral(req.Revision ?? string.Empty);
            var sql = $"exec MGNdSpecDataSet '{safePart}', '{safeRev}', {req.SpecType}";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> SpecDataClear([FromBody] SpecActionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeRev = EscapeSqlLiteral(req.Revision ?? string.Empty);
            var sql = $"exec MGNdSpecDataClear '{safePart}', '{safeRev}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustPn([FromBody] UpdateCustPnRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safePn = EscapeSqlLiteral(req.NewCustPn ?? string.Empty);
            var safeUser = EscapeSqlLiteral(req.UserId ?? string.Empty);
            var safeEg = EscapeSqlLiteral(req.NewCustEg ?? string.Empty);

            var sql = $"exec MGNdMatInfoUpdateCustPn '{safePart}', '{safePn}', '{safeUser}', '{safeEg}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpPost]
        public async Task<IActionResult> CopyCustomerVersion([FromBody] CopyPnRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PartNum))
                return BadRequest("PartNum is required.");

            var safePart = EscapeSqlLiteral(req.PartNum);
            var safeUser = EscapeSqlLiteral(req.UserId ?? string.Empty);
            var sql = $"exec MGNdCopyPN4YX '{safePart}', '{safeUser}'";

            await using var conn = new SqlConnection(_connStr);
            await using var cmd = new SqlCommand(sql, conn);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            return Ok(new { ok = true });
        }

        [HttpGet]
        public async Task<IActionResult> UiConfig([FromQuery] string itemId)
        {
            var id = (itemId ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("itemId is required.");

            var powerType = id switch
            {
                "MG000008" or "CPN00006" => 2,
                "MG000002" or "CPN00007" => 4,
                _ => 2
            };

            var isPurchase = powerType == 4;
            var isProduct = powerType == 2;

            var mgnParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hideParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var itemRules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();

            await using (var cmd = new SqlCommand(@"
SELECT ParamId, ISNULL(Value,'') AS Value
  FROM CURdSysParams WITH (NOLOCK)
 WHERE SystemId = 'MGN';", conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                while (await rd.ReadAsync())
                {
                    var key = (rd["ParamId"]?.ToString() ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(key)) continue;
                    mgnParams[key] = (rd["Value"]?.ToString() ?? string.Empty).Trim();
                }
            }

            await using (var cmd = new SqlCommand("exec MGNdParamValueGet", conn))
            await using (var rd = await cmd.ExecuteReaderAsync())
            {
                if (await rd.ReadAsync())
                {
                    for (var i = 0; i < rd.FieldCount; i++)
                    {
                        var key = rd.GetName(i);
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        var value = rd.IsDBNull(i) ? string.Empty : (rd.GetValue(i)?.ToString() ?? string.Empty).Trim();
                        hideParams[key] = value;
                    }
                }
            }

            await using (var cmd = new SqlCommand(@"
SELECT RuleId, ISNULL(DLLValue,'') AS DLLValue
  FROM CURdOCXItemOtherRule WITH (NOLOCK)
 WHERE ItemId = @ItemId;", conn))
            {
                cmd.Parameters.AddWithValue("@ItemId", id);
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    var key = (rd["RuleId"]?.ToString() ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(key)) continue;
                    itemRules[key] = (rd["DLLValue"]?.ToString() ?? string.Empty).Trim();
                }
            }

            static bool IsOne(string? v) => string.Equals((v ?? string.Empty).Trim(), "1", StringComparison.OrdinalIgnoreCase);

            var useSpec = !mgnParams.ContainsKey("MGN-Main-UseSpec")
                || IsOne(mgnParams.TryGetValue("MGN-Main-UseSpec", out var useSpecVal) ? useSpecVal : "");
            var mbShowCust = IsOne(mgnParams.TryGetValue("MBShowCustPNTabsheet", out var mbShowVal) ? mbShowVal : "");
            var useAvl = IsOne(itemRules.TryGetValue("UseAVLTab", out var avlVal) ? avlVal : "");

            var productHideUnit = IsOne(hideParams.TryGetValue("ProductHideUnitReplace", out var v1) ? v1 : "");
            var productHideCommon = IsOne(hideParams.TryGetValue("ProductHideCommonUse", out var v2) ? v2 : "");
            var productHidePacking = IsOne(hideParams.TryGetValue("ProductHidePacking", out var v3) ? v3 : "");
            var productHideKit = IsOne(hideParams.TryGetValue("ProductHideKitItem", out var v4) ? v4 : "");
            var purchaseHideSpec = IsOne(hideParams.TryGetValue("PurchaseHideSpec", out var v5) ? v5 : "");
            var purchaseHidePacking = IsOne(hideParams.TryGetValue("PurchaseHidePacking", out var v6) ? v6 : "");
            var purchaseHideKit = IsOne(hideParams.TryGetValue("PurchaseHideKitItem", out var v7) ? v7 : "");

            var specTitle = (mgnParams.TryGetValue("SpecTitleCaption", out var specTitleVal) ? specTitleVal : "").Trim();
            if (string.IsNullOrWhiteSpace(specTitle))
                specTitle = (mgnParams.TryGetValue("MatInfo_ShtCaption2", out var specTitle2Val) ? specTitle2Val : "").Trim();
            if (string.IsNullOrWhiteSpace(specTitle)) specTitle = "規格表";
            var specPrintTitle = (mgnParams.TryGetValue("SpecPrintButtonCaption", out var printTitleVal) ? printTitleVal : "").Trim();
            var engCaption = (mgnParams.TryGetValue("MatInfo_ENGCaption", out var engCapVal) ? engCapVal : "").Trim();
            if (string.IsNullOrWhiteSpace(engCaption)) engCaption = "工程圖";

            var showCustTab = !isPurchase || mbShowCust || useAvl;
            var showSpecTab = useSpec && !(isPurchase && purchaseHideSpec);
            var showUnitTab = !(isProduct && productHideUnit);
            var showCommonTab = !(isProduct && productHideCommon);
            var showKitTab = !(isProduct && productHideKit) && !(isPurchase && purchaseHideKit);

            var allowed = new List<string> { "基本資料", "基本資料2", "屬性及備註", "工程圖", "圖檔" };
            if (showCustTab) allowed.Add(isPurchase ? "廠商料號" : "客戶料號");
            if (showSpecTab) allowed.Add(specTitle);
            if (showUnitTab) allowed.Add("替代單位設定");
            if (showCommonTab) allowed.Add("共用件");
            if (showKitTab) allowed.Add("組合商品");

            var response = new MatInfoUiConfigResponse
            {
                ItemId = id,
                PowerType = powerType,
                IsProduct = isProduct,
                IsPurchase = isPurchase,
                SpecTitleCaption = specTitle,
                SpecPrintButtonCaption = specPrintTitle,
                EngTabCaption = engCaption,
                UseSpecTab = useSpec,
                ProductHideUnitReplace = productHideUnit,
                ProductHideCommonUse = productHideCommon,
                ProductHidePacking = productHidePacking,
                ProductHideKitItem = productHideKit,
                PurchaseHideSpec = purchaseHideSpec,
                PurchaseHidePacking = purchaseHidePacking,
                PurchaseHideKitItem = purchaseHideKit,
                MBShowCustPNTabsheet = mbShowCust,
                UseAVLTab = useAvl,
                CustDetailTableName = isPurchase && useAvl ? "MGNdCustPartNum2" : "MGNdCustPartNum",
                CustTabCaption = isPurchase ? "廠商料號" : "客戶料號",
                AllowedTabs = allowed
            };

            return Ok(response);
        }

        private static string EscapeSqlLiteral(string input)
        {
            return (input ?? string.Empty).Replace("'", "''");
        }
    }
}
