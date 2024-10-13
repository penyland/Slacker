using Infinity.Toolkit.FeatureModules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Slacker.Api.Features;

public class InfoModule : WebFeatureModule
{
    public override IModuleInfo? ModuleInfo { get; } = new FeatureModuleInfo("InfoModule", "1.0.0");

    public override ModuleContext RegisterModule(ModuleContext moduleContext)
    {
        return moduleContext;
    }

    public override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/info")
            .WithOpenApi()
            .WithTags("Info");

        group.MapGet("/", GetSystemInfo)
            .WithName("SystemInfo")
            .WithDisplayName("Get system info");

        group.MapGet("/claims", GetClaims);
        group.MapGet("/config", GetConfig);
        group.MapGet("/headers", GetHeaders);
        group.MapGet("/modules", GetFeatureModuleInfos);
    }

    private static JsonHttpResult<IEnumerable<FeatureModuleInfo>> GetFeatureModuleInfos(IEnumerable<IFeatureModule> featureModules)
    {
        var modules = featureModules.Select(t =>
            new FeatureModuleInfo(t?.ModuleInfo?.Name, t?.ModuleInfo?.Version));
        return TypedResults.Json(modules);
    }

    private static IResult GetClaims(ClaimsPrincipal user)
    {
        var claims = user.Claims.Select(t => new { t.Type, t.Value });

        return TypedResults.Json(claims);
    }

    private static IResult GetHeaders(HttpRequest httpRequest) => TypedResults.Json(httpRequest.Headers);

    private static ContentHttpResult GetConfig(IConfiguration configuration) => TypedResults.Text((configuration as IConfigurationRoot)!.GetDebugView());

    private static JsonHttpResult<Response> GetSystemInfo(IWebHostEnvironment webHostEnvironment)
    {
        var processorArchitecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "64-bit",
            Architecture.X86 => "32-bit",
            Architecture.Arm => "ARM",
            Architecture.Arm64 => "ARM64",
            _ => "Unknown"
        };

        return TypedResults.Json(new Response
        {
            Name = Assembly.GetEntryAssembly()?.GetName().Name ?? webHostEnvironment.ApplicationName ?? "Name",
            Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0",
            DateTime = DateTimeOffset.Now.UtcDateTime,
            Environment = webHostEnvironment.EnvironmentName,
            FrameworkVersion = Environment.Version.ToString(),
            OSVersion = Environment.OSVersion.ToString(),
            BuildDate = File.GetLastWriteTime(Assembly.GetEntryAssembly()!.Location).ToString("yyyy-MM-dd HH:mm:ss"),
            Host = Environment.MachineName,
            ProcessorArchitecture = processorArchitecture,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OSDescription = RuntimeInformation.OSDescription
        });
    }

    private record Response()
    {
        public string? Name { get; init; }

        public string? Version { get; init; }

        public DateTimeOffset DateTime { get; init; }

        public string? Environment { get; init; }

        public string? FrameworkVersion { get; init; }

        public string? OSVersion { get; init; }

        public string? BuildDate { get; init; }

        public string? Host { get; init; }

        public string? ProcessorArchitecture { get; init; }

        public string? FrameworkDescription { get; init; }

        public string? RuntimeIdentifier { get; init; }

        public string? OSArchitecture { get; init; }

        public string? OSDescription { get; init; }
    }
}
