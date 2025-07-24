using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;

public class SpodOrdersModel : TableListModel<SpodOrderMain>
{

    public SpodOrdersModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService)
        : base(httpClientFactory, dictService) { }

    public override string TableName => "SpodOrderMain";



}
