using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Data;
using WebRazor.Models;

namespace PcbErpApi.Pages.FQCdProcInfo
{
    public class IndexModel : PageModel
    {
        private readonly PcbErpContext _db;

        public IndexModel(PcbErpContext db)
        {
            _db = db;
        }

        public MasterMultiDetailConfig Config { get; set; }

        public void OnGet()
        {
            Config = new MasterMultiDetailConfig
            {
                DomId = "fqcProcInfo",
                MasterTitle = "製程大站",
                MasterTable = "FQCdProcInfo",
                MasterDict = "FQCdProcInfo",  // 主頁籤辭典
                MasterApi = "/api/FQCdProcInfo",
                MasterTop = 300,

                Details = new List<DetailConfig>
                {
                    // Tab 1: 製程明細（同一張表，但用不同辭典）
                    new DetailConfig
                    {
                        DetailTitle = "製程明細",
                        DetailTable = "FQCdProcInfo",
                        DetailDict = "FQCdProcInfoDtl",  // 製程明細辭典
                        DetailApi = "/api/FQCdProcInfo",
                        KeyMap = new List<KeyMapMulti>
                        {
                            // 主從表關聯：透過 BProcCode
                            new KeyMapMulti { Master = "BProcCode", Detail = "BProcCode" }
                        },
                        PkFields = new List<string> { "BProcCode" }
                    },

                    // Tab 2: 參數明細組
                    new DetailConfig
                    {
                        DetailTitle = "參數明細組",
                        DetailTable = "FMEdBigProcParam",
                        DetailDict = "FMEdBigProcParamDtl",  // 參數明細辭典
                        DetailApi = "/api/FMEdBigProcParam",
                        KeyMap = new List<KeyMapMulti>
                        {
                            // 主從表關聯：透過 BProcCode -> ProcCode
                            new KeyMapMulti { Master = "BProcCode", Detail = "ProcCode" }
                        },
                        PkFields = new List<string> { "ProcCode", "ParamId" }
                    }
                }
            };
        }
    }
}
