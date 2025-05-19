using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{
    public class AppConfigInfoModel : PageModel
    {
        public string strAppConfigInfoHtml;
        private readonly AppConfig _appconfig;

        public AppConfigInfoModel(AppConfig appconfig)
        {
            _appconfig = appconfig;
            strAppConfigInfoHtml = "";

        }

        public void OnGet()
        {
            string pw = HttpContext.Request.Query["pw"].ToString();
            if (string.IsNullOrEmpty(pw) || pw != _appconfig.config.GetValue<string>("AdminPW"))
                return;

            try
            {
                strAppConfigInfoHtml += "OS Description: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription + "<br/>";
                strAppConfigInfoHtml += "ASPNETCORE_ENVIRONMENT: " + _appconfig.config.GetValue<string>("ASPNETCORE_ENVIRONMENT") + "<br/>";
                strAppConfigInfoHtml += "Framework Description: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription + "<br/>";
                strAppConfigInfoHtml += "Instrumentation Key: " + _appconfig.config.GetValue<string>("ApplicationInsights:InstrumentationKey") + "<br/>";
                strAppConfigInfoHtml += "Build Identifier: " + _appconfig.config.GetValue<string>("BuildIdentifier") + "<br/>";
                strAppConfigInfoHtml += "OTEL_EXPORTER_OTLP_ENDPOINT: " + _appconfig.config.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") + "<br/>";
                strAppConfigInfoHtml += "<br/>";
                strAppConfigInfoHtml += "Project Connection string: " + _appconfig.ProjectCS + "<br/>";
                strAppConfigInfoHtml += "Vector Store Id: " + _appconfig.VectorStoreId + "<br/>";
            }
            catch (Exception ex)
            {
                strAppConfigInfoHtml += ex.Message;
            }

        }
    }
}
