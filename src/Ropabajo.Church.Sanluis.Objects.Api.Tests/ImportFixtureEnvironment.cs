using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

internal sealed class ImportFixtureEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Local";
    public string ApplicationName { get; set; } = "ImportTests";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
