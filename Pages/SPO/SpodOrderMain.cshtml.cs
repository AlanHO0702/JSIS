using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class SpodOrdersModel : TableListModel<SpodOrderMain>
{

    private readonly ILogger<TableListModel<SpodOrderMain>> _logger;
    public SpodOrdersModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService, PcbErpContext context,ILogger<TableListModel<SpodOrderMain>> logger)
        : base(httpClientFactory, dictService, context, logger) {  _logger = logger;}

    public override string TableName => "SpodOrderMain";
    public override string ApiPagedUrl => "/api/SPOdOrderMain/paged";  // 修正大小寫不一致問題

}
