using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class PaginationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<PaginationService> _logger;

    public PaginationService(IMemoryCache cache, ILogger<PaginationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// 分頁查詢（帶 TotalCount 快取）
    /// </summary>
    /// <param name="query">查詢</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁筆數</param>
    /// <param name="cacheKey">快取鍵（如 "FMEdIssueMain"），若有過濾條件請加上參數如 "FMEdIssueMain_PaperNum_xxx"</param>
    /// <param name="cacheDuration">快取秒數，預設 30 秒</param>
    public async Task<PagedResult<T>> GetPagedAsync<T>(
        IQueryable<T> query, int page, int pageSize, string? cacheKey = null, int cacheDuration = 30) where T : class
    {
        var sw = Stopwatch.StartNew();
        int totalCount;
        bool cacheHit = false;

        if (!string.IsNullOrEmpty(cacheKey))
        {
            var countCacheKey = $"{cacheKey}_TotalCount";
            if (!_cache.TryGetValue(countCacheKey, out totalCount))
            {
                totalCount = await query.CountAsync();
                _cache.Set(countCacheKey, totalCount, TimeSpan.FromSeconds(cacheDuration));
            }
            else
            {
                cacheHit = true;
            }
        }
        else
        {
            totalCount = await query.CountAsync();
        }
        var countTime = sw.ElapsedMilliseconds;

        sw.Restart();
        var data = await query.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
        var dataTime = sw.ElapsedMilliseconds;

        _logger.LogInformation("[PaginationService] Page={Page} | Count={CountMs}ms (快取:{CacheHit}) | Data={DataMs}ms | Total={TotalMs}ms",
            page, countTime, cacheHit ? "是" : "否", dataTime, countTime + dataTime);

        return new PagedResult<T> { TotalCount = totalCount, Data = data };
    }
}

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public List<T> Data { get; set; }
}