using AdoToolkit.Models;

namespace AdoToolkit.Config;

public sealed class SchemaValidationService
{
    public void Validate(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(config.CurrentContext);
        ArgumentNullException.ThrowIfNull(config.Settings);

        if (string.IsNullOrWhiteSpace(config.SchemaVersion))
        {
            throw new InvalidOperationException("Config schema version is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Settings.StorageRootPath))
        {
            throw new InvalidOperationException("Config storage root path is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Settings.OutputRootPath))
        {
            throw new InvalidOperationException("Config output root path is required.");
        }

        if (string.IsNullOrWhiteSpace(config.Settings.PromptRootPath))
        {
            throw new InvalidOperationException("Config prompt root path is required.");
        }
    }
}

