using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;
using PcbErpContext = PcbErpApi.Data.PcbErpContext;

public class AJNdJourMainModel : TableListModel<AjndJourMain>
{

    private readonly ILogger<TableListModel<AjndJourMain>> _logger;
    public AJNdJourMainModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService, PcbErpContext context,ILogger<TableListModel<AjndJourMain>> logger)
        : base(httpClientFactory, dictService, context, logger) {  _logger = logger;}

    public override string TableName => "AJNdJourMain";



}
