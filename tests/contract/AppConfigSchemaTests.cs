using System.Text.Json;
using AdoToolkit.Config;
using AdoToolkit.Models;

namespace AdoToolkit.Tests.Contract;

public sealed class AppConfigSchemaTests
{
    [Fact]
    public void Default_config_serializes_required_root_properties()
    {
        var service = new AppConfigService(new AppPaths(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))), new JsonFileStore(), new SchemaValidationService());
        var config = service.CreateDefault();

        var json = JsonSerializer.Serialize(config);
        using var document = JsonDocument.Parse(json);

        Assert.True(document.RootElement.TryGetProperty("SchemaVersion", out _)
            || document.RootElement.TryGetProperty("schemaVersion", out _));
        Assert.True(document.RootElement.TryGetProperty("CurrentContext", out _)
            || document.RootElement.TryGetProperty("currentContext", out _));
        Assert.True(document.RootElement.TryGetProperty("Settings", out _)
            || document.RootElement.TryGetProperty("settings", out _));
    }
}

