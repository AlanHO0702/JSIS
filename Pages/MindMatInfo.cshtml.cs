using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class MindMatInfoModel : TableListModel<MindMatInfo>
{

    public MindMatInfoModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService,PcbErpContext context)
        : base(httpClientFactory, dictService , context) { }

    public override string TableName => "MindMatInfo";

}