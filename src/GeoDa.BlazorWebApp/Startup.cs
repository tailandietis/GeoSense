using System.IO;
using GeoDa.Application;
using GeoDa.BlazorWebApp.Services.MLBuilder;
using QuestPDF.Infrastructure;
using GeoDa.Application.Authentication.StateProvider;
using GeoDa.Application.Databases.Models;
using GeoDa.Application.GeneralForecasts.Services.Utils;
using GeoDa.Application.RegionalForecasts.Services.VolumeMaps;
using GeoDa.BlazorWebApp.HostedServices;
using GeoDa.BlazorWebApp.Services.AreaServices;
using GeoDa.BlazorWebApp.Services.Observers;
using GeoDa.BlazorWebApp.Services.WebAppUtils;
using GeoDa.Database;
using GeoDa.Domain;
using GeoDa.Domain.CurrentForecasts.Models.Settings;
using GeoDa.Domain.RegionalForecasts.Models.Settings;
using GeoDa.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace GeoDa.BlazorWebApp;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

        services.AddControllers();

        services.Configure<DbsSettings>(_configuration.GetSection("DbsSettings"));
        services.Configure<RegionalForecastSettings>(
            _configuration.GetSection("Modules").GetSection("RegionalForecast"));
        services.Configure<CurrentForecastSettings>(
            _configuration.GetSection("Modules").GetSection("CurrentForecast"));

        services.AddInfrastructure();
        services.AddDomain();
        services.AddDatabases();
        services.AddApplication();

        services.Configure<VolumetricBuilderSettings>(_configuration.GetSection("VolumetricBuilder"));
        services.AddScoped<IRfVolumeMapsService, RfVolumeMapsService>();

        services.Configure<MLBuilderSettings>(_configuration.GetSection("MLBuilder"));
        services.AddScoped<IMLBuilderService, MLBuilderService>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication3", Version = "v1" });
        });

        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.AddRazorPages(options =>
        {
            options.RootDirectory = "/Views/Pages";
        });

        services.AddAntDesign();

        services.AddHostedService<GeoDaWebAppHostedService>();
        services.AddSingleton<IObserverService, ObserverService>();
        services.AddSingleton<IAreaService, AreaService>();
        services.AddSingleton<IWebAppUtils, WebAppUtils>();

        services.AddScoped<ProtectedSessionStorage>();
        services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        var mlOutputDir = _configuration["MLBuilder:OutputDir"] ?? ".\\mlbuilder\\output";
        var mlPlotsPath = Path.GetFullPath(Path.Combine(mlOutputDir, "plots"));
        if (!Directory.Exists(mlPlotsPath))
            Directory.CreateDirectory(mlPlotsPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(mlPlotsPath),
            RequestPath = "/ml-plots"
        });

        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication3 v1"));

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
        });
    }
}