using AdoToolkit.Models;

namespace AdoToolkit.Config;

public sealed class AppConfigService
{
    private readonly AppPaths _paths;
    private readonly JsonFileStore _store;
    private readonly SchemaValidationService _validator;

    public AppConfigService(AppPaths paths, JsonFileStore store, SchemaValidationService validator)
    {
        _paths = paths;
        _store = store;
        _validator = validator;
    }

    public async Task<AppConfig> LoadOrCreateAsync(CancellationToken cancellationToken = default)
    {
        EnsureDirectories();

        var config = await _store.LoadAsync<AppConfig>(_paths.GetConfigPath(), cancellationToken);
        if (config is null)
        {
            config = CreateDefault();
            await SaveAsync(config, cancellationToken);
            return config;
        }

        ApplyDefaults(config);
        _validator.Validate(config);
        return config;
    }

    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ApplyDefaults(config);
        _validator.Validate(config);
        await _store.SaveAsync(_paths.GetConfigPath(), config, cancellationToken);
    }

    public AppConfig CreateDefault()
    {
        return new AppConfig
        {
            Settings = new AppSettings
            {
                StorageRootPath = _paths.GetStorageRoot(),
                OutputRootPath = _paths.GetOutputRoot(),
                PromptRootPath = _paths.GetPromptRoot(),
                HeaderStyle = "default"
            }
        };
    }

    private void EnsureDirectories()
    {
        Directory.CreateDirectory(_paths.GetStorageRoot());
        Directory.CreateDirectory(_paths.GetOutputRoot());
        Directory.CreateDirectory(_paths.GetPromptRoot());
    }

    private void ApplyDefaults(AppConfig config)
    {
        config.Settings ??= new AppSettings();
        config.CurrentContext ??= new CurrentContext();
        config.WorkItems ??= [];
        config.PullRequests ??= [];
        config.SchemaVersion = string.IsNullOrWhiteSpace(config.SchemaVersion) ? "1.0.0" : config.SchemaVersion;
        config.Settings.StorageRootPath = _paths.GetStorageRoot();
        config.Settings.OutputRootPath = string.IsNullOrWhiteSpace(config.Settings.OutputRootPath)
            ? _paths.GetOutputRoot()
            : config.Settings.OutputRootPath;
        config.Settings.PromptRootPath = string.IsNullOrWhiteSpace(config.Settings.PromptRootPath)
            ? _paths.GetPromptRoot()
            : config.Settings.PromptRootPath;
        config.Settings.HeaderStyle = string.IsNullOrWhiteSpace(config.Settings.HeaderStyle)
            ? "default"
            : config.Settings.HeaderStyle;
    }
}

