using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace AspNetCoreMvcClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AppSettings _appSettings;

        public IndexModel(ILogger<IndexModel> logger, IOptionsSnapshot<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public void OnGet()
        {
            ViewData["Message"] = _appSettings.Message;
            ViewData["BackgroundColor"] = _appSettings.BackgroundColor;
            ViewData["FontSize"] = _appSettings.FontSize;
            ViewData["FontColor"] = _appSettings.FontColor;
            ViewData["SwaggerOptions:ApiName"] = _appSettings.SwaggerOptions?.ApiName;
            ViewData["SwaggerOptions:ApiVersion"] = _appSettings.SwaggerOptions?.ApiVersion;
        }
    }
}