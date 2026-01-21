using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class FOSdOrdersModel : TableListModel<FosdOrderMain>
{
    private readonly ILogger<TableListModel<FosdOrderMain>> _logger;

    public FOSdOrdersModel(
        IHttpClientFactory httpClientFactory,
        ITableDictionaryService dictService,
        PcbErpContext context,
        ILogger<TableListModel<FosdOrderMain>> logger)
        : base(httpClientFactory, dictService, context, logger)
    {
        _logger = logger;
    }

    public override string TableName => "FOSdOrderMain";
}
