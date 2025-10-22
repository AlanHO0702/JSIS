using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class SPOdMPSOutModel : TableListModel<SPOdMPSOutMain>
{

    private readonly ILogger<TableListModel<SPOdMPSOutMain>> _logger;
    public SPOdMPSOutModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService, PcbErpContext context,ILogger<TableListModel<SPOdMPSOutMain>> logger)
        : base(httpClientFactory, dictService, context, logger) {  _logger = logger;}

    public override string TableName => "SPOdMPSOutMain";

}
