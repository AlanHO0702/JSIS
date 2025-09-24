using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using PcbErpApi.Models;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Data;

public class CurdUsersModel : TableListModel<CurdUser>
{
    private readonly ILogger<TableListModel<CurdUser>> _logger;
    public CurdUsersModel(IHttpClientFactory httpClientFactory, ITableDictionaryService dictService,PcbErpContext context,ILogger<TableListModel<CurdUser>> logger)
        : base(httpClientFactory, dictService , context, logger) {_logger = logger; }

    public override string TableName => "CurdUsers";

}