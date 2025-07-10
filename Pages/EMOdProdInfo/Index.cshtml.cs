using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PcbErpApi.Data;
using PcbErpApi.Models;
using System.Collections.Generic;

namespace PcbErpApi.Pages.EMOdProdInfo
{
    public class IndexModel : PageModel
    {
        private readonly ITableDictionaryService _dictService;

        public IndexModel(ITableDictionaryService dictService)
        {
            _dictService = dictService;
        }

        public List<DynamicFieldViewModel> DynamicFields { get; set; } = new();
        [BindProperty]
        public Dictionary<string, string> DynamicFormData { get; set; } = new();

        // 固定欄位不用變動（你原本那些照放）
        [BindProperty] public string StructCode { get; set; }
        [BindProperty] public string StructDesc { get; set; }
        [BindProperty] public string Type { get; set; }
        [BindProperty] public string ProcessType { get; set; }
        [BindProperty] public string BoardSize { get; set; }
        [BindProperty] public string CustomerNo { get; set; }
        [BindProperty] public string BatchQty { get; set; }
        [BindProperty] public string PcsLength { get; set; }
        [BindProperty] public string PcsWidth { get; set; }
        [BindProperty] public string Wire { get; set; }
        [BindProperty] public string FinalThickness { get; set; }
        [BindProperty] public string LamThickness { get; set; }
        [BindProperty] public string LimitDown { get; set; }
        [BindProperty] public string LimitUp { get; set; }
        [BindProperty] public string MeasurePos { get; set; }
        [BindProperty] public string ProdCount { get; set; }
        [BindProperty] public string CycleTime1 { get; set; }
        [BindProperty] public string SecondProdCount { get; set; }
        [BindProperty] public string CycleTime2 { get; set; }
        [BindProperty] public string ThirdProdCount { get; set; }
        [BindProperty] public string CycleTime3 { get; set; }
        [BindProperty] public string ICT { get; set; }
        [BindProperty] public string LayerCount { get; set; }
        [BindProperty] public string XOut { get; set; }
        [BindProperty] public string MaterialType { get; set; }
        [BindProperty] public string MaterialProperty { get; set; }
        [BindProperty] public string UsageRate { get; set; }
        [BindProperty] public string MachineCount { get; set; }
        [BindProperty] public string SurfaceType { get; set; }
        [BindProperty] public bool UseInnerList { get; set; }
        [BindProperty] public string? SurfaceDesc { get; set; }
        [BindProperty] public string? APS1 { get; set; }
        [BindProperty] public string? APS2 { get; set; }
        [BindProperty] public string? PressLayer { get; set; }
        [BindProperty] public string? AltMaterialNo { get; set; }
        [BindProperty] public string? MaterialModel { get; set; }
        [BindProperty] public string? MaterialSize { get; set; }
        [BindProperty] public string? LayoutNo { get; set; }
        [BindProperty] public string? LayoutRemark { get; set; }

        // 資料辭典清單
        public List<CURdTableField> FieldDictList { get; set; } = new();

        // ViewModel for dynamic fields
        public class DynamicFieldViewModel
        {
            public string FieldName { get; set; }
            public string DisplayLabel { get; set; }
            public int SerialNum { get; set; }
            public string DataType { get; set; }
            // 你有需要可以再加
        }

        public void OnGet()
        {
            // 資料庫設定（型別自動補齊，直接交給 Service 處理）
            FieldDictList = _dictService.GetFieldDict("EMOdProdInfo", typeof(EmodProdInfo));

            // 產生 DynamicFields 給前端表單
            DynamicFields = FieldDictList
                .Where(x => x.Visible == 1)
                .Select(x => new DynamicFieldViewModel
                {
                    FieldName = x.FieldName,
                    DisplayLabel = x.DisplayLabel,
                    SerialNum = (int)x.SerialNum,
                    DataType = x.DataType
                }).ToList();
        }

        public IActionResult OnPost()
        {
            TempData["Success"] = "儲存成功！";
            return Page();
        }
    }
}
