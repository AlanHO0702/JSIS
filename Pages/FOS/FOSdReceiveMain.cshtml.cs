using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class FOSdReceivesModel : TableListModel<FosdReceiveMain>
{
    private readonly ILogger<TableListModel<FosdReceiveMain>> _logger;

    public FOSdReceivesModel(
        IHttpClientFactory httpClientFactory,
        ITableDictionaryService dictService,
        PcbErpContext context,
        ILogger<TableListModel<FosdReceiveMain>> logger)
        : base(httpClientFactory, dictService, context, logger)
    {
        _logger = logger;
    }

    public override string TableName => "FOSdReceiveMain";
}
