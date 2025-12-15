using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class MindMatInfoModel : TableListModel<MindMatInfo>
{

    private readonly ILogger<TableListModel<MindMatInfo>> _logger;
    public MindMatInfoModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService, PcbErpContext context, ILogger<TableListModel<MindMatInfo>> logger)
        : base(httpClientFactory, dictService, context, logger) { _logger = logger; }

    public override string TableName => "MindMatInfo";

}