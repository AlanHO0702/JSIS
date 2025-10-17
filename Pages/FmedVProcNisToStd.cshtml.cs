using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PcbErpApi.Models;

namespace PcbErpApi.Pages
{
    public class FmedVProcNisToStdModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FmedVProcNisToStdModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<FmedVProcNisToStd> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("MyApiClient");
        var response = await client.GetFromJsonAsync<List<FmedVProcNisToStd>>("/api/FmedVProcNisToStd");
        if (response != null)
            Items = response;
    }
}

}
