using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcbErpApi.Data;
using PcbErpApi.Models;

namespace PcbErpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaperInfoController : ControllerBase
    {
        private readonly PcbErpContext _context;
        private readonly ILogger<PaperInfoController> _logger;

        public PaperInfoController(PcbErpContext context, ILogger<PaperInfoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得單據基本資料
        /// </summary>
        [HttpGet("{paperId}")]
        public async Task<IActionResult> GetPaperInfo(string paperId)
        {
            try
            {
                var paper = await _context.CurdPaperInfos
                    .FirstOrDefaultAsync(p => p.PaperId == paperId);

                if (paper == null)
                {
                    return NotFound(new { success = false, message = "找不到該單據" });
                }

                return Ok(new
                {
                    paperId = paper.PaperId,
                    paperName = paper.PaperName,
                    systemId = paper.SystemId,
                    encodeWay = paper.EncodeWay,
                    headFirst = paper.HeadFirst,
                    headDateFormat = paper.HeadDateFormat,
                    numLength = paper.NumLength,
                    yearDiff = paper.YearDiff,
                    tableName = paper.TableName,
                    pkName = paper.PKName,
                    runFlow = paper.RunFlow,
                    selectType = paper.SelectType,
                    lockPaperDate = paper.LockPaperDate,
                    lockUserEdit = paper.LockUserEdit,
                    mustNotes = paper.MustNotes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得單據資料失敗: {PaperId}", paperId);
                return StatusCode(500, new { success = false, message = "取得資料失敗" });
            }
        }

        /// <summary>
        /// 取得單據類型
        /// </summary>
        [HttpGet("{paperId}/Types")]
        public async Task<IActionResult> GetPaperTypes(string paperId)
        {
            try
            {
                var types = await _context.CurdPaperTypes
                    .Where(t => t.PaperId == paperId)
                    .OrderBy(t => t.PaperType)
                    .Select(t => new
                    {
                        paperId = t.PaperId,
                        paperType = t.PaperType,
                        paperTypeName = t.PaperTypeName,
                        headFirst = t.HeadFirst
                    })
                    .ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得單據類型失敗: {PaperId}", paperId);
                return StatusCode(500, new { success = false, message = "取得資料失敗" });
            }
        }

        /// <summary>
        /// 取得表單設定
        /// </summary>
        [HttpGet("{paperId}/Paper")]
        public async Task<IActionResult> GetPaperPaper(string paperId)
        {
            try
            {
                var papers = await _context.CurdPaperPaper
                    .Where(p => p.PaperId == paperId)
                    .OrderBy(p => p.SerialNum)
                    .Select(p => new
                    {
                        paperId = p.PaperId,
                        serialNum = p.SerialNum,
                        itemName = p.ItemName,
                        enabled = p.Enabled,
                        notes = p.Notes,
                        className = p.ClassName,
                        objectName = p.ObjectName,
                        linkType = p.LinkType,
                        displayType = p.DisplayType,
                        outputType = p.OutputType,
                        showTitle = p.ShowTitle,
                        showTree = p.ShowTree,
                        tableIndex = p.TableIndex,
                        itemCount = p.ItemCount,
                        printItemId = p.PrintItemId
                    })
                    .ToListAsync();

                return Ok(papers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得表單設定失敗: {PaperId}", paperId);
                return StatusCode(500, new { success = false, message = "取得資料失敗" });
            }
        }

        /// <summary>
        /// 取得列表設定
        /// </summary>
        [HttpGet("{paperId}/List")]
        public async Task<IActionResult> GetPaperList(string paperId)
        {
            try
            {
                var lists = await _context.CurdPaperLists
                    .Where(l => l.PaperId == paperId)
                    .OrderBy(l => l.SerialNum)
                    .Select(l => new
                    {
                        paperId = l.PaperId,
                        serialNum = l.SerialNum,
                        itemName = l.ItemName,
                        enabled = l.Enabled,
                        notes = l.Notes,
                        className = l.ClassName,
                        objectName = l.ObjectName,
                        linkType = l.LinkType,
                        displayType = l.DisplayType,
                        outputType = l.OutputType,
                        showTitle = l.ShowTitle,
                        showTree = l.ShowTree,
                        tableIndex = l.TableIndex,
                        itemCount = l.ItemCount
                    })
                    .ToListAsync();

                return Ok(lists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得列表設定失敗: {PaperId}", paperId);
                return StatusCode(500, new { success = false, message = "取得資料失敗" });
            }
        }

        /// <summary>
        /// 儲存單據資訊（包含基本資料、類型、表單、列表）
        /// </summary>
        [HttpPost("Save")]
        public async Task<IActionResult> SavePaperInfo([FromBody] SavePaperInfoRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 更新基本資料
                var paper = await _context.CurdPaperInfos
                    .FirstOrDefaultAsync(p => p.PaperId == request.BasicData.PaperId);

                if (paper == null)
                {
                    return NotFound(new { success = false, message = "找不到該單據" });
                }

                paper.PaperName = request.BasicData.PaperName;
                paper.EncodeWay = request.BasicData.EncodeWay;
                paper.HeadFirst = request.BasicData.HeadFirst;
                paper.HeadDateFormat = request.BasicData.HeadDateFormat;
                paper.NumLength = request.BasicData.NumLength;
                paper.YearDiff = request.BasicData.YearDiff;
                paper.TableName = request.BasicData.TableName;
                paper.PKName = request.BasicData.PKName;
                paper.RunFlow = request.BasicData.RunFlow;
                paper.SelectType = request.BasicData.SelectType;
                paper.LockPaperDate = request.BasicData.LockPaperDate;
                paper.LockUserEdit = request.BasicData.LockUserEdit;
                paper.MustNotes = request.BasicData.MustNotes;

                // 2. 更新單據類型（部分更新策略）
                if (request.Types != null)
                {
                    // 載入現有資料
                    var existingTypes = await _context.CurdPaperTypes
                        .Where(t => t.PaperId == request.BasicData.PaperId)
                        .ToListAsync();

                    // 建立前端傳來的 PaperType 集合
                    var requestPaperTypes = request.Types.Select(t => t.PaperType).ToHashSet();

                    // 刪除不在前端資料中的記錄（表示被刪除）
                    var toDelete = existingTypes.Where(e => !requestPaperTypes.Contains(e.PaperType)).ToList();
                    if (toDelete.Any())
                    {
                        _context.CurdPaperTypes.RemoveRange(toDelete);
                    }

                    // 處理新增和更新
                    foreach (var type in request.Types)
                    {
                        var existing = existingTypes.FirstOrDefault(e => e.PaperType == type.PaperType);

                        if (existing != null)
                        {
                            // 檢查是否有變更
                            bool hasChanges =
                                existing.PaperTypeName != type.PaperTypeName ||
                                existing.HeadFirst != type.HeadFirst;

                            // 只有在有變更時才更新
                            if (hasChanges)
                            {
                                existing.PaperTypeName = type.PaperTypeName;
                                existing.HeadFirst = type.HeadFirst;
                            }
                        }
                        else
                        {
                            // 新增
                            _context.CurdPaperTypes.Add(new CurdPaperType
                            {
                                PaperId = request.BasicData.PaperId,
                                PaperType = type.PaperType,
                                PaperTypeName = type.PaperTypeName,
                                HeadFirst = type.HeadFirst
                            });
                        }
                    }
                }

                // 4. 更新表單設定（部分更新策略）
                if (request.Papers != null)
                {
                    // 載入現有資料
                    var existingPapers = await _context.CurdPaperPaper
                        .Where(p => p.PaperId == request.BasicData.PaperId)
                        .ToListAsync();

                    // 建立前端傳來的 SerialNum 集合
                    var requestSerialNums = request.Papers.Select(p => p.SerialNum).ToHashSet();

                    // 刪除不在前端資料中的記錄（表示被刪除）
                    var toDelete = existingPapers.Where(e => !requestSerialNums.Contains(e.SerialNum)).ToList();
                    if (toDelete.Any())
                    {
                        _context.CurdPaperPaper.RemoveRange(toDelete);
                    }

                    // 處理新增和更新
                    foreach (var ppr in request.Papers)
                    {
                        var existing = existingPapers.FirstOrDefault(e => e.SerialNum == ppr.SerialNum);

                        if (existing != null)
                        {
                            // 檢查是否有變更
                            bool hasChanges =
                                existing.ItemName != ppr.ItemName ||
                                existing.Enabled != ppr.Enabled ||
                                existing.Notes != ppr.Notes ||
                                existing.ClassName != ppr.ClassName ||
                                existing.ObjectName != ppr.ObjectName ||
                                existing.LinkType != ppr.LinkType ||
                                existing.DisplayType != ppr.DisplayType ||
                                existing.OutputType != ppr.OutputType ||
                                existing.ShowTitle != (ppr.ShowTitle ?? 0) ||
                                existing.ShowTree != (ppr.ShowTree ?? 0) ||
                                existing.TableIndex != ppr.TableIndex ||
                                existing.ItemCount != (ppr.ItemCount ?? 0) ||
                                existing.PrintItemId != ppr.PrintItemId;

                            // 只有在有變更時才更新
                            if (hasChanges)
                            {
                                existing.ItemName = ppr.ItemName;
                                existing.Enabled = ppr.Enabled;
                                existing.Notes = ppr.Notes;
                                existing.ClassName = ppr.ClassName;
                                existing.ObjectName = ppr.ObjectName;
                                existing.LinkType = ppr.LinkType;
                                existing.DisplayType = ppr.DisplayType;
                                existing.OutputType = ppr.OutputType;
                                existing.ShowTitle = ppr.ShowTitle ?? 0;
                                existing.ShowTree = ppr.ShowTree ?? 0;
                                existing.TableIndex = ppr.TableIndex;
                                existing.ItemCount = ppr.ItemCount ?? 0;
                                existing.PrintItemId = ppr.PrintItemId;
                            }
                        }
                        else
                        {
                            // 新增
                            _context.CurdPaperPaper.Add(new CurdPaperPaper
                            {
                                PaperId = request.BasicData.PaperId,
                                SerialNum = ppr.SerialNum,
                                ItemName = ppr.ItemName,
                                Enabled = ppr.Enabled,
                                Notes = ppr.Notes,
                                ClassName = ppr.ClassName,
                                ObjectName = ppr.ObjectName,
                                LinkType = ppr.LinkType,
                                DisplayType = ppr.DisplayType,
                                OutputType = ppr.OutputType,
                                ShowTitle = ppr.ShowTitle ?? 0,
                                ShowTree = ppr.ShowTree ?? 0,
                                TableIndex = ppr.TableIndex,
                                ItemCount = ppr.ItemCount ?? 0,
                                PrintItemId = ppr.PrintItemId
                            });
                        }
                    }
                }

                // 6. 更新列表設定（部分更新策略）
                if (request.Lists != null)
                {
                    // 載入現有資料
                    var existingLists = await _context.CurdPaperLists
                        .Where(l => l.PaperId == request.BasicData.PaperId)
                        .ToListAsync();

                    // 建立前端傳來的 SerialNum 集合
                    var requestSerialNums = request.Lists.Select(l => l.SerialNum).ToHashSet();

                    // 刪除不在前端資料中的記錄（表示被刪除）
                    var toDelete = existingLists.Where(e => !requestSerialNums.Contains(e.SerialNum)).ToList();
                    if (toDelete.Any())
                    {
                        _context.CurdPaperLists.RemoveRange(toDelete);
                    }

                    // 處理新增和更新
                    foreach (var list in request.Lists)
                    {
                        var existing = existingLists.FirstOrDefault(e => e.SerialNum == list.SerialNum);

                        if (existing != null)
                        {
                            // 檢查是否有變更
                            bool hasChanges =
                                existing.ItemName != list.ItemName ||
                                existing.Enabled != list.Enabled ||
                                existing.Notes != list.Notes ||
                                existing.ClassName != list.ClassName ||
                                existing.ObjectName != list.ObjectName ||
                                existing.LinkType != list.LinkType ||
                                existing.DisplayType != list.DisplayType ||
                                existing.OutputType != list.OutputType ||
                                existing.ShowTitle != (list.ShowTitle ?? 0) ||
                                existing.ShowTree != (list.ShowTree ?? 0) ||
                                existing.TableIndex != list.TableIndex ||
                                existing.ItemCount != (list.ItemCount ?? 0);

                            // 只有在有變更時才更新
                            if (hasChanges)
                            {
                                existing.ItemName = list.ItemName;
                                existing.Enabled = list.Enabled;
                                existing.Notes = list.Notes;
                                existing.ClassName = list.ClassName;
                                existing.ObjectName = list.ObjectName;
                                existing.LinkType = list.LinkType;
                                existing.DisplayType = list.DisplayType;
                                existing.OutputType = list.OutputType;
                                existing.ShowTitle = list.ShowTitle ?? 0;
                                existing.ShowTree = list.ShowTree ?? 0;
                                existing.TableIndex = list.TableIndex;
                                existing.ItemCount = list.ItemCount ?? 0;
                            }
                        }
                        else
                        {
                            // 新增
                            _context.CurdPaperLists.Add(new CurdPaperList
                            {
                                PaperId = request.BasicData.PaperId,
                                SerialNum = list.SerialNum,
                                ItemName = list.ItemName,
                                Enabled = list.Enabled,
                                Notes = list.Notes,
                                ClassName = list.ClassName,
                                ObjectName = list.ObjectName,
                                LinkType = list.LinkType,
                                DisplayType = list.DisplayType,
                                OutputType = list.OutputType,
                                ShowTitle = list.ShowTitle ?? 0,
                                ShowTree = list.ShowTree ?? 0,
                                TableIndex = list.TableIndex,
                                ItemCount = list.ItemCount ?? 0
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "儲存成功" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "儲存單據資料失敗");
                return StatusCode(500, new { success = false, message = "儲存失敗：" + ex.Message });
            }
        }

        #region DTO Classes

        public class SavePaperInfoRequest
        {
            public BasicDataDto BasicData { get; set; } = new();
            public List<PaperTypeDto>? Types { get; set; }
            public List<PaperPaperDto>? Papers { get; set; }
            public List<PaperListDto>? Lists { get; set; }
        }

        public class BasicDataDto
        {
            public string PaperId { get; set; } = null!;
            public string PaperName { get; set; } = null!;
            public string SystemId { get; set; } = null!;
            public int EncodeWay { get; set; }
            public string? HeadFirst { get; set; }
            public string? HeadDateFormat { get; set; }
            public int NumLength { get; set; }
            public int? YearDiff { get; set; }
            public string? TableName { get; set; }
            public string? PKName { get; set; }
            public int? RunFlow { get; set; }
            public int? SelectType { get; set; }
            public int? LockPaperDate { get; set; }
            public int? LockUserEdit { get; set; }
            public int? MustNotes { get; set; }
        }

        public class PaperTypeDto
        {
            public string PaperId { get; set; } = null!;
            public int PaperType { get; set; }
            public string? PaperTypeName { get; set; }
            public string? HeadFirst { get; set; }
        }

        public class PaperPaperDto
        {
            public string PaperId { get; set; } = null!;
            public int SerialNum { get; set; }
            public string? ItemName { get; set; }
            public int Enabled { get; set; }
            public string? Notes { get; set; }
            public string? ClassName { get; set; }
            public string? ObjectName { get; set; }
            public int LinkType { get; set; }
            public int DisplayType { get; set; }
            public int OutputType { get; set; }
            public int? ShowTitle { get; set; }
            public int? ShowTree { get; set; }
            public string? TableIndex { get; set; }
            public int? ItemCount { get; set; }
            public string? PrintItemId { get; set; }
        }

        public class PaperListDto
        {
            public string PaperId { get; set; } = null!;
            public int SerialNum { get; set; }
            public string? ItemName { get; set; }
            public int Enabled { get; set; }
            public string? Notes { get; set; }
            public string? ClassName { get; set; }
            public string? ObjectName { get; set; }
            public int LinkType { get; set; }
            public int DisplayType { get; set; }
            public int OutputType { get; set; }
            public int? ShowTitle { get; set; }
            public int? ShowTree { get; set; }
            public string? TableIndex { get; set; }
            public int? ItemCount { get; set; }
        }

        #endregion
    }
}
