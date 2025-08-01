using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PcbErpApi;
using PcbErpApi.Data;
using PcbErpApi.Models;

// 建立 WebApplication 的建構器，負責設定與註冊服務
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;


// 註冊 Swagger API 文件產生器與 Explorer，方便生成與瀏覽 API 文件
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 取得目前執行組件的 XML 文件路徑（需在專案設定中啟用 XML 文件產生）
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // 告訴 Swagger 使用 XML 文件來生成更完整的 API 註解說明
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // 保持原本 C# 的大寫
});


// 註冊 Razor Pages 服務，並設定 JSON 序列化時使用 camelCase 命名風格
builder.Services.AddRazorPages();
    /*.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });*/

// 註冊 API Controllers 服務（支援 [ApiController]）
builder.Services.AddControllers();


builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("PcbErpApi"));
builder.Services.AddHttpClient("MyApiClient", (sp, client) =>
{
    var apiSettings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(apiSettings.HostAddress);
});

// 註冊 HttpClient，讓服務可注入 HttpClient 用於發送 HTTP 請求
builder.Services.AddHttpClient();

// 註冊 EF Core 的 PcbErpContext 資料庫上下文，並使用 appsettings.json 中的 DefaultConnection 連線字串
builder.Services.AddDbContext<PcbErpContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITableDictionaryService, TableDictionaryService>();

builder.Services.AddScoped<PaginationService>();

// 建立應用程式物件，進入中介軟體與路由設定階段
var app = builder.Build();

// 如果不是開發環境，設定例外處理頁面與啟用 HTTP 嚴格傳輸安全（HSTS）
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error"); // 導向錯誤頁面
    app.UseHsts(); // 強制使用 HTTPS 連線
}

// 強制將 HTTP 請求重新導向到 HTTPS
app.UseHttpsRedirection();

// 允許靜態檔案（wwwroot 下的 CSS、JS、圖片等）被存取
app.UseStaticFiles();

// 啟用路由中介軟體，讓路由功能生效
app.UseRouting();

// 啟用授權中介軟體（若使用授權/認證機制）
app.UseAuthorization();

// 將 API Controller 路由映射 (例如 /api/YourController)
app.MapControllers();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// 將 Razor Pages 頁面路由映射（.cshtml）
app.MapRazorPages();

// 啟用 Swagger 介面和 JSON 文件，方便開發與測試 API
app.UseSwagger();
app.UseSwaggerUI();

// 啟動應用程式，開始監聽 HTTP 請求
app.Run();
