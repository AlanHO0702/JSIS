using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;
using PcbErpApi.Data;

namespace PcbErpApi.Pages
{
    public class FmedVProcNisToStdModel : TableListModel<FmedVProcNisToStd>
    {
        private readonly ILogger<TableListModel<FmedVProcNisToStd>> _logger;

        public FmedVProcNisToStdModel(
            IHttpClientFactory httpClientFactory,
            ITableDictionaryService dictService,
            PcbErpContext context,
            ILogger<TableListModel<FmedVProcNisToStd>> logger)
            : base(httpClientFactory, dictService, context, logger)
        {
            _logger = logger;
        }

        public override string TableName => "FMEdV_ProcNIS_ToStd";
        public override string ApiPagedUrl => "/api/FmedVProcNisToStd/paged";
    }
}
