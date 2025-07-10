using Microsoft.EntityFrameworkCore;

public class PagedQueryService
{
    public async Task<PagedResult<T>> GetPagedAsync<T>(
        IQueryable<T> query, int page, int pageSize) where T : class
    {
        var totalCount = await query.CountAsync();
        var data = await query.Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        return new PagedResult<T> { TotalCount = totalCount, Data = data };
    }
}

public class PagedResult<T>
{
    public int TotalCount { get; set; }
    public List<T> Data { get; set; }
}
