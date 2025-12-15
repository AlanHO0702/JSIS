using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class FMEdIssuesModel : TableListModel<FmedIssueMain>
{
    private readonly ILogger<TableListModel<FmedIssueMain>> _logger;

    public FMEdIssuesModel(
        IHttpClientFactory httpClientFactory, 
        ITableDictionaryService dictService, 
        PcbErpContext context, 
        ILogger<TableListModel<FmedIssueMain>> logger)
        : base(httpClientFactory, dictService, context, logger)
    {
        _logger = logger;
    }

    public override string TableName => "FMEdIssueMain";
}
