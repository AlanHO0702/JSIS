using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PcbErpApi.Pages.EMOdProdInfo
{
    public class IndexModel : PageModel
{
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


        public void OnGet()
        {
            // 可在此讀取資料預設值
        }

        public IActionResult OnPost()
        {
            // 儲存資料邏輯
            TempData["Success"] = "儲存成功！";
            return Page();
        }
    }
}
