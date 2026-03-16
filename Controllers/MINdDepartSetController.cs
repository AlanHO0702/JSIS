using System.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MINdDepartSetController : ControllerBase
    {
        private readonly string _connStr;
        private readonly ILogger<MINdDepartSetController> _logger;

        public MINdDepartSetController(IConfiguration cfg, PcbErpContext db, ILogger<MINdDepartSetController> logger)
        {
            _connStr = cfg.GetConnectionString("Default")
                       ?? db?.Database.GetDbConnection().ConnectionString
                       ?? throw new InvalidOperationException("Missing connection string.");
            _logger = logger;
        }

        public record IdNameDto(string Id, string Name);
        public class DepartPartResponse
        {
            [JsonPropertyName("available")]
            public List<IdNameDto> Available { get; init; } = new();

            [JsonPropertyName("chosen")]
            public List<IdNameDto> Chosen { get; init; } = new();
        }
        public record SaveDepartRequest(string DepartId, List<string>? PartNums);
        public record SavePartRequest(string PartNum, List<string>? DepartIds);
        public record SaveMatClassRequest(string MatClass, List<string>? DepartIds);

        [HttpGet("depart/{departId}/parts")]
        public async Task<ActionResult<DepartPartResponse>> GetPartsByDepart(
            string departId,
            [FromQuery] string? matClass,
            [FromQuery] string? partNum)
        {
            if (string.IsNullOrWhiteSpace(departId))
                return BadRequest(new { ok = false, error = "DepartId is required." });
            try
            {
                var available = await QueryListByProcAsync(
                    "MINdDepartSetClose",
                    r => MapIdName(r,
                        new[] { "PartNum", "Partnum", "ItemId" },
                        new[] { "MatName", "PartName", "ItemName", "Name" }),
                    new SqlParameter("@MatClass", SqlDbType.VarChar, 12) { Value = (matClass ?? string.Empty).Trim() },
                    new SqlParameter("@Partnum", SqlDbType.VarChar, 24) { Value = (partNum ?? string.Empty).Trim() }
                );

                var chosen = await QueryListByProcAsync(
                    "MINdDepartSetAddP",
                    r => MapIdName(r,
                        new[] { "PartNum", "Partnum", "ItemId" },
                        new[] { "MatName", "PartName", "ItemName", "Name" }),
                    new SqlParameter("@PartNum", SqlDbType.VarChar, 24) { Value = string.Empty },
                    new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = departId.Trim() }
                );

                var normalizedChosen = chosen.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                var chosenSet = normalizedChosen.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var normalizedAvailable = available.Where(x => !string.IsNullOrWhiteSpace(x.Id) && !chosenSet.Contains(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                return Ok(new DepartPartResponse
                {
                    Available = normalizedAvailable,
                    Chosen = normalizedChosen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPartsByDepart failed. DepartId={DepartId}, MatClass={MatClass}, PartNum={PartNum}",
                    departId, matClass, partNum);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("part/{partNum}/departments")]
        public async Task<ActionResult<DepartPartResponse>> GetDepartmentsByPart(string partNum)
        {
            if (string.IsNullOrWhiteSpace(partNum))
                return BadRequest(new { ok = false, error = "PartNum is required." });
            try
            {
                await using var conn = new SqlConnection(_connStr);
                await conn.OpenAsync();

                // Align legacy order/session: MINdDepartSetAddD -> MINdPartSetClose
                var chosen = await QueryListByProcAsync(
                    conn,
                    "MINdDepartSetAddD",
                    r => MapIdName(r,
                        new[] { "ItemId", "DepartId", "DeptId" },
                        new[] { "ItemName", "DepartName", "DeptName", "Name" }),
                    new SqlParameter("@PartNum", SqlDbType.VarChar, 24) { Value = partNum.Trim() },
                    new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = string.Empty }
                );

                var allDepts = await QueryAllDepartmentsAsync(conn);

                var normalizedChosen = chosen.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
                var chosenSet = normalizedChosen.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var normalizedAvailable = allDepts.Where(x => !string.IsNullOrWhiteSpace(x.Id) && !chosenSet.Contains(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                return Ok(new DepartPartResponse
                {
                    Available = normalizedAvailable,
                    Chosen = normalizedChosen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDepartmentsByPart failed. PartNum={PartNum}", partNum);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        [HttpGet("mat-class/{matClass}/departments")]
        public async Task<ActionResult<DepartPartResponse>> GetDepartmentsByMatClass(string matClass)
        {
            if (string.IsNullOrWhiteSpace(matClass))
                return BadRequest(new { ok = false, error = "MatClass is required." });
            try
            {
                await using var conn = new SqlConnection(_connStr);
                await conn.OpenAsync();

                // Align trace order/session: MINdDepartSetAddM -> MINdPartSetClose
                var chosen = await QueryListByProcAsync(
                    conn,
                    "MINdDepartSetAddM",
                    r => MapIdName(r,
                        new[] { "ItemId", "DepartId", "DeptId" },
                        new[] { "ItemName", "DepartName", "DeptName", "Name" }),
                    new SqlParameter("@MatClass", SqlDbType.VarChar, 8) { Value = matClass.Trim() }
                );

                var allDepts = await QueryAllDepartmentsAsync(conn);

                var normalizedChosen = chosen.Where(x => !string.IsNullOrWhiteSpace(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
                var chosenSet = normalizedChosen.Select(x => x.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
                var normalizedAvailable = allDepts.Where(x => !string.IsNullOrWhiteSpace(x.Id) && !chosenSet.Contains(x.Id))
                    .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                return Ok(new DepartPartResponse
                {
                    Available = normalizedAvailable,
                    Chosen = normalizedChosen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDepartmentsByMatClass failed. MatClass={MatClass}", matClass);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("depart/save")]
        public async Task<IActionResult> SaveByDepart([FromBody] SaveDepartRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.DepartId))
                return BadRequest(new { ok = false, error = "DepartId is required." });

            var departId = req.DepartId.Trim();
            var partNums = NormalizeList(req.PartNums, 24).ToList();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            try
            {
                await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetDel",
                    new SqlParameter("@PartNum", SqlDbType.VarChar, 24) { Value = string.Empty },
                    new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = departId });

                foreach (var part in partNums)
                {
                    await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetIns",
                        new SqlParameter("@Partnum", SqlDbType.VarChar, 24) { Value = part },
                        new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = departId });
                }

                await ((SqlTransaction)tran).CommitAsync();
                return Ok(new { ok = true, count = partNums.Count });
            }
            catch (Exception ex)
            {
                await ((SqlTransaction)tran).RollbackAsync();
                _logger.LogError(ex, "SaveByDepart failed. DepartId={DepartId}", departId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("part/save")]
        public async Task<IActionResult> SaveByPart([FromBody] SavePartRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.PartNum))
                return BadRequest(new { ok = false, error = "PartNum is required." });

            var partNum = req.PartNum.Trim();
            var departIds = NormalizeList(req.DepartIds, 16).ToList();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            try
            {
                await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetDel",
                    new SqlParameter("@PartNum", SqlDbType.VarChar, 24) { Value = partNum },
                    new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = string.Empty });

                foreach (var departId in departIds)
                {
                    await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetIns",
                        new SqlParameter("@Partnum", SqlDbType.VarChar, 24) { Value = partNum },
                        new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = departId });
                }

                await ((SqlTransaction)tran).CommitAsync();
                return Ok(new { ok = true, count = departIds.Count });
            }
            catch (Exception ex)
            {
                await ((SqlTransaction)tran).RollbackAsync();
                _logger.LogError(ex, "SaveByPart failed. PartNum={PartNum}", partNum);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        [HttpPost("mat-class/save")]
        public async Task<IActionResult> SaveByMatClass([FromBody] SaveMatClassRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.MatClass))
                return BadRequest(new { ok = false, error = "MatClass is required." });

            var matClass = req.MatClass.Trim();
            var departIds = NormalizeList(req.DepartIds, 16).ToList();

            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var tran = await conn.BeginTransactionAsync();
            try
            {
                await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetDelGroup",
                    new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = string.Empty },
                    new SqlParameter("@MatClass", SqlDbType.VarChar, 8) { Value = matClass });

                foreach (var departId in departIds)
                {
                    await ExecProcNonQueryAsync(conn, (SqlTransaction)tran, "MINdDepartSetInsGroup",
                        new SqlParameter("@DepartId", SqlDbType.VarChar, 16) { Value = departId },
                        new SqlParameter("@MatClass", SqlDbType.VarChar, 8) { Value = matClass });
                }

                await ((SqlTransaction)tran).CommitAsync();
                return Ok(new { ok = true, count = departIds.Count });
            }
            catch (Exception ex)
            {
                await ((SqlTransaction)tran).RollbackAsync();
                _logger.LogError(ex, "SaveByMatClass failed. MatClass={MatClass}", matClass);
                return StatusCode(StatusCodes.Status500InternalServerError, new { ok = false, error = ex.Message });
            }
        }

        private static async Task ExecProcNonQueryAsync(SqlConnection conn, SqlTransaction tran, string procName, params SqlParameter[] parameters)
        {
            await using var cmd = new SqlCommand(procName, conn, tran)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<T>> QueryListByProcAsync<T>(string procName, Func<IDataRecord, T> map, params SqlParameter[] parameters)
        {
            var result = new List<T>();
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            result = await QueryListByProcAsync(conn, procName, map, parameters);
            return result;
        }

        private static async Task<List<T>> QueryListByProcAsync<T>(SqlConnection conn, string procName, Func<IDataRecord, T> map, params SqlParameter[] parameters)
        {
            var result = new List<T>();
            await using var cmd = new SqlCommand(procName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(map(rd));
            }
            return result;
        }

        private async Task<List<IdNameDto>> QueryAllDepartmentsAsync()
        {
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            return await QueryAllDepartmentsAsync(conn);
        }

        private async Task<List<IdNameDto>> QueryAllDepartmentsAsync(SqlConnection conn)
        {
            static IdNameDto Map(IDataRecord r) => MapIdName(r,
                new[] { "ItemId", "DepartId", "DeptId" },
                new[] { "ItemName", "DepartName", "DeptName", "Name" });

            try
            {
                var rows = await QueryListByProcAsync(
                    conn,
                    "MINdPartSetClose",
                    Map,
                    new SqlParameter("@Cond", SqlDbType.NVarChar, 255) { Value = DBNull.Value }
                );

                if (rows.Count > 0)
                    return rows;

                _logger.LogWarning("MINdPartSetClose(@Cond=NULL) returned 0 rows, fallback to no-parameter call.");
                rows = await QueryListByProcAsync(conn, "MINdPartSetClose", Map);
                if (rows.Count > 0)
                    return rows;

                _logger.LogWarning("MINdPartSetClose(no-parameter) returned 0 rows, fallback to AJNdDepart.");
                return await QueryListBySqlAsync(@"
SELECT DepartId AS ItemId, DepartName AS ItemName
  FROM dbo.AJNdDepart WITH (NOLOCK)
 ORDER BY DepartId", Map);
            }
            catch (Exception ex1)
            {
                _logger.LogWarning(ex1, "MINdPartSetClose(@Cond) failed, fallback to no-parameter call.");
                try
                {
                    return await QueryListByProcAsync(conn, "MINdPartSetClose", Map);
                }
                catch (Exception ex2)
                {
                    _logger.LogWarning(ex2, "MINdPartSetClose(no-parameter) failed, fallback to AJNdDepart.");
                    // Ultimate fallback to keep UI usable across DB variants.
                    return await QueryListBySqlAsync(@"
SELECT DepartId AS ItemId, DepartName AS ItemName
  FROM dbo.AJNdDepart WITH (NOLOCK)
 ORDER BY DepartId", Map);
                }
            }
        }

        private async Task<List<T>> QueryListBySqlAsync<T>(string sql, Func<IDataRecord, T> map, params SqlParameter[] parameters)
        {
            var result = new List<T>();
            await using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn)
            {
                CommandType = CommandType.Text
            };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                result.Add(map(rd));
            }
            return result;
        }

        private static IEnumerable<string> NormalizeList(IEnumerable<string>? values, int maxLength)
        {
            if (values == null) yield break;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in values)
            {
                var val = (v ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(val)) continue;
                if (val.Length > maxLength) val = val[..maxLength];
                if (seen.Add(val)) yield return val;
            }
        }

        private static string ReadString(IDataRecord record, params string[] names)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                try
                {
                    var ordinal = record.GetOrdinal(name);
                    if (ordinal >= 0 && !record.IsDBNull(ordinal))
                        return Convert.ToString(record.GetValue(ordinal))?.Trim() ?? string.Empty;
                }
                catch (IndexOutOfRangeException)
                {
                    // try next alias
                }
            }
            return string.Empty;
        }

        private static string ReadStringByIndex(IDataRecord record, int index)
        {
            if (index < 0 || index >= record.FieldCount) return string.Empty;
            if (record.IsDBNull(index)) return string.Empty;
            return Convert.ToString(record.GetValue(index))?.Trim() ?? string.Empty;
        }

        private static IdNameDto MapIdName(IDataRecord r, string[] idNames, string[] nameNames)
        {
            var id = ReadString(r, idNames);
            if (string.IsNullOrWhiteSpace(id))
                id = ReadStringByIndex(r, 0);

            var name = ReadString(r, nameNames);
            if (string.IsNullOrWhiteSpace(name))
                name = ReadStringByIndex(r, 1);

            if (string.IsNullOrWhiteSpace(name))
                name = id;

            return new IdNameDto(id, name);
        }
    }
}
