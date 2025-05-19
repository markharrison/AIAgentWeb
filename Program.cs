using AIAgentWeb.Services;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AIAgentWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<AppConfig>(new AppConfig(builder.Configuration));
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1000 * 1000 * 1000; // 1 GB
            });
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 1000 * 1000 * 1000; // 1 GB
            });
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(c => c.AddService("AIAgentWeb"));
            builder.Services.AddSingleton<AgentStateService>();
            builder.Services.AddSingleton<AgentStateServiceSK>();

            if (builder.Environment.IsDevelopment())
            {
                //builder.Services.AddOpenTelemetry()
                //    .WithMetrics(metrics =>
                //    {
                //        metrics.AddAspNetCoreInstrumentation()
                //            .AddHttpClientInstrumentation()
                //            .AddRuntimeInstrumentation();
                //    })
                //    .WithTracing(tracing =>
                //    {
                //        tracing.SetSampler(new AlwaysOnSampler());
                //        tracing.AddAspNetCoreInstrumentation()
                //            .AddHttpClientInstrumentation()
                //            .AddSource("Microsoft.AspNetCore.SignalR.Server");
                //    });
            }
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            var app = builder.Build();

            var agentStateService = app.Services.GetRequiredService<AgentStateService>(); 

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
