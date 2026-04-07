using AdoToolkit.Config;

namespace AdoToolkit.Tests.Integration;

public sealed class AppConfigServiceTests
{
    [Fact]
    public async Task Load_or_create_persists_default_storage_values()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var service = new AppConfigService(new AppPaths(home), new JsonFileStore(), new SchemaValidationService());

        var config = await service.LoadOrCreateAsync();

        Assert.Equal(Path.Combine(home, ".ado-toolkit"), config.Settings.StorageRootPath);
        Assert.True(File.Exists(Path.Combine(home, ".ado-toolkit", "config.json")));
    }
}

